using System.Collections.Generic;
using UnityEngine;

public class dogingCollisionThingy : MonoBehaviour
{
    public List<CapsuleCollider> capsuleColliders = new List<CapsuleCollider>();
    public GameObject shape;

    public thirdPersonMovementDriver movementDriver;

    private List<float> ogCapsuleHeights = new List<float>();
    private float ogShapeHeight;
    private float ogRideHeight;

    void Start()
    {
        foreach(CapsuleCollider collider in capsuleColliders)
        {
            ogCapsuleHeights.Add(collider.height);
        }
        
        ogRideHeight = movementDriver.rideHeight;
        ogShapeHeight = shape.transform.localScale.y;
    }

    void Update()
    {
        if (movementDriver.isDodging)
        {
            foreach(CapsuleCollider collider in capsuleColliders)
            {
                collider.height = ogCapsuleHeights[capsuleColliders.IndexOf(collider)] / 2.5f;
            }
            
            shape.transform.localScale = new Vector3(ogShapeHeight, ogShapeHeight / 2.5f, ogShapeHeight);
            movementDriver.rideHeight = ogRideHeight / 2.5f;
        }
        else
        {
            foreach(CapsuleCollider collider in capsuleColliders)
            {
                collider.height = ogCapsuleHeights[capsuleColliders.IndexOf(collider)];
            }

            shape.transform.localScale = new Vector3(ogShapeHeight, ogShapeHeight, ogShapeHeight);
            movementDriver.rideHeight = ogRideHeight;
        }
    }
}
