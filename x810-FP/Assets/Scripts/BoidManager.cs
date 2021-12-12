using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {
    
    public GameObject boidPrefab;
    public GameObject alignmentArrow, cohesionSphere;
    private Boid[] boidArray;
    private int numOfBoids = 100;

    public float AlignmentWeight, CohesionWeight, SeparationWeight, SteerWeight;
    
    // Start is called before the first frame update
    void Start() {
        AlignmentWeight = 1;
        CohesionWeight = 1;
        SeparationWeight = 1;
        SteerWeight = 10;
        
        boidArray = new Boid[numOfBoids];
        for (int i = 0; i < boidArray.Length; i++) {
            GameObject gameObject = Instantiate(boidPrefab, this.transform.position, this.transform.rotation);
            Boid b = gameObject.GetComponent<Boid>();
            b.Init();
            // b.transform.position = new Vector3(0, 0, 0);
            // Material m = b.GetComponentInChildren<Renderer>().material;
            // Renderer[] M = b.GetComponentsInChildren<Renderer>();
            // if (M != null) {
            //     foreach (Renderer m in M) {
            //         float r = Random.Range(0.0f, 1.0f), g = Random.Range(0.0f, 1.0f), B = Random.Range(0.0f, 1.0f);
            //         m.material.color = new Color(#FFFFFF);
            //     }
            //     
            // }
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
            b.SetAllWeights(AlignmentWeight, CohesionWeight, SeparationWeight, SteerWeight);
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
        //                 boids[j].alignmentDirection += boidB.forward;
        //                 boids[j].centreOfFlockmates += boidB.position;
        //
        //                 if (sqrDst < avoidRadius * avoidRadius) {
        //                     boids[j].seperationDirection -= offset / sqrDst;
        //                 }
        //             }
        //         }
        //
        //     }
        // }

    }
}
