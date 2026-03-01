using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Number of enemies in wave 1")]
    public int initialSpawnCount = 10;
    
    [Tooltip("Spawn speed 1-200 (1 = 3 sec interval, 100 = 0.1 sec, 200 = 0.05 sec interval)")]
    [Range(1, 200)]
    public float initialSpawnSpeed = 1f;

    [Header("Wave Progression")]
    [Tooltip("Percentage increase in enemy count per wave (0.1 = 10%)")]
    public float spawnCountIncreaseRatio = 0.1f;
    
    [Tooltip("Percentage increase in spawn speed per wave (0.1 = 10%)")]
    public float spawnSpeedIncreaseRatio = 0.1f;
    
    [Tooltip("Enemy health multiplier per wave")]
    public float enemyHealthIncreaseMultiplier = 1.2f;

    [Header("Wave Transition")]
    [Tooltip("Delay in seconds before next wave starts")]
    public float waveTransitionDelay = 5f;

    [Header("Enemy Base Stats")]
    public float baseEnemyHealth = 5f;
    public float enemyDamage = 10f;

    [Header("UI Reference")]
    public GameUI gameUI;

    // Runtime variables
    private List<Transform> spawnPoints = new List<Transform>();
    private int currentWave = 0;
    private int currentWaveEnemyCount;
    private float currentSpawnSpeed;
    private float currentEnemyHealth;
    private int enemiesSpawned = 0;
    private int enemiesKilled = 0;
    private int totalEnemiesThisWave = 0;
    private bool isSpawning = false;
    
    // Hierarchy organization
    private Transform enemyContainer;

    void Start()
    {
        // Create enemy container
        GameObject container = new GameObject("_EnemyContainer");
        enemyContainer = container.transform;

        // Get all child objects as spawn points
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("EnemySpawnManager: No spawn points found! Add child GameObjects as spawn points.");
            return;
        }

        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUI>();
        }

        // Initialize first wave values
        currentWaveEnemyCount = initialSpawnCount;
        currentSpawnSpeed = initialSpawnSpeed;
        currentEnemyHealth = baseEnemyHealth;

        // Start first wave
        StartCoroutine(StartNextWave());
    }

    private IEnumerator StartNextWave()
    {
        currentWave++;
        
        // Calculate wave parameters
        if (currentWave > 1)
        {
            currentWaveEnemyCount = Mathf.RoundToInt(currentWaveEnemyCount * (1f + spawnCountIncreaseRatio));
            currentSpawnSpeed = Mathf.Min(200f, currentSpawnSpeed * (1f + spawnSpeedIncreaseRatio));
            currentEnemyHealth = baseEnemyHealth * Mathf.Pow(enemyHealthIncreaseMultiplier, currentWave - 1);
        }

        totalEnemiesThisWave = currentWaveEnemyCount;
        enemiesSpawned = 0;
        enemiesKilled = 0;

        // Update UI
        if (gameUI != null)
        {
            gameUI.ShowWaveStart(currentWave);
        }

        Debug.Log($"Wave {currentWave} starting! Enemies: {totalEnemiesThisWave}, Speed: {currentSpawnSpeed:F1}, Health: {currentEnemyHealth:F1}");

        // Start spawning
        StartCoroutine(SpawnEnemies());
        
        yield return null;
    }

    private IEnumerator SpawnEnemies()
    {
        isSpawning = true;

        while (enemiesSpawned < totalEnemiesThisWave)
        {
            SpawnEnemy();
            enemiesSpawned++;

            // Calculate spawn interval based on speed (1-200)
            // 1 = 3 seconds, 100 = 0.1 seconds, 200 = 0.05 seconds
            float spawnInterval = Mathf.Lerp(3f, 0.05f, (currentSpawnSpeed - 1f) / 199f);
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Count == 0 || enemyPrefab == null) return;

        // Select random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        // Spawn enemy under container
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation, enemyContainer);
        
        // Configure enemy
        ZombieEnemy zombieEnemy = enemy.GetComponent<ZombieEnemy>();
        if (zombieEnemy != null)
        {
            zombieEnemy.Initialize(currentEnemyHealth, enemyDamage, this);
        }
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;
        
        // Notify GameManager for kill count
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
        }

        // Check if wave is complete
        if (enemiesKilled >= totalEnemiesThisWave && !isSpawning)
        {
            StartCoroutine(WaveComplete());
        }
    }

    private IEnumerator WaveComplete()
    {
        Debug.Log($"Wave {currentWave} complete!");

        // Play wave clear sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWaveClear();
        }

        // Show countdown UI
        if (gameUI != null)
        {
            gameUI.ShowWaveCountdown(currentWave + 1, waveTransitionDelay);
        }

        yield return new WaitForSeconds(waveTransitionDelay);

        // Start next wave
        StartCoroutine(StartNextWave());
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetEnemiesRemaining()
    {
        return totalEnemiesThisWave - enemiesKilled;
    }
}
