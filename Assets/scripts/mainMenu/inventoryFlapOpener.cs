using UnityEngine;
using System.Collections;

public class inventoryFlapOpener : MonoBehaviour
{
    public Transform flap;

    public enum RotationAxis { X, Y, Z }
    public RotationAxis axis = RotationAxis.X;

    public enum RotationDirection { Positive, Negative }
    public RotationDirection direction = RotationDirection.Positive;

    [Header("Rotation Settings")]
    public float openAngle = 90f;
    public float rotationSpeed = 180f; // degrees per second

    private float currentAngle;
    private Coroutine rotationRoutine;

    void Awake()
    {
        if (!flap)
            flap = transform;

        currentAngle = 0f;
    }


    public void openFlap()
    {
        float target = openAngle * (direction == RotationDirection.Positive ? 1f : -1f);
        StartRotation(target);
    }

    public void closeFlap()
    {
        StartRotation(0f);
    }

    private void StartRotation(float targetAngle)
    {
        if (rotationRoutine != null)
            StopCoroutine(rotationRoutine);

        rotationRoutine = StartCoroutine(RotateTo(targetAngle));
    }

    private IEnumerator RotateTo(float targetAngle)
    {
        while (!Mathf.Approximately(currentAngle, targetAngle))
        {
            currentAngle = Mathf.MoveTowards(
                currentAngle,
                targetAngle,
                rotationSpeed * Time.unscaledDeltaTime
            );

            ApplyRotation(currentAngle);
            yield return null;
        }

        ApplyRotation(targetAngle);
        rotationRoutine = null;
    }

    private void ApplyRotation(float angle)
    {
        Vector3 euler = Vector3.zero;

        switch (axis)
        {
            case RotationAxis.X: euler.x = angle; break;
            case RotationAxis.Y: euler.y = angle; break;
            case RotationAxis.Z: euler.z = angle; break;
        }

        flap.localRotation = Quaternion.Euler(euler);
    }
}
