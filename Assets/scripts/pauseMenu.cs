using System.Collections;
using PhysicalWalk;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class pauseMenu : MonoBehaviour
{
    public InputActionReference pauseAction;
    public PlayerInput playerInput;

    public randomRotator randomRotator;
    public DampedSpringMotionCopier pauseSpring;
    public pauseMenuSpwanAnimat spawnAnim;

    public faceDirectionChecker faceDirectionChecker;

    public bool isPaused = false;

    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPaused)
        {
            if (pauseAction.action.triggered)
            {
                Time.timeScale = 0;
                isPaused = true;
                Cursor.lockState = CursorLockMode.None;

                playerInput.enabled = false;

                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }

                randomRotator.setDefaultRot(Quaternion.Euler(0, 180, 0));

                pauseSpring.rotationalSpring.lastRotation = Quaternion.identity;
                pauseSpring.rotationalSpring.lastSourceRotation = Quaternion.identity;
                pauseSpring.rotationalSpring.springVelocity = Vector3.zero;
            }
        }
        else
        {
            if (pauseAction.action.triggered && faceDirectionChecker.currentDirection == faceDirectionChecker.FaceDirection.Front)
            {
                Time.timeScale = 1;
                isPaused = false;
                Cursor.lockState = CursorLockMode.Locked;

                playerInput.enabled = true;
                spawnAnim.triggerDissapear();
                foreach (Transform child in transform)
                {
                    //child.gameObject.SetActive(false);
                }
            }
        }
    }
}
