using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class inputSystemInputDriver : MonoBehaviour, kccIinputDriver
{
    public event Action onCameraChangeEvent;

    [SerializeField] private Vector2 lookInput;
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private bool jumpInput;
    [SerializeField] private bool dodgeInput;
    [SerializeField] private bool swordAttackInput;

    public Vector2 getLookInput()
    {
        return lookInput;
    }

    public Vector2 getMoveInput()
    {
        return moveInput;
    }

    public bool getJumpInput()
    {
        return jumpInput;
    }

    public bool getDodgeInput()
    {
        return dodgeInput;
    }

    public bool getAttackInput()
    {
        return swordAttackInput;
    }

    public void onCameraChange()
    {
        onCameraChangeEvent?.Invoke();
    }

    public void collectLookInput(InputAction.CallbackContext callbackContext)
    {
        lookInput = callbackContext.ReadValue<Vector2>();
    }

    public void collectMoveInput(InputAction.CallbackContext callbackContext)
    {
        moveInput = callbackContext.ReadValue<Vector2>();
    }

    public void collectJumpInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            jumpInput = true;
        }
        else if (callbackContext.canceled)
        {
            jumpInput = false;
        }
    }

    public void collectCameraInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            onCameraChange();
        }
    }

    public void collectSwordAttackInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            swordAttackInput = true;
        }
        else if (callbackContext.canceled)
        {
            swordAttackInput = false;
        }
    }
    
    public void collectDodgeInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            dodgeInput = true;
        }
        else if (callbackContext.canceled)
        {
            dodgeInput = false;
        }
    }
}
