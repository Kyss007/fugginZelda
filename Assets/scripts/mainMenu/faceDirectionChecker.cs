using UnityEngine;

public class faceDirectionChecker : MonoBehaviour
{
    public Transform target;

    public enum FaceDirection { Front, Left, Right, Up, Down }
    public FaceDirection currentDirection;

    public GameObject frontMenu;
    public GameObject leftMenu;
    public GameObject rightMenu;
    public GameObject upMenu;
    public GameObject downMenu;

    void Update()
    {
        if (target == null) return;

        Vector3 toTarget = (target.position - transform.position).normalized;

        float frontDot = Vector3.Dot(transform.forward, toTarget);
        float rightDot = Vector3.Dot(transform.right, toTarget);
        float upDot = Vector3.Dot(transform.up, toTarget);

        float absFront = Mathf.Abs(frontDot);
        float absRight = Mathf.Abs(rightDot);
        float absUp = Mathf.Abs(upDot);

        if (absFront >= absRight && absFront >= absUp)
        {
            currentDirection = FaceDirection.Front;
        }
        else if (absRight >= absFront && absRight >= absUp)
        {
            currentDirection = (rightDot > 0) ? FaceDirection.Right : FaceDirection.Left;
        }
        else
        {
            currentDirection = (upDot > 0) ? FaceDirection.Up : FaceDirection.Down;
        }

        HandleDirection(currentDirection);
    }

    void HandleDirection(FaceDirection dir)
    {
        if (frontMenu != null) frontMenu.SetActive(dir == FaceDirection.Front);
        if (leftMenu != null) leftMenu.SetActive(dir == FaceDirection.Left);
        if (rightMenu != null) rightMenu.SetActive(dir == FaceDirection.Right);
        if (upMenu != null) upMenu.SetActive(dir == FaceDirection.Up);
        if (downMenu != null) downMenu.SetActive(dir == FaceDirection.Down);
    }
}
