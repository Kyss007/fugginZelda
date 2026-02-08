using UnityEngine;

public class lookAt : MonoBehaviour
{
    public bool atCamera = false;
    public Transform targetTransform;
    public Camera cam;

    [Header("Rotation Options")]
    public bool matchTargetRotation = false;

    [Header("Interpolation")]
    public bool interpolateRotation = false;
    public float rotationLerpSpeed = 10f;

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
            targetTransform = cam.transform;

        if (targetTransform == null)
            return;

        Quaternion finalRotation;

        if (matchTargetRotation)
        {
            finalRotation = targetTransform.rotation;
        }
        else
        {
            Vector3 direction;

            if (atCamera)
                direction = transform.position - targetTransform.position;
            else
                direction = targetTransform.position - transform.position;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            finalRotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        if (addSpin)
        {
            spinAngle += spinSpeed * Time.deltaTime;

            Vector3 axis = spinAxis switch
            {
                Axis.X => Vector3.right,
                Axis.Y => Vector3.up,
                Axis.Z => Vector3.forward,
                _ => Vector3.up
            };

            finalRotation *= Quaternion.AngleAxis(spinAngle, axis);
        }

        if (interpolateRotation)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                finalRotation,
                rotationLerpSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = finalRotation;
        }
    }
}
