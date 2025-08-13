using UnityEngine;

public class faceDirectionChecker : MonoBehaviour
{
    public Transform target;

    public enum FaceDirection { Front, Left, Right, Up }
    public FaceDirection currentDirection;

    public GameObject frontMenu;
    public GameObject leftMenu;
    public GameObject rightMenu;
    public GameObject upMenu;



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
            currentDirection = FaceDirection.Up;
        }

        HandleDirection(currentDirection);
    }

    void HandleDirection(FaceDirection dir)
    {
        switch (dir)
        {
            case FaceDirection.Front:
                frontMenu?.SetActive(true);

                leftMenu?.SetActive(false);
                rightMenu?.SetActive(false);
                upMenu?.SetActive(false);
                break;

            case FaceDirection.Left:
                leftMenu?.SetActive(true);

                frontMenu?.SetActive(false);
                rightMenu?.SetActive(false);
                upMenu?.SetActive(false);
                break;

            case FaceDirection.Right:
                rightMenu?.SetActive(true);

                frontMenu?.SetActive(false);
                leftMenu?.SetActive(false);
                upMenu?.SetActive(false);
                break;

            case FaceDirection.Up:
                upMenu?.SetActive(true);

                frontMenu?.SetActive(false);
                leftMenu?.SetActive(false);
                rightMenu?.SetActive(false);
                break;
        }
    }
}
