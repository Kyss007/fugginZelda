using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destructTrigger : MonoBehaviour
{
    public bool destroyAfterTime = false;
    public float timeInSeconds = 60f;

    private void Start()
    {
        if (destroyAfterTime)
        {
            Invoke("destroySelf", timeInSeconds);
        }
    }

    public void destroySelf()
    {
        Destroy(this.gameObject);
    }
}
