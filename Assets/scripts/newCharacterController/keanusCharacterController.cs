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

    [SerializeField] public playerStatusData statusData = new playerStatusData();

    [SerializeField]private List<kccIMovementDriver> movementDrivers = new List<kccIMovementDriver>();
    [SerializeField]private kccIMovementDriver currentMovementDriver;
    [SerializeField]public kccIinputDriver inputDriver;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        foreach(Transform child in transform)
        {
            kccIMovementDriver childMovementDriver = child.GetComponent<kccIMovementDriver>();

            if(childMovementDriver != null)
            {
                childMovementDriver.initDriver(rb);
    
                if(movementDrivers.Count == 0)
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

    public void Update()
    {
        if(canMove)
            currentMovementDriver.setLookInput(inputDriver.getLookInput());
    }
    
    public void FixedUpdate()
    {
        if(true)//currentVehicle == null)
        {
            if(canMove)
            {
                currentMovementDriver.setMoveInput(inputDriver.getMoveInput());
                currentMovementDriver.setJumpInput(inputDriver.getJumpInput());
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
    }

    /*public void getLeaveVehicleInput(InputAction.CallbackContext context)
    {
        if(currentVehicle == null)
            return;

        if(context.started)
        {
            currentVehicle.doInteractAction(im);
        }
    }*/
}
