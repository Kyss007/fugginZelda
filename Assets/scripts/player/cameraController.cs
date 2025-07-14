using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public bool isTargeting = false;

    public bool canMoveCam = true;

    public GameObject cameraTrackingTarget;
    public GameObject cameraLookAtTarget;

    public float targetSideAngle = -15f;
    public float targetSideAngleSpeed = 10;
    public float targetRotSpeed = 5;
    public float targetPlayerDistancePercent = 0.25f;

    private CinemachineOrbitalFollow cmOrbitFollow;
    private CinemachineCamera cm;
    private CinemachineInputAxisController cmInputAxis;

    private keanusCharacterController cc;

    private kccIinputDriver inputDriver;
    private kccIMovementDriver movementDriver;
    private targetController targetController;

    private float ogFov;

    private Vector3 currentTargetTrackingOffset;

    private void Awake()
    {
        cmOrbitFollow = GetComponent<CinemachineOrbitalFollow>();
        cm = GetComponent<CinemachineCamera>();
        cmInputAxis = GetComponent<CinemachineInputAxisController>();

        ogFov = cm.Lens.FieldOfView;
    }

    void Start()
    {
        cc = FindFirstObjectByType<keanusCharacterController>();
        inputDriver = cc.inputDriver;
        movementDriver = cc.currentMovementDriver;
        targetController = cc.GetComponentInChildren<targetController>();
    }

    void Update()
    {
        isTargeting = inputDriver.getTargetInput();

        cmOrbitFollow.HorizontalAxis.Recentering.Enabled = isTargeting;
        cmOrbitFollow.VerticalAxis.Recentering.Enabled = isTargeting;

        cmInputAxis.Controllers[0].Enabled = !isTargeting && canMoveCam;
        cmInputAxis.Controllers[1].Enabled = !isTargeting && canMoveCam;

        thirdPersonMovementDriver thirdPersonMovementDriver = (thirdPersonMovementDriver)movementDriver;
        thirdPersonMovementDriver.lookRelativeInput = isTargeting;

        cm.Lens.FieldOfView = Mathf.Lerp(cm.Lens.FieldOfView, isTargeting ? (ogFov / 1.1f) : ogFov, 10 * Time.deltaTime);
    }
    
    private void LateUpdate()
    {
        //rotate camera target a bit when target selected for side view
        Quaternion targetRot = Quaternion.Euler(0, isTargeting && targetController.currentSellectedTarget != null ? targetSideAngle : 0, 0);
        cameraTrackingTarget.transform.localRotation = Quaternion.Slerp(cameraTrackingTarget.transform.localRotation, targetRot, targetSideAngleSpeed * Time.deltaTime);

        //move cam target between target and player
        Vector3 desiredOffset = isTargeting && targetController.currentSellectedTarget != null ? (targetController.currentSellectedTarget.transform.position - cc.transform.position) * targetPlayerDistancePercent : Vector3.zero;
        currentTargetTrackingOffset = Vector3.Lerp(currentTargetTrackingOffset, isTargeting && targetController.currentSellectedTarget != null ? desiredOffset : Vector3.zero, targetRotSpeed * Time.deltaTime);
        cameraLookAtTarget.transform.position = cc.transform.position + currentTargetTrackingOffset;
    }
}
