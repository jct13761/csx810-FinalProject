using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {
    
    public Vector3 velocity; // velocity of the boid
    public Vector3 acceleration; // acceleration value of the boid 
    
    private Vector3 forward; // direction the boid is facing
    private Transform boidTransfrom; // the transform of the boid
    private Vector3 boidPosition; // the position of the boid
    private float speed = 10; // the speed of the boid
    
    private float width = 50.0f, height = 30.0f; // the width/height of the arena box
    private float buffer = 1.0f; // a buffer to make the transition from top to bottom more smooth
    
    
    // Start is called before the first frame update
    void Start() {
        //init();
       
    } // Start

    /*
     * Initialize the variables for the boid
     */
    public void init(Vector3 spawnPosition) {
        boidTransfrom = this.transform;
        acceleration = Vector3.zero;
        forward = Random.insideUnitSphere;
        velocity = forward * speed;
    }

    // Update is called once per frame
    void Update() {
        // Debug.Log("pos = " + boidTransfrom.position);
        // Debug.Log("Xpos % width = " + (int)boidTransfrom.position.x % 25);

        boidPosition = this.transform.position;
        
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
        
        // Calculate the velocity of the boid
        // add the time as well as any acceleration to make it move
        velocity += acceleration * Time.deltaTime; 
        // get the magnitude 
        float s = velocity.magnitude;
        // calulate the forward direction of the boid with the new velocity 
        Vector3 dir = velocity / s;
        // set the velocity to the direction and speed
        velocity = dir * s;

        // update the position of the boid by moving it to by the velocity
        transform.position += velocity * Time.deltaTime;
        // set the new forward direction
        forward = dir;
        // rotate towards the new forward direction.
        transform.rotation = Quaternion.LookRotation(forward);





    } // Update
}