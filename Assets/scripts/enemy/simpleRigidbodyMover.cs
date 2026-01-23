using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class simpleRigibodyMover : MonoBehaviour
{
    public float acceleration = 12f;
    public float deceleration = 18f;
    public float maxSpeed = 6f;
    public float turnSpeed = 120f;

    private Rigidbody rb;
    private float currentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        // -------- INPUT --------
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) moveInput = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveInput = -1f;

        float turnInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) turnInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) turnInput = 1f;

        // -------- SPEED --------
        if (moveInput != 0f)
        {
            currentSpeed += moveInput * acceleration * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                deceleration * Time.fixedDeltaTime
            );
        }

        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // -------- VELOCITY (NO FORCES) --------
        Vector3 forwardVelocity = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(
            forwardVelocity.x,
            rb.linearVelocity.y, // gravity / vertical motion bleibt physikalisch
            forwardVelocity.z
        );

        // -------- ANGULAR VELOCITY --------
        rb.angularVelocity = Vector3.up * turnInput * turnSpeed * Mathf.Deg2Rad;
    }
}
