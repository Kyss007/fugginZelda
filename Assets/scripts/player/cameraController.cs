using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public bool isTargeting = false;

    private CinemachineOrbitalFollow cmOrbitFollow;
    private CinemachineCamera cm;

    private keanusCharacterController characterController;

    private kccIinputDriver inputDriver;
    private kccIMovementDriver movementDriver;

    private float ogFov;

    private void Awake()
    {
        cmOrbitFollow = GetComponent<CinemachineOrbitalFollow>();
        cm = GetComponent<CinemachineCamera>();

        ogFov = cm.Lens.FieldOfView;
    }

    void Start()
    {
        characterController = FindFirstObjectByType<keanusCharacterController>();
        inputDriver = characterController.inputDriver;
        movementDriver = characterController.currentMovementDriver;
    }

    void Update()
    {
        isTargeting = inputDriver.getTargetInput();

        cmOrbitFollow.HorizontalAxis.Recentering.Enabled = isTargeting;
        cmOrbitFollow.VerticalAxis.Recentering.Enabled = isTargeting;

        thirdPersonMovementDriver thirdPersonMovementDriver = (thirdPersonMovementDriver)movementDriver;
        thirdPersonMovementDriver.lookRelativeInput = isTargeting;

        cm.Lens.FieldOfView = Mathf.Lerp(cm.Lens.FieldOfView , isTargeting ? (ogFov / 1.1f) : ogFov, 10 * Time.deltaTime);
    }
}
