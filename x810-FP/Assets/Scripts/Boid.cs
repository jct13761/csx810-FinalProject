using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


/*
 * REFERENCES/Sources:
 * https://www.youtube.com/watch?v=bqtqltqcQhw
 * https://www.youtube.com/watch?v=mhjuuHl6qHM
 * https://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
 */
public class Boid : MonoBehaviour {
    public Vector3 velocity; // velocity of the boid
    public Vector3 acceleration; // acceleration value of the boid 
    private Vector3 forward; // direction the boid is facing
    private Transform boidTransfrom; // the transform of the boid
    private Vector3 boidPosition; // the position of the boid


    private float minSpeed = 9; // the speed of the boid
    private float maxSpeed = 10; // the speed of the boid
    private float maxSteerForce = 2; // the Steer force of the boid of the boid

    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;

    private float viewAngle = 180.0f;
    private float perceptionRadius = 2.5f;
    private float avoidanceRadius = 1;
    private int neighborBoids = 0;

    private float width = 50.0f, height = 30.0f; // the width/height of the arena box
    private float buffer = 1.0f; // a buffer to make the transition from top to bottom more smooth

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Initialize the variables for the boid
    /// </summary>
    public void Init() {
        boidTransfrom = this.transform;
        acceleration = Vector3.zero;
        forward = Random.insideUnitSphere;
        velocity = forward * (minSpeed + maxSpeed) / 2;
    }

    /// <summary>
    /// Update the Boid's position, direction, etc. This is the function that applies the 3 main rules of Boids.
    /// Gets called from BoidManager. 
    /// </summary>
    /// <param name="boids">The array of boids to update</param>
    public void UpdateBoid(Boid[] boids) {
        Vector3 alignRule = this.Align(boids);

        if (neighborBoids > 0) {
            Vector3 alignDir = SteerTowards(alignRule) * alignWeight;
            acceleration += alignDir;
        }

        // set the current position
        boidPosition = this.transform.position;

        // This is the collision detection for the edges of the arena box
        if (boidPosition.x < -width / 2 + buffer) {
            // check -/+ of X-axis
            this.gameObject.transform.position = new Vector3(width / 2 - buffer, boidPosition.y, boidPosition.z);
        }
        else if (boidPosition.x > width / 2 - buffer) {
            this.gameObject.transform.position = new Vector3(-width / 2 + buffer, boidPosition.y, boidPosition.z);
        }
        else if (boidPosition.y < 0 + buffer) {
            // check -/+ of Y-axis
            this.gameObject.transform.position = new Vector3(boidPosition.x, height - buffer, boidPosition.z);
        }
        else if (boidPosition.y > height - buffer) {
            this.gameObject.transform.position = new Vector3(boidPosition.x, 0 + buffer, boidPosition.z);
        }
        else if (boidPosition.z < -width / 2 + buffer) {
            // check -/+ of Z-axis
            this.gameObject.transform.position = new Vector3(boidPosition.x, boidPosition.y, width / 2 - buffer);
        }
        else if (boidPosition.z > width / 2 - buffer) {
            this.gameObject.transform.position = new Vector3(boidPosition.x, boidPosition.y, -width / 2 + buffer);
        } // if-else

        // Calculate the velocity of the boid
        // add the time as well as any acceleration to make it move
        velocity += acceleration * Time.deltaTime;
        // get the magnitude 
        float s = velocity.magnitude;
        // calulate the forward direction of the boid with the new velocity 
        Vector3 dir = velocity / s;
        s = Mathf.Clamp(s, minSpeed, maxSpeed);
        // set the velocity to the direction and speed
        velocity = dir * s;

        // update the position of the boid by moving it to by the velocity
        transform.position += velocity * Time.deltaTime;
        // set the new forward direction
        forward = dir;
        // rotate towards the new forward direction.
        transform.rotation = Quaternion.LookRotation(forward);
    } // updateBoid()

    
    /// <summary>
    /// implements the align functionality of the boids
    /// </summary>
    /// <param name="boids">The array of boids to check</param>
    /// <returns>The average directions of the neighboring Boids</returns>
    Vector3 Align(Boid[] boids) {
        // reset the neighbor count
        neighborBoids = 0;

        // create a vector to be returned with the average direction of the neighboring boids 
        Vector3 avgDir = Vector3.zero;

        // check each boid against "this" boid
        foreach (Boid b in boids) {
            // check if b is "this"
            if (b != this) {
                // // Get the distance between boid b and "this"
                // Vector3 offset = b.boidPosition - this.boidPosition;
                // // get the radius distance. 
                // float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;
                //
                // // if the distnace is less than the perception radius 
                // if (sqrDst < perceptionRadius * perceptionRadius) {
                //     // increment the neighbor boid count
                //     neighborBoids++;
                //     // add the forward of b to the avg direction vector
                //     avgDir += b.forward;
                // }

                // get the distance from boid b to "this" in a radius around "this"
                float dist = Vector3.Distance(this.boidPosition, b.transform.position);

                // if the distnace is less than the perception radius 
                if (dist < perceptionRadius * perceptionRadius) {
                    // add the forward of b to the avg direction vector
                    avgDir += b.forward;
                    // increment the neighbor boid count
                    neighborBoids++;
                } // if
            } // if
        } // foreach()

        // if there were neighbor boids, find the average of the direction vector
        if (neighborBoids > 0) {
            avgDir = avgDir / neighborBoids;
        } // if

        return avgDir;
    } // align()

    // 
    /// <summary>
    /// This function Takes a vector and makes the boid steer towards that direction
    /// </summary>
    /// <param name="vector">The Intended direction's vector</param>
    /// <returns>the new direction for the boid</returns>
    Vector3 SteerTowards(Vector3 vector) {
        // gets the new direction of the vector
        // Formula from Boids Paper: https://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
        /*
         *     IntendedDir
         *      ------->
         *      ^      ^
         *      |     /
         *      |    / New Dir
         *curDir|   / 
         *      |  /
         *      | /
         *      |/
         */
        // NewDir = IntendedDir - curDir
        Vector3 v = vector.normalized * maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, maxSteerForce);
    }
}