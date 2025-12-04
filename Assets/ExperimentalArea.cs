using UnityEngine;

public class ExperimentalArea : MonoBehaviour
{
    [Header("Size")]
    public float floorWidth = 10f;
    public float floorLength = 10f;
    public float wallHeight = 0.5f;
    public float wallThickness = 0.1f;
    
    [Header("Materials")]
    public Material floorMaterial;
    public Material wallMaterial;
    
    private GameObject floor;
    private GameObject[] walls = new GameObject[4];
    
    private void Start()
    {
        CreateFloor();
        CreateWalls();
    }
    
    private void CreateFloor()
    {
        floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "ExperimentalFloor";
        floor.transform.SetParent(transform);
        floor.transform.localPosition = Vector3.zero;
        floor.transform.localScale = new Vector3(floorWidth / 10f, 1f, floorLength / 10f);
        
        ExperimentalFloor floorScript = floor.AddComponent<ExperimentalFloor>();
        floorScript.floorMaterial = floorMaterial;
    }
    
    private void CreateWalls()
    {
        float halfWidth = floorWidth / 2f;
        float halfLength = floorLength / 2f;
        float halfHeight = wallHeight / 2f;
        
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0f, halfHeight, halfLength + wallThickness / 2f),
            new Vector3(0f, halfHeight, -halfLength - wallThickness / 2f),
            new Vector3(halfWidth + wallThickness / 2f, halfHeight, 0f),
            new Vector3(-halfWidth - wallThickness / 2f, halfHeight, 0f)
        };
        
        Vector3[] scales = new Vector3[]
        {
            new Vector3(floorWidth + wallThickness * 2f, wallHeight, wallThickness),
            new Vector3(floorWidth + wallThickness * 2f, wallHeight, wallThickness),
            new Vector3(wallThickness, wallHeight, floorLength),
            new Vector3(wallThickness, wallHeight, floorLength)
        };
        
        for (int i = 0; i < 4; i++)
        {
            walls[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls[i].name = $"ExperimentalWall_{i + 1}";
            walls[i].transform.SetParent(transform);
            walls[i].transform.localPosition = positions[i];
            walls[i].transform.localScale = scales[i];
            
            ExperimentalWall wallScript = walls[i].AddComponent<ExperimentalWall>();
            wallScript.wallMaterial = wallMaterial;
            
            if (wallMaterial != null)
            {
                MeshRenderer renderer = walls[i].GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = wallMaterial;
                }
            }
        }
    }
}

