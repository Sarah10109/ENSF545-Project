using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPuzzleController : MonoBehaviour {

    public GameObject ToyBlock;
    private Vector3 initialPosition;
    private Quaternion initialRotation;


	// Remember the original positions of the blocks.
    void Start()
    {
        if (ToyBlock == null)
        {
            Debug.LogWarning("BlockPuzzleController: ToyBlock is not assigned.");
            return;
        }

        initialPosition = ToyBlock.transform.position;
        initialRotation = ToyBlock.transform.rotation;
    }

	// Return the blocks to their original position.
    public void ResetBlocks()
    {
        if (ToyBlock == null) return;

        ToyBlock.transform.position = initialPosition;
        ToyBlock.transform.rotation = initialRotation;

        // Extra: also reset its Rigidbody so it stops moving.
        Rigidbody rb = ToyBlock.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

	// Update is called once per frame
	void Update () 
	{

		// Return to starting position?
		if (Input.GetKeyDown("space"))
		{
			ResetBlocks();
			return;
		}
        else if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }
}
