using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface kccIinputDriver
{
    public Vector2 getLookInput();
    public Vector2 getMoveInput();
    public bool getJumpInput();
 
 
    event Action onCameraChangeEvent;
    public void onCameraChange();
}
