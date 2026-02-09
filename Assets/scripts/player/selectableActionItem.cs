using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class selectableActionItem : MonoBehaviour
{
    [Flags]
    public enum ActionTriggerState
    {
        Started = 1 << 0,
        Performed = 1 << 1,
        Canceled = 1 << 2
    }

    public List<InputActionReference> inputActions;

    public InputActionReference assigedAction = null;

    public ActionTriggerState triggerOn = ActionTriggerState.Performed;

    public UnityEvent onAssignedActionTriggered;

    private bool wasSubscribed = false;

    public void assignAction(InputActionReference action)
    {
        UnsubscribeFromAction();
        assigedAction = action;
        SubscribeToAction();

        if (action == null)
            return;

        var items = FindObjectsByType<selectableActionItem>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (var item in items)
        {
            if (item == this)
                continue;

            if (item.assigedAction != null &&
                item.assigedAction.action == action.action)
            {
                item.assignAction(null);
            }
        }
    }


    private void OnEnable()
    {
        SubscribeToAction();
    }

    private void OnDisable()
    {
        UnsubscribeFromAction();
    }

    private void SubscribeToAction()
    {
        if(assigedAction == null || wasSubscribed)
            return;

        if((triggerOn & ActionTriggerState.Started) != 0)
            assigedAction.action.started += OnActionStarted;

        if((triggerOn & ActionTriggerState.Performed) != 0)
            assigedAction.action.performed += OnActionPerformed;

        if((triggerOn & ActionTriggerState.Canceled) != 0)
            assigedAction.action.canceled += OnActionCanceled;

        wasSubscribed = true;
    }

    private void UnsubscribeFromAction()
    {
        if(assigedAction == null || !wasSubscribed)
            return;

        assigedAction.action.started -= OnActionStarted;
        assigedAction.action.performed -= OnActionPerformed;
        assigedAction.action.canceled -= OnActionCanceled;

        wasSubscribed = false;
    }

    private void OnActionStarted(InputAction.CallbackContext context)
    {
        TriggerEvent("started");
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        TriggerEvent("performed");
    }

    private void OnActionCanceled(InputAction.CallbackContext context)
    {
        TriggerEvent("canceled");
    }

    private void TriggerEvent(string stateName)
    {
        onAssignedActionTriggered.Invoke();

        if(onAssignedActionTriggered.GetPersistentEventCount() == 0)
        {
            Debug.Log(gameObject.name + ": action " + stateName + " with " + assigedAction.name);
        }
    }
}