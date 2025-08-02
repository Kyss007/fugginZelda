using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;

public class interactionController : MonoBehaviour
{
    public interaction currentAvailabeInteraction;

    public float targetInteractionRange = 5f;

    private keanusCharacterController cc;

    private inputSystemInputDriver inputDriver;
    private objectHold objectHold;
    private swordSwinger swordSwinger;
    private targetController targetController;

    private bool lastJumpInput;

    private Vector3 ogPos;

    private void Start()
    {
        cc = GetComponentInParent<keanusCharacterController>();

        inputDriver = cc.GetComponentInChildren<inputSystemInputDriver>();
        objectHold = cc.GetComponentInChildren<objectHold>();
        swordSwinger = cc.GetComponentInChildren<swordSwinger>();
        targetController = cc.GetComponentInChildren<targetController>();

        ogPos = transform.localPosition;
    }

    public enum interactionType
    {
        triggerEvent,
        dialog
    }

    public void Update()
    {
        if (targetController.currentSellectedTarget != null)
        {
            float distance = Vector3.Distance(transform.parent.position, targetController.currentSellectedTarget.transform.position);

            if (distance < targetInteractionRange)
            {
                transform.position = targetController.currentSellectedTarget.transform.position;
            }
            else
            {
                transform.localPosition = ogPos;
            }
        }
        else
        {
            transform.localPosition = ogPos;
        }

        if (currentAvailabeInteraction != null)
        {
            if (!currentAvailabeInteraction.isEnabled)
            {
                currentAvailabeInteraction = null;
                enableShit();
            }
            
            if (inputDriver.getJumpInput() && !lastJumpInput)
            {
                switch (currentAvailabeInteraction.interactionType)
                {
                    case interactionType.triggerEvent:
                        currentAvailabeInteraction.triggerEvent();
                        break;

                    case interactionType.dialog:
                        currentAvailabeInteraction.startDialog();
                        break;
                }
            }
            
        }

        lastJumpInput = inputDriver.getJumpInput();
    }

    public void disableShit()
    {
        cc.disableJump = true;
        objectHold.disableHold = true;
    }

    public void enableShit()
    {
        cc.disableJump = false;
        objectHold.disableHold = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        interaction interaction = other.GetComponent<interaction>();

        if (interaction.mustBeInfront)
        {
            if (Vector3.Dot((interaction.transform.position - transform.parent.position).normalized, interaction.transform.forward) > 0.5f)
            {
                currentAvailabeInteraction = interaction;
                disableShit();
            }
        }
        else
        {
            currentAvailabeInteraction = interaction;
            disableShit();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (currentAvailabeInteraction == other.GetComponent<interaction>())
        {
            currentAvailabeInteraction = null;
            enableShit();
        }
    }
}
