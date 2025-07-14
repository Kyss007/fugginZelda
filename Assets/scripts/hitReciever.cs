using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class hitReciever : MonoBehaviour
{
    public UnityEvent<Vector3, string> onHit;

    [HideInInspector]
    public Collider collider;

    [HideInInspector]
    public healthProvider healthProvider;

    private void Awake()
    {
        collider = GetComponent<Collider>();

        healthProvider = GetComponentInParent<healthProvider>();
    }

    //for debug
    [ContextMenu("trigger onHit")]
    public void triggerOnHit(Vector3 direction, string input = "null")
    {
        onHit.Invoke(direction, input);
    }
}
