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
}
