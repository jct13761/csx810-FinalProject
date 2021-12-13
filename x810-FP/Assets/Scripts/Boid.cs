using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * Primary References/Sources:
 * https://www.youtube.com/watch?v=bqtqltqcQhw
 * https://www.youtube.com/watch?v=mhjuuHl6qHM
 * https://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
 * https://jumpoff.io/portfolio/boids
 */
public class Boid : MonoBehaviour {
    
    private Vector3 _velocity; // velocity of the boid
    [HideInInspector] public Vector3 forward; // direction the boid is facing
    private Transform _boidTransfrom; // the transform of the boid
    [HideInInspector] public Vector3 boidPosition; // the position of the boid

    private Vector3 _alignmentDirection; // alignment
    private Vector3 _cohesionDirection; // cohesion 
    private Vector3 _separationDirection; // separation
    private Vector3 _leaderDirection; // Leadership
    
    private float minSpeed = 19; // the speed of the boid
    private float maxSpeed = 20; // the speed of the boid
    private float maxSteerForce = 100; // the Steer force of the boid of the boid

    private float _alignWeight; // The weight for the alignment rule
    private float _cohesionWeight; // The weight for the cohesion rule
    private float _separateWeight; // The weight for the separate rule
    private float _leaderWeight; // The weight for the leadership rule

    private float perceptionRadius = 2.5f; // The perception Radius for detecting neighbor boids 
    private float avoidanceRadius = 1f; // The avoidance Radius for avoiding neighbor boids
    private int _neighborBoids = 0; // the number of neighbor boids

    private float width = 50.0f, height = 30.0f; // the width/height of the arena box
    private float buffer = -1.0f; // a buffer to make the transition from top to bottom more smooth

    // Collision Variables
    private float _boundsRadius; // 0.27 default
    private float _avoidCollisionWeight; // 10 default
    private float _collisionAvoidDst; // 15 
    
    private int _raceIndex; // The Race of the Boid
    private bool _isBiasActive;
    private Renderer[] _renderers;

    private bool _2DMode = false;
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Initialize the variables for the boid
    /// </summary>
    public void Init() {
        _boidTransfrom = this.transform;
        forward = Random.insideUnitSphere;
        _velocity = forward * (minSpeed + maxSpeed) / 2;
    } // Init()

    /// <summary>
    /// Update the Boid's position, direction, etc. This is the function that applies the 3 main rules of Boids.
    /// Gets called from BoidManager. 
    /// </summary>
    /// <param name="boids">The array of boids to update</param>
    public void UpdateBoid(Boid[] boids) {
        // set the current position
        boidPosition = this.transform.position;
        // create the acceleration value
        Vector3 acceleration = Vector3.zero;
        // calculate the rule's direction vectors
        BoidRules(boids);

        // if there is something to update
        if (_neighborBoids != 0) {
            // calculate and apply the alignment force
            Vector3 alignmentForce = SteerTowards(_alignmentDirection) * _alignWeight; // Alignment 
            acceleration += alignmentForce;

            // calculate and apply the cohesion force
            Vector3 cohesionForce = SteerTowards(_cohesionDirection) * _cohesionWeight; // cohesion
            acceleration += cohesionForce;

            // calculate and apply the separation force
            Vector3 separationForce = SteerTowards(_separationDirection) * _separateWeight; // separation
            acceleration += separationForce;
            
            // calculate and apply the leadership force
            Vector3 leadershipForce = SteerTowards(_leaderDirection) * _leaderWeight; // leadership
            acceleration += leadershipForce;
        } // if

        // Check if the Boid is going to colide
        if (IsHeadingForCollision()) {
            // get the direction the boid should steer in to avoid collision.
            Vector3 collisionAvoidDir = ObstacleRays();
            // get the force using the steer towards function with the collision weight
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * _avoidCollisionWeight;
            // replace the current acceleration with the new steer force
            acceleration = collisionAvoidForce;
        } // if

        // This is the collision detection for the edges of the arena box and teleports them to the other side 
        // CheckForWallCollision();

        // Calculate the velocity of the boid
        // add the time as well as any acceleration to make it move
        _velocity += acceleration * Time.deltaTime;
        // get the magnitude 
        float s = _velocity.magnitude;
        // calculate the forward direction of the boid with the new velocity 
        Vector3 dir = _velocity / s;
        s = Mathf.Clamp(s, minSpeed, maxSpeed);
        // set the velocity to the direction and speed
        _velocity = dir * s;

        // if the mode is 2D, then set the y-velocity to 0 so it only moves on 2 axis
        if (_2DMode)
            _velocity = new Vector3(_velocity.x, 0, _velocity.z);

        // set the new position based on the velocity and time
        _boidTransfrom.position += _velocity * Time.deltaTime;
        // set the new forward based on the above direction
        _boidTransfrom.forward = dir;
        // update the position variable
        boidPosition = _boidTransfrom.position;
        // update the forward variable
        forward = dir;
    } // updateBoid()

