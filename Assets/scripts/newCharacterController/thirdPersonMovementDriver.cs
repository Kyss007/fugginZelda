using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class thirdPersonMovementDriver : MonoBehaviour, kccIMovementDriver
{
    [Header("Movement Parameters")]
    public float maxSpeed = 15f;
    public float acceleration = 200f;
    public AnimationCurve accelerationFactorFromDotCurve;
    public float deceleration = 200f;
    public float accelMod = 1f;
    public float decelMod = 1f;
    public float turnSpeed = 15f;
    public AnimationCurve rotationSpeedByMoveSpeed;
    public float maxGroundAngle = 45f;
    public bool lookRelativeInput = false;

    [Header("Jump Parameters")]
    public float targetJumpHeight = 3.0f;
    public float cyoteTime = 0.2f;
    public float bufferedJumpTime = 0.1f;
    public float deccendGravityMultiplier = 1.7f;

    [Header("Float Parameters")]
    public float rideHeight = 1.5f;
    public float maxRideHeight = 2.5f;
    public float groundedDistance = 1.3f;
    public float rideSpringStrength = 800f;
    public float rideSpringDamper = 20f;
    public LayerMask groundLayerMask;

    [Header("Dodge Parameters")]
    public float dodgeDistance = 2f;
    public float dodgeSpeed = 20f;
    public float dodgeCooldown = 1f;

    //private shits
    private float lastGroundTime;
    private float lastJumpInputTime = -Mathf.Infinity;

    [Space]
    public bool isMoving;
    public bool disableJump = false;
    public bool isJumping = false;
    public bool cancelJump = false;
    public bool isGrounded = true;
    public bool isDodging = false;
    public bool canDodge = true;
    private float goalSpeed;

    public RaycastHit groundHit;
    private bool wasGrounded;

    // Platform rotation tracking
    private Rigidbody lastPlatformRigidbody;
    private Quaternion lastPlatformRotation;

    private bool jumpInput;
    private bool lastJumpInput;
    private Vector2 inputDir;
    private Vector3 moveDirection;
    private Vector3 lastMoveDirection;
    private Vector3 lookDirection;
    public Rigidbody rb;
    public Camera camera;

    public void Start()
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        lookDirection = transform.forward.normalized;
        lastMoveDirection = transform.forward.normalized;
    }

    private void OnEnable()
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        camera.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        try
        {
            //sometimes errors out when stopping the editor. can be safely ignored
            camera.gameObject.SetActive(false);

        }
        catch
        {
            
        }
    }

    public void initDriver(Rigidbody rigidbody)
    {
        rb = rigidbody;
    }

    public void setMoveInput(Vector2 input)
    {
        inputDir = input;
    }

    public void setJumpInput(bool input)
    {
        jumpInput = input;

        if (jumpInput && !lastJumpInput)
        {
            lastJumpInputTime = Time.time;
        }
        else if (!jumpInput && lastJumpInput)
        {
            if (isJumping)
            {
                cancelJump = true;
            }
        }
    }

    public void movePlayer()
    {
        isGrounded = checkGrounded(out groundHit);

        handlePlatformRotation();

        calculateDesiredMoveDirection();

        rotatePlayerSpeedBased();

        isMoving = doMovePlayer();

        if (Time.time - lastJumpInputTime <= bufferedJumpTime)
        {
            doJump();
        }

        if (isGrounded)
        {
            hoverAboveGround(groundHit);
        }
        else
        {
            if (wasGrounded && !isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            }

            if (rb.linearVelocity.y < 0 || cancelJump)
            {
                updateGravityMultiplier(deccendGravityMultiplier);
            }
        }

        if (moveDirection != Vector3.zero && !lookRelativeInput)
        {
            lookDirection = moveDirection;
        }

        wasGrounded = isGrounded;
        lastJumpInput = jumpInput;
    }

    private void handlePlatformRotation()
    {
        if (isGrounded && groundHit.rigidbody != null)
        {
            // if on  new platform store rotation
            if (lastPlatformRigidbody != groundHit.rigidbody)
            {
                lastPlatformRigidbody = groundHit.rigidbody;
                lastPlatformRotation = groundHit.rigidbody.rotation;
            }
            else
            {
                //calculate rotation difference since last frame
                Quaternion currentPlatformRotation = groundHit.rigidbody.rotation;
                Quaternion rotationDifference = currentPlatformRotation * Quaternion.Inverse(lastPlatformRotation);
                
                // apply the rotation difference to the look direction
                lookDirection = rotationDifference * lookDirection;
                lookDirection.y = 0; // Keep look direction horizontal
                lookDirection = lookDirection.normalized;
                
                // update the stored platform rotation
                lastPlatformRotation = currentPlatformRotation;
            }
        }
        else
        {
            // clear platform tracking when not on a rigidbody
            lastPlatformRigidbody = null;
        }
    }

    public bool checkGrounded(out RaycastHit groundHit)
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

    public void calculateDesiredMoveDirection()
    {
        float moveHorizontal = inputDir.x;
        float moveVertical = inputDir.y;

        Vector3 camForward = camera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        if (lookRelativeInput)
        {
            Vector3 playerForward = transform.forward;
            playerForward.y = 0f;
            playerForward.Normalize();
            
            Vector3 desiredMoveDirection1 = (playerForward * moveVertical) + (transform.right * moveHorizontal);
            moveDirection = desiredMoveDirection1.normalized;

            if (moveDirection != Vector3.zero)
            {
                lastMoveDirection = moveDirection;
            }

            return;
        }

        Vector3 desiredMoveDirection = (camForward * moveVertical) + (camera.transform.right * moveHorizontal);
        moveDirection = desiredMoveDirection.normalized;
    }

    private bool doMovePlayer()
    {
        if (isDodging)
            return false;

        bool isMoving = false;

        float idealSpeed = moveDirection.magnitude * maxSpeed;
        float speedDifference = idealSpeed - goalSpeed;

        Vector3 unitVel = rb.linearVelocity.normalized;
        Vector3 unitGoal = moveDirection.normalized;
        float velDot = Vector3.Dot(unitGoal, unitVel);

        if (speedDifference > 0)
        {
            goalSpeed = Mathf.Min(goalSpeed + ((acceleration * accelMod) * accelerationFactorFromDotCurve.Evaluate(velDot)) * Time.deltaTime, idealSpeed);
        }
        else if (speedDifference < 0)
        {
            if (!isJumping)
            {
                goalSpeed = Mathf.Max(goalSpeed - (deceleration * decelMod) * Time.deltaTime, idealSpeed);
            }
        }

        if (goalSpeed > Mathf.Epsilon)
        {
            isMoving = true;

            Vector3 movementDirection = lookRelativeInput ? (moveDirection.magnitude > 0 ? moveDirection : lastMoveDirection) : transform.forward;
            Vector3 idealVel = movementDirection * goalSpeed;

            Vector3 neededAccel = (idealVel - rb.linearVelocity) / Time.fixedDeltaTime;

            rb.AddForce(new Vector3(neededAccel.x, 0, neededAccel.z), ForceMode.Acceleration);
        }
        else
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0;
            velocity.z = 0;

            rb.linearVelocity = velocity;
        }

        if (groundHit.rigidbody != null && isGrounded)
        {
            Vector3 pointVelocity = groundHit.rigidbody.GetPointVelocity(transform.position);
            rb.linearVelocity += new Vector3(pointVelocity.x, 0f, pointVelocity.z);
        }

        return isMoving;
    }

    private void rotatePlayerSpeedBased()
    {
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        float turnSpeedFactor = rotationSpeedByMoveSpeed.Evaluate(calculateRotationSpeedFactor());
        rb.rotation = Quaternion.Lerp(rb.rotation, targetRotation, Time.fixedDeltaTime * ((turnSpeed * accelMod) * turnSpeedFactor));
    }

    private void hoverAboveGround(RaycastHit groundHit)
    {
        if (groundHit.distance <= maxRideHeight)
        {
            float distanceToGround = groundHit.distance;
            Vector3 upForce = Vector3.up * (rideHeight - distanceToGround) * rideSpringStrength;
            Vector3 dampingForce = new Vector3(0f, -rb.linearVelocity.y, 0f) * rideSpringDamper;

            rb.AddForce(upForce + dampingForce);

            if (groundHit.rigidbody != null)
            {
                groundHit.rigidbody.AddForceAtPosition(-(upForce + dampingForce), groundHit.point);    
            }
        }
    }

    public void doJump()
    {
        if (disableJump)
            return;

        if (!(isGrounded || Time.time - lastGroundTime <= cyoteTime) || isJumping)
            {
                return;
            }

        isGrounded = false;
        isJumping = true;

        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0; // Reset vertical velocity
        rb.linearVelocity = currentVelocity;

        float jumpForce = calculateJumpForce();

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void updateGravityMultiplier(float multiplier)
    {
        rb.AddForce(Physics.gravity * multiplier, ForceMode.Acceleration);
    }

    private float calculateRotationSpeedFactor()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);

        return normalizedSpeed;
    }

    private float calculateJumpForce()
    {
        float initialVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * (targetJumpHeight - rideHeight));
        return initialVelocity * rb.mass;
    }

    public void setLookInput(Vector2 input)
    {
        lookDirection.x = input.x;
        lookDirection.z = input.y;
    }

    public void dodge()
    {
        if (!canDodge || isDodging || !isGrounded)
            return;

        if (lookRelativeInput && inputDir == Vector2.zero)
            return;

        StartCoroutine(performDodge());
    }

    private IEnumerator performDodge()
    {
        isDodging = true;
        canDodge = false;

        Vector3 dodgeDirection = (lookRelativeInput ? moveDirection : lookDirection).normalized;
        float dodgeDuration = dodgeDistance / dodgeSpeed;
        float elapsed = 0f;

        float originalMaxSpeed = maxSpeed;
        maxSpeed = 0f;

        while (elapsed < dodgeDuration)
        {
            rb.linearVelocity = new Vector3(dodgeDirection.x * dodgeSpeed, rb.linearVelocity.y, dodgeDirection.z * dodgeSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        maxSpeed = originalMaxSpeed;
        isDodging = false;

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }
}