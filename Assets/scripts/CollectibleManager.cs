using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CollectibleManager : MonoBehaviour
{
    [Header("Collectible Settings")]
    public GameObject collectiblePrefab;
    public int totalCollectibles = 50;

    [Tooltip("Minimum distance between spawned collectibles.")]
    public float minDistanceBetweenCollectibles = 10f;

    [Tooltip("Radius used to determine overlap with obstacles.")]
    public float collectibleRadius = 10f;

    [Tooltip("Name of the script that marks objects to avoid.")]
    public string excludedScriptName = "CollectibleManager";

    [Header("Win Sound")]
    public AudioSource winAudioSource;

    [Header("Win Screen")]
    public GameObject winScreen;

    private int collectedCount = 0;
    private bool hasWon = false;
    private List<Collider> excludedColliders = new List<Collider>();

    void Start()
    {
        FindExcludedColliders();
        SpawnCollectibles();
    }

    void OnEnable()
    {
        Collectible.OnCollectibleCollected += HandleCollectibleCollected;
    }

    void OnDisable()
    {
        Collectible.OnCollectibleCollected -= HandleCollectibleCollected;
    }

    void FindExcludedColliders()
    {
        excludedColliders.Clear();

        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");
        foreach (var building in buildings)
        {
            Collider col = building.GetComponent<Collider>();
            if (col != null)
            {
                excludedColliders.Add(col);
            }
        }

        Debug.Log($"Found {excludedColliders.Count} excluded colliders with tag: Building");
    }

    void SpawnCollectibles()
    {
        if (collectiblePrefab == null)
        {
            Debug.LogError("Collectible prefab not assigned.");
            return;
        }

        Collider area = GetComponent<Collider>();
        if (area == null)
        {
            Debug.LogError("No collider on spawn area.");
            return;
        }

        Bounds bounds = area.bounds;
        List<Vector3> usedPositions = new List<Vector3>();

        int attempts = 0;
        int maxAttempts = totalCollectibles * 30;

        while (usedPositions.Count < totalCollectibles && attempts < maxAttempts)
        {
            attempts++;

            Vector3 randomPos = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.max.y + 2f, // Just above the area, not using collectibleRadius
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Too close to another collectible
            bool tooClose = false;
            foreach (Vector3 pos in usedPositions)
            {
                if (Vector3.Distance(pos, randomPos) < minDistanceBetweenCollectibles)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // Inside an excluded collider?
            bool insideExcluded = false;
            Collider buildingCollider = null;
            foreach (Collider col in excludedColliders)
            {
                if (col == null) continue;
                Vector3 closest = col.ClosestPoint(randomPos);
                if (Vector3.Distance(closest, randomPos) < 0.01f)
                {
                    insideExcluded = true;
                    buildingCollider = col; // Store building collider for roof calculation
                    break;
                }
            }

            if (insideExcluded)
            {
                // Instead of skipping, reposition on roof
                Vector3 roofPos = GetRoofPosition(buildingCollider, randomPos);
                if (roofPos == Vector3.zero)
                {
                    // Roof position not found, skip
                    continue;
                }

                // Check if roof position is too close to other collectibles
                bool tooCloseOnRoof = false;
                foreach (Vector3 pos in usedPositions)
                {
                    if (Vector3.Distance(pos, roofPos) < minDistanceBetweenCollectibles)
                    {
                        tooCloseOnRoof = true;
                        break;
                    }
                }
                if (tooCloseOnRoof) continue;

                usedPositions.Add(roofPos);
                GameObject collectible = Instantiate(collectiblePrefab, roofPos, Quaternion.identity);
                AdjustCollectibleDown(collectible, buildingCollider);
            }
            else
            {
                // Passed all checks, spawn as normal
                usedPositions.Add(randomPos);
                Instantiate(collectiblePrefab, randomPos, Quaternion.identity);
            }
        }

        if (usedPositions.Count < totalCollectibles)
        {
            Debug.LogWarning("Not all collectibles were spawned due to space restrictions.");
        }
    }

    // Helper to get roof position given a building collider and a position inside it
    Vector3 GetRoofPosition(Collider buildingCollider, Vector3 insidePos)
    {
        RaycastHit hit;

        // Start ray well above the building, at the XZ of the spawn point
        Vector3 rayOrigin = new Vector3(insidePos.x, buildingCollider.bounds.max.y + 50f, insidePos.z);
        float rayDistance = 100f;

        // Raycast down onto the building's collider
        if (buildingCollider.Raycast(new Ray(rayOrigin, Vector3.down), out hit, rayDistance))
        {
            // Place collectible exactly on the roof
            return new Vector3(insidePos.x, hit.point.y, insidePos.z);
        }

        // Fallback: place on top of the building's bounds
        return new Vector3(
            insidePos.x,
            buildingCollider.bounds.max.y,
            insidePos.z
        );
    }

    void HandleCollectibleCollected()
    {
        if (hasWon) return;

        collectedCount++;
        Debug.Log($"Collected {collectedCount}/{totalCollectibles}");

        if (collectedCount >= totalCollectibles)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        if (hasWon) return;

        hasWon = true;
        Debug.Log("You win!");

        if (winScreen != null)
            winScreen.SetActive(true);

        if (winAudioSource != null)
        {
            winAudioSource.Play();
            StartCoroutine(StopGameAfterDelay(winAudioSource.clip.length));
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    System.Collections.IEnumerator StopGameAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (hasWon && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void AdjustCollectibleDown(GameObject collectible, Collider buildingCollider)
    {
        SphereCollider sphere = collectible.GetComponent<SphereCollider>();
        if (sphere == null) return;

        float step = 0.01f;
        int maxSteps = 200;
        int steps = 0;

        // Move down until the collectible's SphereCollider just starts to overlap the building collider
        while (steps < maxSteps)
        {
            Vector3 sphereWorldCenter = collectible.transform.TransformPoint(sphere.center);
            Collider[] overlaps = Physics.OverlapSphere(
                sphereWorldCenter,
                sphere.radius * Mathf.Max(
                    collectible.transform.lossyScale.x,
                    collectible.transform.lossyScale.y,
                    collectible.transform.lossyScale.z
                ),
                ~0 // all layers
            );

            bool touchingBuilding = false;
            foreach (var col in overlaps)
            {
                if (col == buildingCollider)
                {
                    touchingBuilding = true;
                    break;
                }
            }

            if (touchingBuilding)
            {
                // Move up one step so it's just touching, not intersecting
                collectible.transform.position += Vector3.up * step;
                break;
            }

            collectible.transform.position += Vector3.down * step;
            steps++;
        }
    }

    public int GetCollectedCount()
    {
        return collectedCount;
    }
}
