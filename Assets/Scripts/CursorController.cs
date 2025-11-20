using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reads the cursor position from InputManager and moves the Cursor object.
/// </summary>
public class CursorController : MonoBehaviour
{
    public InputManager inputManager;

    void Update()
    {
        if (inputManager == null) return;

        transform.position = inputManager.CursorPosition;
    }
}
