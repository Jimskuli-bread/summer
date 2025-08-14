using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    public GameObject[] buildingPrefabs; // Array of building prefabs
    public GameObject roadPrefab; // Road prefab
    public int cityWidth = 10; // Number of grid cells in the X direction
    public int cityLength = 10; // Number of grid cells in the Z direction
    public float cellSize = 10f; // Size of each grid cell

    void Start()
    {
        GenerateCity();
    }

    void GenerateCity()
    {
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityLength; z++)
            {
                // Randomly decide what to place in each cell
                float randomValue = Random.value;

                if (randomValue < 0.2f)
                {
                    // Place a road
                    PlaceObject(roadPrefab, x, z);
                }
                else
                {
                    // Place a random building
                    GameObject building = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                    PlaceObject(building, x, z);
                }
            }
        }
    }

    void PlaceObject(GameObject prefab, int x, int z)
    {
        // Calculate the position of the object
        Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);

        // Instantiate the object at the calculated position
        Instantiate(prefab, position, Quaternion.identity, transform);
    }
}