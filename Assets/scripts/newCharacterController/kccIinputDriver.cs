using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface kccIinputDriver
{
    public Vector2 getLookInput();
    public Vector2 getMoveInput();
    public bool getJumpInput();

    public bool getDodgeInput();
    public bool getAttackInput();
 
 
    event Action onCameraChangeEvent;
    public void onCameraChange();
}
