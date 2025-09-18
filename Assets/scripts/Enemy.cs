using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float health = 3f;
    public float damage = 1f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float moveSpeed = 3.5f;
    public float jumpForce = 8f;
    public float climbForce = 12f;
    public float obstacleCheckDistance = 2f;
    public float climbCheckHeight = 2f;

    private Transform player;
    private float lastAttackTime = 0f;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private bool isGrounded = true;
    private bool isClimbing = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        if (agent != null)
            agent.speed = moveSpeed;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // If close enough, attack
        if (dist <= attackRange)
        {
            if (Time.time > lastAttackTime + attackCooldown)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.TakeDamage(damage);

                lastAttackTime = Time.time;
            }
            if (agent != null)
                agent.ResetPath();
            return;
        }

        // If player is above, try to climb
        if (player.position.y - transform.position.y > 1.5f)
        {
            TryClimb();
        }
        else
        {
            // Normal chase
            if (agent != null)
                agent.SetDestination(player.position);

            // If stuck or blocked, try to jump over obstacle
            if (IsObstacleAhead())
            {
                TryJump();
            }
        }
    }

    bool IsObstacleAhead()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = transform.forward;
        if (Physics.Raycast(origin, dir, out hit, obstacleCheckDistance))
        {
            if (hit.collider.CompareTag("Building"))
                return true;
        }
        return false;
    }

    void TryJump()
    {
        if (isGrounded && rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void TryClimb()
    {
        if (!isClimbing && rb != null)
        {
            // Raycast up to check for climbable surface
            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 1f;
            if (Physics.Raycast(origin, Vector3.up, out hit, climbCheckHeight))
            {
                if (hit.collider.CompareTag("Building"))
                {
                    rb.AddForce(Vector3.up * climbForce, ForceMode.Impulse);
                    isClimbing = true;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Reset grounded/climbing state when touching ground or building
        if (collision.contacts.Length > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            if (Vector3.Angle(normal, Vector3.up) < 45f)
            {
                isGrounded = true;
                isClimbing = false;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}