using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    public GameObject collectiblePrefab; // The collectible prefab to spawn
    public int collectibleCount = 100;   // Number of collectibles to spawn
    public Vector3 spawnAreaSize = new Vector3(10, 0, 10); // Size of the spawn area
    public float spawnHeight = 10f;     // Height from which to raycast down
    public LayerMask groundLayer;       // Layer mask for the ground
    private int i = 0;

    void Start()
{
    int i = 0;
    SpawnCollectibles();
}

void SpawnCollectibles()
{
    int spawnedCount = 0;

    while ( i < collectibleCount )
    {
        Vector3 randomPosition = GetRandomPosition();
        if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // Spawn the collectible at the hit point
            Instantiate(collectiblePrefab, hit.point, Quaternion.identity);
            spawnedCount++;
        }
        else
        {
            Debug.LogWarning($"Raycast failed for position {randomPosition}. Check ground layer or spawn area.");
        }
        i++;
    }

    Debug.Log($"Spawned {spawnedCount}/{collectibleCount} collectibles.");
}

Vector3 GetRandomPosition()
{
    // Generate a random position within the spawn area
    float x = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
    float z = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
    float y = spawnHeight; // Start the raycast from above
    return new Vector3(x, y, z) + transform.position;
}
}