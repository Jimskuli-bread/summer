using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Player Health: " + currentHealth);
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // You can add respawn, game over, etc.
        Debug.Log("Player Died!");
        // For now, just disable player
        gameObject.SetActive(false);
    }
}