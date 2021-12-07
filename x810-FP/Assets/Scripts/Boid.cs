using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    public Vector3 direction;
    public Vector3 forward;
    public float speed;
    public Vector3 velocity;
    public Vector3 acceleration;
    private float width = 50.0f, height = 30.0f;
    float buffer = 1.0f;
    
    
    // Start is called before the first frame update
    void Start() {
        speed = 1;
        direction = new Vector3(1, 1, 1);
        this.gameObject.transform.position = new Vector3(0, 15, 0);
        acceleration = Vector3.zero;
        forward = this.transform.forward;
        velocity = forward * speed;

    }

    // Update is called once per frame
    void Update() {

        Transform boidTransfrom = this.GetComponent<Transform>();

        Debug.Log("pos = " + boidTransfrom.position);
        // Debug.Log("Xpos % width = " + (int)boidTransfrom.position.x % 25);

        Vector3 boidPos = boidTransfrom.position;
        
        if (boidPos.x < -width/2 + buffer) { // check -/+ of X-axis
            Debug.Log("pos is less than neg edge");
            this.gameObject.transform.position = new Vector3(width/2 - buffer, boidPos.y, boidPos.z);
        }else if (boidPos.x > width/2 - buffer) {
            Debug.Log("pos is greater than pos edge");
            this.gameObject.transform.position = new Vector3(-width/2 + buffer, boidPos.y, boidPos.z);
        } else if (boidPos.y < 0 + buffer) { // check -/+ of Y-axis
            Debug.Log("pos is less than neg edge");
            this.gameObject.transform.position = new Vector3(boidPos.x, height - buffer, boidPos.z);
        } else if (boidPos.y > height - buffer) {
            Debug.Log("pos is greater than pos edge");
            this.gameObject.transform.position = new Vector3(boidPos.x,0 + buffer, boidPos.z);
        } else if (boidPos.z < -width/2 + buffer) { // check -/+ of Z-axis
            Debug.Log("pos is less than neg edge");
            this.gameObject.transform.position = new Vector3(boidPos.x, boidPos.y, width/2 - buffer);
        }else if (boidPos.z > width/2 - buffer) {
            Debug.Log("pos is greater than pos edge");
            this.gameObject.transform.position = new Vector3(boidPos.x, boidPos.y, -width/2 + buffer);
        } 
        
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