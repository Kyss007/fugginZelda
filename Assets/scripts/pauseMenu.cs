using UnityEngine;
using UnityEngine.InputSystem;

public class pauseMenu : MonoBehaviour
{
    public InputActionReference pauseAction;
    public PlayerInput playerInput;

    public bool isPaused = false;

    private void Update()
    {
        if (!isPaused)
        {
            if (pauseAction.action.triggered)
            {
                Time.timeScale = 0;
                isPaused = true;

                playerInput.enabled = false;

                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (pauseAction.action.triggered)
            {
                Time.timeScale = 1;
                isPaused = false;

                playerInput.enabled = true;

                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}
