using UnityEngine;
using System.Collections;

public class slashNut : MonoBehaviour
{
    public bool isOpen = false;
    public GameObject openSlashNutPrefab;
    public GameObject closeSlashNutPrefab;

    [Header("Physics Components")]
    public SpringJoint springJoint;
    public ConfigurableJoint jointChild1;
    public ConfigurableJoint jointChild2;

    public enum LocalAxis { X, Y, Z, NegativeX, NegativeY, NegativeZ }

    [Header("Orientation Settings")]
    public LocalAxis child1ForwardAxis = LocalAxis.Z;
    public LocalAxis child2ForwardAxis = LocalAxis.Z;
    
    public float torqueIntensity = 5000f;
    public float torqueDamper = 100f;
    public float initialBurstForce = 15f;

    public float upOffset = 0.5f;
    public float forwardOffset = 0.3f;

    [Header("Line Renderer Settings")]
    public bool enableLineRenderer = true;
    public Material lineMaterial;
    public float lineWidthAtEnds = 0.2f;
    public float minLineWidthAtCenter = 0.02f;
    public AnimationCurve widthCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
    public int lineSegments = 20;
    public Color lineColor = Color.white;

    [Header("Closing & Breaking Settings")]
    public float timeUntilClose = 5f;
    public float springForceIncreaseRate = 10f; 
    public float torqueIncreaseRate = 1000f;
    public float closeDistanceThreshold = 0.5f; 
    public float breakDistanceThreshold = 8.0f;
    
    [Header("AI Settings (Closed Nut Only)")]
    public bool enableAI = false;
    public float playerDetectionRange = 15f;
    public float attackRange = 12f;
    
    [Header("AI - Wander Settings")]
    public float wanderJumpForce = 8f;
    public float wanderJumpInterval = 2f;
    public float wanderJumpRandomness = 1f;
    public float tumbleForce = 300f;
    public float tumbleDuration = 1.5f;
    
    [Header("AI - Attack Settings")]
    public float rollSpeed = 12f;
    public float attackJumpForce = 18f;
    public float attackJumpArc = 1.2f;
    public float circleOrbitRadius = 6f;
    public float circleOrbitSpeed = 8f;
    public float circleOrbitDuration = 3f;
    public float reboundForce = 10f;
    
    [Header("AI - Behavior Weights")]
    [Range(0f, 1f)] public float chanceRollAttack = 0.4f;
    [Range(0f, 1f)] public float chanceJumpAttack = 0.35f;
    [Range(0f, 1f)] public float chanceCircleAttack = 0.25f;

    private LineRenderer lineRenderer;
    private Transform child1;
    private Transform child2;
    private float openTimer = 0f;
    private float currentExtraTorque = 0f;
    private bool isClosing = false;
    private bool isBroken = false;

    private int layerWhenHeld;
    private int layerWhenDropped;
    private holdableObject holdable1;
    private holdableObject holdable2;

    private keanusCharacterController cc;
    
    // AI State Machine
    private enum AIState { Idle, Wandering, Tumbling, Attacking, Circling, Rebounding }
    private AIState currentState = AIState.Idle;
    private Rigidbody rb;
    private Transform player;
    private float stateTimer = 0f;
    private Vector3 wanderDirection;
    private int tumbleDirection; // -1 for left, 1 for right
    private float circleAngle = 0f;
    private Vector3 circleCenter;
    private Vector3 lastPlayerPosition;

    void Start()
    {
        cc = FindFirstObjectByType<keanusCharacterController>();

        if (isOpen)
        {
            FindChildren();
            SetupPhysicsJoints();

            if (child1) holdable1 = child1.GetComponent<holdableObject>();
            if (child2) holdable2 = child2.GetComponent<holdableObject>();
            
            layerWhenDropped = child1.gameObject.layer;
            layerWhenHeld = LayerMask.NameToLayer("heldObject");
            
            if (enableLineRenderer)
            {
                SetupLineRenderer();
            }
            
            StartCoroutine(DelayedInitialBurst());
        }
        else if (enableAI)
        {
            // Closed nut AI initialization
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            
            if (cc != null)
            {
                player = cc.transform;
            }
            
            // Start wandering behavior
            StartCoroutine(AIBehaviorLoop());
        }
    }
    
