using UnityEngine;

public class twoBodeIK : MonoBehaviour
{
    [Header("Hierarchy")]
    public Transform topLegRoot;    // Hip joint
    public Transform bottomLegRoot; // Knee joint
    public Transform footTip;       // Empty GO at the very end of the leg
    public Transform footTarget;    // Desired world position for the tip

    [Header("Settings")]
    public Vector3 upAxis = Vector3.up; // Use the Spider Body's 'up' for the bend plane

    private float upperLength;
    private float lowerLength;

    void Start()
    {
        // Calculate exact lengths based on physical distance between pivots
        if (topLegRoot && bottomLegRoot && footTip)
        {
            upperLength = Vector3.Distance(topLegRoot.position, bottomLegRoot.position);
            lowerLength = Vector3.Distance(bottomLegRoot.position, footTip.position);
        }
        else
        {
            Debug.LogError("Please assign all transforms in the inspector!", this);
        }
    }

    void LateUpdate()
    {
        if (footTarget == null) return;
        SolveIK();
    }

    void SolveIK()
    {
        Vector3 rootPos = topLegRoot.position;
        Vector3 targetPos = footTarget.position;
        Vector3 toTarget = targetPos - rootPos;
        float distToTarget = toTarget.magnitude;

        // 1. Clamp distance to ensure the triangle is solvable
        // (Prevent target from being further than the leg length or inside the hip)
        float maxLen = upperLength + lowerLength;
        float minLen = Mathf.Abs(upperLength - lowerLength);
        float solveDist = Mathf.Clamp(distToTarget, minLen + 0.01f, maxLen - 0.01f);

        // 2. Law of Cosines to find internal angles
        // a = upper leg, b = lower leg, c = distance to target
        float a = upperLength;
        float b = lowerLength;
        float c = solveDist;

        // Angle at the hip (angle between upper leg and line to target)
        float cosHip = (a * a + c * c - b * b) / (2f * a * c);
        float hipAngleInner = Mathf.Acos(Mathf.Clamp(cosHip, -1f, 1f)) * Mathf.Rad2Deg;

        // Angle at the knee (internal angle between upper and lower leg)
        float cosKnee = (a * a + b * b - c * c) / (2f * a * b);
        float kneeAngleInner = Mathf.Acos(Mathf.Clamp(cosKnee, -1f, 1f)) * Mathf.Rad2Deg;

        // 3. Define the Bend Plane
        // Without a pole target, we use 'upAxis' to decide where the knee points.
        // We cross the target direction with 'up' to get the side-axis (bendAxis).
        Vector3 bendAxis = Vector3.Cross(toTarget, upAxis).normalized;
        
        // If the leg is pointing exactly up/down, we need a fallback axis
        if (bendAxis.sqrMagnitude < 0.001f)
            bendAxis = transform.right;

        // Create the rotation that looks at the target along the bend plane
        Quaternion lookRotation = Quaternion.LookRotation(toTarget, Vector3.Cross(bendAxis, toTarget));

        // 4. Apply Rotations
        // Hip: Point at target, then rotate "Up" by the hipAngleInner
        topLegRoot.rotation = lookRotation * Quaternion.Euler(-hipAngleInner, 0, 0);

        // Knee: Point straight (180) then subtract the inner angle to fold it
        // Note: If your leg model is already bent 90 degrees by default, 
        // you may need to adjust 180f to 90f.
        bottomLegRoot.localRotation = Quaternion.Euler(180f - kneeAngleInner, 0, 0);
    }

    // Visual aid in Scene View
    private void OnDrawGizmos()
    {
        if (!topLegRoot || !bottomLegRoot || !footTip) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(topLegRoot.position, bottomLegRoot.position);
        Gizmos.DrawLine(bottomLegRoot.position, footTip.position);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(footTarget.position, 0.05f);
    }
}