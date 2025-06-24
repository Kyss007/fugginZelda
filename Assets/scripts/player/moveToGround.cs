using UnityEngine;

public class moveToGround : MonoBehaviour
{
    public LayerMask groundLayer;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.parent.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            transform.position = hit.point;
            transform.up = hit.normal;
        }
        else
        {
            transform.position = transform.parent.position + Vector3.down * 1000000;
        }
    }
}
