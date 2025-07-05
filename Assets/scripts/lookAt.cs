using UnityEngine;

public class lookAt : MonoBehaviour
{
    public bool atCamera = false;

    public Transform targetTransform;


    public Camera cam;

    void Start()
    {
        cam = Camera.main;   
    }

    void LateUpdate()
    {
        if (atCamera)
        {
            targetTransform = cam.transform;
        }

        transform.LookAt(targetTransform);
    }
}
