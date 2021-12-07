using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public GameObject boidPrefab;
    private Boid[] boidArray;
    private int numOfBoids = 100;
    
    
    // Start is called before the first frame update
    void Start() {

        boidArray = new Boid[numOfBoids];

        for (int i = 0; i < boidArray.Length; i++) {
            GameObject gameObject = Instantiate(boidPrefab, this.transform.position, this.transform.rotation);
            Boid b = gameObject.GetComponent<Boid>();
            b.init(this.transform.position);
            // b.transform.position = new Vector3(0, 0, 0);
            boidArray[i] = b;
        } // for

        
    } // Start()
}