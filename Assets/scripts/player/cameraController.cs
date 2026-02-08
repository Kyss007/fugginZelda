using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public GameObject firstPersonCamObject;

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
        thirdPersonMovementDriver.lookRelativeInput = firstPersonCamObject.activeSelf ? thirdPersonMovementDriver.lookRelativeInput : isTargeting;

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

    public void changeView()
    {
        firstPersonCamObject.SetActive(!firstPersonCamObject.activeSelf);
    }

    public void changeViewInput(InputAction.CallbackContext callbackContext)
    {
        if(callbackContext.started)
            changeView();
    }

    public void resetCameraPosition()
    {
        Debug.Log("Resetting camera position");
        
        if (cc == null) return;
        
        // Get the camera's current world position and player position
        Vector3 camPos = cm.transform.position;
        Vector3 playerPos = cc.transform.position;
        Vector3 playerForward = cc.transform.forward;
        
        // Calculate the direction from player to camera (on horizontal plane)
        Vector3 camDirection = camPos - playerPos;
        camDirection.y = 0; // Flatten to horizontal plane
        
        // Calculate the direction that would be "behind" the player
        Vector3 behindPlayer = -playerForward;
        behindPlayer.y = 0;
        
        // Calculate the angle between current camera position and "behind player"
        float currentAngle = Mathf.Atan2(camDirection.x, camDirection.z) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(behindPlayer.x, behindPlayer.z) * Mathf.Rad2Deg;
        
        // Calculate the difference
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        
        // Apply to horizontal axis (add the difference to current value)
        cmOrbitFollow.HorizontalAxis.Value += angleDifference;
        
        // Reset vertical to default
        cmOrbitFollow.VerticalAxis.Value = 0.5f;
        
        // Cancel damping for instant snap
        cmOrbitFollow.HorizontalAxis.CancelRecentering();
        cmOrbitFollow.VerticalAxis.CancelRecentering();
        
        // Force update
        cm.InternalUpdateCameraState(Vector3.up, 0);
    }
}
