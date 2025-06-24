using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class orbitingMovement : MonoBehaviour
{
    public float healAmount = 2f;
    public float pickupWaitTime = 0.5f;
    public float pickupWaitTimeMaxDifference = 0.2f;
    [Space]
    public float topSpeed = 10f;
    public float acceleration = 1f;
    public float sineFrequency = 1f;
    public float sineAmplitude = 1f;
    [Space]
    public float spawnTopSpeed = 15f;
    public float spawnAcceleration = 2f;
    [Space]
    public float atPlayerDistance = 0.01f;


    private GameObject target;

    private float currentSpeed;
    private bool pickUppable = false;

    private Vector3 initialTarget;


    private Vector3 sineStartPos;
    private float sineOffset;

    private bool doesWait = false;

    private void Start()
    {
        initialTarget = transform.position;
        initialTarget += GetRandomPointInUpperHemisphere() * Random.Range(2.5f,5);

        sineOffset = Random.Range(0f, 2f * Mathf.PI);

        pickupWaitTime += Random.Range(-pickupWaitTimeMaxDifference, pickupWaitTimeMaxDifference);
    }

    private void Update()
    {
        if (!pickUppable)
        {
            moveToPosition(initialTarget, spawnTopSpeed, spawnAcceleration);

            if(transform.position - initialTarget == Vector3.zero * Mathf.Epsilon && !doesWait)
            {
                doesWait = true;
                StartCoroutine(setPickuppable());
            }
        }

        if(target == null && pickUppable)
        {
            float sineValue = Mathf.Sin((Time.time + sineOffset) * sineFrequency);

            Vector3 newPos = sineStartPos + Vector3.up * sineValue * sineAmplitude;

            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime);
        }

        if(target != null && pickUppable)
        {
            moveToPosition(target.transform.position, topSpeed, acceleration);

            if(Vector3.Distance(target.transform.position, this.transform.position) <= atPlayerDistance)
            {
                target.GetComponent<healthProvider>().heal(healAmount);
                Destroy(this.gameObject);
            }
        }
    }

    public IEnumerator setPickuppable()
    {
        yield return new WaitForSeconds(pickupWaitTime);

        pickUppable = true;

        sineStartPos = this.transform.position;
    }

    public void moveToPosition(Vector3 targetPos, float topSpeed, float acceleration)
    {
        transform.LookAt(targetPos, Vector3.up);

        currentSpeed = Mathf.Lerp(currentSpeed, topSpeed, acceleration * Time.deltaTime);

        transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = other.gameObject;
        }
    }

    Vector3 GetRandomPointInUpperHemisphere()
    {
        Vector3 point;
        do
        {
            point = Random.insideUnitSphere;
        } while (point.y < 0.2);
        return point;
    }
}
