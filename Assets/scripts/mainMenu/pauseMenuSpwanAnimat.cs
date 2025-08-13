using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pauseMenuSpwanAnimat : MonoBehaviour
{
    private Animator animator;

    public List<GameObject> toDisable;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        animator.SetTrigger("summon");
    }

    public void triggerDissapear()
    {
        animator.SetTrigger("dissapear");
    }

    public void disableSelf()
    {
        foreach (GameObject gameObject in toDisable)
        {
            gameObject.SetActive(false);
        }
    }
}
