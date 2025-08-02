using UnityEngine;
using UnityEngine.Events;

public class interaction : MonoBehaviour
{
    public interactionController.interactionType interactionType;
    public bool mustBeInfront = true;

    [Space]
    public triggerDialog triggerDialog;
    public dialogScriptableObject dialog;
    [Space]
    public UnityEvent interactionEvent;

    private void Awake()
    {
        triggerDialog = GetComponent<triggerDialog>();
    }

    public void startDialog()
    {
        if (dialog != null)
        {
            triggerDialog.doTriggerDialog(dialog);
        }
        else
        {
            Debug.Log("no dialog on interaction: " + name);
        }
    }

    public void triggerEvent()
    {
        if (interactionEvent != null && interactionEvent.GetPersistentEventCount() > 0)
        {
            interactionEvent.Invoke();
        }
        else
        {
            Debug.Log("no event on interaction: " + name);
        }
    }
}
