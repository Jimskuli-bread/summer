using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public float attackRange = 2f;
    public float attackDamage = 1f;
    public KeyCode attackKey = KeyCode.Mouse1;

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                }
            }
        }
    }
}