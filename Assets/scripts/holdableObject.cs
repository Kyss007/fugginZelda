using System.Collections;
using JetBrains.Annotations;
using PhysicalWalk;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class holdableObject : MonoBehaviour
{
    public bool isHeld = false;

    public float throwDistance = 10f;
    public float throwSpeed = 20f;

    public Collider collider;
    public LayerMask groundLayer;

    public bool doNotRotate = false;
    public bool dontSetKinematic = false;

    public string heldObjectLayer = "heldObject";
    private Rigidbody rb;

    private int ogHeldObjectLayer;


    private DampedSpringMotionCopier motionCopier;

    private Transform holdPoint;
    public Vector3 holdOffset;
    public float throwTargetHeightOffset;

    public bool isThrow = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        motionCopier = GetComponent<DampedSpringMotionCopier>();
    }

    public void Update()
    {
        if (isHeld)
        {
            motionCopier.positionalSpring.sourceObject = holdPoint;

            if(!doNotRotate)
                motionCopier.transform.forward = holdPoint.forward;
            
            motionCopier.positionalSpring.frozenLocalOffset = holdOffset;
        }
        else
        {
            motionCopier.positionalSpring.sourceObject = null;
        }
    }


    public void doPickup(Transform holdPointInput)
    {
        if (!isHeld && !isThrow)
        {
            isHeld = true;

            rb.isKinematic = !dontSetKinematic;

            ogHeldObjectLayer = collider.gameObject.layer;
            collider.gameObject.layer = LayerMask.NameToLayer(heldObjectLayer);

            holdPoint = holdPointInput;

            motionCopier.Reset();
        }
    }

    public void doDrop()
    {
        if (isHeld)
        {
            isHeld = false;

            rb.isKinematic = false;

            collider.gameObject.layer = ogHeldObjectLayer;

            holdPoint = null;
        }
    }

    public void doThrow(Vector3 target = new Vector3(), bool withTarget = false)
    {
        if (!withTarget)
        {
            Vector3 origin = transform.position + transform.forward * throwDistance;

            RaycastHit hit;
            if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
            {
                target = hit.point + new Vector3(0, throwTargetHeightOffset, 0);
            }
            else
            {
                Debug.Log("no ground found in throw. janky fallback throw target");
                target = origin;
            }
        }

        doDrop();

        StartCoroutine(ThrowRigidbodyAlongArc(rb, throwSpeed, throwDistance, target));
    }

    public IEnumerator ThrowRigidbodyAlongArc(Rigidbody rigidbody, float speed, float maxHorizontalDistance, Vector3 targetPosition)
    {
        isThrow = true;
        // Disable physics during arc movement
        rigidbody.isKinematic = true;

        Vector3 startPosition = rigidbody.transform.position;

        // Calculate direction to target (horizontal plane only)
        Vector3 toTarget = targetPosition - startPosition;
        toTarget.y = 0f; // Remove vertical component
        Vector3 horizontalDirection = toTarget.normalized;

        // Calculate actual horizontal distance, capped by max distance
        float actualHorizontalDistance = Mathf.Min(toTarget.magnitude, maxHorizontalDistance);

        // Calculate arc parameters for 45-degree trajectory
        float maxHeight = actualHorizontalDistance * 0.5f; // At 45 degrees, max height = horizontal distance / 2

        // Calculate total arc length (approximation for 45-degree parabola)
        float arcLength = actualHorizontalDistance * 1.414f; // √2 * horizontal distance for 45-degree arc

        // Calculate time to complete the arc at the given speed
        float totalTime = arcLength / speed;

        Vector3 endPosition = startPosition + (horizontalDirection * actualHorizontalDistance);

        // Do a ground check to ensure we end at ground level
        Vector3 groundCheckOrigin = endPosition + Vector3.up * 100f; // Start high above the end position
        RaycastHit hit;
        if (Physics.Raycast(groundCheckOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            endPosition.y = hit.point.y + throwTargetHeightOffset; // Ground + collider offset
        }
        else
        {
            endPosition.y = targetPosition.y; // Fallback to original target height
        }

        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            // Calculate progress along the arc (0 to 1)
            float t = elapsedTime / totalTime;

            // Calculate position along the parabolic arc
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, t);

            // Add parabolic height (45-degree arc peaks at middle)
            float height = 4f * maxHeight * t * (1f - t); // Parabolic formula
            currentPosition.y = Mathf.Lerp(startPosition.y, endPosition.y, t) + height;

            // Move the rigidbody
            rigidbody.transform.position = currentPosition;

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Ensure final position is exact
        rigidbody.transform.position = endPosition;

        // Re-enable physics
        rigidbody.isKinematic = false;


        // Calculate final velocity to continue the arc naturally
        Vector3 previousPosition = rigidbody.transform.position;
        float previousTime = Time.fixedDeltaTime;
        yield return new WaitForFixedUpdate();

        Vector3 finalPosition = rigidbody.transform.position;
        Vector3 finalVelocity = (finalPosition - previousPosition) / previousTime;

        // Re-enable physics
        rigidbody.isKinematic = false;
        rigidbody.linearVelocity = finalVelocity;

        isThrow = false;
    }
}