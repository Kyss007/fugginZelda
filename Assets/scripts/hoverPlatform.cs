using UnityEngine;

public class hoverPlatform : MonoBehaviour
{
    public float uprightForce = 10f;
    public float springConstant = 5f;
    public float dampingFactor = 0.5f;
    public float uprightDampingFactor = 0.5f;
    public float maxInitialForce = 20f;
    public float maxInitialTorque = 5f;
    [Space]
    public float turnForce = 2f;

    private Rigidbody rb;
    private Vector3 targetPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetPosition = transform.position;

        ApplyRandomInitialForce();
        ApplyRandomInitialTorque();
    }

    private void FixedUpdate()
    {
        Vector3 hoverForceVector = Vector3.up;

        Vector3 displacement = targetPosition - transform.position;
        Vector3 springForce = springConstant * displacement - dampingFactor * rb.linearVelocity;

        rb.AddForce(hoverForceVector, ForceMode.Acceleration);
        rb.AddForce(springForce, ForceMode.Acceleration);

        Vector3 uprightForces = Vector3.Cross(transform.up, Vector3.up) * uprightForce;
        Vector3 dampingUprightForces = -uprightDampingFactor * rb.angularVelocity;

        rb.AddTorque(uprightForces + dampingUprightForces, ForceMode.Acceleration);


        if(turnForce != 0)
        {
            rb.AddTorque(Vector3.up * turnForce, ForceMode.Acceleration);
        }
    }

    private void ApplyRandomInitialForce()
    {
        Vector3 randomForce = new Vector3(Random.Range(-maxInitialForce, maxInitialForce),
                                          Random.Range(-maxInitialForce, maxInitialForce),
                                          Random.Range(-maxInitialForce, maxInitialForce));
        rb.AddForce(randomForce, ForceMode.Impulse);
    }

    private void ApplyRandomInitialTorque()
    {
        Vector3 randomTorque = new Vector3(Random.Range(-maxInitialTorque, maxInitialTorque),
                                           Random.Range(-maxInitialTorque, maxInitialTorque),
                                           Random.Range(-maxInitialTorque, maxInitialTorque));
        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }
}
