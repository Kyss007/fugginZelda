using UnityEngine;

public class menuCubeController : MonoBehaviour
{
    private randomRotator randomRotator;

    public bool allowUp = true;
    public bool allowDown = true;
    public bool allowLeft = true;
    public bool allowRight = true;

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
    }

    public void rotDown()
    {
        if(!allowDown)
            return;

        Quaternion rotDown = Quaternion.AngleAxis(-90f, Vector3.right);

        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotDown);
    }

    public void rotLeft()
    {
        if(!allowLeft)
            return;

        Quaternion rotLeft = Quaternion.AngleAxis(-90f, Vector3.up);

        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotLeft);
    }

    public void rotRight()
    {
        if(!allowRight)
            return;

        Quaternion rotRight = Quaternion.AngleAxis(90f, Vector3.up);
        
        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotRight);
    }
}
