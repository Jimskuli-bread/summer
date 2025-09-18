using UnityEngine;

public class Collectible : MonoBehaviour
{
    public static event System.Action OnCollectibleCollected;

    public int points = 1; // Points awarded
    public AudioSource collectAudioSource; // Optional sound

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        if (!other.CompareTag("Player")) return;

        isCollected = true;

        // Add points to the score
        ScoreManager.Instance?.AddPoints(points);

        // Notify listeners (e.g. CollectibleManager)
        OnCollectibleCollected?.Invoke();

        // Play sound and delay destruction if needed
        if (collectAudioSource != null && collectAudioSource.clip != null)
        {
            collectAudioSource.Play();
            // Detach audio so it can play after object is destroyed
            collectAudioSource.transform.parent = null;
            Destroy(collectAudioSource.gameObject, collectAudioSource.clip.length);
        }

        // Destroy the collectible object immediately
        Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            OnTriggerEnter(GameObject.FindGameObjectWithTag("Player").GetComponent<Collider>());
        }
    }
}
