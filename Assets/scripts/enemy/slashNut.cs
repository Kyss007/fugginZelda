using UnityEngine;

public class slashNut : MonoBehaviour
{
    public bool isOpen = false;
    public GameObject openSlashNutPrefab;
    public GameObject closeSlashNutPrefab;

    public SpringJoint springJoint;

    private Rigidbody rb;

    [Header("Line Renderer Settings")]
    public bool enableLineRenderer = true;
    public Material lineMaterial;
    public float lineWidthAtEnds = 0.2f;
    public float minLineWidthAtCenter = 0.02f;
    public AnimationCurve widthCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
    public int lineSegments = 20;
    public Color lineColor = Color.white;

    [Header("Closing & Breaking Settings")]
    public float timeUntilClose = 10f;
    public float springForceIncreaseRate = 5f; 
    public float closeDistanceThreshold = 0.2f; // Distance to snap shut
    public float breakDistanceThreshold = 5.0f; // Distance where the joint snaps/breaks

    private LineRenderer lineRenderer;
    private Transform child1;
    private Transform child2;
    private float openTimer = 0f;
    private bool isClosing = false;
    private bool isBroken = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (isOpen)
        {
            FindChildren();
            
            if (enableLineRenderer)
            {
                SetupLineRenderer();
            }
        }
    }

    void Update()
    {
        if (isOpen && !isBroken)
        {
            if (child1 == null || child2 == null) return;

            float currentDistance = Vector3.Distance(child1.position, child2.position);

            // --- BREAK LOGIC ---
            // If they get too far apart, the "tether" snaps
            if (currentDistance > breakDistanceThreshold)
            {
                BreakJoint();
                return; 
            }

            // --- CLOSE LOGIC ---
            openTimer += Time.deltaTime;

            if (openTimer >= timeUntilClose && !isClosing)
            {
                isClosing = true;
            }

            if (isClosing && springJoint != null)
            {
                springJoint.spring += springForceIncreaseRate * Time.deltaTime;
                springJoint.minDistance = 0;

                if (currentDistance <= closeDistanceThreshold)
                {
                    Destroy(springJoint);
                    CloseNut();
                }
            }

            // --- VISUALS ---
            if (enableLineRenderer && lineRenderer != null)
            {
                UpdateLineRenderer();
            }
        }
    }

    void BreakJoint()
    {
        isBroken = true;
        if (springJoint != null) Destroy(springJoint);
        if (lineRenderer != null) lineRenderer.enabled = false;
        
        Destroy(gameObject, 0.3f);
    }

    void SetupLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = lineSegments;
        lineRenderer.useWorldSpace = true;
        
        if (lineMaterial != null)
            lineRenderer.material = lineMaterial;
        
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.numCapVertices = 20;
        lineRenderer.widthCurve = widthCurve;
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
            float widthAtPoint = (t < 0.5f) ? Mathf.Lerp(1f, minLineWidthAtCenter / lineWidthAtEnds, t * 2f) : Mathf.Lerp(minLineWidthAtCenter / lineWidthAtEnds, 1f, (t - 0.5f) * 2f);
            widthAtPoint *= widthCurve.Evaluate(t);
            customWidthCurve.AddKey(t, widthAtPoint * lineWidthAtEnds);
        }
        lineRenderer.widthCurve = customWidthCurve;
    }

    void CloseNut()
    {
        // Spawning logic: perfectly between halves with orientation matching the gap
        Vector3 spawnPos = (child1.position + child2.position) * 0.5f;
        Vector3 direction = (child1.position - child2.position).normalized;
        Quaternion spawnRot = Quaternion.LookRotation(direction);

        Instantiate(closeSlashNutPrefab, spawnPos, spawnRot);
        Destroy(gameObject);
    }

    public void triggerHit(Vector3 dir, string attackName)
    {
        if (attackName == "jumpSwing") openNut();
    }

    public void openNut()
    {
        Instantiate(openSlashNutPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}