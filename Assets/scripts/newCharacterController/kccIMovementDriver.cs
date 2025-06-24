using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface kccIMovementDriver
{
    public void initDriver(Rigidbody rigidbody);
    public void movePlayer();

    public void setLookInput(Vector2 input);
    public void setMoveInput(Vector2 input);
    public void setJumpInput(bool input);
}
