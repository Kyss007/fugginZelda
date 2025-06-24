using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class firstPersonMovementDriver : MonoBehaviour, kccIMovementDriver
{
    public List<GameObject> objectsToToggleActive;

    public Rigidbody rb;


    private Vector2 moveInput;
    private bool jumpInput;
    private bool lastJumpInput;

    public float mouseSensitivity = 1f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float maxSpeed = 5f;
    [Space]
    public float targetJumpHeight = 3f;
    public float bufferedJumpTime = 0.1f;
    public float cyoteTime = 0.2f;
    public float deccendGravityMultiplier = 1.7f;
    [Space]
    public LayerMask groundLayerMask;
    public bool isGrounded = true;
    public float rideHeight = 0.8f;
    public float maxRideHeight = 1f;
    public float groundedDistance = 1f;
    public float rideSpringStrength = 1f;
    public float rideSpringDamper = 1f;

    public GameObject cameraPivot;
    public CapsuleCollider collider;
    
    private Vector2 lookInput;
    private bool crouchInput;

    private Vector3 platformVelocity;

    private float cameraPitchAngle = 0f;

    public bool isMoving = false;
    public bool isJumping = false;
    public bool cancelJump = false;

    private float lastJumpInputTime;
    private float lastGroundTime;

    public void initDriver(Rigidbody rigidbody)
    {
        rb = rigidbody;
    }

    public void movePlayer()
    {
        isGrounded = CheckGrounded(out RaycastHit hit);
        
        movePlayer(hit.rigidbody);

        if (Time.time - lastJumpInputTime <= bufferedJumpTime)
        {
            if(isGrounded || Time.time - lastGroundTime <= cyoteTime)
            {
                if (!isJumping)
                {
                    jump();
                }
            }
        }

        if (isGrounded)
        {
            hoverAboveGround(hit);
        }
        else
        {
            platformVelocity = Vector3.zero;

            if (rb.linearVelocity.y < 0 || cancelJump)
            {
                rb.AddForce(Physics.gravity * deccendGravityMultiplier, ForceMode.Acceleration);
            }
        }

        lastJumpInput = jumpInput;
    }

    public void setJumpInput(bool input)
    {
        jumpInput = input;

        if(jumpInput && !lastJumpInput)
        {
            lastJumpInputTime = Time.time;
        }
        else if (!jumpInput && lastJumpInput)
        {
            if(isJumping)
            {
                cancelJump = true;
            }
        }
    }

    public void setMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void Start()
    {
        cameraPitchAngle = cameraPivot.transform.rotation.eulerAngles.x;
        lastJumpInputTime = -Mathf.Infinity;
    }

    private void rotatePlayer()
    {
        if(Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX = /*Input.GetAxis("Mouse X")*/ lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = /*Input.GetAxis("Mouse Y")*/ lookInput.y * mouseSensitivity * Time.deltaTime;

        cameraPitchAngle -= mouseY;
        cameraPitchAngle = Mathf.Clamp(cameraPitchAngle, -90f, 90f);

        cameraPivot.transform.localRotation = Quaternion.Euler(cameraPitchAngle, 0f, 0f);

        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, mouseX, 0f));
    }

    private void movePlayer(Rigidbody groundRigidbody)
    {
        Vector3 inputDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        inputDirection = inputDirection.normalized;
        Vector3 targetVelocity = inputDirection * maxSpeed;

        Vector3 groundVelocity = Vector3.zero;
        Vector3 groundAngularVelocity = Vector3.zero;
        if (groundRigidbody != null)
        {
            if (false)//!groundRigidbody.TryGetComponent<flying>(out flying flying))
            {
                groundVelocity = groundRigidbody.linearVelocity;
                groundAngularVelocity = groundRigidbody.angularVelocity;
                //targetVelocity += groundVelocity;
            }
        }

        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 actualVelocity;

        if (inputDirection.magnitude > 0.1f)
        {
            // Accelerate
            actualVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            isMoving = true;
        }
        else
        {
            // Decelerate
            if (!isJumping)
            {
                actualVelocity = Vector3.MoveTowards(currentVelocity, groundVelocity, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                actualVelocity = currentVelocity;
            }
            isMoving = false;
        }
        Vector3 velocityChange = actualVelocity - currentVelocity;
        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Preserve vertical velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);

        if (groundRigidbody != null)
        {
            Vector3 pointOfRotation = groundRigidbody.worldCenterOfMass;
            Quaternion rotationDelta = Quaternion.Euler(groundAngularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime);
            // Calculate the desired position
            Vector3 desiredPosition = rotationDelta * (rb.position - pointOfRotation) + pointOfRotation;

            // Calculate the velocity change needed to reach the desired position
            velocityChange = (desiredPosition - rb.position) / Time.fixedDeltaTime;

            // Apply the force to change velocity
            //rb.AddForce(velocityChange, ForceMode.VelocityChange);

            // Calculate the desired angular velocity
            Vector3 desiredAngularVelocity = groundAngularVelocity;

            // Calculate the change in angular velocity
            Vector3 angularVelocityChange = desiredAngularVelocity - rb.angularVelocity;
            angularVelocityChange.x = 0;
            angularVelocityChange.z = 0;
            // Apply the torque to change angular velocity
            rb.AddTorque(angularVelocityChange, ForceMode.VelocityChange);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }

    }

    public void jump()
    {
        isGrounded = false;
        isJumping = true;

        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0; // Reset vertical velocity
        rb.linearVelocity = currentVelocity;

        float jumpForce = calculateJumpForce();
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void hoverAboveGround(RaycastHit groundHit)
    {
        if (groundHit.distance <= maxRideHeight)
        {
            float distanceToGround = groundHit.distance;
            Vector3 upForce = Vector3.up * (rideHeight - distanceToGround) * rideSpringStrength;
            Vector3 dampingForce = new Vector3(0f, -rb.linearVelocity.y, 0f) * rideSpringDamper;

            rb.AddForce(upForce + dampingForce);
        }
    }

    public bool CheckGrounded(out RaycastHit groundHit)
    {
        RaycastHit hit;

        Vector3 origin = transform.position;
        if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
        {
            groundHit = hit;

            if (isGrounded)
            {
                isJumping = false;
                cancelJump = false;

                return hit.distance <= maxRideHeight;
            }
            else
            {
                lastGroundTime = Time.time;

                return hit.distance <= groundedDistance;
            }
        }

        groundHit = hit;
        return false;
    }

    private float calculateJumpForce()
    {
        float initialVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * (targetJumpHeight - rideHeight));
        return initialVelocity * rb.mass;
    }

    public void getCrouchInput(InputAction.CallbackContext context)
    {
        bool shouldToggle = false;
        
        if (false)//lastInputDeviceTracker.IsGamepad())
        {
            shouldToggle = true;
        }
        if (!shouldToggle)
        {
            if (context.started && !crouchInput)
            {
                crouchInput = true;
                goCrouch();
            }
            else if(context.canceled && crouchInput)
            {
                crouchInput = false;
                goStand();
            }
        }
        else
        {
            if(context.started && !crouchInput)
            {
                crouchInput = true;
                goCrouch();
            }
            else if(context.started && crouchInput)
            {
                crouchInput = false;
                goStand();
            }
        }
    }

    public void goCrouch()
    {
        rideHeight = rideHeight / 3;
        maxRideHeight = maxRideHeight / 3;
        groundedDistance = groundedDistance / 3;

        maxSpeed = maxSpeed / 2;
        targetJumpHeight = targetJumpHeight / 2;

        collider.height = collider.height / 1.5f;

        //viewBobbing bob = GetComponentInChildren<viewBobbing>();
        //bob.bobbingSpeed = bob.bobbingSpeed / 2;
    }

    public void goStand()
    {
        rideHeight = rideHeight * 3;
        maxRideHeight = maxRideHeight * 3;
        groundedDistance = groundedDistance * 3;

        maxSpeed = maxSpeed * 2;
        targetJumpHeight = targetJumpHeight * 2;

        collider.height = collider.height * 1.5f;

        //viewBobbing bob = GetComponentInChildren<viewBobbing>();
        //bob.bobbingSpeed = bob.bobbingSpeed * 2;
    }

    public void OnEnable()
    {
        foreach(Transform child in cameraPivot.transform)
        {
            child.gameObject.SetActive(true);
        }

        toogleObjects();
    }

    public void OnDisable()
    {
        cameraPivot.transform.localRotation = Quaternion.identity;
        cameraPitchAngle = 0;

        foreach(Transform child in cameraPivot.transform)
        {
            child.gameObject.SetActive(false);
        }

        toogleObjects();
    }

    public void toogleObjects()
    {
        foreach(GameObject gameObject in objectsToToggleActive)
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }

    public void setLookInput(Vector2 input)
    {
        lookInput = input;

        rotatePlayer();
    }
}
