using UnityEngine;

public class spikeBall : MonoBehaviour
{
    [Header("Settings")]
    public float velocityThreshold = 5f;

    private Rigidbody rb;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        float speed = rb.linearVelocity.magnitude;
        //print(speed);

        if (speed >= velocityThreshold)
        {
            animator.SetTrigger("spikeUp");
        }
    }
}
