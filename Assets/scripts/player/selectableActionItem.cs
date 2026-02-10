using System;
using System.Collections;
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

    public float assignmentCooldown = 0.2f;

    public float resumeCooldown = 0.2f;

    private bool wasSubscribed = false;
    private float lastAssignmentTime = -999f;
    private float lastResumeTime = -999f;
    private float lastTimeScale = 1f;

    public void assignAction(InputActionReference action)
    {
        UnsubscribeFromAction();
        assigedAction = action;
        lastAssignmentTime = Time.unscaledTime;
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
        lastTimeScale = Time.timeScale;
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

    private void CheckForResume()
    {
        // Detect transition from paused (0) to unpaused (>0)
        if (lastTimeScale == 0 && Time.timeScale > 0)
        {
            lastResumeTime = Time.unscaledTime;
        }
        lastTimeScale = Time.timeScale;
    }

    private void OnActionStarted(InputAction.CallbackContext context)
    {
        CheckForResume();
        TriggerEvent("started");
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        CheckForResume();
        TriggerEvent("performed");
    }

    private void OnActionCanceled(InputAction.CallbackContext context)
    {
        CheckForResume();
        TriggerEvent("canceled");
    }

    private void TriggerEvent(string stateName)
    {
        // Block events during cooldown period after resuming from pause
        if(Time.unscaledTime - lastResumeTime < resumeCooldown)
            return;

        if(Time.timeScale == 0)
            return;

        // Block events during cooldown period after assignment
        if(Time.unscaledTime - lastAssignmentTime < assignmentCooldown)
            return;

        onAssignedActionTriggered.Invoke();

        if(onAssignedActionTriggered.GetPersistentEventCount() == 0)
        {
            Debug.Log(gameObject.name + ": action " + stateName + " with " + assigedAction.name);
        }
    }
}