    IEnumerator DelayedInitialBurst()
    {
        yield return new WaitForFixedUpdate();
        ApplyInitialBurst();
    }

    void FixedUpdate()
    {
        if (isOpen && !isBroken)
        {
            if (child1 == null || child2 == null) return;
            SyncChildLayers();
            UpdateJointRotations();

            float currentDistance = Vector3.Distance(child1.position, child2.position);

            if (currentDistance > breakDistanceThreshold)
            {
                BreakJoint();
                return; 
            }

            openTimer += Time.fixedDeltaTime;

            if (openTimer >= timeUntilClose && !isClosing)
            {
                isClosing = true;
            }

            if (isClosing)
            {
                if (springJoint != null)
                {
                    springJoint.spring += springForceIncreaseRate * Time.fixedDeltaTime;
                    springJoint.minDistance = 0;
                }

                currentExtraTorque += torqueIncreaseRate * Time.fixedDeltaTime;
                ApplyClosingTorque();

                if (currentDistance <= closeDistanceThreshold)
                {
                    CloseNut();
                }
            }
        }
        else if (enableAI && rb != null)
        {
            // AI physics updates
            UpdateAIPhysics();
        }
    }

    void LateUpdate()
    {
        if (isOpen && !isBroken && enableLineRenderer && lineRenderer != null)
        {
            UpdateLineRenderer();
        }
    }

    #region AI System
    
    IEnumerator AIBehaviorLoop()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        
        while (enableAI)
        {
            if (player == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Check if player is in range
            if (distanceToPlayer <= attackRange)
            {
                // Choose attack behavior
                yield return StartCoroutine(ChooseAndExecuteAttack());
                yield return StartCoroutine(PerformTumble());
            }
            else if (distanceToPlayer <= playerDetectionRange)
            {
                // Close but not attacking - wander with slight bias toward player
                yield return StartCoroutine(WanderTowardPlayer());
                yield return StartCoroutine(PerformTumble());
            }
            else
            {
                // Wander randomly
                yield return StartCoroutine(WanderRandomly());
                yield return StartCoroutine(PerformTumble());
            }
            
            // Small pause between behaviors
            yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
        }
    }
    
    IEnumerator WanderRandomly()
    {
        currentState = AIState.Wandering;
        
        // Pick random direction on XZ plane
        wanderDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;
        
        // Apply jump force
        Vector3 jumpForce = wanderDirection * wanderJumpForce + Vector3.up * wanderJumpForce;
        jumpForce += Random.insideUnitSphere * wanderJumpRandomness;
        rb.AddForce(jumpForce, ForceMode.Impulse);
        
        yield return new WaitForSeconds(wanderJumpInterval);
    }
    
    IEnumerator WanderTowardPlayer()
    {
        currentState = AIState.Wandering;
        
        // Jump with slight bias toward player
        Vector3 toPlayer = (player.position - transform.position).normalized;
        wanderDirection = Vector3.Lerp(
            new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized,
            toPlayer,
            0.3f
        ).normalized;
        
        Vector3 jumpForce = wanderDirection * wanderJumpForce + Vector3.up * wanderJumpForce;
        rb.AddForce(jumpForce, ForceMode.Impulse);
        
        yield return new WaitForSeconds(wanderJumpInterval);
    }
    
    IEnumerator ChooseAndExecuteAttack()
    {
        // Normalize probabilities
        float total = chanceRollAttack + chanceJumpAttack + chanceCircleAttack;
        float roll = Random.Range(0f, total);
        
        if (roll < chanceRollAttack)
        {
            yield return StartCoroutine(RollAttack());
        }
        else if (roll < chanceRollAttack + chanceJumpAttack)
        {
            yield return StartCoroutine(JumpAttack());
        }
        else
        {
            yield return StartCoroutine(CircleAttack());
        }
    }
    
