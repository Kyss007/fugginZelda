using UnityEngine;

public class hookshot : MonoBehaviour
{

    public inventory inventory;
    public bool hookshotActive = false;

    public GameObject hookshotCrosshair;
    public GameObject hookshotCrosshairDot;

    public FirstPersonCameraDriver firstPersonCamera;

    public LayerMask validTargetLayers;
    public LayerMask excludeLayers;

    public float crosshairSmoothTime = 0.05f;
    public float hookshotSpeed = 20f;
    public float arrivalThreshold = 0.1f;
    
    public float lineWidth = 0.05f;
    public Material lineMaterial;
    public float lineExtendSpeed = 30f;

    public float minHookshotDistance = 2f;
    public float maxHookshotDistance = 50f;

    private bool hasValidTarget = false;

    private Vector3 targetPosition;
    private Vector3 crosshairVelocity;
    private Vector3 dotVelocity;

    private GameObject hookshotAnchor;
    private FixedJoint fixedJoint;
    private bool hookshotInProgress = false;
    private Rigidbody parentRigidbody;
    
    private LineRenderer lineRenderer;
    private GameObject lineObject;
    private bool lineExtending = false;
    private float lineExtendProgress = 0f;
    private Vector3 lineStartPosition;

    void Start()
    {
        hookshotCrosshair.SetActive(false);
        hookshotCrosshairDot.SetActive(false);

        hookshotCrosshair.transform.parent = null;
        hookshotCrosshairDot.transform.parent = null;

        parentRigidbody = GetComponent<Rigidbody>();
        if (parentRigidbody == null)
        {
            parentRigidbody = GetComponentInParent<Rigidbody>();
        }
    }

    void LateUpdate()
    {
        bool hasHit = false;

        if (hookshotActive)
        {
            Ray ray = new Ray(
                firstPersonCamera.cameraPivot.position,
                firstPersonCamera.cameraPivot.forward
            );

            RaycastHit hit;
            int rayMask = ~excludeLayers;

            if (Physics.Raycast(ray, out hit, maxHookshotDistance, rayMask))
            {
                float distance = Vector3.Distance(firstPersonCamera.cameraPivot.position, hit.point);

                hasHit = distance >= minHookshotDistance;
                targetPosition = hit.point;

                hasValidTarget =
                    hasHit &&
                    ((1 << hit.collider.gameObject.layer) & validTargetLayers) != 0;
            }
            else
            {
                hasValidTarget = false;
            }
        }
        else
        {
            hasValidTarget = false;
        }

        if (hasHit)
        {
            hookshotCrosshair.transform.position = Vector3.SmoothDamp(
                hookshotCrosshair.transform.position,
                targetPosition,
                ref crosshairVelocity,
                crosshairSmoothTime
            );

            hookshotCrosshairDot.transform.position = Vector3.SmoothDamp(
                hookshotCrosshairDot.transform.position,
                targetPosition,
                ref dotVelocity,
                crosshairSmoothTime
            );
        }

        hookshotCrosshair.SetActive(hookshotActive && hasHit && hasValidTarget);
        hookshotCrosshairDot.SetActive(hookshotActive && hasHit && !hasValidTarget);

        // Update line extension
        if (lineExtending && lineRenderer != null)
        {
            lineExtendProgress += lineExtendSpeed * Time.deltaTime;
            float distance = Vector3.Distance(lineStartPosition, targetPosition);
            
            if (lineExtendProgress >= distance)
            {
                lineExtendProgress = distance;
                lineExtending = false;
                StartPullIn();
            }
            
            Vector3 currentEndPoint = Vector3.Lerp(lineStartPosition, targetPosition, lineExtendProgress / distance);
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, currentEndPoint);
        }

        // Update hookshot anchor movement and line
        if (hookshotInProgress && hookshotAnchor != null)
        {
            Vector3 direction = (targetPosition - hookshotAnchor.transform.position).normalized;
            hookshotAnchor.transform.position += direction * hookshotSpeed * Time.deltaTime;

            // Update line renderer to follow anchor and hookshot position
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, targetPosition);
            }

            // Check if anchor has reached target
            if (Vector3.Distance(hookshotAnchor.transform.position, targetPosition) <= arrivalThreshold)
            {
                ReleaseHookshot();
            }
        }
    }

    public void doHookshotAction()
    {
        if(!inventory.unlockedHookShot)
            return;

        if (!hookshotActive)
        {
            //print("activate");
            hookshotActive = true;

            if (!firstPersonCamera.gameObject.activeSelf)
                firstPersonCamera.gameObject.SetActive(true);
        }
        else
        {
            //print("trigger hookshot");
            TriggerHookshot();
        }
    }

    private void TriggerHookshot()
    {
        if (!hasValidTarget || hookshotInProgress || lineExtending)
            return;

        if (parentRigidbody == null)
        {
            Debug.LogWarning("No Rigidbody found on parent object!");
            return;
        }

        // Create line renderer
        CreateLineRenderer();
        
        // Start extending the line
        lineStartPosition = transform.position;
        lineExtendProgress = 0f;
        lineExtending = true;

        firstPersonCamera.gameObject.SetActive(false);
    }

    private void CreateLineRenderer()
    {
        lineObject = new GameObject("HookshotLine");
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = lineMaterial;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        
        // Set initial positions
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    private void StartPullIn()
    {
        // Create the anchor object
        hookshotAnchor = new GameObject("HookshotAnchor");
        hookshotAnchor.transform.position = transform.position;

        // Add rigidbody to anchor
        Rigidbody anchorRb = hookshotAnchor.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;

        // Create fixed joint connecting parent rigidbody to anchor
        fixedJoint = parentRigidbody.gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = anchorRb;

        hookshotInProgress = true;
    }

    private void ReleaseHookshot()
    {
        if (fixedJoint != null)
        {
            Destroy(fixedJoint);
        }

        if (hookshotAnchor != null)
        {
            Destroy(hookshotAnchor);
        }

        if (lineRenderer != null)
        {
            Destroy(lineObject);
            lineRenderer = null;
        }

        hookshotInProgress = false;
        lineExtending = false;

        hookshotActive = false;
    }
}