using UnityEngine;
using System.Collections;

public class lasso : MonoBehaviour
{
    public inventory inventory;
    public targetController targetController;
    public thirdPersonMovementDriver thirdPersonMovementDriver;

    [Header("References")]
    public Transform origin;
    public Transform target;
    public LineRenderer line;
    public Rigidbody playerRb; 

    [Header("Lasso Constraints")]
    public float maxLassoLength = 8f;
    public float breakThreshold = 1.5f; 
    public float pullInDistance = 1f;
    public float pullInForce = 3f;

    [Header("Swing Physics")]
    public float swingSpring = 1000f; 
    public float swingDamper = 20f;
    private ConfigurableJoint swingJoint;
    private bool isSwinging = false;
    private Transform activeGrapplePoint;
    private float initialSwingDistance;

    [Header("Lasso Timing")]
    public int segments = 30;
    public float travelTime = 0.4f;
    public float retractMultiplier = 1.8f;
    
    [Header("Cooldown")]
    public float lassoCooldown = 0.5f;
    private float lastFireTime;

    [Header("Visual Arc & Wobble")]
    public float arcHeight = 0.6f;
    public float arcAngleRandom = 20f;
    public float swingAmplitude = 0.6f;
    public float swingFrequency = 6f;
    public float randomness = 0.4f;
    public float pullForce = 5f;

    Vector3 targetPos;
    Vector3 arcDirection;
    Coroutine lassoRoutine;
    
    bool reachedActualTarget = false;
    private Vector3 ogLocalPosOfLassoTarget;
    private GameObject localHitObject;

    private target currentSwungOnTarget;

    void Awake()
    {
        if (playerRb == null) playerRb = GetComponentInParent<Rigidbody>();
        
        ogLocalPosOfLassoTarget = target.localPosition;
        line.positionCount = segments;
        line.enabled = false;
        lastFireTime = -lassoCooldown; 
    }

    void Update()
    {
        // 1. CLAMP RETICLE VISUALS
        if (targetController.currentSellectedTarget != null)
        {
            Vector3 dirToTarget = targetController.currentSellectedTarget.transform.position - origin.position;
            // Limit reticle position to maxLassoLength
            if (dirToTarget.magnitude > maxLassoLength)
            {
                target.position = origin.position + (dirToTarget.normalized * maxLassoLength);
            }
            else
            {
                target.position = targetController.currentSellectedTarget.transform.position;
            }
        }
        else
        {
            target.localPosition = ogLocalPosOfLassoTarget;
        }

        // 2. SWING LOGIC
        if (isSwinging && activeGrapplePoint != null)
        {
            float currentDist = Vector3.Distance(playerRb.position, activeGrapplePoint.position);

            if (currentDist > initialSwingDistance + breakThreshold || thirdPersonMovementDriver.isGrounded)
            {
                StopSwinging();
            }
            else
            {
                targetPos = activeGrapplePoint.position;
                targetController.removeTarget(currentSwungOnTarget);
                UpdateLine(1f); 
            }
        }
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

        // Check distance before deciding to swing
        if (targetController.currentSellectedTarget != null && targetController.currentSellectedTarget.isLassoTarget)
        {
            float dist = Vector3.Distance(origin.position, targetController.currentSellectedTarget.transform.position);
            
            if (dist <= maxLassoLength)
            {
                activeGrapplePoint = targetController.currentSellectedTarget.transform;
                currentSwungOnTarget = targetController.currentSellectedTarget;
                lassoRoutine = StartCoroutine(GrappleSequence(activeGrapplePoint));
                return;
            }
        }

        // Default to normal shot if no target or target too far
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

        SetupSwingJoint(grapplePoint.position);
        isSwinging = true;
        lassoRoutine = null;

        playerRb.AddForce((grapplePoint.position - playerRb.position).normalized * pullInForce ,ForceMode.Impulse);
    }

    void SetupSwingJoint(Vector3 anchorPos)
    {
        swingJoint = playerRb.gameObject.AddComponent<ConfigurableJoint>();
        swingJoint.autoConfigureConnectedAnchor = false;
        swingJoint.connectedAnchor = anchorPos;

        initialSwingDistance = Vector3.Distance(playerRb.position, anchorPos) - pullInDistance;

        swingJoint.xMotion = ConfigurableJointMotion.Limited;
        swingJoint.yMotion = ConfigurableJointMotion.Limited;
        swingJoint.zMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = initialSwingDistance; 
        limit.contactDistance = 0.05f; 
        swingJoint.linearLimit = limit;
        
        SoftJointLimitSpring spring = new SoftJointLimitSpring();
        spring.spring = swingSpring;
        spring.damper = swingDamper;
        swingJoint.linearLimitSpring = spring;

        thirdPersonMovementDriver.autoWalk = true;
    }

    void StopSwinging()
    {
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
        if (targetController != null && targetController.currentSellectedTarget != null)
        {
            localHitObject = targetController.currentSellectedTarget.gameObject;
            Vector3 dir = (localHitObject.transform.position - origin.position);
            float dist = dir.magnitude;

            if (dist <= maxLassoLength)
            {
                reachedActualTarget = true;
                FireLasso(localHitObject.transform.position);
            }
            else
            {
                // TARGET TOO FAR: Shoot line to max length only
                reachedActualTarget = false;
                FireLasso(origin.position + (dir.normalized * maxLassoLength));
            }
        }
        else
        {
            Vector3 direction = transform.parent.forward;
            Ray ray = new Ray(origin.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, maxLassoLength))
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
        
        // Ensure visual line never renders past maxLassoLength
        Vector3 currentTargetPos = targetPos;
        Vector3 fullDir = targetPos - start;
        if (fullDir.magnitude > maxLassoLength)
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
}