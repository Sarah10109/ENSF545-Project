using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides a 3D cursor position and a button state, using the mouse.
/// Later you can replace this with haptic device input.
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float cursorDepth = 2.0f;       // How far in front of camera the cursor sits

    public Vector3 CursorPosition { get; private set; }
    public bool ButtonPressed { get; private set; }

    void Update()
    {
        UpdateCursorFromMouse();
    }

    void UpdateCursorFromMouse()
    {
        // Use mouse position on screen, plus a fixed depth
        Vector3 mouse = Input.mousePosition;
        mouse.z = cursorDepth;  // distance from camera

        // Convert screen point to world point
        Camera cam = Camera.main;
        if (cam != null)
        {
            CursorPosition = cam.ScreenToWorldPoint(mouse);
        }

        // Left mouse button = "grab" button
        ButtonPressed = Input.GetMouseButton(0);
    }
}

