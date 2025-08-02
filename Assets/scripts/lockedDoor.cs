using Unity.Mathematics;
using UnityEngine;

public class lockedDoor : MonoBehaviour
{
    public bool isLocked = true;
    public bool isOpen = false;
    public float openSpeed = 3f;

    public float closeDelay = 5f;
    private float closeTimer = 0f;

    public GameObject doorObj;
    public GameObject lockObj;

    private inventory inventory;

    void Start()
    {
        inventory = FindFirstObjectByType<inventory>();
    }

    private void Update()
    {
        lockObj.SetActive(isLocked);

        doorObj.transform.localScale = new Vector3(1, Mathf.Lerp(doorObj.transform.localScale.y, isOpen ? 0 : 1, openSpeed * Time.deltaTime), 1);

        if (isOpen)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f)
            {
                isOpen = false;
            }
        }
    }

    public void openDoor()
    {
        if (isLocked)
        {
            if (inventory.keys > 0)
            {
                isLocked = false;
                inventory.keys--;
                return;
            }
        }
        else
        {
            if (!isOpen)
            {
                isOpen = true;
                closeTimer = closeDelay;
            }
        }
    }
}
