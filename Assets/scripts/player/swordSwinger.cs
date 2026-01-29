using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Unity.Collections;

public class swordSwinger : MonoBehaviour
{
    public Animator animator;

    public UnityEvent onEnableHitDetection;
    public UnityEvent onDisableHitDetection;

    private damageProvider damageProvider;

    private keanusCharacterController cc;
    private kccIinputDriver inputProvider;
    private thirdPersonMovementDriver movementDriver;

    private bool hasTriggeredAttackThisButtonPress = false;

    private targetController targetController;

    private bool wasGrounded = false;

    public bool disableSword = false;

    private float dodgeDisableTimer = 0f;
    public float dodgeBufferTime = 0.2f; 


    private void Start()
    {
        damageProvider = GetComponentInParent<damageProvider>();

        cc = GetComponentInParent<keanusCharacterController>();
        inputProvider = cc.inputDriver;

        targetController = transform.parent.parent.GetComponentInChildren<targetController>();

        movementDriver = (thirdPersonMovementDriver)cc.currentMovementDriver;
    }

    void Update()
    {
        if (disableSword)
            return;

        thirdPersonMovementDriver movementDriver = (thirdPersonMovementDriver)cc.currentMovementDriver;
        if (inputProvider == null)
            return;

        // --- Attack Logic ---
        if (inputProvider.getAttackInput() && !hasTriggeredAttackThisButtonPress)
        {
            // ... (existing attack logic)
            animator.SetTrigger("swing");
            hasTriggeredAttackThisButtonPress = true;
        }
        else if (!inputProvider.getAttackInput() && hasTriggeredAttackThisButtonPress)
        {
            hasTriggeredAttackThisButtonPress = false;
        }

        // --- Improved Dodge Logic with Delay ---
        if (movementDriver.isDodging)
        {
            // Set true instantly and reset the timer
            animator.SetBool("isDodge", true);
            dodgeDisableTimer = dodgeBufferTime; 
        }
        else
        {
            // If we aren't dodging, countdown the timer
            if (dodgeDisableTimer > 0)
            {
                dodgeDisableTimer -= Time.deltaTime;
            }
            else
            {
                // Only set to false once the timer hits zero
                animator.SetBool("isDodge", false);
            }
        }

        // --- Grounded Logic ---
        if (movementDriver.isGrounded && !wasGrounded)
        {
            animator.SetBool("jumpSwing", false);
        }
        wasGrounded = movementDriver.isGrounded;
    }

    /*public void triggerSwing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetTrigger("swing");
        }
    }*/

    public void switchSideToLeft()
    {
        animator.SetBool("swingLeft", true);
    }

    public void switchSideToRight()
    {
        animator.SetBool("swingLeft", false);
    }

    public void setDamage(string damageName)
    {
        damageProvider.updateDamageState(damageName);
    }

    public void enableHitDetection()
    {
        onEnableHitDetection.Invoke();
    }

    public void disableHitDetection()
    {
        onDisableHitDetection.Invoke();
    }
}

