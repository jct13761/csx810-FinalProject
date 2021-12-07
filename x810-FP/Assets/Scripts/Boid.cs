using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    public Vector3 direction;
    public float speed;
    float constraint = 50.0f;
    float buffer = 1.0f;
    
    
    // Start is called before the first frame update
    void Start() {
        
        this.gameObject.transform.position = new Vector3(0, 25, 0);
        
    }

    // Update is called once per frame
    void Update() {

        Transform boidTransfrom = this.GetComponent<Transform>();

        Debug.Log("pos = " + boidTransfrom.position);
        // Debug.Log("Xpos % width = " + (int)boidTransfrom.position.x % 25);

        Vector3 boidPos = boidTransfrom.position;
        
        if (boidPos.x < -constraint/2 + buffer) { // ship is past world-space view / off screen
            Debug.Log("pos is less than neg edge");
            this.gameObject.transform.position = new Vector3(constraint/2 - buffer, boidPos.y, boidPos.z);
        }else if (boidPos.x > constraint/2 - buffer) {
            Debug.Log("pos is greater than pos edge");
            this.gameObject.transform.position = new Vector3(-constraint/2 + buffer, boidPos.y, boidPos.z);
        } else if (boidPos.y < 0 + buffer) { // ship is past world-space view / off screen
            Debug.Log("pos is less than neg edge");
            this.gameObject.transform.position = new Vector3(boidPos.x, constraint - buffer, boidPos.z);
        } else if (boidPos.y > constraint - buffer) {
            Debug.Log("pos is greater than pos edge");
            this.gameObject.transform.position = new Vector3(boidPos.x,0 + buffer, boidPos.z);
        } else if (boidPos.z < -constraint/2 + buffer) { // ship is past world-space view / off screen
            Debug.Log("pos is less than neg edge");
            this.gameObject.transform.position = new Vector3(boidPos.x, boidPos.y, constraint/2 - buffer);
        }else if (boidPos.z > constraint/2 - buffer) {
            Debug.Log("pos is greater than pos edge");
            this.gameObject.transform.position = new Vector3(boidPos.x, boidPos.y, -constraint/2 + buffer);
        } 
        
        
        
        boidTransfrom.Translate(direction * speed * Time.deltaTime);

        


    } // Update
}