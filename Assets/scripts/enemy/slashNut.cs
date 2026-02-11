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
    public float torqueIncreaseRate = 1000f; // NEW: How fast manual torque builds
    public float closeDistanceThreshold = 0.5f; 
    public float breakDistanceThreshold = 8.0f; 

    private LineRenderer lineRenderer;
    private Transform child1;
    private Transform child2;
    private float openTimer = 0f;
    private float currentExtraTorque = 0f; // Tracks accumulated torque strength
    private bool isClosing = false;
    private bool isBroken = false;


    private int layerWhenHeld;
    private int layerWhenDropped;
    private holdableObject holdable1;
    private holdableObject holdable2;

    private keanusCharacterController cc;

    void Start()
    {
        cc = FindFirstObjectByType<keanusCharacterController>();

        if (isOpen)
        {
            FindChildren();
            SetupPhysicsJoints();

            if (child1) holdable1 = child1.GetComponent<holdableObject>();
            if (child2) holdable2 = child2.GetComponent<holdableObject>();
            
            layerWhenDropped = child1.gameObject.layer; // Or a specific layer like "Interactable"
            layerWhenHeld = LayerMask.NameToLayer("heldObject");
            
            if (enableLineRenderer)
            {
                SetupLineRenderer();
            }
            
            StartCoroutine(DelayedInitialBurst());
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

            // Break if pulled too far
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
                // Pull them back together via Spring
                if (springJoint != null)
                {
                    springJoint.spring += springForceIncreaseRate * Time.fixedDeltaTime;
                    springJoint.minDistance = 0;
                }

                // NEW: Increase and Apply Manual Torque for extra alignment strength
                currentExtraTorque += torqueIncreaseRate * Time.fixedDeltaTime;
                ApplyClosingTorque();

                if (currentDistance <= closeDistanceThreshold)
                {
                    CloseNut();
                }
            }
        }
    }

    void LateUpdate()
    {
        if (isOpen && !isBroken && enableLineRenderer && lineRenderer != null)
        {
            UpdateLineRenderer();
        }
    }

    void SyncChildLayers()
    {
        if (holdable1 == null || holdable2 == null) return;

        // If EITHER part is held, BOTH should be on the heldObject layer
        if (holdable1.isHeld || holdable2.isHeld)
        {
            child1.gameObject.layer = layerWhenHeld;
            foreach(Transform transform in child1.transform)
            {
                transform.gameObject.layer = layerWhenHeld;
            }

            child2.gameObject.layer = layerWhenHeld;
            foreach(Transform transform in child2.transform)
            {
                transform.gameObject.layer = layerWhenHeld;
            }
        }
        else if (!holdable1.isThrow && !holdable2.isThrow) 
        {
            // Only reset to default layer if neither is currently being thrown
            child1.gameObject.layer = layerWhenDropped;
            foreach(Transform transform in child1.transform)
            {
                transform.gameObject.layer = layerWhenDropped;
            }

            child2.gameObject.layer = layerWhenDropped;
            foreach(Transform transform in child2.transform)
            {
                transform.gameObject.layer = layerWhenDropped;
            }
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

    // Manual torque application to assist the ConfigurableJoints during closing
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

        // Cross product finds the axis needed to rotate toward the target direction
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
}