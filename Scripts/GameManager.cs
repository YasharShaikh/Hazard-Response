using System;
using UnityEngine;
using UnityEngine.Events;

public enum PowerupType
{
    Speed,
    Damage,
    FireRate,
    MaxHealth,
    Heal
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Health")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    [Header("Multiplier Percentages (cumulative)")]
    public float speedPercentage = 0f;
    public float damagePercentage = 0f;
    public float fireRatePercentage = 0f;


    [Header("Leveling System")]
    public int killCount = 0;
    public int currentLevel = 1;
    public int killsToLevelUp = 20;
    public float killRequirementRatio = 2f;

    [Header("References")]
    public PlayerMovement playerMovement;
    public PowerUpUI powerUpUI;

    [Header("Events")]
    public UnityEvent<float, float> OnMultipliersChanged; // damagePercentage, fireRatePercentage
    public UnityEvent OnPlayerDied;
    public UnityEvent<int> OnLevelUp; // passes new level

    [HideInInspector]
    public bool isPlayerDead = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;

        Texture.streamingTextureForceLoadAll = true;
        
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }
        
        if (powerUpUI == null)
        {
            powerUpUI = FindObjectOfType<PowerUpUI>();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isPlayerDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHurt();
        }
        if (CameraController.Instance != null)
        {
            CameraController.Instance.Shake();
        }

        if (currentHealth <= 0)
        {
            PlayerDied();
        }
    }

    public void AddKill()
    {
        if (isPlayerDead) return;

        killCount++;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (killCount >= killsToLevelUp)
        {
            currentLevel++;
            killCount = 0;
            killsToLevelUp = Mathf.RoundToInt(killsToLevelUp * killRequirementRatio);

            // Play level up sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLevelUp();
            }

            // Pause game and show powerup UI
            Time.timeScale = 0f;
            OnLevelUp?.Invoke(currentLevel);
            
            if (powerUpUI != null)
            {
                powerUpUI.ShowPowerupSelection();
            }
        }
    }

    public void ApplyPowerup(PowerupDataSO powerupData)
    {
        if (powerupData == null)
        {
            Debug.LogWarning("ApplyPowerup called with null PowerupDataSO!");
            return;
        }

        float value = powerupData.statModifierValue;

        switch (powerupData.powerupType)
        {
            case PowerupType.Speed:
                speedPercentage += value;
                ApplySpeedMultiplier();
                break;

            case PowerupType.Damage:
                damagePercentage += value;
                break;

            case PowerupType.FireRate:
                fireRatePercentage += value;
                break;

            case PowerupType.MaxHealth:
                float healthIncrease = maxHealth * (value / 100f);
                maxHealth += healthIncrease;
                currentHealth += healthIncrease;
                break;

            case PowerupType.Heal:
                currentHealth = Mathf.Min(currentHealth + value, maxHealth);
                break;
        }

        Debug.Log($"Applied powerup: {powerupData.displayName} (value: {value})");

        // Notify weapons about multiplier changes
        OnMultipliersChanged?.Invoke(damagePercentage, fireRatePercentage);

        // Resume game
        Time.timeScale = 1f;
    }

    public void ApplyPowerup(PowerupType powerupType)
    {
        float defaultValue = 10f;

        switch (powerupType)
        {
            case PowerupType.Speed:
                speedPercentage += defaultValue;
                ApplySpeedMultiplier();
                break;

            case PowerupType.Damage:
                damagePercentage += defaultValue;
                break;

            case PowerupType.FireRate:
                fireRatePercentage += defaultValue;
                break;

            case PowerupType.MaxHealth:
                float healthIncrease = maxHealth * (defaultValue / 100f);
                maxHealth += healthIncrease;
                currentHealth += healthIncrease;
                break;

            case PowerupType.Heal:
                currentHealth = Mathf.Min(currentHealth + 30f, maxHealth);
                break;
        }

        OnMultipliersChanged?.Invoke(damagePercentage, fireRatePercentage);
        Time.timeScale = 1f;
    }

    private void ApplySpeedMultiplier()
    {
        if (playerMovement != null)
        {
            // Speed = baseSpeed + (baseSpeed * speedPercentage / 100)
            float newSpeed = playerMovement.baseSpeed * (1f + speedPercentage / 100f);
            playerMovement.speed = newSpeed;
        }
    }

    public void PlayerDied()
    {
        if (isPlayerDead) return;
        
        isPlayerDead = true;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerDied();
        }
        
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        OnPlayerDied?.Invoke();
        Debug.Log("Player Died!");
    }

    public float GetCurrentDamageMultiplier()
    {
        return 1f + damagePercentage / 100f;
    }

    public float GetCurrentFireRateMultiplier()
    {
        return 1f + fireRatePercentage / 100f;
    }
}
