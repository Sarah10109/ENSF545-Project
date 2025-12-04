using UnityEngine;

public class ExperimentalWall : MonoBehaviour
{
    public Material wallMaterial;
    
    private void Start()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            Destroy(col);
        }
        
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null && wallMaterial != null)
        {
            renderer.material = wallMaterial;
        }
    }
}

