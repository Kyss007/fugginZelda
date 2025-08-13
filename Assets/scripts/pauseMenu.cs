using UnityEngine;
using UnityEngine.InputSystem;

public class pauseMenu : MonoBehaviour
{
    public InputActionReference pauseAction;

    public bool isPaused = false;

    private void Update()
    {
        if (!isPaused)
        {
            if (pauseAction.action.triggered)
            {
                Time.timeScale = 0;
                isPaused = true;
            }
        }
        else
        {
            if (pauseAction.action.triggered)
            {
                Time.timeScale = 1;
                isPaused = false;
            }
        }
    }
}
