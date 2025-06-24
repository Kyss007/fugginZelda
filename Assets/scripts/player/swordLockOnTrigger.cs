using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swordLockOnTrigger : StateMachineBehaviour
{
    private characterController player;

    private void Awake()
    {
        player = FindObjectOfType<characterController>();
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player.isMoving)
        {
            player.currentState = characterController.state.swordDash;
        }
    }

    
}
