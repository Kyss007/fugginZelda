using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debugDodgeAnimation : MonoBehaviour
{
    public characterController characterController;
    public CapsuleCollider[] capsuleColliders;

    public bool lastDodgeState;

    private void Start()
    {
        characterController = GetComponentInParent<characterController>();

        lastDodgeState = characterController.isDodge;
    }

    private void Update()
    {
        if(lastDodgeState != characterController.isDodge)
        {
            if (characterController.isDodge)
            {
                Vector3 finalScale = this.transform.localScale;
                finalScale.y = finalScale.y / 2;
                this.transform.localScale = finalScale;

                characterController.GetComponent<CapsuleCollider>().height = 1;

                characterController.rideHeight = characterController.rideHeight / 1.5f;


                foreach(CapsuleCollider collider in capsuleColliders)
                {
                    collider.height = 1; 
                }
            }
            else
            {
                Vector3 finalScale = this.transform.localScale;
                finalScale.y = finalScale.y * 2;
                this.transform.localScale = finalScale;

                characterController.GetComponent<CapsuleCollider>().height = 2;

                characterController.rideHeight = characterController.rideHeight * 1.5f;

                foreach (CapsuleCollider collider in capsuleColliders)
                {
                    collider.height = 2;
                }
            }
        }

        lastDodgeState = characterController.isDodge;
    }
}
