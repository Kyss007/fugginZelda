using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class swordSwinger : MonoBehaviour
{
    public Animator animator;

    public float stabBufferTime = 0.2f;

    public UnityEvent onEnableHitDetection;
    public UnityEvent onDisableHitDetection;

    private damageProvider damageProvider;

    private keanusCharacterController cc;
    private kccIinputDriver inputProvider;

    private bool hasTriggeredAttackThisButtonPress = false;

    private void Start()
    {
        damageProvider = GetComponentInParent<damageProvider>();

        cc = GetComponentInParent<keanusCharacterController>();
        inputProvider = cc.inputDriver;
    }

    void Update()
    {
        if (inputProvider == null)
            return;

        if (inputProvider.getAttackInput() && !hasTriggeredAttackThisButtonPress)
        {
            animator.SetTrigger("swing");

            hasTriggeredAttackThisButtonPress = true;
        }
        else if (!inputProvider.getAttackInput() && hasTriggeredAttackThisButtonPress)
        {
            hasTriggeredAttackThisButtonPress = false;
        }

        thirdPersonMovementDriver movementDriver = (thirdPersonMovementDriver)cc.currentMovementDriver;
        animator.SetBool("isDodge", movementDriver.isDodging);
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

