using UnityEngine;
using TMPro; // If using TextMeshPro
using UnityEngine.UI;

public class CollectibleProgressUI : MonoBehaviour
{
    public TextMeshProUGUI progressText; // Assign in inspector
    public CollectibleManager collectibleManager; // Assign in inspector

    void Start()
    {
        UpdateProgress(collectibleManager.GetCollectedCount(), collectibleManager.totalCollectibles);
        Collectible.OnCollectibleCollected += OnCollectibleCollected;
    }

    void OnDestroy()
    {
        Collectible.OnCollectibleCollected -= OnCollectibleCollected;
    }

    void OnCollectibleCollected()
    {
        UpdateProgress(collectibleManager.GetCollectedCount(), collectibleManager.totalCollectibles);
    }

    void UpdateProgress(int collected, int total)
    {
        progressText.text = $"{collected}/{total}";
    }
}