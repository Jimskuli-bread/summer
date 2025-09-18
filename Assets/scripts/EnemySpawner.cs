using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int maxEnemies = 20;
    public float minDistanceBetweenEnemies = 10f;
    public float spawnInterval = 10f;
    public LayerMask groundMask;
    public LayerMask buildingMask;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private float spawnTimer = 0f;
    private List<Collider> buildingColliders = new List<Collider>();
    private BoxCollider spawnArea;

    void Start()
    {
        spawnArea = GetComponent<BoxCollider>();
        if (spawnArea == null)
        {
            Debug.LogError("EnemySpawner requires a BoxCollider as the spawn area.");
            enabled = false;
            return;
        }

        // Cache all building colliders
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");
        foreach (var building in buildings)
        {
            Collider col = building.GetComponent<Collider>();
            if (col != null)
                buildingColliders.Add(col);
        }
    }

    void Update()
    {
        // Clean up destroyed enemies
        spawnedEnemies.RemoveAll(e => e == null);

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && spawnedEnemies.Count < maxEnemies)
        {
            if (SpawnEnemy())
                spawnTimer = 0f;
        }
    }

    bool SpawnEnemy()
    {
        if (enemyPrefab == null || spawnArea == null)
            return false;

        Bounds bounds = spawnArea.bounds;
        int attempts = 0;
        int maxAttempts = 30;

        while (attempts < maxAttempts)
        {
            attempts++;
            Vector3 randomPos = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.max.y + 2f,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Avoid buildings
            bool insideBuilding = false;
            foreach (Collider col in buildingColliders)
            {
                if (col == null) continue;
                Vector3 closest = col.ClosestPoint(randomPos);
                if (Vector3.Distance(closest, randomPos) < 0.01f)
                {
                    insideBuilding = true;
                    break;
                }
            }
            if (insideBuilding) continue;

            // Too close to another enemy
            bool tooClose = false;
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy == null) continue;
                if (Vector3.Distance(enemy.transform.position, randomPos) < minDistanceBetweenEnemies)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // Raycast down to ground
            RaycastHit hit;
            if (Physics.Raycast(randomPos + Vector3.up * 50f, Vector3.down, out hit, 100f, groundMask))
            {
                // Don't spawn on buildings
                if (buildingMask != 0 && ((1 << hit.collider.gameObject.layer) & buildingMask) != 0)
                    continue;

                Vector3 spawnPos = hit.point + Vector3.up * 0.10f; // <-- Spawns a few cm above ground
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                spawnedEnemies.Add(enemy);
                return true;
            }
        }
        return false;
    }
}