using UnityEngine;
using UnityEngine.InputSystem;

public class mainMenuInputThing : MonoBehaviour
{
    public menuCubeController menuCubeController;
    public InputActionReference upAction;
    public InputActionReference downAction;
    public InputActionReference leftAction;
    public InputActionReference rightAction;
    public InputActionReference backAction;
    public InputActionReference acceptAction;

    private faceDirectionChecker faceDirectionChecker;

    private float inputTimer = 0f;
    private float inputDelay = 0.1f;

    private void Start()
    {
        faceDirectionChecker = GetComponent<faceDirectionChecker>();
    }

    void Update()
    {
        if (inputTimer > 0f)
        {
            inputTimer -= Time.unscaledDeltaTime;
            return;
        }

        switch (faceDirectionChecker.currentDirection)
        {
            case faceDirectionChecker.FaceDirection.Front:
                if (upAction.action.triggered)
                {
                    menuCubeController.rotUp();
                    inputTimer = inputDelay;
                }

                if (downAction.action.triggered)
                {
                    menuCubeController.rotDown();
                    inputTimer = inputDelay;
                }

                if (leftAction.action.triggered)
                {
                    menuCubeController.rotLeft();
                    inputTimer = inputDelay;
                }

                if (rightAction.action.triggered)
                {
                    menuCubeController.rotRight();
                    inputTimer = inputDelay;
                }
                break;

            case faceDirectionChecker.FaceDirection.Up:
                if (downAction.action.triggered || backAction.action.triggered)
                {
                    menuCubeController.rotDown();
                    inputTimer = inputDelay;
                }

                if (acceptAction.action.triggered)
                {
                    GetComponent<changeScene>().loadNextScene();
                }
                break;

            case faceDirectionChecker.FaceDirection.Down:
                if (upAction.action.triggered || backAction.action.triggered)
                {
                    menuCubeController.rotUp();
                    inputTimer = inputDelay;
                }
                break;

            case faceDirectionChecker.FaceDirection.Left:
                if (rightAction.action.triggered || backAction.action.triggered)
                {
                    menuCubeController.rotRight();
                    inputTimer = inputDelay;
                }
                break;

            case faceDirectionChecker.FaceDirection.Right:
                if (leftAction.action.triggered || backAction.action.triggered)
                {
                    menuCubeController.rotLeft();
                    inputTimer = inputDelay;
                }
                break;
        }
    }
}
