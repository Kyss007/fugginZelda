using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UI;

public class settingsManager : MonoBehaviour
{
    public float mouseSensitivity = 50;
    public float controllerSensitivity = 50;

    [Space]
    public InputActionReference lookInput;
    public Slider mouseSensitivitySlider;
    public Slider controllerSensitivitySlider;

    private float defaultCamXGain = 1;
    private float defaultCamYGain = -0.01f;

    private void Start()
    {
        mouseSensitivitySlider.onValueChanged.AddListener(updateMouseSensitivity);
        controllerSensitivitySlider.onValueChanged.AddListener(updateControllerSensitivity);
    }

    public void updateMouseSensitivity(float value)
    {
        mouseSensitivity = value;

        lookInput.action.ApplyParameterOverride("scaleVector2:x", value, new InputBinding { groups = "keyboard" });
        lookInput.action.ApplyParameterOverride("scaleVector2:y", value, new InputBinding { groups = "keyboard" });
    }
    
    public void updateControllerSensitivity(float value)
    {
        mouseSensitivity = value;

        lookInput.action.ApplyParameterOverride("scaleVector2:x", value * 5, new InputBinding { groups = "gamepad"});
        lookInput.action.ApplyParameterOverride("scaleVector2:y", value * 5, new InputBinding { groups = "gamepad"});
    }
}
