using UnityEngine;

public class lookAt : MonoBehaviour
{
    public bool atCamera = false;
    public Transform targetTransform;
    public Camera cam;
    
    [Header("Rotation Options")]
    public bool matchTargetRotation = false; // New: Syncs rotation instead of looking at position
    
    [Space]
    public bool addSpin = false;
    public float spinSpeed = 90f;
    public enum Axis { X, Y, Z }
    public Axis spinAxis = Axis.Y;

    private float spinAngle = 0f;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void LateUpdate()
    {
        if (atCamera && cam != null)
        {
            targetTransform = cam.transform;
        }

        if (targetTransform == null)
            return;

        Quaternion finalRotation;

        if (matchTargetRotation)
        {
            // Simply copy the rotation of the target
            finalRotation = targetTransform.rotation;
        }
        else
        {
            // Calculate the rotation to face the target's position
            Vector3 direction = (targetTransform.position - transform.position).normalized;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            finalRotation = Quaternion.LookRotation(direction);
        }

        if (addSpin)
        {
            spinAngle += spinSpeed * Time.deltaTime;

            Vector3 axis = Vector3.up;
            switch (spinAxis)
            {
                case Axis.X: axis = Vector3.right; break;
                case Axis.Y: axis = Vector3.up; break;
                case Axis.Z: axis = Vector3.forward; break;
            }

            Quaternion spinRotation = Quaternion.AngleAxis(spinAngle, axis);
            transform.rotation = finalRotation * spinRotation;
        }
        else
        {
            transform.rotation = finalRotation;
        }
    }
}