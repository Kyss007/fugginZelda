using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class spikeBall : MonoBehaviour
{
    public GameObject line;

    [Header("Settings")]
    public float velocityThreshold = 5f;

    private Rigidbody rb;
    private Animator animator;

    public bool onSword = false;

    private spikeBallTarget spikeBallTarget;

    public SpringJoint springJoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        springJoint = GetComponent<SpringJoint>();
    }

    private void Start()
    {
        spikeBallTarget = FindFirstObjectByType<spikeBallTarget>();
    }

    void FixedUpdate()
    {
        float speed = rb.linearVelocity.magnitude;
        //print(speed);

        if (speed >= velocityThreshold && !onSword)
        {
            animator.SetTrigger("spikeUp");
        }
    }

    public void reciveHit(Vector3 dir, string attackName)
    {
        if(onSword)
            return;

        if(attackName == "frontStab")
        {
            line.SetActive(false);

            Destroy(springJoint);
            rb.detectCollisions = false;
            rb.isKinematic = true;
            transform.parent = spikeBallTarget.transform;
            transform.localPosition = Vector3.zero;

            onSword = true;
        }
    }
}
