using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {
    
    public GameObject boidPrefab;
    public Material[] BoidMaterials;
    public GameObject alignmentArrow, cohesionSphere;
    private Boid[] boidArray;
    private int numOfBoids = 200;

    public float AlignmentWeight, CohesionWeight, SeparationWeight, SteerWeight, BoundsRadius, CollisionAvoidDst;
    
    // Start is called before the first frame update
    void Start() {
        AlignmentWeight = 1;
        CohesionWeight = 1;
        SeparationWeight = 1;
        SteerWeight = 50;
        BoundsRadius = 0.27f;
        CollisionAvoidDst = 15;
        
        boidArray = new Boid[numOfBoids];
        for (int i = 0; i < boidArray.Length; i++) {
            GameObject gameObject = Instantiate(boidPrefab, this.transform.position, this.transform.rotation);
            Boid b = gameObject.GetComponent<Boid>();
            b.Init();
            
            Renderer[] renderers = b.GetComponentsInChildren<Renderer>();
            if (renderers != null)
                foreach (Renderer r in renderers) {
                    // float R = Random.Range(0.0f, 1.0f), G = Random.Range(0.0f, 1.0f), B = Random.Range(0.0f, 1.0f);
                    // r.material.color = new Color(R, G, B);
                    int randMatPos = Random.Range(0, BoidMaterials.Length);
                    // int randMatPos = Random.Range(0, 2);
                    r.material = BoidMaterials[randMatPos];
                    b.SetRaceIndex(randMatPos);
                } // foreach
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
            b.SetCollisionAvoidDistancet(CollisionAvoidDst);
            b.SetBoundsRadius(BoundsRadius);
        }

        if (alignmentArrow != null)
            alignmentArrow.transform.rotation = Quaternion.LookRotation(avgAlign);
        if (cohesionSphere != null)
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
