using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    BoidSettings settings;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    // Cached
    Material material;
    Transform cachedTransform;
    Transform target;

    void Awake () {
        material = transform.GetComponentInChildren<MeshRenderer> ().material;
        cachedTransform = transform;
    }

    public void Initialize (BoidSettings settings, Transform target) {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    public void SetColour (Color col) {
        if (material != null) {
            material.color = col;
        }
    }

    public void UpdateBoid () {
        Vector3 acceleration = Vector3.zero;

        if (target != null) {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards (offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0) {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards (avgFlockHeading) * settings.alignWeight; // Alignment 
            acceleration += alignmentForce;

            var cohesionForce = SteerTowards (offsetToFlockmatesCentre) * settings.cohesionWeight; // cohesion
            acceleration += cohesionForce;
            
            // var seperationForce = SteerTowards (avgAvoidanceHeading) * settings.seperateWeight; // seperation
            // acceleration += seperationForce;
        }

        // if (IsHeadingForCollision ()) {
        //     Vector3 collisionAvoidDir = ObstacleRays ();
        //     Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
        //     acceleration += collisionAvoidForce;
        // }
        
        Vector3 boidPosition = this.transform.position;
        float width = 20f;
        float height = 8f;
        float buffer = 1.0f;
        // This is the collision detection for the edges of the arena box
        if (boidPosition.x < -width/2 + buffer) { // check -/+ of X-axis
            this.gameObject.transform.position = new Vector3(width/2 - buffer, boidPosition.y, boidPosition.z);
        }else if (boidPosition.x > width/2 - buffer) {
            this.gameObject.transform.position = new Vector3(-width/2 + buffer, boidPosition.y, boidPosition.z);
        } else if (boidPosition.y < 0 + buffer) { // check -/+ of Y-axis
            this.gameObject.transform.position = new Vector3(boidPosition.x, height - buffer, boidPosition.z);
        } else if (boidPosition.y > height - buffer) {
            this.gameObject.transform.position = new Vector3(boidPosition.x,0 + buffer, boidPosition.z);
        } else if (boidPosition.z < -width/2 + buffer) { // check -/+ of Z-axis
            this.gameObject.transform.position = new Vector3(boidPosition.x, boidPosition.y, width/2 - buffer);
        }else if (boidPosition.z > width/2 - buffer) {
            this.gameObject.transform.position = new Vector3(boidPosition.x, boidPosition.y, -width/2 + buffer);
        } // if-else

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp (speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }

    bool IsHeadingForCollision () {
        RaycastHit hit;
        if (Physics.SphereCast (position, settings.boundsRadius, forward, out hit, settings.collisionAvoidDst, settings.obstacleMask)) {
            return true;
        } else { }
        return false;
    }

    Vector3 ObstacleRays () {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = cachedTransform.TransformDirection (rayDirections[i]);
            Ray ray = new Ray (position, dir);
            if (!Physics.SphereCast (ray, settings.boundsRadius, settings.collisionAvoidDst, settings.obstacleMask)) {
                return dir;
            }
        }

        return forward;
    }

    Vector3 SteerTowards (Vector3 vector) {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude (v, settings.maxSteerForce);
    }

}