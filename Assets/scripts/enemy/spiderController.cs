using UnityEngine;
using System.Collections;

public class SpiderController : MonoBehaviour
{
    [System.Serializable]
    public class Leg
    {
        public Transform footTarget;
        public twoBodeIK ikSolver;
        [HideInInspector] public Vector3 localHomeOffset;
        [HideInInspector] public Vector3 worldHomePosition;
        [HideInInspector] public bool isStepping;
        [HideInInspector] public float stepProgress;
        [HideInInspector] public bool isGrounded; // Track individual leg contact
    }

    [Header("Legs - Order: Front-Left, Front-Right, Back-Left, Back-Right")]
    [SerializeField] private Leg[] legs = new Leg[4];

    [Header("Step Behavior")]
    [SerializeField] private float stepTriggerDistance = 0.5f;
    [SerializeField] private float stepHeight = 0.25f;
    [SerializeField] private float stepDuration = 0.15f;
    [SerializeField] private AnimationCurve stepCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Speed Adaptation")]
    [SerializeField] private float speedStepReduction = 0.5f;
    [SerializeField] private float minStepDuration = 0.08f;
    [SerializeField] private float velocityPrediction = 0.4f;
    
    [Header("Body Stabilization")]
    [SerializeField] private float uprightTorque = 50f;
    [SerializeField] private float uprightDamping = 10f;
    [SerializeField] private float maxTiltAngle = 30f;
    [SerializeField] private bool useAngularDrag = true;
    [SerializeField] private float targetAngularDrag = 5f;
    
    [Header("Height Stabilization")]
    [SerializeField] private bool maintainHeight = true;
    [SerializeField] private float targetHeight = 1f;
    [SerializeField] private float heightCorrectionForce = 20f;
    [SerializeField] private float heightDamping = 5f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundMask = -1;
    [SerializeField] private float maxGroundCheckDistance = 3f;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showGizmos = true;

