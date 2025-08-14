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

    private void Start()
    {
        damageProvider = GetComponentInParent<damageProvider>();

        cc = GetComponentInParent<keanusCharacterController>();
        inputProvider = cc.inputDriver;

        targetController = transform.parent.GetComponentInChildren<targetController>();

        movementDriver = (thirdPersonMovementDriver)cc.currentMovementDriver;
    }

    void Update()
    {
        if (disableSword)
            return;

        thirdPersonMovementDriver movementDriver = (thirdPersonMovementDriver)cc.currentMovementDriver;
        if (inputProvider == null)
            return;

        if (inputProvider.getAttackInput() && !hasTriggeredAttackThisButtonPress)
        {
            if (targetController.isTargeting)
            {
                if (movementDriver.isJumping)
                {
                    animator.SetBool("jumpSwing", true);
                }
                else
                {
                    animator.SetBool("jumpSwing", false);
                }
            }
            else
            {
                animator.SetBool("jumpSwing", false);
            }

            animator.SetTrigger("swing");

            hasTriggeredAttackThisButtonPress = true;
        }
        else if (!inputProvider.getAttackInput() && hasTriggeredAttackThisButtonPress)
        {
            hasTriggeredAttackThisButtonPress = false;
        }

        animator.SetBool("isDodge", movementDriver.isDodging);

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

