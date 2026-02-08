using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class keanusCharacterController : MonoBehaviour
{
    [System.Serializable]
    public class playerStatusData
    {
        public bool isAllowedToMove;
        public bool isMoving;
        public bool isGrounded;
    }

    public Rigidbody rb;
    public bool canMove = true;
    public bool disableJump = false;

    [SerializeField] public playerStatusData statusData = new playerStatusData();

    [SerializeField]private List<kccIMovementDriver> movementDrivers = new List<kccIMovementDriver>();
    [SerializeField]public kccIMovementDriver currentMovementDriver;
    [SerializeField]public kccIinputDriver inputDriver;

    private bool wasDodge = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        foreach (Transform child in transform)
        {
            kccIMovementDriver childMovementDriver = child.GetComponent<kccIMovementDriver>();

            if (childMovementDriver != null)
            {
                childMovementDriver.initDriver(rb);

                if (movementDrivers.Count == 0)
                {
                    currentMovementDriver = childMovementDriver;
                }

                movementDrivers.Add(childMovementDriver);
                child.gameObject.SetActive(false);
            }
        }

        ((MonoBehaviour)currentMovementDriver).gameObject.SetActive(true);

        inputDriver = GetComponentInChildren<kccIinputDriver>();
    }

    public void FixedUpdate()
    {
        if (true)//currentVehicle == null)
        {
            if (canMove)
            {
                currentMovementDriver.setMoveInput(inputDriver.getMoveInput());

                currentMovementDriver.setJumpInput(disableJump ? false : inputDriver.getJumpInput());

                if (inputDriver.getDodgeInput() && !wasDodge)
                {
                    currentMovementDriver.dodge();
                }
            }
            else
            {
                currentMovementDriver.setMoveInput(Vector2.zero);
                currentMovementDriver.setJumpInput(false);
            }

            currentMovementDriver.movePlayer();
        }
        else
        {

            //currentVehicle.collectMoveInput(inputDriver.getMoveInput());
        }

        wasDodge = inputDriver.getDodgeInput();
    }

    /*public void getLeaveVehicleInput(InputAction.CallbackContext context)
    {
        if(currentVehicle == null)
            return;

        if(context.started)
        {
            currentVehicle.doInteractAction(im);
        }

        please reload when i save man
    }*/
}
