using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class clickableObject : MonoBehaviour
{
    public UnityEvent onClicked;

    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
        {
            onClicked?.Invoke();
        }
    }
}