    IEnumerator RollAttack()
    {
        currentState = AIState.Attacking;
        lastPlayerPosition = player.position;
        
        float rollDuration = 1.5f;
        float elapsed = 0f;
        
        while (elapsed < rollDuration)
        {
            if (player != null)
            {
                Vector3 directionToPlayer = (player.position - transform.position);
                directionToPlayer.y = 0;
                directionToPlayer.Normalize();
                
                // Apply rolling force
                rb.AddForce(directionToPlayer * rollSpeed, ForceMode.Acceleration);
                
                // Add spin torque for rolling effect
                Vector3 spinAxis = Vector3.Cross(Vector3.up, directionToPlayer);
                rb.AddTorque(spinAxis * rollSpeed * 2f, ForceMode.Acceleration);
            }
            
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
    
    IEnumerator JumpAttack()
    {
        currentState = AIState.Attacking;
        
        if (player == null) yield break;
        
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        toPlayer.y = 0;
        toPlayer.Normalize();
        
        // Calculate jump with arc
        Vector3 jumpDirection = toPlayer * attackJumpForce + Vector3.up * attackJumpForce * attackJumpArc;
        rb.AddForce(jumpDirection, ForceMode.Impulse);
        
        // Wait for landing
        yield return new WaitForSeconds(0.8f);
        
        // Check if we need to rebound
        if (Vector3.Distance(transform.position, player.position) < 3f)
        {
            yield return StartCoroutine(Rebound());
        }
    }
    
    IEnumerator CircleAttack()
    {
        currentState = AIState.Circling;
        
        if (player == null) yield break;
        
        // Set up circle parameters
        circleCenter = player.position;
        Vector3 toNut = transform.position - circleCenter;
        toNut.y = 0;
        
        // If too close or too far, adjust position first
        float currentDistance = toNut.magnitude;
        if (currentDistance < circleOrbitRadius * 0.5f || currentDistance > circleOrbitRadius * 1.5f)
        {
            // Quick jump to orbit distance
            Vector3 targetPos = circleCenter + toNut.normalized * circleOrbitRadius;
            Vector3 toTarget = targetPos - transform.position;
            toTarget.y = 0;
            rb.AddForce(toTarget.normalized * rollSpeed + Vector3.up * wanderJumpForce, ForceMode.Impulse);
            yield return new WaitForSeconds(0.5f);
        }
        
        // Start circling
        circleAngle = Mathf.Atan2(toNut.z, toNut.x);
        float elapsed = 0f;
        
        while (elapsed < circleOrbitDuration)
        {
            if (player != null)
            {
                // Update circle center to follow player slightly
                circleCenter = Vector3.Lerp(circleCenter, player.position, Time.fixedDeltaTime * 0.5f);
                
                // Calculate target position on circle
                circleAngle += (circleOrbitSpeed / circleOrbitRadius) * Time.fixedDeltaTime;
                Vector3 targetPos = circleCenter + new Vector3(
                    Mathf.Cos(circleAngle) * circleOrbitRadius,
                    0f,
                    Mathf.Sin(circleAngle) * circleOrbitRadius
                );
                
                // Apply force toward target position
                Vector3 toTarget = targetPos - transform.position;
                toTarget.y = 0;
                rb.AddForce(toTarget * circleOrbitSpeed, ForceMode.Acceleration);
                
                // Add spin for rolling effect
                Vector3 tangent = new Vector3(-Mathf.Sin(circleAngle), 0f, Mathf.Cos(circleAngle));
                Vector3 spinAxis = Vector3.Cross(Vector3.up, tangent);
                rb.AddTorque(spinAxis * circleOrbitSpeed * 2f, ForceMode.Acceleration);
            }
            
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
    
    IEnumerator Rebound()
    {
        currentState = AIState.Rebounding;
        
        // Bounce away from player
        Vector3 awayFromPlayer = (transform.position - player.position).normalized;
        awayFromPlayer.y = 0;
        
        Vector3 reboundJump = awayFromPlayer * reboundForce + Vector3.up * reboundForce * 0.8f;
        rb.AddForce(reboundJump, ForceMode.Impulse);
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator PerformTumble()
    {
        currentState = AIState.Tumbling;
        
        // Randomly pick left or right tumble
        tumbleDirection = Random.Range(0f, 1f) > 0.5f ? 1 : -1;
        
        // Get current movement direction or use forward
        Vector3 moveDir = rb.linearVelocity;
        moveDir.y = 0;
        if (moveDir.magnitude < 0.1f)
        {
            moveDir = transform.forward;
        }
        moveDir.Normalize();
        
        // Calculate tumble axis (perpendicular to movement)
        Vector3 tumbleAxis = Vector3.Cross(Vector3.up, moveDir) * tumbleDirection;
        
        float elapsed = 0f;
        while (elapsed < tumbleDuration)
        {
            // Apply spinning torque
            rb.AddTorque(tumbleAxis * tumbleForce * Time.fixedDeltaTime, ForceMode.Acceleration);
            
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        currentState = AIState.Idle;
    }
    
    void UpdateAIPhysics()
    {
        // Apply slight damping when not actively moving
        if (currentState == AIState.Idle || currentState == AIState.Tumbling)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0, rb.linearVelocity.y, 0), Time.fixedDeltaTime * 0.5f);
        }
        
        // Limit maximum velocity
        if (rb.linearVelocity.magnitude > rollSpeed * 2f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * rollSpeed * 2f;
        }
    }
    
    #endregion

    #region Original Methods
    
    void SyncChildLayers()
    {
        if (holdable1 == null || holdable2 == null) return;

        bool eitherHeld = holdable1.isHeld || holdable2.isHeld;
        
        Rigidbody rb1 = child1.GetComponent<Rigidbody>();
        Rigidbody rb2 = child2.GetComponent<Rigidbody>();

        if (eitherHeld)
        {
            SetLayerRecursive(child1, layerWhenHeld);
            SetLayerRecursive(child2, layerWhenHeld);
        }
        else
        {
            if (!holdable1.isThrow) 
            {
                SetLayerRecursive(child1, layerWhenDropped);
            }
            if (!holdable2.isThrow) 
            {
                SetLayerRecursive(child2, layerWhenDropped);
            }
        }
    }

    void ForceRotationToOther(Transform self, Transform target, LocalAxis axis)
    {
        Vector3 direction = (target.position - self.position).normalized;
        if (direction == Vector3.zero) return;

        Vector3 localForward = axis switch
        {
            LocalAxis.X => Vector3.right,
            LocalAxis.Y => Vector3.up,
            LocalAxis.Z => Vector3.forward,
            LocalAxis.NegativeX => Vector3.left,
            LocalAxis.NegativeY => Vector3.down,
            LocalAxis.NegativeZ => Vector3.back,
            _ => Vector3.forward
        };

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        self.rotation = lookRotation * Quaternion.Inverse(Quaternion.LookRotation(localForward));
    }

    void SetLayerRecursive(Transform obj, int layer)
    {
        obj.gameObject.layer = layer;
        foreach (Transform child in obj)
        {
            child.gameObject.layer = layer;
        }
    }

    void ApplyInitialBurst()
    {
        if (child1 == null || child2 == null) return;

        Rigidbody rb1 = child1.GetComponent<Rigidbody>();
        Rigidbody rb2 = child2.GetComponent<Rigidbody>();

        rb1.sleepThreshold = 0;
        rb2.sleepThreshold = 0;

        Vector3 separateDir = (rb1.position - rb2.position).normalized;
        Vector3 jumpDirection1 = (separateDir + Vector3.up).normalized;
        Vector3 jumpDirection2 = (-separateDir + Vector3.up).normalized;

        rb1.AddForceAtPosition(jumpDirection1 * initialBurstForce, rb1.position + Vector3.up * upOffset, ForceMode.Impulse);
        rb2.AddForceAtPosition(jumpDirection2 * initialBurstForce, rb2.position + Vector3.up * upOffset, ForceMode.Impulse);
    }

    void SetupPhysicsJoints()
    {
        ConfigureSlerpDrive(jointChild1);
        ConfigureSlerpDrive(jointChild2);
    }

    void ConfigureSlerpDrive(ConfigurableJoint joint)
    {
        if (joint == null) return;

        joint.rotationDriveMode = RotationDriveMode.Slerp;
        JointDrive drive = new JointDrive
        {
            positionSpring = torqueIntensity,
            positionDamper = torqueDamper,
            maximumForce = float.MaxValue
        };
        joint.slerpDrive = drive;
    }

    void UpdateJointRotations()
    {
        Vector3 dirTo2 = (child2.position - child1.position).normalized;
        Vector3 dirTo1 = -dirTo2;

        if (dirTo2 != Vector3.zero)
        {
            if (jointChild1) jointChild1.targetRotation = GetPhysicsRotation(dirTo2, child1ForwardAxis);
            if (jointChild2) jointChild2.targetRotation = GetPhysicsRotation(dirTo1, child2ForwardAxis);
        }
    }

    void ApplyClosingTorque()
    {
        Rigidbody rb1 = child1.GetComponent<Rigidbody>();
        Rigidbody rb2 = child2.GetComponent<Rigidbody>();

        if (rb1 == null || rb2 == null) return;

        ApplyAlignmentTorque(rb1, (child2.position - child1.position).normalized, child1ForwardAxis);
        ApplyAlignmentTorque(rb2, (child1.position - child2.position).normalized, child2ForwardAxis);
    }

    void ApplyAlignmentTorque(Rigidbody rb, Vector3 targetDir, LocalAxis axis)
    {
        Vector3 localAxisDir = axis switch
        {
            LocalAxis.X => rb.transform.right,
            LocalAxis.Y => rb.transform.up,
            LocalAxis.Z => rb.transform.forward,
            LocalAxis.NegativeX => -rb.transform.right,
            LocalAxis.NegativeY => -rb.transform.up,
            LocalAxis.NegativeZ => -rb.transform.forward,
            _ => rb.transform.forward
        };

        Vector3 torqueVector = Vector3.Cross(localAxisDir, targetDir);
        rb.AddTorque(torqueVector * currentExtraTorque, ForceMode.Acceleration);
    }

    Quaternion GetPhysicsRotation(Vector3 targetDirection, LocalAxis axis)
    {
        Vector3 localAxisDir = axis switch
        {
            LocalAxis.X => Vector3.right,
            LocalAxis.Y => Vector3.up,
            LocalAxis.Z => Vector3.forward,
            LocalAxis.NegativeX => Vector3.left,
            LocalAxis.NegativeY => Vector3.down,
            LocalAxis.NegativeZ => Vector3.back,
            _ => Vector3.forward
        };

        Quaternion lookRot = Quaternion.LookRotation(targetDirection);
        return Quaternion.Inverse(lookRot) * transform.rotation * Quaternion.LookRotation(localAxisDir);
    }

    void BreakJoint()
    {
        isBroken = true;
        if (springJoint != null) Destroy(springJoint);
        if (jointChild1 != null) Destroy(jointChild1);
        if (jointChild2 != null) Destroy(jointChild2);
        if (lineRenderer != null) lineRenderer.enabled = false;
        
        Destroy(gameObject, 0.3f);
    }

    void SetupLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = lineSegments;
        lineRenderer.useWorldSpace = true;
        if (lineMaterial != null) lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.numCapVertices = 20;
    }

    void FindChildren()
    {
        if (transform.childCount >= 2)
        {
            child1 = transform.GetChild(0);
            child2 = transform.GetChild(1);
        }
    }

    void UpdateLineRenderer()
    {
        Vector3 pos1 = child1.position;
        Vector3 pos2 = child2.position;
        for (int i = 0; i < lineSegments; i++)
        {
            float t = i / (float)(lineSegments - 1);
            lineRenderer.SetPosition(i, Vector3.Lerp(pos1, pos2, t));
        }

        AnimationCurve customWidthCurve = new AnimationCurve();
        for (int i = 0; i < lineSegments; i++)
        {
            float t = i / (float)(lineSegments - 1);
            float taper = (t < 0.5f) ? Mathf.Lerp(1f, minLineWidthAtCenter / lineWidthAtEnds, t * 2f) : Mathf.Lerp(minLineWidthAtCenter / lineWidthAtEnds, 1f, (t - 0.5f) * 2f);
            customWidthCurve.AddKey(t, taper * lineWidthAtEnds * widthCurve.Evaluate(t));
        }
        lineRenderer.widthCurve = customWidthCurve;
    }

    void CloseNut()
    {
        Vector3 spawnPos = (child1.position + child2.position) * 0.5f;
        Quaternion spawnRot = child1.rotation; 

        Instantiate(closeSlashNutPrefab, spawnPos, spawnRot);
        Destroy(gameObject);
    }

    public void triggerHit(Vector3 dir, string attackName)
    {
        if (attackName == "jumpSwing") openNut();
    }

    public void openNut()
    {
        Vector3 direction = cc.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Instantiate(openSlashNutPrefab, transform.position, targetRotation);
        
        Destroy(gameObject);
    }
    
    #endregion
}