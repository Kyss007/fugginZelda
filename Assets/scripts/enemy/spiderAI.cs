using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpiderAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 12f;
    public float deceleration = 18f;
    public float wanderSpeed = 3f;
    public float chaseSpeed = 8f;
    public float retreatSpeed = 10f;
    public float turnSpeed = 120f;

    [Header("Detection Settings")]
    public float detectionRadius = 15f;
    public float attackDistance = 3f;
    public float retreatDistance = 6f;
    public LayerMask enemyLayer;

    [Header("Behavior Settings")]
    public float avoidanceRadius = 3f;
    public float avoidanceStrength = 5f;
    public float wanderChangeInterval = 3f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 4f;
    public float waitTimeAfterRetreat = 1.5f;
    public float circleRadius = 5f;
    public float circleSpeed = 2f;

    public bool hasBall = true;
    public float waitTimeForNewBall = 15;
    public Transform ballSpot;
    public GameObject ballPrefab;
    public Transform body;

    private Rigidbody rb;
    private float currentSpeed;
    private Transform player;
    
    private enum State { Wandering, Chasing, Retreating, Waiting, Circling, WanderingWait }
    private State currentState = State.Wandering;
    
    private Vector3 wanderDirection;
    private float wanderTimer;
    private float waitTimer;
    private float circleAngle;

    private float ballRespawnTimer;


   void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        wanderDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        wanderTimer = wanderChangeInterval;

        ballRespawnTimer = waitTimeForNewBall;
    }

    void FixedUpdate()
    {
        HandleBallLogic();
        DetectPlayer();
        UpdateState();

        Vector3 desiredDirection = GetDesiredDirection();
        Vector3 avoidanceDirection = GetAvoidanceDirection();

        Vector3 finalDirection;
        if (desiredDirection.magnitude > 0.01f)
            finalDirection = (desiredDirection + avoidanceDirection).normalized;
        else
            finalDirection = avoidanceDirection.normalized;

        ApplyMovement(finalDirection);
    }


    void DetectPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void UpdateState()
    {
        if (!hasBall)
        {
            currentState = State.Retreating;
        }

        if (player == null)
        {
            // Even without a player, handle wandering wait state
            if (currentState == State.WanderingWait)
            {
                waitTimer -= Time.fixedDeltaTime;
                if (waitTimer <= 0f)
                {
                    currentState = State.Wandering;
                    wanderDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                    wanderTimer = wanderChangeInterval;
                }
            }
            else if (currentState != State.Wandering)
            {
                currentState = State.Wandering;
            }
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Wandering:
                if (distanceToPlayer <= detectionRadius)
                {
                    currentState = State.Chasing;
                }
                break;

            case State.WanderingWait:
                waitTimer -= Time.fixedDeltaTime;
                if (waitTimer <= 0f)
                {
                    currentState = State.Wandering;
                    wanderDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                    wanderTimer = wanderChangeInterval;
                }
                
                // Can still detect player while waiting
                if (distanceToPlayer <= detectionRadius)
                {
                    currentState = State.Chasing;
                }
                break;

            case State.Chasing:
                if (distanceToPlayer <= attackDistance)
                {
                    currentState = State.Retreating;
                }
                else if (distanceToPlayer > detectionRadius * 1.5f)
                {
                    // Go to wandering wait instead of directly to wandering
                    currentState = State.WanderingWait;
                    waitTimer = Random.Range(minWaitTime, maxWaitTime);
                }
                break;

            case State.Retreating:
                if (distanceToPlayer >= retreatDistance)
                {
                    currentState = State.Waiting;
                    waitTimer = waitTimeAfterRetreat;
                    circleAngle = Random.Range(0f, 360f);
                }
                break;

            case State.Waiting:
                waitTimer -= Time.fixedDeltaTime;
                if (waitTimer <= 0f)
                {
                    currentState = State.Circling;
                }
                break;

            case State.Circling:
                // Circle for a bit, then chase again
                if (Random.value < 0.02f) // Small chance each frame to attack again
                {
                    currentState = State.Chasing;
                }
                break;
        }
    }

    Vector3 GetDesiredDirection()
    {
        switch (currentState)
        {
            case State.Wandering:
                wanderTimer -= Time.fixedDeltaTime;
                if (wanderTimer <= 0f)
                {
                    // Switch to waiting state instead of immediately changing direction
                    currentState = State.WanderingWait;
                    waitTimer = Random.Range(minWaitTime, maxWaitTime);
                    return Vector3.zero;
                }
                return wanderDirection;

            case State.WanderingWait:
                return Vector3.zero;

            case State.Chasing:
                return (player.position - transform.position).normalized;

            case State.Retreating:
                return (transform.position - player.position).normalized;

            case State.Waiting:
                return Vector3.zero;

            case State.Circling:
                // Circle around the player
                circleAngle += circleSpeed * Time.fixedDeltaTime * 50f;
                Vector3 offset = new Vector3(
                    Mathf.Cos(circleAngle * Mathf.Deg2Rad),
                    0,
                    Mathf.Sin(circleAngle * Mathf.Deg2Rad)
                ) * circleRadius;
                Vector3 targetPosition = player.position + offset;
                return (targetPosition - transform.position).normalized;

            default:
                return Vector3.zero;
        }
    }

    Vector3 GetAvoidanceDirection()
    {
        Vector3 avoidance = Vector3.zero;
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, avoidanceRadius, enemyLayer);

        int avoidanceCount = 0;
        
        foreach (Collider other in nearbyColliders)
        {
            // Check if the other object has a SpiderAI component
            if(other.transform.parent != null)
            {
                other.transform.parent.TryGetComponent<SpiderAI>(out SpiderAI otherSpider);
                
                if (otherSpider != null && otherSpider != this)
                {
                    Vector3 awayFromOther = transform.position - other.transform.position;
                    float distance = awayFromOther.magnitude;
                    
                    if (distance > 0.01f && distance < avoidanceRadius)
                    {
                        // Stronger avoidance when closer
                        float avoidanceFactor = 1f - (distance / avoidanceRadius);
                        avoidance += awayFromOther.normalized * avoidanceFactor;
                        avoidanceCount++;
                    }
                }
            }
        }

        // Average and apply strength
        if (avoidanceCount > 0)
        {
            avoidance = avoidance.normalized * avoidanceStrength;
        }

        return avoidance;
    }

    void ApplyMovement(Vector3 direction)
    {
        float targetSpeed = GetTargetSpeed();
        
        // If we have avoidance force but no desired direction, still allow movement
        float moveInput = direction.magnitude > 0.1f ? 1f : 0f;

        // -------- SPEED --------
        if (moveInput != 0f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        // -------- VELOCITY --------
        Vector3 targetVelocity = direction * currentSpeed;
        rb.linearVelocity = new Vector3(
            targetVelocity.x,
            rb.linearVelocity.y,
            targetVelocity.z
        );

        // -------- ROTATION --------
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            );
            rb.MoveRotation(newRotation);
        }
    }

    float GetTargetSpeed()
    {
        switch (currentState)
        {
            case State.Wandering:
                return wanderSpeed;
            case State.WanderingWait:
                return wanderSpeed * 0.5f; // Allow some movement for avoidance during wait
            case State.Chasing:
                return chaseSpeed;
            case State.Retreating:
                return retreatSpeed;
            case State.Waiting:
                return wanderSpeed * 0.5f; // Allow some movement for avoidance during wait
            case State.Circling:
                return wanderSpeed * 1.5f;
            default:
                return wanderSpeed;
        }
    }

    void HandleBallLogic()
    {
        hasBall = ballSpot.childCount > 0;

        if (!hasBall) 
        {
            // Force retreat while ball is missing
            currentState = State.Retreating;

            ballRespawnTimer -= Time.fixedDeltaTime;
            if (ballRespawnTimer <= 0f)
            {
                SpawnBall();
            }
        }
        else
        {
            ballRespawnTimer = waitTimeForNewBall;
        }
    }

    void SpawnBall()
    {
        GameObject ball = Instantiate(ballPrefab, ballSpot.position, ballSpot.rotation);
        ball.transform.SetParent(ballSpot);
        ball.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        hasBall = true;
        ballRespawnTimer = waitTimeForNewBall;

        ball.GetComponent<SpringJoint>().connectedBody = rb;
        ball.GetComponentInChildren<segmentedLineRenderer>().source = body;
    }


    // Visualize detection and avoidance radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}