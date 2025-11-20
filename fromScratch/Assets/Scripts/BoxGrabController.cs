using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows grabbing the box when the cursor is close and the mouse button is pressed.
/// Box follows the cursor while grabbed.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BoxGrabController : MonoBehaviour
{
    public InputManager inputManager;
    public Transform cursor;       // reference to Cursor object
    public float grabRadius = 0.1f;

    public bool IsGrabbed { get; private set; }

    private Rigidbody rb;
    private Vector3 grabOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (inputManager == null || cursor == null) return;

        float dist = Vector3.Distance(cursor.position, transform.position);

        // Start grabbing
        if (!IsGrabbed && inputManager.ButtonPressed && dist < grabRadius)
        {
            IsGrabbed = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;

            grabOffset = transform.position - cursor.position;

            // Inform trial manager
            TrialManager.Instance?.OnBoxGrabbed();
        }

        // Release
        if (IsGrabbed && !inputManager.ButtonPressed)
        {
            IsGrabbed = false;
            rb.useGravity = true;

            TrialManager.Instance?.OnBoxReleased(transform.position);
        }
    }

    void FixedUpdate()
    {
        if (IsGrabbed)
        {
            // Move box to follow cursor (with offset)
            Vector3 targetPos = cursor.position + grabOffset;
            rb.MovePosition(targetPos);
        }
    }

    public void ResetBox(Vector3 position)
    {
        IsGrabbed = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = true;
        transform.position = position;
    }
}

