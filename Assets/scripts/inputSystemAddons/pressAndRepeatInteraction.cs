using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class pressAndRepeatInteraction : IInputInteraction
{
    public float initialDelay = 0.5f;

    public float repeatInterval = 0.05f;
    public float pressThreshold = 0.9f;

    private double _nextTriggerTime;
    private bool _initialFired;
    private bool _wasActuated;

    static pressAndRepeatInteraction()
    {
       InputSystem.RegisterInteraction<pressAndRepeatInteraction>();
    }


    public void Process(ref InputInteractionContext context)
    {
        bool isActuated = context.ControlIsActuated(pressThreshold);

        if (context.timerHasExpired)
        {
            context.Performed();
            _nextTriggerTime = context.time + repeatInterval;
            context.SetTimeout((float)(_nextTriggerTime - context.time));
            return;
        }

        if (isActuated && !_wasActuated)
        {
            _initialFired = true;
            context.Performed();
            _nextTriggerTime = context.time + initialDelay;
            context.SetTimeout((float)(_nextTriggerTime - context.time));
        }
        else if (!isActuated && context.phase != InputActionPhase.Canceled)
        {
            _initialFired = false;
            context.Canceled();
        }

        _wasActuated = isActuated;
    }


    public void Reset()
    {
        _initialFired = false;
        _nextTriggerTime = 0;
    }
}
