using UnityEditor.Animations;
using UnityEngine;

public class spikeBallTarget : MonoBehaviour
{
    public targetController targetController;

    public void decreaseUsage()
    {
        if(transform.childCount <= 0)
            return;

        Transform child = transform.GetChild(0);

        if(child != null)
        {
            child.GetComponent<spikeBall>().decreaseUsage();
        }
    }
}
