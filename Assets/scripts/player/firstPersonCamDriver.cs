using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class FirstPersonCameraDriver : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot;
    public thirdPersonMovementDriver movementDriver;
    public PhysicalWalk.DampedSpringMotionCopier springMotionCopier;
    public cameraController cameraController;

    [Header("Settings")]
    public float sensitivity = 30f;
    public float minPitch = -90f;
    public float maxPitch = 90f;

    public UnityEvent diable;
    public UnityEvent enable;

    private float pitch;
    private float yaw;
    private Vector2 currentLookInput;

    private void Start()
    {
        Vector3 pivotEuler = cameraPivot.localEulerAngles;
        if (pivotEuler.x > 180f) pivotEuler.x -= 360f;
        pitch = pivotEuler.x;

        if (movementDriver != null)
        {
            Vector3 forward = movementDriver.transform.forward;
            yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        }

        if (springMotionCopier == null)
        {
            springMotionCopier = GetComponent<PhysicalWalk.DampedSpringMotionCopier>();
        }
    }

    private void LateUpdate()
    {
        if (Cursor.lockState != CursorLockMode.Locked || !gameObject.activeSelf)
            return;

        Vector2 delta = currentLookInput * sensitivity * Time.deltaTime;

        pitch -= delta.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Vector3 pivotEuler = cameraPivot.localEulerAngles;
        if (pivotEuler.x > 180f) pivotEuler.x -= 360f;

        pivotEuler.x = pitch;               
        cameraPivot.localEulerAngles = pivotEuler;

        yaw += delta.x;

        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 lookDir = yawRotation * Vector3.forward;

        if (movementDriver != null)
        {
            movementDriver.setLookInput(new Vector2(lookDir.x, lookDir.z));
        }
    }

    public void takeInput(InputAction.CallbackContext callbackContext)
    {
        currentLookInput = callbackContext.ReadValue<Vector2>();
    }

    private void OnDisable()
    {
        diable.Invoke();

        if (movementDriver != null)
            movementDriver.lookRelativeInput = false;
        
        cameraController.resetCameraPosition();
    }

    private void OnEnable()
    {
        pitch = 0f;
        currentLookInput = Vector2.zero;

        if (movementDriver != null)
        {
            Vector3 forward = movementDriver.transform.forward;
            yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, yaw, 0);
            
            Vector3 pivotEuler = cameraPivot.localEulerAngles;
            pivotEuler.x = 0f;
            cameraPivot.localEulerAngles = pivotEuler;
        }

        if (springMotionCopier != null)
        {
            springMotionCopier.Reset();
        }

        enable.Invoke();

        if (movementDriver != null)
            movementDriver.lookRelativeInput = true;
    }
}