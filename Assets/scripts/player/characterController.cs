using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class characterController : MonoBehaviour
{
    [Header("General data")]
    public Camera camera;
    public GameObject atGround;

    [Header("Movement Parameters")]
    public float maxSpeed = 8f;
    public float acceleration = 200f;
    public AnimationCurve accelerationFactorFromDotCurve;
    public float deceleration = 200f;
    public float accelMod = 1f;
    public float decelMod = 1f;
    public float turnSpeed = 10f;
    public AnimationCurve rotationSpeedByMoveSpeed;
    public float maxGroundAngle = 45f;

    [Header("Jump Parameters")]
    public float targetJumpHeight = 5.0f;
    public float cyoteTime = 0.1f;
    public float bufferedJumpTime = 0.1f;
    public float deccendGravityMultiplier = 2f;

    [Header("Float Parameters")]
    public float groundCheckRadius = 0.5f;
    public float rideHeight = 0.8f;
    public float maxRideHeight = 1f;
    public float groundedDistance = 1f;
    public float rideSpringStrength = 1f;
    public float rideSpringDamper = 1f;
    public LayerMask groundLayerMask;

    [Header("swordDash")]
    public LayerMask hitDetectionLayer;
    public float enemyCheckRadius = 1.5f;
    public float enemyCheckDistance = 2f;
    public float swordDashSpeed = 30f;
    public float swordDashMaxDistance = 2f;
    public float swordDashAtEnemyDistance = 0.5f;

    [Header("dodge")]
    public bool allowInAirDodge = false;
    public float dodgeSpeed = 120;
    public float standingdodgeDistance = 5f;
    public float movingDodgeDistance = 8f;
    public float dodgeCooldown = 0.5f;


    [Header("state info")]
    public state currentState = state.baseState;
    public bool isMoving;
    public bool isJumping = false;
    public bool cancelJump = false;
    public bool isGrounded = true;
    public bool isSwordDash = false;
    public bool isDodge = false;
    public bool wasDodge;

    public enum state
    {
        brainDead,
        baseState,
        swordDash,
        dodge,
        knockback
    };

    [HideInInspector] public RaycastHit groundHit;
    [HideInInspector] public float knockbackUpForce = 50;
    [HideInInspector] public float knockbackBackForce = 50;

    [HideInInspector]public Rigidbody rb;
    //private fields

    private float goalSpeed;
    private Vector2 inputDir;
    private Vector3 moveDirection;
    private Vector3 lookDirection;

    private bool wasGrounded;

    private float lastGroundTime;
    private float lastJumpInputTime;
    private float lastDodgeTime;

    private hitReciever toDashToEnemy;
    private Vector3 swordDashStartPos;
    private Vector3 dodgeStartPos;

    private bool endDash = false;
    private bool didKnockback = false;

    private Transform knockbackPartner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        lastJumpInputTime = -Mathf.Infinity;

        lookDirection = transform.forward;
    }

    private void FixedUpdate()
    {
        isGrounded = CheckGrounded(out groundHit);
        Debug.DrawLine(groundHit.point, groundHit.point + getGroundSlope(), Color.yellow);

        Vector3 slopeDir = getGroundSlope();
        slopeDir = Vector3.ProjectOnPlane(slopeDir, Vector3.up);
        Debug.DrawLine(groundHit.point, groundHit.point + slopeDir, Color.blue);

        atGround.transform.position = groundHit.point + (Vector3.up * 0.01f);
        atGround.transform.up = groundHit.normal;

        //state switcher
        switch (currentState)
        {
            case state.baseState:
                break;

            case state.swordDash:
                if (isSwordDash)
                {
                    if(toDashToEnemy == null)
                    {
                        if(Vector3.Distance(this.transform.position, swordDashStartPos) >= swordDashMaxDistance || endDash)
                        {
                            isSwordDash = false;

                            isDodge = false;

                            currentState = state.baseState;
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(this.transform.position, swordDashStartPos) >= swordDashMaxDistance ||
                            Physics.Raycast(this.transform.position, this.transform.forward, swordDashAtEnemyDistance) || endDash)
                        {
                            toDashToEnemy = null;
                            isSwordDash = false;

                            isDodge = false;

                            currentState = state.baseState;
                        }
                    }
                }
                break;

            case state.dodge:
                if (isDodge)
                {
                    float dodgeDistance;

                    if (isMoving)
                    {
                        dodgeDistance = movingDodgeDistance;
                    }
                    else
                    {
                        dodgeDistance = standingdodgeDistance;
                    }

                    if (Vector3.Distance(this.transform.position, dodgeStartPos) >= dodgeDistance || endDash)
                    {
                        isDodge = false;
                        lastDodgeTime = Time.time;
                        currentState = state.baseState;
                    }
                }
                break;

            case state.knockback:
                if (didKnockback)
                {
                    currentState = state.baseState;
                    didKnockback = false;
                }
                break;
        }

        //state logic
        switch (currentState)
        {
            case state.brainDead:
                break;

            case state.baseState:
                calculateDesiredMoveDirection();

                rotatePlayerSpeedBased();

                isMoving = movePlayer();

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
                break;

            case state.swordDash:
                //get pos of closest enemy
                //dash to enemy
                //change state to base
                if (isGrounded)
                {
                    hoverAboveGround(groundHit);
                }

                if (isSwordDash)
                {
                    break;
                }
                else
                {
                    isSwordDash = true;
                }

                RaycastHit hit;
                if(Physics.SphereCast(this.transform.position, enemyCheckRadius, this.transform.forward, out hit, enemyCheckDistance, hitDetectionLayer))
                {
                    hitReciever hitReciever = hit.collider.gameObject.GetComponent<hitReciever>();

                    if (hitReciever != null)
                    {
                        toDashToEnemy = hitReciever;
                    }
                }

                if(toDashToEnemy == null)
                {
                    swordDashStartPos = this.transform.position;

                    float swordDashDistance = swordDashMaxDistance;
                    float swordDashAcceleration = Mathf.Pow(swordDashSpeed, 2) / (2 * swordDashDistance);

                    rb.AddForce(this.transform.forward * swordDashAcceleration, ForceMode.Acceleration);
                }
                else
                {
                    swordDashStartPos = this.transform.position;

                    Vector3 directionToEnemy = toDashToEnemy.collider.transform.position - this.transform.position;
                    directionToEnemy.Normalize();

                    Vector3 moddedDirToEnemy = directionToEnemy;
                    moddedDirToEnemy.y = 0;
                    rb.rotation = Quaternion.LookRotation(moddedDirToEnemy, Vector3.up);
                    lookDirection = EulerToLookRotation(rb.rotation.eulerAngles);

                    float swordDashDistance = swordDashMaxDistance;
                    float swordDashAcceleration = Mathf.Pow(swordDashSpeed, 2) / (2 * swordDashDistance);

                    rb.AddForce(directionToEnemy * swordDashAcceleration, ForceMode.Acceleration);
                }
                
                break;

            case state.dodge:
                if (isGrounded)
                {
                    hoverAboveGround(groundHit);
                }

                //calculateDesiredMoveDirection();
                //rb.MoveRotation(Quaternion.Euler(moveDirection));

                if (isDodge)
                {
                    break;
                }
                else
                {
                    isDodge = true;
                }

                dodgeStartPos = this.transform.position;

                float dodgeDistance = swordDashMaxDistance;
                float dodgeAcceleration = Mathf.Pow(swordDashSpeed, 2) / (2 * dodgeDistance);

                rb.AddForce(this.transform.forward * dodgeAcceleration, ForceMode.Acceleration);

                break;

            case state.knockback:
                if (!didKnockback)
                {
                    Vector3 dirToKnockbackPartner = (knockbackPartner.position - this.transform.position).normalized;
                    dirToKnockbackPartner.y = 0;

                    rb.AddForce(Vector3.up * knockbackUpForce, ForceMode.Impulse);
                    rb.AddForce(-dirToKnockbackPartner * knockbackBackForce, ForceMode.Impulse);

                    lookDirection = EulerToLookRotation(Quaternion.LookRotation(dirToKnockbackPartner, Vector3.up).eulerAngles);

                    didKnockback = true;
                }
                break;
        }
        if (moveDirection != Vector3.zero)
        {
            lookDirection = moveDirection;
        }

        wasGrounded = isGrounded;
        wasDodge = isDodge;


        if (endDash)
        {
            endDash = false;
        }

        if(Vector3.Angle(groundHit.normal, Vector3.up) > maxGroundAngle)
        {
            endDash = true;
        }
    }

    public Vector3 EulerToLookRotation(Vector3 eulerAngles)
    {
        // Convert Euler angles to Quaternion
        Quaternion rotation = Quaternion.Euler(eulerAngles);

        // Extract yaw (rotation around the vertical axis, usually represented by the y-axis)
        float yaw = rotation.eulerAngles.y * Mathf.Deg2Rad;

        // Extract pitch (rotation around the lateral axis, usually represented by the x-axis)
        float pitch = rotation.eulerAngles.x * Mathf.Deg2Rad;

        // Calculate x and z components based on yaw and pitch
        float x = Mathf.Sin(yaw);
        float z = Mathf.Cos(yaw);

        // Apply pitch to adjust z component
        z *= Mathf.Cos(pitch);

        // Return the resulting lookRotation vector
        return new Vector3(x, 0, z);
    }

    private void OnCollisionStay(Collision collision)
    {
        if(groundLayerMask == (groundLayerMask | (1 << collision.gameObject.layer)))
        {
            RaycastHit hit;

            if(Physics.Raycast(this.transform.position, this.transform.forward, out hit, 0.6f))
            {
                endDash = true;
            }

            return;
        }

        endDash = true;
    }

    public void getMovementInput(InputAction.CallbackContext context)
    {
        inputDir.x = context.ReadValue<Vector2>().x;
        inputDir.y = context.ReadValue<Vector2>().y;
    }

    public void calculateDesiredMoveDirection()
    {
        float moveHorizontal = inputDir.x;
        float moveVertical = inputDir.y;

        Vector3 camForward = camera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 desiredMoveDirection = (camForward * moveVertical) + (camera.transform.right * moveHorizontal);
        moveDirection = desiredMoveDirection.normalized;
    }

    private float calculateRotationSpeedFactor()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);

        return normalizedSpeed;
    }

    private bool movePlayer()
    {
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
        else if(speedDifference < 0)
        {
            if (!isJumping)
            {
                goalSpeed = Mathf.Max(goalSpeed - (deceleration * decelMod) * Time.deltaTime, idealSpeed);
            }
        }

        if (goalSpeed > Mathf.Epsilon)
        {
            isMoving = true;

            Vector3 idealVel = transform.forward * goalSpeed;

            if (Vector3.Angle(groundHit.normal, Vector3.up) > maxGroundAngle)
            {
                Vector3 slopeDir = -getGroundSlope();
                slopeDir = Vector3.ProjectOnPlane(slopeDir, Vector3.up);

                float slopeAccelDot = Vector3.Dot(lookDirection.normalized, slopeDir.normalized);
                // modify idealVel here

                if (slopeAccelDot > Mathf.Epsilon)
                {
                    // Calculate a perpendicular direction to the slope
                    Vector3 perpDirection = Vector3.Cross(slopeDir, Vector3.up).normalized;

                    // Project the ideal velocity onto the perpendicular direction to move along the slope
                    idealVel = Vector3.Project(idealVel, perpDirection);
                }
            }

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

        return isMoving;
    }

    private void rotatePlayerSpeedBased()
    {
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        float turnSpeedFactor = 5;
        if (Vector3.Angle(groundHit.normal, Vector3.up) <= maxGroundAngle)
        {
            turnSpeedFactor = rotationSpeedByMoveSpeed.Evaluate(calculateRotationSpeedFactor());
        }

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

            if(groundHit.rigidbody != null)
            {
                Vector3 groundVel = groundHit.rigidbody.linearVelocity;
                groundVel.y = 0;


                Vector3 platformAngularVelocity = groundHit.rigidbody.angularVelocity;

                // Calculate player's position relative to rotation axis (assuming axis is centered)
                Vector3 relativePosition = transform.position - groundHit.rigidbody.position;

                // Calculate tangential velocity based on angular velocity and relative position
                Vector3 tangentialVelocity = Vector3.Cross(platformAngularVelocity, relativePosition);

                // Add tangential velocity to player's velocity, scaled by a factor
                rb.linearVelocity += tangentialVelocity + groundVel;

                groundHit.rigidbody.AddForceAtPosition(-(upForce / 2) + dampingForce, groundHit.point);
            }
        }
    }

    public void triggerJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            lastJumpInputTime = Time.time;
        }
        else if (context.canceled)
        {
            if (isJumping)
            {
                cancelJump = true;
            }
        }
    }

    public void doJump()
    {
        if (!(isGrounded || Time.time - lastGroundTime <= cyoteTime) || isJumping)
        {
            return;
        }

        isGrounded = false;
        isJumping = true;

        float jumpForce = calculateJumpForce();

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private float calculateJumpForce()
    {
        float initialVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * targetJumpHeight);
        return initialVelocity * rb.mass;
    }

    public void triggerDodge(InputAction.CallbackContext context)
    {
        if (context.performed && Vector3.Angle(groundHit.normal, Vector3.up) <= maxGroundAngle)
        {
            if (!isDodge && (isGrounded || allowInAirDodge) && !isSwordDash)
            {
                if(dodgeCooldown <= Time.time - lastDodgeTime)
                {
                    currentState = state.dodge;
                }
            }
        }
    }


    private bool CheckGrounded(out RaycastHit groundHit)
    {
        RaycastHit hit;

        Vector3 origin = transform.position;// + Vector3.up* groundCheckRadius;
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

    private Vector3 getGroundSlope()
    {
        Vector3 gravityDirection = Physics.gravity.normalized;
        Vector3 slopeDirection = Vector3.ProjectOnPlane(gravityDirection, groundHit.normal).normalized;

        return slopeDirection;
    }

    private void updateGravityMultiplier(float multiplier)
    {
        rb.AddForce(Physics.gravity * multiplier, ForceMode.Acceleration);
    }

    public float getLastDodgeTime()
    {
        return lastDodgeTime;
    }

    public void setKnockbackPartner(Transform transform)
    {
        knockbackPartner = transform;
    }
}
