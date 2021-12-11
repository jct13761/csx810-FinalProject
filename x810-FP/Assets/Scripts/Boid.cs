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
    // public Vector3 acceleration; // acceleration value of the boid 
    [HideInInspector]
    public Vector3 forward; // direction the boid is facing
    private Transform boidTransfrom; // the transform of the boid
    [HideInInspector]
    public Vector3 boidPosition; // the position of the boid

    private Vector3 _alignmentDirection; // alignment
    private Vector3 _cohesionDirection; // cohesion 
    private Vector3 _separationDirection; // separation


    private float minSpeed = 19; // the speed of the boid
    private float maxSpeed = 20; // the speed of the boid
    private float maxSteerForce = 100; // the Steer force of the boid of the boid

    public float alignWeight = 1; 
    public float cohesionWeight = 1; 
    public float seperateWeight = 1; 

    private float viewAngle = 180.0f;
    private float perceptionRadius = 25f;
    private float avoidanceRadius = 1;
    public int neighborBoids = 0;

    private float width = 50.0f, height = 30.0f; // the width/height of the arena box
    private float buffer = 1.0f; // a buffer to make the transition from top to bottom more smooth

    
    
    private float NoClumpingRadius = 5f;
    private float LocalAreaRadius = 5f;

    
    
    
    
    public LayerMask obstacleMask;
    public float boundsRadius = .27f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDst = 5;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Initialize the variables for the boid
    /// </summary>
    public void Init() {
        boidTransfrom = this.transform;
        // acceleration = Vector3.zero;
        forward = Random.insideUnitSphere;
        velocity = forward * (minSpeed + maxSpeed) / 2;
    } // Init()

    /// <summary>
    /// Update the Boid's position, direction, etc. This is the function that applies the 3 main rules of Boids.
    /// Gets called from BoidManager. 
    /// </summary>
    /// <param name="boids">The array of boids to update</param>
    public void UpdateBoid(Boid[] boids) {
        
        // set the current position
        boidPosition = this.transform.position;
        
        Vector3 acceleration = Vector3.zero;
        
        // Vector3 alignmentRule = this.Alignment(boids);
        // Vector3 cohestionRule = this.Cohesion(boids);
        // Vector3 separationRule = this.Separation(boids);
        
        BoidRules(boids);

        if (neighborBoids != 0) {
            
            Vector3 alignmentForce = SteerTowards (_alignmentDirection) * alignWeight; // Alignment 
            acceleration += alignmentForce;
            
            Vector3 cohesionForce = SteerTowards (_cohesionDirection) * cohesionWeight; // cohesion
            acceleration += cohesionForce;
            
            Vector3 separationForce = SteerTowards (_separationDirection) * seperateWeight; // separation
            acceleration += separationForce;
            
        } // if
        

        // set the current position
        boidPosition = this.transform.position;
        
        
        
        // if (IsHeadingForCollision ()) {
        //     Vector3 collisionAvoidDir = ObstacleRays ();
        //     Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * avoidCollisionWeight;
        //     acceleration += collisionAvoidForce;
        // }
        
        
        
        // This is the collision detection for the edges of the arena box
        CheckForWallCollision();
        

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

        // // update the position of the boid by moving it to by the velocity
        // boidPosition += velocity * Time.deltaTime;
        // // set the new forward direction
        // forward = dir;
        // this.transform.position = boidPosition;
        // // rotate towards the new forward direction.
        // transform.rotation = Quaternion.LookRotation(forward);
        
        // Forces boids to ignore Y-axis and only move in 2D
        // velocity = new Vector3(velocity.x, 0, velocity.z);
        
        boidTransfrom.position += velocity * Time.deltaTime;
        boidTransfrom.forward = dir;
        boidPosition = boidTransfrom.position;
        forward = dir;
        
    } // updateBoid()


    void CheckForWallCollision() {
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
    } // CheckForWallCollision()
    
    
    
    /// <summary>
    /// Checks for and applies Boid Alignment, Cohesion, and Separation.
    /// </summary>
    /// <param name="boids">The array of boids to check.</param>
    void BoidRules(Boid[] boids) {
        // reset the neighbor count
        neighborBoids = 0;
        // create temp counters for the rules 
        int alignmentCohesionCount = 0, separationCount = 0;
        
        // create a temp vectors to hold the sum of the differenc fules
        Vector3 alignmentDir = Vector3.zero;
        Vector3 cohesionDir = Vector3.zero;
        Vector3 separationDir = Vector3.zero;
        
        // temp vars for the position of b and self
        Vector3 thisPos = this.transform.position;

        // check each boid against self boid
        foreach (Boid b in boids) {
            // check if b is not self
            if (b != this) {
                // temp var for the position of b
                Vector3 bPos = b.transform.position;
                // calculate the distance between the self and b as a float
                float dist = Vector3.Distance(bPos, thisPos);
                // calculate the distance between the self and b as a Vector3
                Vector3 difference = bPos - thisPos;

                // Alignment and Cohesion - if the distance is within the specified perception radius...
                if (dist < LocalAreaRadius) {
                    // add b's forward direction for the alignment var
                    alignmentDir += b.transform.forward;
                    // add the distance between b and self for the cohesion var
                    cohesionDir += difference; 
                    // increment the counter
                    alignmentCohesionCount++;
                } // if
                
                // Separation - if the if the distance is within the specified avoidance radius...
                if (dist < NoClumpingRadius) {
                    // add the distance between b and self for the separation var
                    separationDir += difference;
                    separationCount++;
                } // if
            } // if
        } // foreach()

        // set the neighbor count to the larger of the 2 numbers 
        neighborBoids = Mathf.Max(alignmentCohesionCount, separationCount);

        // set the global var for alignment
        _alignmentDirection = alignmentDir;
        // subtract the self position from the cohesion summation
        cohesionDir -= this.transform.position;
        // set the global var for cohesion
        _cohesionDirection = cohesionDir;
        
        // if the Separation is not 0, calculate the average separation value
        if (separationCount > 0) separationDir /= separationCount;
        // flip and normalize the average separation distance
        separationDir = -separationDir.normalized;
        // set the global var for separation
        _separationDirection = separationDir;
    } // BoidRules()
    
    
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
        // return v;
    }
    
    /*
    bool IsHeadingForCollision () {
        RaycastHit hit;
        if (Physics.SphereCast (boidPosition, boundsRadius, forward, out hit, collisionAvoidDst, obstacleMask)) {
            return true;
        } else { }
        return false;
    }

    Vector3 ObstacleRays () {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = boidTransfrom.TransformDirection (rayDirections[i]);
            Ray ray = new Ray (boidPosition, dir);
            if (!Physics.SphereCast (ray, boundsRadius, collisionAvoidDst, obstacleMask)) {
                return dir;
            }
        }
        return forward;
    }
    */

    
    /**************************************** Gettters/Setters *****************************************/
    public void SetAllWeights(float a, float c, float s) {
        SetAlignmentWeight(a);
        SetCohesionWeight(c);
        SetSeparationWeight(s);
    } //SetWeights()
    public void SetAlignmentWeight(float a) { alignWeight = a; }
    public void SetCohesionWeight(float c) { cohesionWeight = c; }
    public void SetSeparationWeight(float s) { seperateWeight = s; }
}

/*
public static class BoidHelper {

    const int numViewDirections = 300;
    public static readonly Vector3[] directions;

    static BoidHelper () {
        directions = new Vector3[BoidHelper.numViewDirections];

        float goldenRatio = (1 + Mathf.Sqrt (5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++) {
            float t = (float) i / numViewDirections;
            float inclination = Mathf.Acos (1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin (inclination) * Mathf.Cos (azimuth);
            float y = Mathf.Sin (inclination) * Mathf.Sin (azimuth);
            float z = Mathf.Cos (inclination);
            directions[i] = new Vector3 (x, y, z);
        }
    }

}
*/