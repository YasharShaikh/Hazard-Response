using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float health = 5f;
    public float damage = 10f;
    public float chaseSpeed = 3.5f;

    [Header("Attack Settings")]
    public float damageDistance = 2f;
    public float attackCooldown = 1f;

    [Header("References")]
    public Animator animator;

    [HideInInspector]
    public GameObject m_player;
    [HideInInspector]
    public bool m_isDead = false;

    // Runtime
    private NavMeshAgent agent;
    private EnemySpawnManager spawnManager;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
    }

    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        if (m_player == null)
        {
            Debug.LogWarning("ZombieEnemy: Player not found! Make sure player has 'Player' tag.");
        }

        // Configure NavMeshAgent
        agent.speed = chaseSpeed;
        agent.stoppingDistance = damageDistance * 0.8f;

        // Start with Chase animation (skip patrol since we always chase)
        if (animator != null)
        {
            animator.SetBool("Walk", true);
            animator.SetBool("Chase", true);
        }
    }

    public void Initialize(float enemyHealth, float enemyDamage, EnemySpawnManager manager)
    {
        health = enemyHealth;
        damage = enemyDamage;
        spawnManager = manager;
    }

    void Update()
    {
        if (m_isDead || m_player == null) return;

        // Always move towards player
        agent.SetDestination(m_player.transform.position);

        // Check distance for attack
        float distance = Vector3.Distance(m_player.transform.position, transform.position);
        
        if (distance <= damageDistance)
        {
            // Close enough to attack
            if (!isAttacking)
            {
                isAttacking = true;
                if (animator != null)
                {
                    animator.SetBool("Attack", true);
                }
            }
            
            // Face the player while attacking
            Vector3 direction = m_player.transform.position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        else
        {
            // Out of attack range
            if (isAttacking)
            {
                isAttacking = false;
                if (animator != null)
                {
                    animator.SetBool("Attack", false);
                }
            }
        }

        // Destroy if fallen below map
        if (transform.position.y < -10f)
        {
            Die();
        }
    }

    // Called by animation event during attack animation
    public void DamagePlayer()
    {
        if (m_isDead || m_player == null) return;
        
        // Check if close enough and cooldown passed
        float distance = Vector3.Distance(m_player.transform.position, transform.position);
        if (distance < damageDistance && Time.time >= lastAttackTime + attackCooldown)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TakeDamage(damage);
                lastAttackTime = Time.time;
                Debug.Log($"Zombie dealt {damage} damage to player!");
            }
        }
    }

    // Alternative animation event name
    void DamagePlayerAnimEvent()
    {
        DamagePlayer();
    }

    public void TakeDamage(float damageAmount)
    {
        if (m_isDead) return;

        health -= damageAmount;

        if (health <= 0)
        {
            Die();
        }
    }

    // Called by weapon particle collision
    void OnParticleCollision(GameObject other)
    {
        if (m_isDead) return;

        // Get the weapon that shot us
        Weapon weapon = other.GetComponentInParent<Weapon>();
        if (weapon != null)
        {
            TakeDamage(weapon.GetCurrentDamage());
        }
    }

    private void Die()
    {
        if (m_isDead) return;
        
        m_isDead = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayZombieDeath();
        }

        if (spawnManager != null)
        {
            spawnManager.OnEnemyKilled();
        }

        if (animator != null)
        {
            animator.SetBool("Attack", false);
            animator.SetBool("Chase", false);
            animator.SetBool("Dead", true);
        }

        // Stop movement
        if (agent != null)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Disable colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Destroy after delay (for death animation)
        Destroy(gameObject, 3f);
    }

    // Animation event for stopping nav agent
    void StopNavAgent()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.destination = transform.position;
            agent.acceleration = 0;
        }
    }

    // Animation event for death state
    void SetDeadState()
    {
        m_isDead = true;
        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
        }
        
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Disable colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }
}
