using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    public GameObject collectiblePrefab;
    public int collectibleCount = 100;
    public Vector3 spawnAreaSize = new Vector3(100, 0, 100);
    public float spawnHeight = 150f;  // Should be above highest point of terrain
    public LayerMask groundLayer;     // Assign the ground layer in Inspector

    void Start()
    {
        SpawnCollectibles();
    }

    void SpawnCollectibles()
    {
        int spawnedCount = 0;
        int attempts = 0;
        int maxAttempts = collectibleCount * 100;

        while (spawnedCount < collectibleCount && attempts < maxAttempts)
        {
            attempts++;
            Vector3 rayOrigin = GetRandomPosition();

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Vector3 spawnPoint = hit.point; // exactly on ground surface
                Instantiate(collectiblePrefab, spawnPoint, Quaternion.identity);
                spawnedCount++;
            }

            if (attempts % 1000 == 0)
            {
                Debug.Log($"Spawn progress: {spawnedCount}/{collectibleCount} after {attempts} attempts.");
            }
        }

        if (spawnedCount < collectibleCount)
        {
            Debug.LogError($"Failed to spawn all collectibles. Spawned {spawnedCount}/{collectibleCount} after {attempts} attempts.");
        }
        else
        {
            Debug.Log($"Successfully spawned all {collectibleCount} collectibles.");
        }
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float z = Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f);
        return new Vector3(x, spawnHeight, z) + transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawWireCube(center, new Vector3(spawnAreaSize.x, 0.2f, spawnAreaSize.z));
    }
}
