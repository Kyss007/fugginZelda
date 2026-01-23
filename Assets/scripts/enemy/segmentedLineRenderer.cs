using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class segmentedLineRenderer : MonoBehaviour
{
    [Header("Endpoints (world transforms)")]
    public Transform source;
    public Transform target;

    [Header("Line Settings")]
    [Min(2)]
    public int segments = 10;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
    }

    void Update()
    {
        if (source == null || target == null)
            return;

        UpdateLine();
    }

    void UpdateLine()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = segments;

        // Convert world positions into local space of this object
        Vector3 localStart = transform.InverseTransformPoint(source.position);
        Vector3 localEnd   = transform.InverseTransformPoint(target.position);

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector3 point = Vector3.Lerp(localStart, localEnd, t);
            lineRenderer.SetPosition(i, point);
        }
    }
}
