using UnityEngine;

public class menuCubeController : MonoBehaviour
{
    private randomRotator randomRotator;

    private void Start()
    {
        randomRotator = GetComponent<randomRotator>();
    }

    public void rotUp()
    {
        Quaternion rotUp = Quaternion.AngleAxis(90f, Vector3.right);

        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotUp);
    }

    public void rotDown()
    {
        Quaternion rotDown = Quaternion.AngleAxis(-90f, Vector3.right);

        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotDown);
    }

    public void rotLeft()
    {
        Quaternion rotLeft = Quaternion.AngleAxis(-90f, Vector3.up);

        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotLeft);
    }

    public void rotRight()
    {
        Quaternion rotRight = Quaternion.AngleAxis(90f, Vector3.up);
        
        randomRotator.setDefaultRot(randomRotator.getDefaultRot() * rotRight);
    }
}
