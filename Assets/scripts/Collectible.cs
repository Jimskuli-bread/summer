using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int points = 1; // Points awarded for collecting this item

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ScoreManager.Instance.AddPoints(points);
            Destroy(gameObject); // Destroy the collectible
        }
    }
}