    /// <summary>
    /// Check for a collision with one of the walls and if there is, teleport the Boid to the opposite side.
    /// </summary>
    void CheckForWallCollision() {
        if (boidPosition.x < -width / 2 + buffer || boidPosition.x > width / 2 - buffer ||  // check -/+ of X-axis
            boidPosition.y < 0 + buffer          || boidPosition.y > height - buffer ||     // check -/+ of Y-axis
            boidPosition.z < -width / 2 + buffer || boidPosition.z > width / 2 - buffer)    // check -/+ of Z-axis
            this.gameObject.transform.position = new Vector3(0, 10, 0);
    } // CheckForWallCollision()

    /// <summary>
    /// Checks for and applies Boid Alignment, Cohesion, and Separation.
    /// Source: https://www.dawn-studio.de/tutorials/boids/
    /// Source: https://www.youtube.com/watch?v=bqtqltqcQhw
    /// </summary>
    /// <param name="boids">The array of boids to check.</param>
    void BoidRules(Boid[] boids) {
        // reset the neighbor count
        _neighborBoids = 0;
        // create temp counters for the rules 
        int alignmentCohesionCount = 0, separationCount = 0;

        // create a temp vectors to hold the sum of the different rules
        Vector3 alignmentDir = Vector3.zero;
        Vector3 cohesionDir = Vector3.zero;
        Vector3 separationDir = Vector3.zero;
        
        // Leadership variables
        float leaderAngle = 0f;
        Boid leaderBoid = null;

        // temp vars for the position of b and self
        Vector3 thisPos = this.transform.position;

        // check each boid against self boid
        foreach (Boid b in boids) {
            // check if b is not self
            if (b != this) {
                // temp var for the position of b
                Vector3 bPos = b.transform.position;
                // calculate the distance between the self and b as a float
                float dist = Vector3.Distance(bPos, thisPos);
                // calculate the distance between the self and b as a Vector3
                Vector3 difference = bPos - thisPos;
                
                
                // Alignment and Cohesion - if the distance is within the specified perception radius, and if Bias is
                // active and b is not the same race as self... 
                if (dist < perceptionRadius && !(_isBiasActive && b.GetRaceIndex() != this._raceIndex)) {
                    // add b's forward direction for the alignment var
                    alignmentDir += b.transform.forward;
                    // add the distance between b and self for the cohesion var
                    cohesionDir += difference;
                    // increment the counter
                    alignmentCohesionCount++;
                    
                    float angle = Vector3.Angle(bPos - thisPos, this.forward);
                    if (angle < leaderAngle && angle < 90f) {
                        leaderBoid = b;
                        leaderAngle = angle;
                    }
                } // if

                // Separation - if the if the distance is within the specified avoidance radius...
                if (dist < avoidanceRadius) {
                    // add the distance between b and self for the separation var
                    separationDir += difference;
                    separationCount++;
                } // if
            } // if
        } // foreach()

        // set the neighbor count to the larger of the 2 numbers 
        _neighborBoids = Mathf.Max(alignmentCohesionCount, separationCount);

        // set the global var for alignment
        _alignmentDirection = alignmentDir;
        // subtract the self position from the cohesion summation
        cohesionDir -= this.transform.position;
        // set the global var for cohesion
        _cohesionDirection = cohesionDir;
        
        // apply the leadership direction
        if (leaderBoid != null)
            _leaderDirection = (leaderBoid.transform.position - thisPos).normalized * 0.5f;

        // if the Separation is not 0, calculate the average separation value
        if (separationCount > 0) separationDir /= separationCount;
        // flip and normalize the average separation distance
        separationDir = -separationDir.normalized;
        // set the global var for separation
        _separationDirection = separationDir;
    } // BoidRules()


