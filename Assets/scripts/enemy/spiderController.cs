using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SpiderController : MonoBehaviour
{
    [Header("Legs")]
    public twoBodeIK[] legs;

    [Header("Step Settings")]
    public float stepDistance = 0.7f;
    public float stepHeight = 0.3f;
    public float stepSpeed = 12f;
    public float footForwardBias = 0.2f;

    [Header("Stability")]
    public float uprightTorque = 50f;
    public float maxAngularVelocity = 5f;

    private Vector3[] localHomePositions;
    private bool[] isStepping;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;

        localHomePositions = new Vector3[legs.Length];
        isStepping = new bool[legs.Length];

        for (int i = 0; i < legs.Length; i++)
        {
            localHomePositions[i] = transform.InverseTransformPoint(legs[i].footTarget.position);
            legs[i].footTarget.position = GetGroundPoint(legs[i].footTarget.position);
            legs[i].footTarget.parent = null; // world space foot targets
        }
    }

    void Update()
    {
        HandleLegs();
    }

    void FixedUpdate()
    {
        StayUpright();
    }

    void HandleLegs()
    {
        int steppingCount = CountSteppingLegs();

        for (int i = 0; i < legs.Length; i++)
        {
            if (isStepping[i])
                continue; // skip legs already stepping

            // Only allow stepping if at least 2 legs are not moving
            if (steppingCount > legs.Length - 2)
                continue;

            Vector3 worldHome = transform.TransformPoint(localHomePositions[i]);
            Vector3 footPos = legs[i].footTarget.position;
            float dist = Vector3.Distance(footPos, worldHome);

            if (dist > stepDistance)
            {
                Vector3 stepDir = (worldHome - footPos).normalized;
                Vector3 targetPos = GetGroundPoint(worldHome + stepDir * footForwardBias);

                StartCoroutine(ExecuteStep(i, targetPos));
                steppingCount++; // count this leg as stepping for next iterations
            }
        }
    }

    int CountSteppingLegs()
    {
        int count = 0;
        for (int i = 0; i < isStepping.Length; i++)
            if (isStepping[i])
                count++;
        return count;
    }

    IEnumerator ExecuteStep(int index, Vector3 targetWorldPos)
    {
        isStepping[index] = true;
        Vector3 startPos = legs[index].footTarget.position;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * stepSpeed;

            Vector3 pos = Vector3.Lerp(startPos, targetWorldPos, progress);
            pos += Vector3.up * Mathf.Sin(progress * Mathf.PI) * stepHeight;

            legs[index].footTarget.position = pos;
            yield return null;
        }

        isStepping[index] = false;
    }

    void StayUpright()
    {
        Vector3 upright = Vector3.up;
        Vector3 currentUp = transform.up;
        Vector3 torqueAxis = Vector3.Cross(currentUp, upright);
        rb.AddTorque(torqueAxis * uprightTorque, ForceMode.Force);
    }

    Vector3 GetGroundPoint(Vector3 point)
    {
        if (Physics.Raycast(point + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
            return hit.point;
        return point;
    }

    private void OnDrawGizmos()
    {
        if (legs == null || localHomePositions == null) return;

        for (int i = 0; i < legs.Length; i++)
        {
            Gizmos.color = Color.green;
            Vector3 worldHome = transform.TransformPoint(localHomePositions[i]);
            Gizmos.DrawWireSphere(worldHome, 0.1f);
            Gizmos.DrawLine(worldHome, legs[i].footTarget.position);
        }
    }
}
