using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class triangleEnemy : MonoBehaviour
{
    public LayerMask groundLayer;

    public float knockbackForce = 15f;

    [Header("jump settings")]
    public float minJumpForce = 5f;
    public float maxJumpForce = 8f;
    public float jumpCooldownMin = 0.5f;
    public float jumpCooldownMax = 1.5f;
    public float jumpDistance = 4f;

    [Header("player detection")]
    public float playerDetectionRange = 10f;
    public float rotationSpeed = 5f;
    public float groundCheckDistance = 0.2f;

    private Transform player;
    private Rigidbody rb;
    private Vector3 nextDirection;
    private bool isGrounded;
    private bool isJumping = false;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        pickNextDirection();

        player = FindFirstObjectByType<keanusCharacterController>().transform;
    }

    public void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundLayer);

        Vector3 flatDir = new Vector3(nextDirection.x, 0f, nextDirection.z);
        if (flatDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        if (isGrounded && !isJumping)
        {
            StartCoroutine(jumpRoutine());
        }
    }

    public IEnumerator jumpRoutine()
    {
        isJumping = true;

        float waitTime = Random.Range(jumpCooldownMin, jumpCooldownMax);
        yield return new WaitForSeconds(waitTime);

        Vector3 targetDir = getDirection();
        yield return new WaitForSeconds(0.3f);

        float jumpForce = Random.Range(minJumpForce, maxJumpForce);
        Vector3 jumpVelocity = (targetDir * jumpDistance) + (Vector3.up * jumpForce);
        rb.linearVelocity = jumpVelocity;

        yield return new WaitForSeconds(0.1f);
        isJumping = false;
    }

    public Vector3 getDirection()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer < playerDetectionRange)
        {
            nextDirection = (player.position - transform.position).normalized;
        }
        else
        {
            pickNextDirection();
        }
        return nextDirection;
    }

    public void pickNextDirection()
    {
        Vector2 random = Random.insideUnitCircle.normalized;
        nextDirection = new Vector3(random.x, 0f, random.y);
    }

    public void doKnockback(Vector3 direction)
    {
        direction.Normalize();

        rb.linearVelocity = Vector3.zero;
        rb.AddForce((direction + Vector3.up) * knockbackForce, ForceMode.Impulse);
    }

    public void triggerHit(Vector3 direction, string damageName)
    {
        switch (damageName)
        {
            case "lightSwingLeft":
                print("left + kill");
                break;
            case "lightSwingRight":
                print("right + knockbak");
                doKnockback(direction);
                break;
        }
    }
}
