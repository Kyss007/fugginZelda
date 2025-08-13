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
        frontMenu?.SetActive(false);
        leftMenu?.SetActive(false);
        rightMenu?.SetActive(false);
        upMenu?.SetActive(false);
        downMenu?.SetActive(false);

        switch (dir)
        {
            case FaceDirection.Front:
                frontMenu?.SetActive(true);
                break;
            case FaceDirection.Left:
                leftMenu?.SetActive(true);
                break;
            case FaceDirection.Right:
                rightMenu?.SetActive(true);
                break;
            case FaceDirection.Up:
                upMenu?.SetActive(true);
                break;
            case FaceDirection.Down:
                downMenu?.SetActive(true);
                break;
        }
    }
}
