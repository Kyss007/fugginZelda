using PhysicalWalk;
using UnityEditor.UIElements;
using UnityEngine;

public class objectHold : MonoBehaviour
{
    public bool debugDopickUp = false;
    public bool isHolding = false;
    public Animator animator;

    public Rigidbody objectToPickup;
    public Rigidbody pickedUpObject;

    public Collider pickedUpCollider;

    public DampedSpringMotionCopier motionCopier;

    public string heldObjectLayer;

    public int ogHeldObjectLayer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (debugDopickUp)
        {
            debugDopickUp = false;
            doPickup();
        }

        if (pickedUpObject != null)
        {
            motionCopier = pickedUpObject.GetComponent<DampedSpringMotionCopier>();
            motionCopier.positionalSpring.sourceObject = transform;
            motionCopier.transform.forward = transform.forward;
        }
    }

    public void doPickup()
    {
        if (!isHolding)
        {
            isHolding = true;

            objectToPickup.isKinematic = true;

            pickedUpObject = objectToPickup;

            animator.SetBool("holdingObject", true);

            ogHeldObjectLayer = pickedUpObject.gameObject.layer;
            pickedUpCollider.gameObject.layer = LayerMask.NameToLayer(heldObjectLayer);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody.GetComponent<holdableObject>())
        {
            if (objectToPickup == null && pickedUpObject == null && !isHolding)
            {
                objectToPickup = other.attachedRigidbody;
                pickedUpCollider = other;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody.GetComponent<holdableObject>())
        {
            if (objectToPickup == other.attachedRigidbody)
            {
                objectToPickup = null;
            }
        }
    }
}
