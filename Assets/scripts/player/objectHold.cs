using PhysicalWalk;
using UnityEngine;

public class objectHold : MonoBehaviour
{
    public bool isHolding = false;
    public Animator animator;

    public holdableObject objectToPickup;
    public holdableObject pickedUpObject;

    private swordSwinger swordSwinger;
    private keanusCharacterController cc;

    private inputSystemInputDriver inputDriver;
    private thirdPersonMovementDriver movementDriver;
    private targetController targetController;

    private bool lastJumpInput = false;

    private float jumpDisableTimer = 0f;
    public float jumpDisableDuration = 0.2f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        cc = transform.parent.GetComponent<keanusCharacterController>();
        swordSwinger = cc.GetComponentInChildren<swordSwinger>();
        inputDriver = cc.GetComponentInChildren<inputSystemInputDriver>();
        movementDriver = cc.GetComponentInChildren<thirdPersonMovementDriver>();
        targetController = cc.GetComponentInChildren<targetController>();
    }

    public void Update()
    {
        swordSwinger.disableSword = isHolding;

        if (jumpDisableTimer > 0)
            jumpDisableTimer -= Time.deltaTime;

        movementDriver.disableJump = jumpDisableTimer > 0 || objectToPickup != null || (inputDriver.getMoveInput() == Vector2.zero && isHolding) || (isHolding && inputDriver.getTargetInput());

        bool grounded = movementDriver.isGrounded && !movementDriver.isJumping;
        bool jumpPressed = inputDriver.getJumpInput();
        bool jumpJustPressed = jumpPressed && !lastJumpInput;

        if (grounded && jumpJustPressed && isHolding && inputDriver.getTargetInput())
        {
            jumpDisableTimer = jumpDisableDuration;
            doDrop();

            if (targetController.currentSellectedTarget != null)
            {
                pickedUpObject.doThrow(targetController.currentSellectedTarget.transform.position, true);
            }
            else
            {
                pickedUpObject.doThrow();
            }
        }
        else if (grounded && jumpJustPressed)
        {
            if (!isHolding && objectToPickup != null && !objectToPickup.isThrow)
            {
                doPickup();
            }
            else if (isHolding && inputDriver.getMoveInput() == Vector2.zero)
            {
                doDrop();
            }
        }

        lastJumpInput = jumpPressed;
    }

    public void doPickup()
    {
        if (!isHolding && objectToPickup != null)
        {
            isHolding = true;

            animator.SetBool("holdingObject", true);

            objectToPickup.doPickup(transform);

            pickedUpObject = objectToPickup;
            objectToPickup = null;
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

                jumpDisableTimer = jumpDisableDuration;
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
