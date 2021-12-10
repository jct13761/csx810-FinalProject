using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {
    
    public GameObject boidPrefab;
    public GameObject alignmentArrow, cohesionSphere;
    private Boid[] boidArray;
    private int numOfBoids = 100;
    
    // Start is called before the first frame update
    void Start() {
        boidArray = new Boid[numOfBoids];
        for (int i = 0; i < boidArray.Length; i++) {
            GameObject gameObject = Instantiate(boidPrefab, this.transform.position, this.transform.rotation);
            Boid b = gameObject.GetComponent<Boid>();
            b.Init();
            // b.transform.position = new Vector3(0, 0, 0);
            boidArray[i] = b;
        } // for
    } // Start()
    
    
    private void Update() {

        Vector3 avgAlign = Vector3.zero;
        Vector3 avgCohesionPos = Vector3.zero;
        
        
        foreach (Boid b in boidArray) {
            b.UpdateBoid(boidArray);
            avgAlign += b.forward;
            avgCohesionPos += b.boidPosition;
        }

        alignmentArrow.transform.rotation = Quaternion.LookRotation(avgAlign);
        cohesionSphere.transform.position = avgCohesionPos/numOfBoids;

        // int numBoids = boids.Length;
        // float viewRadius = settings.perceptionRadius;
        // float avoidRadius = settings.avoidanceRadius;
        //
        // for (int j = 0; j < numBoids; j++) {
        //     for (int indexB = 0; indexB < numBoids; indexB++) {
        //         if (j != indexB) {
        //             Boid boidB = boids[indexB];
        //             Vector3 offset = boidB.position - boids[j].position;
        //             float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;
        //
        //             if (sqrDst < viewRadius * viewRadius) {
        //                 boids[j].numPerceivedFlockmates += 1;
        //                 boids[j].avgFlockHeading += boidB.forward;
        //                 boids[j].centreOfFlockmates += boidB.position;
        //
        //                 if (sqrDst < avoidRadius * avoidRadius) {
        //                     boids[j].avgAvoidanceHeading -= offset / sqrDst;
        //                 }
        //             }
        //         }
        //
        //     }
        // }

    }
}
