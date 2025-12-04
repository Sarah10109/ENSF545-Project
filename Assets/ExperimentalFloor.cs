using UnityEngine;

public class ExperimentalFloor : MonoBehaviour
{
    public Material floorMaterial;
    
    private void Start()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            Destroy(col);
        }
        
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            if (floorMaterial != null)
            {
                renderer.material = floorMaterial;
            }
            else
            {
                Material marbleMat = new Material(Shader.Find("Standard"));
                marbleMat.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                marbleMat.SetFloat("_Metallic", 0.1f);
                marbleMat.SetFloat("_Glossiness", 0.7f);
                renderer.material = marbleMat;
            }
        }
    }
}

