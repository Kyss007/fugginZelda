using JetBrains.Annotations;
using PhysicalWalk;
using Unity.VisualScripting;
using UnityEngine;

public class holdableObject : MonoBehaviour
{
    public bool isHeld = false;

    public Collider collider;

    public string heldObjectLayer = "heldObject";
    private Rigidbody rb;

    private int ogHeldObjectLayer;


    private DampedSpringMotionCopier motionCopier;

    private Transform holdPoint;


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
            motionCopier.transform.forward = holdPoint.forward;
        }
        else
        {
            motionCopier.positionalSpring.sourceObject = null;
        }
    }


    public void doPickup(Transform holdPointInput)
    {
        if (!isHeld)
        {
            isHeld = true;

            rb.isKinematic = true;

            ogHeldObjectLayer = collider.gameObject.layer;
            collider.gameObject.layer = LayerMask.NameToLayer(heldObjectLayer);

            holdPoint = holdPointInput;
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
            target = transform.position + transform.forward * 1;
        }

        doDrop();

        addArcImpulse(rb, target, 45, 10);
    }
    
    public void addArcImpulse(Rigidbody rb, Vector3 target, float launchAngleDeg, float maxDistance)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        Vector3 start = rb.position;
        Vector3 toTarget = target - start;

        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
        float horizontalDistance = Mathf.Min(toTargetXZ.magnitude, maxDistance); // clamp distance
        float heightDifference = toTarget.y;

        float angleRad = launchAngleDeg * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angleRad);
        float sinAngle = Mathf.Sin(angleRad);

        float numerator = gravity * horizontalDistance * horizontalDistance;
        float denominator = 2f * (heightDifference - horizontalDistance * Mathf.Tan(angleRad)) * cosAngle * cosAngle;

        float speed;

        // Check for invalid trajectory, fallback if needed
        if (denominator > 0)
        {
            speed = Mathf.Sqrt(numerator / denominator);
        }
        else
        {
            // Estimate max distance throw at this angle
            speed = Mathf.Sqrt(gravity * maxDistance / (sinAngle * 2f));
        }

        Vector3 directionXZ = toTargetXZ.normalized;
        Vector3 velocity = directionXZ * speed * cosAngle;
        velocity.y = speed * sinAngle;

        rb.linearVelocity = Vector3.zero; // optional reset
        rb.AddForce(velocity, ForceMode.Impulse);
    }
}
