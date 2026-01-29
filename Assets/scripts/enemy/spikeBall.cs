using UnityEngine;

public class spikeBall : MonoBehaviour
{
    public GameObject line;

    [Header("Settings")]
    public float velocityThreshold = 5f;

    public float[] distances = {0.15f, -0.4f, -0.7f};

    public int usesLeft = 2;

    private Rigidbody rb;
    private Animator animator;

    public bool onSword = false;
    public bool thrown = false;

    private spikeBallTarget spikeBallTarget;

    public SpringJoint springJoint;
    public targetController targetController;

    private throwableObject throwableObject;
    public LayerMask nobreakLayers;

    public GameObject breakEffectPrefab;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        springJoint = GetComponent<SpringJoint>();
        throwableObject = GetComponent<throwableObject>();
    }

    private void Start()
    {
        spikeBallTarget = FindFirstObjectByType<spikeBallTarget>(FindObjectsInactive.Include);
        targetController = spikeBallTarget.targetController;
    }

    private void Update()
    {
        if(onSword && !thrown)
        {
            transform.localPosition = new Vector3(0, distances[usesLeft], 0);
        }
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

        if(attackName == "frontStab" && spikeBallTarget.transform.childCount == 0)
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

    public void decreaseUsage()
    {
        usesLeft--;
        
        if(usesLeft < 0)
        {
            throwBall();
        }
    }

    public void throwBall()
    {
        rb.detectCollisions = true;
        rb.isKinematic = false;
        transform.parent = null;

        if (targetController.currentSellectedTarget != null)
        {
            throwableObject.doThrow(targetController.currentSellectedTarget.transform.position, true);
        }
        else
        {
            throwableObject.doThrow();
        }

        thrown = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(thrown && ((nobreakLayers.value & (1 << collision.gameObject.layer)) == 0))
        {
            Instantiate(breakEffectPrefab, this.transform.position, this.transform.rotation);
            Destroy(gameObject);
        }    
    }
}
