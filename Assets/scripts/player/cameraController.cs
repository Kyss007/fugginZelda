using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public bool isTargeting = false;

    private CinemachineOrbitalFollow cmOrbitFollow;

    private keanusCharacterController characterController;

    private kccIinputDriver inputDriver;

    private void Awake()
    {
        cmOrbitFollow = GetComponent<CinemachineOrbitalFollow>();
    }

    void Start()
    {
        characterController = FindFirstObjectByType<keanusCharacterController>();
        inputDriver = characterController.inputDriver;
    }

    void Update()
    {
        isTargeting = inputDriver.getTargetInput();

        cmOrbitFollow.HorizontalAxis.Recentering.Enabled = isTargeting;
        cmOrbitFollow.VerticalAxis.Recentering.Enabled = isTargeting;
    }
}
