using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {
    
    public GameObject alignmentArrow, cohesionSphere; // the debug prefabs 
    public GameObject[] boidPrefabs; // the array of boid prefabs
    public Material[] BoidMaterials; // The array of Boid materials
    [Range(0, 3)]
    public int BoidShape; // The shape of the boid, Set in the editor
    private int _cacheBoidShape;// the cached index of the shape of the boids
    public int NumOfBoids; // The number of boids 
    private int _numOfBoids = 200; // the default number of boids 
    [Range(0.0f, 100.0f)]
    public float AlignmentWeight; // The alignment weight for the boids rules 
    [Range(0.0f, 100.0f)]
    public float CohesionWeight; // The cohesion weight for the boids rules 
    [Range(0.0f, 100.0f)]
    public float SeparationWeight; // The separation weight for the boids rules 
    [Range(0.0f, 100.0f)]
    public float LeaderWeight; // The steer weight for the boids rules 
    [Range(0.0f, 100.0f)]
    public float SteerWeight; // The steer weight for the boids rules 
    
    
    private Boid[] boidArray; // The cached array of spawned boids
    private float CollisionAvoidDst; // the collision avoidance distance for collision avoidance
    private float BoundsRadius; // the bounds radius for collision avoidance

    // Show Debug Objects 
    public bool ShowDebugTools; // The bool to show the debug tools 
    private bool _cacheShowDebugTools; // The cached bool to show the debug tools 
    private MeshRenderer[] debugObjs; // The number of child debug objects on the spawner object 
    
    // For Diversity/Bias/Racism
    [Range(1, 10)]
    public int Diversity; // the user specified number of races to have 
    private int maxDiversity; // how much of array will be used 
    private int _cacheDiversity; // the user specified number of races to have 
    public bool BiasActive; // the public bool for is racism is active
    
    // For 2D mode
    public bool Set2DMode; // the bool to set 2D mode or not
    private bool _cacheSet2DMode; // the cached bool to set 2D mode or not
    
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
        LeaderWeight = 0;
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
        
        // Diversity/Bias/Racism setup
        Diversity = 1; // set the default number of Races
        maxDiversity = Mathf.Min(Diversity, BoidMaterials.Length); // smallest of the 2 numbers will be max
        BiasActive = true;
        
        // 2D Mode setup
        Set2DMode = false;
        _cacheSet2DMode = Set2DMode;
        
        // Create and spawn the boids 
        CreateBoid(); 
    } // Start()
    
    /// <summary>
    /// The update function. This will update the position of the boids as well as check for and update the user controls.
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
            b.SetAllWeights(AlignmentWeight, CohesionWeight, SeparationWeight, LeaderWeight, SteerWeight);
            b.SetCollisionAvoidDistancet(CollisionAvoidDst);
            b.SetBoundsRadius(BoundsRadius);
            b.SetBaisActive(BiasActive);
        } // foreach

        // If the debug controls are showing, update their position and rotation
        if (ShowDebugTools) {
            if (alignmentArrow != null) alignmentArrow.transform.rotation = Quaternion.LookRotation(avgAlign);
            if (cohesionSphere != null) cohesionSphere.transform.position = avgCohesionPos/_numOfBoids;
        } // if

        // check if the boid shape variable was updated 
        if (BoidShape != _cacheBoidShape)
            setBoidShape();
        
        // check for an update to the variable and set the new variable
        if (ShowDebugTools != _cacheShowDebugTools)
            SetDebugToolVisibilty();
        
        // if the race number has been updated.
        if (Diversity != _cacheDiversity && Diversity > 0 && Diversity <= BoidMaterials.Length) 
            SetNewNumOfRaces();

        // check if the 2D mode variable was updated 
        if (Set2DMode != _cacheSet2DMode)
            SetBoid2DMode();


    } // Update()

    /// <summary>
    /// Creates the boids and spawns them
    /// </summary>
    private void CreateBoid() {
        // initialize the new boid array
        boidArray = new Boid[_numOfBoids];
        
        // Create each boid and set its starting data.
        for (int i = 0; i < boidArray.Length; i++) {
            GameObject gameObject = Instantiate(boidPrefabs[BoidShape], this.transform.position, this.transform.rotation);
            Boid b = gameObject.GetComponent<Boid>();
            b.Init();
            b.SetBaisActive(BiasActive);
            Renderer[] renderers = b.GetComponentsInChildren<Renderer>();
            b.SetRenderers(renderers);
            if (renderers != null) SetRandomMaterial(renderers, b);
            boidArray[i] = b;
        } // for
    } // CreateBoid()
    
    /// <summary>
    /// Set the Boids Shape by destroying all of them and Respawning them
    /// </summary>
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
        _cacheDiversity = Diversity;
        maxDiversity = Mathf.Min(Diversity, BoidMaterials.Length); // smallest of the 2 numbers will be max
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
        int randMatPos = Random.Range(0, Diversity); // Generate a random number for the Race
        b.SetRaceIndex(randMatPos); // Set the race index in the boid
        // loop over all the child renderers and update them
        foreach (Renderer r in renderers)
            r.material = BoidMaterials[randMatPos];
    } // setRandomMaterial()

    /// <summary>
    /// Set the 2D mode bool for each boid
    /// </summary>
    private void SetBoid2DMode() {
        _cacheSet2DMode = Set2DMode;
        foreach (Boid b in boidArray) 
            b.Set2DMode(Set2DMode);
    } // SetBoid2DMode()
    
} // BoidManager
