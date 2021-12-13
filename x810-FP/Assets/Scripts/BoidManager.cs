using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {
    
    public GameObject alignmentArrow, cohesionSphere;
    public GameObject[] boidPrefabs;
    public Material[] BoidMaterials;
    [Range(0, 3)]
    public int BoidShape;
    public int NumOfBoids;
    private int _cacheBoidShape;
    [Range(0.0f, 100.0f)]
    public float AlignmentWeight;
    [Range(0.0f, 100.0f)]
    public float CohesionWeight;
    [Range(0.0f, 100.0f)]
    public float SeparationWeight;
    [Range(0.0f, 100.0f)]
    public float SteerWeight;
    
    private Boid[] boidArray;
    private int _numOfBoids = 200;
    private float CollisionAvoidDst;
    private float BoundsRadius;

    // Show Debug Objects 
    public bool ShowDebugTools;
    private bool _cacheShowDebugTools;
    private MeshRenderer[] debugObjs;
    
    // For Racism
    [Range(1, 10)]
    public int NumOfRaces; // the user specified number of races to have 
    private int maxRaces; // how much of array will be used 
    private int _cacheNumOfRaces; // the user specified number of races to have 
    public bool RacismActive;
    
    // For 2D mode
    public bool Set2DMode;
    private bool _cacheSet2DMode;
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// The Start method for the BoidManager. This will create all the boids and set the default values for the various
    /// parameters. 
    /// </summary>
    void Start() {
        // Set default values for variables
        AlignmentWeight = 1;
        CohesionWeight = 1;
        SeparationWeight = 1;
        SteerWeight = 50;
        BoundsRadius = 0.27f;
        CollisionAvoidDst = 15;
        
        // Boid Prefab setup
        NumOfBoids = _numOfBoids;
        BoidShape = 0;
        _cacheBoidShape = BoidShape;

        // Debug obj setup
        ShowDebugTools = false;
        debugObjs = this.GetComponentsInChildren<MeshRenderer>();
        SetDebugToolVisibilty();
        
        // Racism setup
        NumOfRaces = 1; // set the default number of Races
        maxRaces = Mathf.Min(NumOfRaces, BoidMaterials.Length); // smallest of the 2 numbers will be max
        RacismActive = true;
        
        // 2D Mode setup
        Set2DMode = false;
        _cacheSet2DMode = Set2DMode;
        
        CreateBoid();
        
        
    } // Start()
    
    /// <summary>
    /// The update function. This will 
    /// </summary>
    private void Update() {

        Vector3 avgAlign = Vector3.zero;
        Vector3 avgCohesionPos = Vector3.zero;
        
        // call the update function on the boids to update their position and directions
        foreach (Boid b in boidArray) {
            b.UpdateBoid(boidArray); // calculate the boid rules
            if (ShowDebugTools) {
                avgAlign += b.forward;
                avgCohesionPos += b.boidPosition;
            } // if
            b.SetAllWeights(AlignmentWeight, CohesionWeight, SeparationWeight, SteerWeight);
            b.SetCollisionAvoidDistancet(CollisionAvoidDst);
            b.SetBoundsRadius(BoundsRadius);
            b.SetRacismActive(RacismActive);
        } // foreach

        // If the debug controls are showing, update their position and rotation
        if (ShowDebugTools) {
            if (alignmentArrow != null) alignmentArrow.transform.rotation = Quaternion.LookRotation(avgAlign);
            if (cohesionSphere != null) cohesionSphere.transform.position = avgCohesionPos/_numOfBoids;
        } // if

        if (BoidShape != _cacheBoidShape)
            setBoidShape();
        
        // check for an update to the variable and set the new variable
        if (ShowDebugTools != _cacheShowDebugTools)
            SetDebugToolVisibilty();
        
        // if the race number has been updated.
        if (NumOfRaces != _cacheNumOfRaces && NumOfRaces > 0 && NumOfRaces <= BoidMaterials.Length) 
            SetNewNumOfRaces();

        if (Set2DMode != _cacheSet2DMode)
            SetBoid2DMode();


    } // Update()

    private void CreateBoid() {
        // initialize the new boid array
        boidArray = new Boid[_numOfBoids];
        
        // Create each boid and set its starting data.
        for (int i = 0; i < boidArray.Length; i++) {
            GameObject gameObject = Instantiate(boidPrefabs[BoidShape], this.transform.position, this.transform.rotation);
            Boid b = gameObject.GetComponent<Boid>();
            b.Init();
            b.SetRacismActive(RacismActive);
            Renderer[] renderers = b.GetComponentsInChildren<Renderer>();
            b.SetRenderers(renderers);
            if (renderers != null) SetRandomMaterial(renderers, b);
            boidArray[i] = b;
        } // for
        
    } // CreateBoid()
    
    
    private void setBoidShape() {
        _cacheBoidShape = BoidShape;
        _numOfBoids = NumOfBoids;
        foreach (Boid b in boidArray) b.DestroyGameObject();
        CreateBoid();
    } // setBoidShape()
    

    /// <summary>
    /// Helper function that will set if the debug tools are visible or not.
    /// </summary>
    private void SetDebugToolVisibilty() {
        _cacheShowDebugTools = ShowDebugTools;
        foreach (MeshRenderer meshRenderer in debugObjs) {
            meshRenderer.enabled = _cacheShowDebugTools;
        } // for
    } // SetDebugToolVisibilty()

    /// <summary>
    /// Helper function that sets all the boids to a new Race 
    /// </summary>
    private void SetNewNumOfRaces() {
        _cacheNumOfRaces = NumOfRaces;
        maxRaces = Mathf.Min(NumOfRaces, BoidMaterials.Length); // smallest of the 2 numbers will be max
        Renderer[] boidRenderer;
        foreach (Boid b in boidArray) {
                boidRenderer = b.GetRenderers();
                if (boidRenderer != null) SetRandomMaterial(boidRenderer, b);
        } // foreach
    } // SetNewNumOfRaces()
    
    /// <summary>
    /// Helper function that sets the boid passed in to a random material.
    /// </summary>
    /// <param name="renderers">The Renderers of the boid to update</param>
    /// <param name="b">The boid to change</param>
    private void SetRandomMaterial(Renderer[] renderers, Boid b) {
        int randMatPos = Random.Range(0, NumOfRaces); // Generate a random number for the Race
        b.SetRaceIndex(randMatPos); // Set the race index in the boid
        // loop over all the child renderers and update them
        foreach (Renderer r in renderers)
            r.material = BoidMaterials[randMatPos];
    } // setRandomMaterial()

    private void SetBoid2DMode() {
        _cacheSet2DMode = Set2DMode;
        foreach (Boid b in boidArray) 
            b.Set2DMode(Set2DMode);
    } // SetBoid2DMode()
    
} // BoidManager
