using System.Collections;
using UnityEngine;

public class ArcForceController : MonoBehaviour
{
    [Header("Arc Settings")]
    public float arcHeight = 3f;
    public float arcDistance = 10f;
    public float speed = 1f;
    public AnimationCurve arcCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // Method 1: Continuous force application along predetermined path
    public void StartArcMovement(Rigidbody rb, Vector3 target)
    {
        StartCoroutine(ApplyArcForces(rb, target));
    }
    
    private IEnumerator ApplyArcForces(Rigidbody rb, Vector3 target)
    {
        Vector3 startPos = rb.position;
        Vector3 toTarget = target - startPos;
        Vector3 horizontalDirection = new Vector3(toTarget.x, 0, toTarget.z).normalized;
        float totalDistance = Vector3.Distance(startPos, target);
        
        float progress = 0f;
        Vector3 lastPos = startPos;
        
        while (progress < 1f)
        {
            progress += Time.fixedDeltaTime * speed;
            progress = Mathf.Clamp01(progress);
            
            // Calculate target position along arc
            Vector3 horizontalPos = startPos + horizontalDirection * (totalDistance * progress);
            float arcHeightAtProgress = arcCurve.Evaluate(progress) * arcHeight;
            Vector3 targetPos = horizontalPos + Vector3.up * arcHeightAtProgress;
            
            // Calculate required force to reach target position
            Vector3 desiredVelocity = (targetPos - rb.position) / Time.fixedDeltaTime;
            Vector3 velocityDiff = desiredVelocity - rb.linearVelocity;
            
            // Apply force to achieve desired velocity
            rb.AddForce(velocityDiff, ForceMode.VelocityChange);
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    // Method 2: Waypoint-based impulse system
    public void StartWaypointArc(Rigidbody rb, Vector3 target, int waypointCount = 10)
    {
        StartCoroutine(WaypointArcMovement(rb, target, waypointCount));
    }
    
    private IEnumerator WaypointArcMovement(Rigidbody rb, Vector3 target, int waypointCount)
    {
        Vector3 startPos = rb.position;
        Vector3[] waypoints = GenerateArcWaypoints(startPos, target, waypointCount);
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector3 targetWaypoint = waypoints[i];
            
            // Calculate impulse to reach next waypoint
            Vector3 toWaypoint = targetWaypoint - rb.position;
            float timeToWaypoint = 1f / (speed * waypointCount);
            Vector3 requiredVelocity = toWaypoint / timeToWaypoint;
            
            // Apply impulse
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(requiredVelocity, ForceMode.VelocityChange);
            
            yield return new WaitForSeconds(timeToWaypoint);
        }
    }
    
    private Vector3[] GenerateArcWaypoints(Vector3 start, Vector3 target, int count)
    {
        Vector3[] waypoints = new Vector3[count];
        Vector3 toTarget = target - start;
        Vector3 horizontalDirection = new Vector3(toTarget.x, 0, toTarget.z);
        
        for (int i = 0; i < count; i++)
        {
            float t = (float)(i + 1) / count;
            Vector3 horizontalPos = start + horizontalDirection * t;
            float height = arcCurve.Evaluate(t) * arcHeight;
            waypoints[i] = horizontalPos + Vector3.up * height;
        }
        
        return waypoints;
    }
    
    // Method 3: Physics-based arc with custom gravity
    public void StartCustomGravityArc(Rigidbody rb, Vector3 target)
    {
        StartCoroutine(CustomGravityArc(rb, target));
    }
    
    private IEnumerator CustomGravityArc(Rigidbody rb, Vector3 target)
    {
        Vector3 startPos = rb.position;
        Vector3 toTarget = target - startPos;
        float totalDistance = new Vector3(toTarget.x, 0, toTarget.z).magnitude;
        
        // Initial horizontal velocity
        Vector3 horizontalVel = new Vector3(toTarget.x, 0, toTarget.z).normalized * speed;
        
        // Initial upward velocity for arc
        float initialUpwardVel = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * arcHeight);
        
        rb.linearVelocity = horizontalVel + Vector3.up * initialUpwardVel;
        
        float progress = 0f;
        bool useCustomGravity = true;
        
        while (progress < 1f && useCustomGravity)
        {
            progress = Vector3.Distance(startPos, rb.position) / totalDistance;
            
            // Apply custom gravity force to maintain arc shape
            float gravityMultiplier = 1f + (arcCurve.Evaluate(progress) * 2f);
            Vector3 customGravity = Physics.gravity * gravityMultiplier;
            
            rb.AddForce(customGravity - Physics.gravity, ForceMode.Acceleration);
            
            // Stop when close to target
            if (Vector3.Distance(rb.position, target) < 0.5f)
            {
                useCustomGravity = false;
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    // Method 4: Magnetic attraction approach
    public void StartMagneticArc(Rigidbody rb, Vector3 target)
    {
        StartCoroutine(MagneticArcMovement(rb, target));
    }
    
    private IEnumerator MagneticArcMovement(Rigidbody rb, Vector3 target)
    {
        Vector3 startPos = rb.position;
        float totalDistance = Vector3.Distance(startPos, target);
        
        // Initial impulse
        Vector3 initialDirection = (target - startPos).normalized;
        rb.AddForce(initialDirection * speed * 10f, ForceMode.Impulse);
        
        while (Vector3.Distance(rb.position, target) > 0.5f)
        {
            float progress = 1f - (Vector3.Distance(rb.position, target) / totalDistance);
            
            // Calculate ideal arc position
            Vector3 idealPos = Vector3.Lerp(startPos, target, progress);
            idealPos.y += arcCurve.Evaluate(progress) * arcHeight;
            
            // Apply magnetic force toward ideal position
            Vector3 magneticForce = (idealPos - rb.position) * speed * 5f;
            rb.AddForce(magneticForce, ForceMode.Force);
            
            yield return new WaitForFixedUpdate();
        }
    }
}