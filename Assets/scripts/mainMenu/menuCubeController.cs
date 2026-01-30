using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class menuCubeController : MonoBehaviour
{
    private randomRotator randomRotator;

    [Header("Movement Permissions")]
    public bool allowUp = true;
    public bool allowDown = true;
    public bool allowLeft = true;
    public bool allowRight = true;

    public bool isUpToggle = false;
    public bool isDownToggle = false;
    public bool isLeftToggle = false;
    public bool isRightToggle = false;

    private void Start()
    {
        randomRotator = GetComponent<randomRotator>();
    }

    public void rotUp()
    {
        if(!allowUp)
            return;

        Quaternion rotUp = Quaternion.AngleAxis(90f, Vector3.right);
        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotUp);

        if (isUpToggle)
        {
            allowUp = false;
            allowDown = true;
        }
    }

    public void rotDown()
    {
        if(!allowDown)
            return;

        Quaternion rotDown = Quaternion.AngleAxis(-90f, Vector3.right);
        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotDown);

        if (isDownToggle)
        {
            allowDown = false;
            allowUp = true;
        }
    }

    public void rotLeft()
    {
        if(!allowLeft)
            return;

        Quaternion rotLeft = Quaternion.AngleAxis(-90f, Vector3.up);
        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotLeft);

        if (isLeftToggle)
        {
            allowLeft = false;
            allowRight = true;
        }
    }

    public void rotRight()
    {
        if(!allowRight)
            return;

        Quaternion rotRight = Quaternion.AngleAxis(90f, Vector3.up);
        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotRight);

        if (isRightToggle)
        {
            allowRight = false;
            allowLeft = true;
        }
    }
}