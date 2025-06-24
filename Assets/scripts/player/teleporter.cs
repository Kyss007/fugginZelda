using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teleporter : MonoBehaviour
{
    public GameObject goalPos;

    private characterController characterController;

    private void Start()
    {
        characterController = FindObjectOfType<characterController>();
    }

    public void teleportPlayer()
    {
        characterController.rb.MovePosition(goalPos.transform.position);
        characterController.camera.transform.position = characterController.transform.position;
    }
}
