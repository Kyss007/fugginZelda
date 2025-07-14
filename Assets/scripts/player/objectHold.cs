using PhysicalWalk;
using UnityEditor.UIElements;
using UnityEngine;

public class objectHold : MonoBehaviour
{
    public bool debugDopickUp = false;
    public bool isHolding = false;
    public Animator animator;

    public holdableObject objectToPickup;
    public holdableObject pickedUpObject;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (debugDopickUp)
        {
            debugDopickUp = false;

            if (!isHolding)
            {
                doPickup();
            }
            else
            {
                doDrop();
            }
        }
    }

    public void doPickup()
    {
        if (!isHolding && objectToPickup != null)
        {
            isHolding = true;

            animator.SetBool("holdingObject", true);

            objectToPickup.doPickup(transform);

            pickedUpObject = objectToPickup;
        }
    }

    public void doDrop()
    {
        if (isHolding && pickedUpObject != null)
        {
            animator.SetBool("holdingObject", false);
        }
    }

    public void triggerDropOnObject()
    {
        if (pickedUpObject != null)
        {
            isHolding = false;

            pickedUpObject.doDrop();

            pickedUpObject = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null)
            return;

        if (other.attachedRigidbody.TryGetComponent<holdableObject>(out holdableObject holdableObject))
        {
            if (objectToPickup == null && pickedUpObject == null && !isHolding)
            {
                objectToPickup = holdableObject;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == null)
            return;
            
        if (other.attachedRigidbody.TryGetComponent<holdableObject>(out holdableObject holdableObject))
        {
            if (objectToPickup == holdableObject)
            {
                objectToPickup = null;
            }
        }
    }
}
