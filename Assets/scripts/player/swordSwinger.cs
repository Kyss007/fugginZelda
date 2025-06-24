using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class swordSwinger : MonoBehaviour
{
    public Animator animator;

    public float stabBufferTime = 0.2f;

    public UnityEvent onEnableHitDetection;
    public UnityEvent onDisableHitDetection;

    private characterController characterController;
    private damageProvider damageProvider;

    private void Start()
    {
        characterController = GetComponentInParent<characterController>();
        damageProvider = GetComponentInParent<damageProvider>();
    }

    private void Update()
    {
        animator.SetBool("isDodge", characterController.wasDodge || Time.time - characterController.getLastDodgeTime() <= stabBufferTime);
    }

    public void triggerSwing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetTrigger("swing");
        }
    }

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

