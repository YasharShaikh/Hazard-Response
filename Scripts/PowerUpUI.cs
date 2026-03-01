using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject powerupPanel;
    public Button[] powerupButtons;
    public TMP_Text[] powerupButtonTexts;
    public TMP_Text levelUpText;

    [Header("Available Powerups")]
    public PowerupDataSO[] availablePowerups;

    private PowerupDataSO[] currentOptions = new PowerupDataSO[3];

    void Awake()
    {
        if (powerupPanel != null)
        {
            powerupPanel.SetActive(false);
        }
        for (int i = 0; i < powerupButtons.Length; i++)
        {
            int index = i; // Capture for closure
            if (powerupButtons[i] != null)
            {
                powerupButtons[i].onClick.AddListener(() => OnPowerupSelected(index));
            }
        }
    }

    public void ShowPowerupSelection()
    {
        if (powerupPanel == null)
        {
            Debug.LogWarning("PowerUpUI: Panel not assigned!");
            return;
        }

        if (availablePowerups == null || availablePowerups.Length == 0)
        {
            Debug.LogWarning("PowerUpUI: No powerups assigned!");
            return;
        }

        // Select 3 random powerups from available list
        List<PowerupDataSO> available = new List<PowerupDataSO>(availablePowerups);
        
        for (int i = 0; i < 3 && available.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            currentOptions[i] = available[randomIndex];
            available.RemoveAt(randomIndex);

            // Update button text using display name from ScriptableObject
            if (powerupButtonTexts != null && i < powerupButtonTexts.Length && powerupButtonTexts[i] != null)
            {
                powerupButtonTexts[i].text = currentOptions[i].displayName;
            }
        }

        // Update level text
        if (levelUpText != null && GameManager.Instance != null)
        {
            levelUpText.text = $"Level {GameManager.Instance.currentLevel}!";
        }

        powerupPanel.SetActive(true);
    }

    private void OnPowerupSelected(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= currentOptions.Length) return;
        if (currentOptions[buttonIndex] == null) return;

        PowerupDataSO selectedPowerup = currentOptions[buttonIndex];
        
        Debug.Log($"Selected powerup: {selectedPowerup.displayName}");

        powerupPanel.SetActive(false);

        // Apply powerup through GameManager using the full ScriptableObject data
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyPowerup(selectedPowerup);
        }
    }

    public void HidePowerupSelection()
    {
        if (powerupPanel != null)
        {
            powerupPanel.SetActive(false);
        }
    }
}
