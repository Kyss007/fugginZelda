using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class hitReciever : MonoBehaviour
{
    public UnityEvent onHit;

    [HideInInspector]
    public Collider collider;

    [HideInInspector]
    public healthProvider healthProvider;

    private void Awake()
    {
        collider = GetComponent<Collider>();

        healthProvider = GetComponentInParent<healthProvider>();
    }
}