    /// <summary>
    /// This function Takes a vector and makes the boid steer towards that direction
    /// Source: https://www.youtube.com/watch?v=bqtqltqcQhw
    /// </summary>
    /// <param name="vector">The Intended direction's vector</param>
    /// <returns>the new direction for the boid</returns>
    Vector3 SteerTowards(Vector3 vector) {
        // gets the new direction of the vector
        // Formula from Boids Paper: https://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
        /*
         *     IntendedDir
         *      ------->
         *      ^      ^
         *      |     /
         *      |    / New Dir
         *curDir|   / 
         *      |  /
         *      | /
         *      |/
         */
        // NewDir = IntendedDir - curDir
        Vector3 v = vector.normalized * maxSpeed - _velocity;
        // clamps the value between the new one and the max Force 
        return Vector3.ClampMagnitude(v, maxSteerForce);
        // return v;
    } // SteerTowards()


    /// <summary>
    /// This Function casts a ray from the boid and checks if it is going to hit something.
    /// Source: https://www.dawn-studio.de/tutorials/boids/
    /// </summary>
    /// <returns>a bool of whether or not a collision was detected</returns>
    bool IsHeadingForCollision() {
        RaycastHit hit;
        if (Physics.SphereCast(this.boidPosition, this._boundsRadius, this.forward, out hit,
            this._collisionAvoidDst, LayerMask.GetMask("BoidCollision")))
            return true;
        return false;
    } // IsHeadingForCollision()

    /// <summary>
    /// This Function takes the directions generated from the BoidHelper class and checks the directions to find the
    /// first open direction to take.
    /// Source: https://www.youtube.com/watch?v=bqtqltqcQhw
    /// </summary>
    /// <returns>The Direction the Boid should turn towards.</returns>
    Vector3 ObstacleRays() {
        // get the directional points on the sphere
        Vector3[] rayDirections = BoidHelper.directions;
        // loop over the all the directions to check which one is a safe direction to take
        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = _boidTransfrom.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(boidPosition, dir);
            if (!Physics.SphereCast(ray, _boundsRadius, _collisionAvoidDst,
                LayerMask.GetMask("BoidCollision")))
                return dir;
        } // for
        return forward;
    } // ObstacleRays()

    /// <summary>
    /// Sets the Boids to only move in X and Z axis
    /// </summary>
    /// <param name="d">bool to set the mode</param>
    public void Set2DMode(bool d) {
        _2DMode = d;
        if (d) this.gameObject.transform.position = new Vector3(boidPosition.x, 40, boidPosition.z);
    } // Set2DMode()

    /// <summary>
    /// Destroys itself
    /// </summary>
    public void DestroyGameObject() { Destroy(this.gameObject); }
    
    
    
    /**************************************** Gettters/Setters *****************************************/
    public void SetAllWeights(float a, float c, float s, float l, float t) {
        SetAlignmentWeight(a);
        SetCohesionWeight(c);
        SetSeparationWeight(s);
        SetLeaderWeight(l);
        SetSteerWeight(t);
    } //SetWeights()

    public void SetAlignmentWeight(float a) { _alignWeight = a; }
    public void SetCohesionWeight(float c) { _cohesionWeight = c; }
    public void SetSeparationWeight(float s) { _separateWeight = s; }
    public void SetLeaderWeight(float s) { _leaderWeight = s; }
    public void SetSteerWeight(float t) { _avoidCollisionWeight = t; }
    public void SetBoundsRadius(float r) { _boundsRadius = r; }
    public void SetCollisionAvoidDistancet(float c) { _collisionAvoidDst = c; }
    public int GetRaceIndex() { return _raceIndex; }
    public void SetRaceIndex(int i) { _raceIndex = i; }
    public void SetBaisActive(bool r) { _isBiasActive = r; }
    public void SetRenderers(Renderer[] r) { _renderers = r; }
    public Renderer[] GetRenderers() { return _renderers; }

} // Boid Class


/// <summary>
/// This class is simply a helper class to generate points on a sphere. This is done by using the Golden Spiral Method,
/// which will evenly distribute n dots on a sphere. This is calculated using the Golden Ratio (1 + sqrt(5))/2 ~= 1.618
/// for the turn fraction. 
/// Reference: https://www.youtube.com/watch?v=bqtqltqcQhw
/// Source: https://stackoverflow.com/questions/9600801/evenly-distributing-n-points-on-a-sphere/44164075#44164075
/// </summary>
public static class BoidHelper {
    const int numViewDirections = 300;
    public static readonly Vector3[] directions;
    static BoidHelper() {
        directions = new Vector3[BoidHelper.numViewDirections];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++) {
            float t = (float) i / numViewDirections;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            directions[i] = new Vector3(x, y, z);
        } // for
    } // if
} // BoidHelper Class