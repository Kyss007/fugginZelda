using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class selectableActionItem : MonoBehaviour
{
    public List<InputActionReference> inputActions;

    public InputActionReference assigedAction = null;

    public UnityEvent onAssignedActionTriggered;

    public void assignAction(InputActionReference action)
    {
        if(inputActions.Contains(action))
            assigedAction = action;
    }

    public void Update()
    {
        if(assigedAction == null)
            return;

        if(assigedAction.action.triggered)
        {
            onAssignedActionTriggered.Invoke();
        }
    }
}
