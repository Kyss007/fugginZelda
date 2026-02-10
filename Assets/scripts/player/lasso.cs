using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Lasso : MonoBehaviour
{
    public inventory inventory;
    public thirdPersonMovementDriver thirdPersonMovementDriver;
    public InputActionReference moveInput;

    [Header("References")]
    public Transform origin;
    public LineRenderer line;
    public Rigidbody playerRb; 
    public GameObject lassoReticle; // Dedicated reference for the lasso's target visual

    [Header("Lasso Constraints")]
    public float maxLassoLength = 12f;
    public float breakThreshold = 2f; 
    public float pullInDistance = 1.5f;
    public float pullInForce = 5f;

    [Header("Dedicated Lasso Targeting")]
    public List<target> lassoTargetsInRange = new List<target>();
    public target currentLassoTarget;

    [Header("Vine Swing Physics")]
    public float swingSpring = 150f;
    public float swingDamper = 10f;
    public float swingMassScale = 4.5f;
    public float swingForce = 40f;
    private SpringJoint swingJoint;
    private bool isSwinging = false;
    private Transform activeGrapplePoint;
    private float currentRopeLength;
    public float minSwingDuration = 0.2f;

    [Header("Lasso Timing")]
    public int segments = 30;
    public float travelTime = 0.3f;
    public float retractMultiplier = 2f;
    
    [Header("Cooldown")]
    public float lassoCooldown = 0.4f;
    private float lastFireTime;

    [Header("Visual Arc & Wobble")]
    public float arcHeight = 0.5f;
    public float arcAngleRandom = 15f;
    public float swingAmplitude = 0.4f;
    public float swingFrequency = 5f;
    public float randomness = 0.3f;
    public float pullForce = 8f;

    Vector3 targetPos;
    Vector3 arcDirection;
    Coroutine lassoRoutine;
    
    bool reachedActualTarget = false;
    private GameObject localHitObject;
    private target currentSwungOnTarget;

    private Camera mainCam;
    private float swingStartTime;

    void Awake()
    {
        if (playerRb == null) playerRb = GetComponentInParent<Rigidbody>();
        
        line.positionCount = segments;
        line.enabled = false;
        lastFireTime = -lassoCooldown; 

        if (lassoReticle != null)
        {
            lassoReticle.transform.SetParent(null);
            lassoReticle.SetActive(false);
        }
    }

    void Start()
    {
        mainCam = Camera.main;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<target>(out target t) && t.isLassoTarget)
        {
            if (!lassoTargetsInRange.Contains(t)) lassoTargetsInRange.Add(t);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<target>(out target t))
        {
            if (lassoTargetsInRange.Contains(t)) lassoTargetsInRange.Remove(t);
            if (currentLassoTarget == t) currentLassoTarget = null;
        }
    }

    private void CalculateBestLassoTarget()
    {
        currentLassoTarget = null;

        // Keep the grounded check to ensure default lasso happens on floor
        if (thirdPersonMovementDriver.isGrounded) return;

        float bestScore = float.MaxValue;

        for (int i = lassoTargetsInRange.Count - 1; i >= 0; i--)
        {
            target t = lassoTargetsInRange[i];
            if (t == null)
            {
                lassoTargetsInRange.RemoveAt(i);
                continue;
            }

            Vector3 dirToTarget = (t.transform.position - transform.parent.position).normalized;
            
            // We compare against camera forward for "aiming" feel
            // Dot result is 1.0 (perfectly in front) to -1.0 (perfectly behind)
            float dot = Vector3.Dot(mainCam.transform.forward, dirToTarget);
            
            // Map dot from (1 to -1) to a "Penalty Multiplier" (1 to 10)
            // 1.0 (Front) -> Multiplier 1 (No penalty)
            // -1.0 (Behind) -> Multiplier 10 (Heavy penalty)
            float anglePenalty = Mathf.Lerp(10f, 1f, (dot + 1f) / 2f);

            float dist = Vector3.Distance(transform.parent.position, t.transform.position);
            
            // Final Score: Distance x Penalty
            // A target 5m away in front (5x1 = 5) beats a target 2m away behind (2x10 = 20)
            float score = dist * anglePenalty;

            if (score < bestScore)
            {
                bestScore = score;
                currentLassoTarget = t;
            }
        }
    }

    void Update()
    {
        CalculateBestLassoTarget();
        HandleReticle();

        if (isSwinging && activeGrapplePoint != null)
        {
            HandleSwingPhysics();
            
            if (Time.time >= swingStartTime + minSwingDuration)
            {
                float currentDist = Vector3.Distance(playerRb.position, activeGrapplePoint.position);
                if (currentDist > currentRopeLength + breakThreshold || thirdPersonMovementDriver.isGrounded)
                {
                    StopSwinging();
                }
            }
        }
    }

    private void HandleReticle()
    {
        if (currentLassoTarget != null && !isSwinging && !thirdPersonMovementDriver.isGrounded)
        {
            if (lassoReticle != null)
            {
                lassoReticle.SetActive(true);
                lassoReticle.transform.position = currentLassoTarget.transform.position;
            }
        }
        else
        {
            if (lassoReticle != null) lassoReticle.SetActive(false);
        }
    }

    private void HandleSwingPhysics()
    {
        float h = moveInput.action.ReadValue<Vector2>().x;
        float v = moveInput.action.ReadValue<Vector2>().y;
        Vector3 moveDir = (mainCam.transform.right * h + mainCam.transform.forward * v).normalized;
        moveDir.y = 0;

        playerRb.AddForce(moveDir * swingForce, ForceMode.Acceleration);

        targetPos = activeGrapplePoint.position;
        UpdateLine(1f); 
    }

    public void doLassoAction()
    {
        if (isSwinging)
        {
            StopSwinging();
            return;
        }

        if (lassoRoutine != null || Time.time < lastFireTime + lassoCooldown)
            return;

        lastFireTime = Time.time;

        if (currentLassoTarget != null && !thirdPersonMovementDriver.isGrounded)
        {
            float dist = Vector3.Distance(origin.position, currentLassoTarget.transform.position);
            if (dist <= maxLassoLength)
            {
                activeGrapplePoint = currentLassoTarget.transform;
                currentSwungOnTarget = currentLassoTarget;
                lassoRoutine = StartCoroutine(GrappleSequence(activeGrapplePoint));
                return;
            }
        }

        ExecuteNormalLasso();
    }

    IEnumerator GrappleSequence(Transform grapplePoint)
    {
        line.enabled = true;
        targetPos = grapplePoint.position;
        arcDirection = Quaternion.AngleAxis(Random.Range(-arcAngleRandom, arcAngleRandom), Random.onUnitSphere) * (Vector3.up + Vector3.right);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (travelTime * 0.7f);
            UpdateLine(Mathf.Clamp01(t));
            yield return null;
        }

        if (currentSwungOnTarget != null)
        {
             lassoTargetsInRange.Remove(currentSwungOnTarget);
        }

        SetupSpringJoint(grapplePoint.position);
        isSwinging = true;
        lassoRoutine = null;
        swingStartTime = Time.time;

        Vector3 kickDir = Vector3.ProjectOnPlane(playerRb.linearVelocity, (grapplePoint.position - playerRb.position).normalized);
        playerRb.AddForce(kickDir.normalized * 10f, ForceMode.Impulse);
    }

    void SetupSpringJoint(Vector3 anchorPos)
    {
        swingJoint = playerRb.gameObject.AddComponent<SpringJoint>();
        swingJoint.autoConfigureConnectedAnchor = false;
        swingJoint.connectedAnchor = anchorPos;

        float distance = Vector3.Distance(playerRb.position, anchorPos);
        swingJoint.maxDistance = distance * 0.85f;
        swingJoint.minDistance = distance * 0.1f;

        swingJoint.spring = swingSpring;
        swingJoint.damper = swingDamper;
        swingJoint.massScale = swingMassScale;

        currentRopeLength = swingJoint.maxDistance;
        thirdPersonMovementDriver.autoWalk = true; 
    }

    void StopSwinging()
    {
        if (!isSwinging) return;
        
        isSwinging = false;
        activeGrapplePoint = null;
        if (swingJoint != null) Destroy(swingJoint);
        
        if (lassoRoutine != null) StopCoroutine(lassoRoutine);
        lassoRoutine = StartCoroutine(RetractRoutine());

        currentSwungOnTarget = null;
        thirdPersonMovementDriver.autoWalk = false;
    }

    IEnumerator RetractRoutine()
    {
        float t = 1f;
        float retractTime = travelTime / retractMultiplier;
        while (t > 0f)
        {
            t -= Time.deltaTime / retractTime;
            UpdateLine(Mathf.Clamp01(t));
            yield return null;
        }
        line.enabled = false;
        lassoRoutine = null;
    }

    private void ExecuteNormalLasso()
    {
        if (currentLassoTarget != null)
        {
            localHitObject = currentLassoTarget.gameObject;
            Vector3 dir = (localHitObject.transform.position - origin.position);
            if (dir.magnitude <= maxLassoLength)
            {
                reachedActualTarget = true;
                FireLasso(localHitObject.transform.position);
            }
            else
            {
                reachedActualTarget = false;
                FireLasso(origin.position + (dir.normalized * maxLassoLength));
            }
        }
        else
        {
            Vector3 direction = transform.parent.forward;
            if (Physics.Raycast(origin.position, direction, out RaycastHit hit, maxLassoLength))
            {
                localHitObject = hit.collider.gameObject;
                reachedActualTarget = true;
                FireLasso(hit.point);
            }
            else
            {
                localHitObject = null;
                reachedActualTarget = false;
                FireLasso(origin.position + (direction * maxLassoLength));
            }
        }
    }

    public void FireLasso(Vector3 destination)
    {
        if (lassoRoutine != null) StopCoroutine(lassoRoutine);
        targetPos = destination;
        arcDirection = Quaternion.AngleAxis(Random.Range(-arcAngleRandom, arcAngleRandom), Random.onUnitSphere) * (Vector3.up + Vector3.right);
        lassoRoutine = StartCoroutine(LassoSequence());
    }

    IEnumerator LassoSequence()
    {
        line.enabled = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / travelTime;
            UpdateLine(Mathf.Clamp01(t));
            yield return null;
        }

        if (reachedActualTarget) pullTarget();

        yield return new WaitForSeconds(0.05f);

        t = 1f;
        float retractTime = travelTime / retractMultiplier;
        while (t > 0f)
        {
            t -= Time.deltaTime / retractTime;
            UpdateLine(Mathf.Clamp01(t));
            yield return null;
        }

        line.enabled = false;
        lassoRoutine = null;
    }

    public void pullTarget()
    {
        if (reachedActualTarget && localHitObject != null)
        {
            Rigidbody targetRB = localHitObject.GetComponentInParent<Rigidbody>();
            if (targetRB != null)
            {
                Vector3 pullDir = (transform.parent.position - targetRB.position).normalized;
                targetRB.AddForce(pullDir * pullForce, ForceMode.Impulse);
            }
        }
    }

    void UpdateLine(float progress)
    {
        Vector3 start = origin.position;
        Vector3 currentTargetPos = targetPos;
        Vector3 fullDir = targetPos - start;
        
        if (fullDir.magnitude > maxLassoLength && !isSwinging)
        {
            currentTargetPos = start + (fullDir.normalized * maxLassoLength);
        }

        float currentDistance = Vector3.Distance(start, currentTargetPos);
        float lengthRatio = Mathf.Clamp01(currentDistance / maxLassoLength);
        float arcScale = Mathf.Lerp(1f, 0.2f, lengthRatio);
        float currentArcHeight = arcHeight * arcScale;
        float arcT = progress * (1f - progress);
        Vector3 arcOffset = arcDirection * currentArcHeight * arcT;

        Vector3 end = Vector3.Lerp(start, currentTargetPos, progress) + arcOffset;
        Vector3 direction = (end - start).normalized;

        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
        if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(direction, Vector3.forward).normalized;
        Vector3 up = Vector3.Cross(right, direction).normalized;

        float stretchStraighten = Mathf.Lerp(1f, 0.1f, lengthRatio); 
        float progressStraighten = Mathf.SmoothStep(1f, 0.35f, progress);

        for (int i = 0; i < segments; i++)
        {
            float p = i / (float)(segments - 1);
            Vector3 point = Vector3.Lerp(start, end, p);

            float wave = Mathf.Sin(p * Mathf.PI * swingFrequency + Time.time * swingFrequency);
            float noise = Mathf.PerlinNoise(Time.time * 3f, p * 5f) * 2f - 1f;
            
            float amp = swingAmplitude * (1f - p) * progressStraighten * stretchStraighten;

            point += right * wave * amp + up * noise * amp * randomness;
            line.SetPosition(i, point);
        }
    }

    void OnDisable()
    {
        if(lassoReticle != null) lassoReticle.SetActive(false);
    }
}