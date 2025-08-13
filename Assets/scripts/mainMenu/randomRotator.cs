using UnityEngine;

public class randomRotator : MonoBehaviour
{
    public Vector3 swayAmplitude = new Vector3(5f, 5f, 5f);
    public Vector3 swaySpeed = new Vector3(1f, 1.2f, 0.8f);

    private Quaternion defaultRotation;
    private Vector3 randomPhase;

    void Start()
    {
        defaultRotation = transform.rotation;
        randomPhase = new Vector3(Random.value * Mathf.PI * 2, Random.value * Mathf.PI * 2, Random.value * Mathf.PI * 2);
    }

    void Update()
    {
        float swayX = Mathf.Sin(Time.time * swaySpeed.x + randomPhase.x) * swayAmplitude.x;
        float swayY = Mathf.Sin(Time.time * swaySpeed.y + randomPhase.y) * swayAmplitude.y;
        float swayZ = Mathf.Sin(Time.time * swaySpeed.z + randomPhase.z) * swayAmplitude.z;

        Quaternion swayRotation = Quaternion.Euler(swayX, swayY, swayZ);

        transform.rotation = defaultRotation * swayRotation;
    }

    public void setDefaultRot(Quaternion input)
    {
        defaultRotation = input;
    }

    public Quaternion getDefaultRot()
    {
        return defaultRotation;
    }
}
