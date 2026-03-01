using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon to Equip")]
    public WeaponDataSO weaponData;

    private bool playerInRange = false;
    private PlayerCombat playerCombat;
    private GameUI gameUI;

    void Start()
    {
        gameUI = FindObjectOfType<GameUI>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PickupWeapon();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerCombat = other.GetComponent<PlayerCombat>();
            
            if (playerCombat == null)
            {
                playerCombat = other.GetComponentInChildren<PlayerCombat>();
            }
            
            if (playerCombat == null)
            {
                playerCombat = other.GetComponentInParent<PlayerCombat>();
            }

            // Show pickup prompt via GameUI
            if (gameUI != null && weaponData != null)
            {
                gameUI.ShowPickupPrompt(weaponData.weaponType);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerCombat = null;

            // Hide pickup prompt
            if (gameUI != null)
            {
                gameUI.HidePickupPrompt();
            }
        }
    }

    private void PickupWeapon()
    {
        if (playerCombat != null && weaponData != null)
        {
            playerCombat.EquipWeapon(weaponData);
            Debug.Log($"Picked up: {weaponData.weaponName}");
            
            // Hide prompt after pickup
            if (gameUI != null)
            {
                gameUI.HidePickupPrompt();
            }
        }
        else
        {
            if (playerCombat == null)
                Debug.LogWarning("PlayerCombat not found on player!");
            if (weaponData == null)
                Debug.LogWarning("WeaponPickup has no WeaponDataSO assigned!");
        }
    }
}