    private Rigidbody rb;
    private Vector3 lastBodyPosition;
    private Vector3 bodyVelocity;
    private bool isDiagonalLeftTurn = true;
    private float averageGroundHeight;
    private Vector3 surfaceNormal = Vector3.up;
    private bool isAnyLegGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("SpiderController requires a Rigidbody component!");
            enabled = false;
            return;
        }
        
        lastBodyPosition = transform.position;
        InitializeLegs();
        
        if (useAngularDrag)
        {
            rb.angularDamping = targetAngularDrag;
        }
    }

    void InitializeLegs()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i].footTarget == null) continue;
            
            legs[i].localHomeOffset = transform.InverseTransformPoint(legs[i].footTarget.position);
            legs[i].worldHomePosition = legs[i].footTarget.position;
            
            Vector3 groundPoint = FindGroundPoint(legs[i].footTarget.position, out _);
            legs[i].footTarget.position = groundPoint;
            legs[i].footTarget.SetParent(null);
            legs[i].isStepping = false;
            legs[i].stepProgress = 0f;
        }
        
        CalculateAverageGroundHeightAndNormal();
    }

    void Update()
    {
        UpdateBodyVelocity();
        UpdateHomePositions();
        
        if (isAnyLegGrounded)
        {
            EvaluateStepNeeds();
        }
        else
        {
            // If airborne, keep feet at home positions
            for (int i = 0; i < legs.Length; i++)
            {
                legs[i].footTarget.position = legs[i].worldHomePosition;
                legs[i].isStepping = false;
            }
        }
    }

    void FixedUpdate()
    {
        CalculateAverageGroundHeightAndNormal();
        StabilizeBody();
        if (isAnyLegGrounded) MaintainBodyHeight();
    }

    void UpdateBodyVelocity()
    {
        bodyVelocity = (transform.position - lastBodyPosition) / Time.deltaTime;
        lastBodyPosition = transform.position;
    }

    void StabilizeBody()
    {
        Vector3 currentUp = transform.up;
        // Target Up is now the surface normal calculated from leg hits
        Vector3 targetUp = surfaceNormal;
        
        float tiltAngle = Vector3.Angle(currentUp, targetUp);
        
        if (tiltAngle > 0.1f)
        {
            Vector3 torqueAxis = Vector3.Cross(currentUp, targetUp);
            float torqueMagnitude = Mathf.Min(tiltAngle / maxTiltAngle, 1f) * uprightTorque;
            
            Vector3 correctionTorque = torqueAxis.normalized * torqueMagnitude;
            rb.AddTorque(correctionTorque, ForceMode.Force);
            
            rb.AddTorque(-rb.angularVelocity * uprightDamping, ForceMode.Force);
        }
    }

    void MaintainBodyHeight()
    {
        if (!maintainHeight) return;
        
        float currentHeight = transform.position.y;
        float desiredHeight = averageGroundHeight + targetHeight;
        float heightError = desiredHeight - currentHeight;
        
        if (Mathf.Abs(heightError) > 0.05f)
        {
            float correctionForce = heightError * heightCorrectionForce;
            float damping = -rb.linearVelocity.y * heightDamping;
            rb.AddForce(Vector3.up * (correctionForce + damping), ForceMode.Force);
        }
    }

    void CalculateAverageGroundHeightAndNormal()
    {
        float totalHeight = 0f;
        int groundedFeet = 0;
        Vector3 combinedNormal = Vector3.zero;
        
        for (int i = 0; i < legs.Length; i++)
        {
            // Raycast down from the leg's home position to find surface
            Vector3 rayOrigin = transform.TransformPoint(legs[i].localHomeOffset) + Vector3.up * 1f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, maxGroundCheckDistance, groundMask))
            {
                totalHeight += hit.point.y;
                combinedNormal += hit.normal;
                groundedFeet++;
                legs[i].isGrounded = true;
            }
            else
            {
                legs[i].isGrounded = false;
            }
        }
        
        isAnyLegGrounded = groundedFeet > 0;

        if (isAnyLegGrounded)
        {
            averageGroundHeight = totalHeight / groundedFeet;
            surfaceNormal = (combinedNormal / groundedFeet).normalized;
        }
        else
        {
            // Default to world up if completely airborne
            surfaceNormal = Vector3.up;
        }
    }

    void UpdateHomePositions()
    {
        float speed = new Vector3(bodyVelocity.x, 0, bodyVelocity.z).magnitude;
        
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i].footTarget == null) continue;
            
            Vector3 baseHome = transform.TransformPoint(legs[i].localHomeOffset);
            Vector3 velocityOffset = Vector3.zero;

            if (speed > 0.1f)
            {
                Vector3 flatVelocity = new Vector3(bodyVelocity.x, 0, bodyVelocity.z);
                velocityOffset = flatVelocity.normalized * (speed * velocityPrediction);
            }
            
            legs[i].worldHomePosition = baseHome + velocityOffset;
        }
    }

    void EvaluateStepNeeds()
    {
        int[] leftDiagonal = { 0, 3 };
        int[] rightDiagonal = { 1, 2 };
        
        bool leftNeedsStep = NeedsStep(leftDiagonal[0]) || NeedsStep(leftDiagonal[1]);
        bool rightNeedsStep = NeedsStep(rightDiagonal[0]) || NeedsStep(rightDiagonal[1]);
        
        bool leftDiagonalGrounded = !legs[leftDiagonal[0]].isStepping && !legs[leftDiagonal[1]].isStepping;
        bool rightDiagonalGrounded = !legs[rightDiagonal[0]].isStepping && !legs[rightDiagonal[1]].isStepping;
        
        if (isDiagonalLeftTurn && leftNeedsStep && rightDiagonalGrounded)
        {
            TriggerDiagonalStep(leftDiagonal);
            isDiagonalLeftTurn = false;
        }
        else if (!isDiagonalLeftTurn && rightNeedsStep && leftDiagonalGrounded)
        {
            TriggerDiagonalStep(rightDiagonal);
            isDiagonalLeftTurn = true;
        }
    }

    bool NeedsStep(int legIndex)
    {
        if (legs[legIndex].isStepping || !legs[legIndex].isGrounded) return false;
        
        float distanceFromHome = Vector3.Distance(
            legs[legIndex].footTarget.position,
            legs[legIndex].worldHomePosition
        );
        
        return distanceFromHome > stepTriggerDistance;
    }

    void TriggerDiagonalStep(int[] legIndices)
    {
        foreach (int index in legIndices)
        {
            if (NeedsStep(index))
            {
                StartCoroutine(PerformStep(index));
            }
        }
    }

    IEnumerator PerformStep(int legIndex)
    {
        legs[legIndex].isStepping = true;
        legs[legIndex].stepProgress = 0f;
        
        Vector3 startPosition = legs[legIndex].footTarget.position;
        Vector3 targetPosition = FindGroundPoint(legs[legIndex].worldHomePosition, out _);
        
        float speed = new Vector3(bodyVelocity.x, 0, bodyVelocity.z).magnitude;
        float adaptedDuration = Mathf.Lerp(stepDuration, minStepDuration, speed * speedStepReduction);
        
        float elapsed = 0f;
        
        while (elapsed < adaptedDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / adaptedDuration;
            legs[legIndex].stepProgress = normalizedTime;
            
            targetPosition = FindGroundPoint(legs[legIndex].worldHomePosition, out _);
            
            float curveValue = stepCurve.Evaluate(normalizedTime);
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            
            float arcHeight = Mathf.Sin(normalizedTime * Mathf.PI) * stepHeight;
            currentPosition.y += arcHeight;
            
            legs[legIndex].footTarget.position = currentPosition;
            
            yield return null;
        }
        
        legs[legIndex].footTarget.position = FindGroundPoint(legs[legIndex].worldHomePosition, out _);
        legs[legIndex].isStepping = false;
        legs[legIndex].stepProgress = 1f;
    }

    Vector3 FindGroundPoint(Vector3 worldPosition, out Vector3 normal)
    {
        Vector3 rayOrigin = worldPosition + Vector3.up * 2f;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, maxGroundCheckDistance, groundMask))
        {
            normal = hit.normal;
            return hit.point + Vector3.up * 0.02f;
        }
        
        normal = Vector3.up;
        return worldPosition;
    }

    // ... Gizmos code remains mostly the same, drawing surfaceNormal instead of Vector3.up ...
}