using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Wave Display")]
    public TMP_Text waveText;
    public TMP_Text countdownText;
    public GameObject waveStartPanel;

    [Header("Player Stats")]
    public TMP_Text healthText;
    public Slider healthSlider;
    public TMP_Text enemiesRemainingText;
    public TMP_Text currentWaveText;
    public TMP_Text levelText;

    [Header("Level Progress")]
    public Slider levelProgressSlider;
    public TMP_Text levelProgressText;

    [Header("Weapon Display")]
    public Image weaponImage;
    public Sprite rifleSprite;
    public Sprite shotgunSprite;
    public Sprite machineGunSprite;

    [Header("Pickup Prompt")]
    public TMP_Text pickupPromptText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Button retryButton;
    public Button menuButton;
    public string menuSceneName = "MainMenu";

    [Header("Settings")]
    public float waveDisplayDuration = 2f;

    [Header("References")]
    public EnemySpawnManager spawnManager;

    private Coroutine countdownCoroutine;

    void Start()
    {
        if (waveStartPanel != null) waveStartPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pickupPromptText != null) pickupPromptText.gameObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied.AddListener(ShowGameOver);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuClicked);
        }

        if (spawnManager == null)
        {
            spawnManager = FindObjectOfType<EnemySpawnManager>();
        }
    }

    void Update()
    {
        UpdatePlayerStats();
        UpdateLevelProgress();
        UpdateEnemiesRemaining();
    }

    private void UpdatePlayerStats()
    {
        if (GameManager.Instance == null) return;

        if (healthText != null)
        {
            healthText.text = $"HP: {GameManager.Instance.currentHealth:F0}/{GameManager.Instance.maxHealth:F0}";
        }
        if (healthSlider != null)
        {
            healthSlider.maxValue = GameManager.Instance.maxHealth;
            healthSlider.value = GameManager.Instance.currentHealth;
        }

        if (levelText != null)
        {
            levelText.text = $"Level: {GameManager.Instance.currentLevel}";
        }
    }

    private void UpdateEnemiesRemaining()
    {
        if (spawnManager == null) return;
        
        if (enemiesRemainingText != null)
        {
            int remaining = spawnManager.GetEnemiesRemaining();
            enemiesRemainingText.text = remaining.ToString();
        }
        
        if (currentWaveText != null)
        {
            currentWaveText.text = $"Wave: {spawnManager.GetCurrentWave()}";
        }
    }

    private void UpdateLevelProgress()
    {
        if (GameManager.Instance == null) return;

        if (levelProgressSlider != null)
        {
            levelProgressSlider.minValue = 0;
            levelProgressSlider.maxValue = GameManager.Instance.killsToLevelUp;
            levelProgressSlider.value = GameManager.Instance.killCount;
        }

        if (levelProgressText != null)
        {
            levelProgressText.text = $"{GameManager.Instance.killCount}/{GameManager.Instance.killsToLevelUp}";
        }
    }

    public void UpdateWeaponDisplay(WeaponType weaponType)
    {
        if (weaponImage == null) return;

        switch (weaponType)
        {
            case WeaponType.Rifle:
                if (rifleSprite != null) weaponImage.sprite = rifleSprite;
                break;
            case WeaponType.Shotgun:
                if (shotgunSprite != null) weaponImage.sprite = shotgunSprite;
                break;
            case WeaponType.MachineGun:
                if (machineGunSprite != null) weaponImage.sprite = machineGunSprite;
                break;
        }
    }

    public void ShowWaveStart(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {waveNumber}";
        }

        if (waveStartPanel != null)
        {
            waveStartPanel.SetActive(true);
            StartCoroutine(HideWaveStartAfterDelay());
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    private IEnumerator HideWaveStartAfterDelay()
    {
        yield return new WaitForSeconds(waveDisplayDuration);
        
        if (waveStartPanel != null)
        {
            waveStartPanel.SetActive(false);
        }
    }

    public void ShowWaveCountdown(int nextWave, float delay)
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        countdownCoroutine = StartCoroutine(CountdownRoutine(nextWave, delay));
    }

    private IEnumerator CountdownRoutine(int nextWave, float totalDelay)
    {
        if (countdownText == null) yield break;

        countdownText.gameObject.SetActive(true);

        float remaining = totalDelay;
        while (remaining > 0)
        {
            countdownText.text = $"Next Wave in {remaining:F0}...";
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        countdownText.gameObject.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Stop countdown if running
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
    }

    private void OnRetryClicked()
    {
        // Reset time scale and reload current scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnMenuClicked()
    {
        // Reset time scale and load main menu
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void UpdateWaveDisplay(int currentWave, int enemiesRemaining)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {currentWave} - {enemiesRemaining} enemies";
        }
    }

    public void ShowPickupPrompt(WeaponType weaponType)
    {
        if (pickupPromptText == null) return;

        string weaponName = weaponType.ToString();
        weaponName = System.Text.RegularExpressions.Regex.Replace(weaponName, "([a-z])([A-Z])", "$1 $2");

        pickupPromptText.text = $"Press E to switch to {weaponName}";
        pickupPromptText.gameObject.SetActive(true);
    }

    public void HidePickupPrompt()
    {
        if (pickupPromptText != null)
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }
}
