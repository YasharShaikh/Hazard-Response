using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlot
    {
        public WeaponDataSO weaponData;
        public GameObject weaponObject;
    }

    [Header("Weapon Slots")]
    public WeaponSlot[] weaponSlots;
    public int defaultWeaponIndex = 0;

    [Header("Current Weapon")]
    public Weapon currentWeapon;
    public WeaponType currentWeaponType = WeaponType.Rifle;

    [Header("UI Reference")]
    public GameUI gameUI;

    private bool canShoot = true;

    void Start()
    {
        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUI>();
        }
        DisableAllWeapons();

        // Equip default weapon
        if (weaponSlots != null && weaponSlots.Length > 0 && defaultWeaponIndex < weaponSlots.Length)
        {
            EquipWeapon(weaponSlots[defaultWeaponIndex].weaponData);
        }
    }

    void Update()
    {
        // Don't allow shooting when paused, dead, or disabled
        if (!canShoot) return;
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.isPlayerDead) return;

        if (Input.GetMouseButton(0))
        {
            if (currentWeapon != null)
            {
                currentWeapon.StartFiring();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentWeapon != null)
            {
                currentWeapon.StopFiring();
            }
        }
    }

    public void EquipWeapon(WeaponDataSO data)
    {
        if (data == null)
        {
            Debug.LogWarning("EquipWeapon called with null WeaponDataSO!");
            return;
        }

        if (currentWeapon != null)
        {
            currentWeapon.StopFiring();
        }
        DisableAllWeapons();

        // Find matching slot
        WeaponSlot matchingSlot = null;
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i].weaponData == data)
            {
                matchingSlot = weaponSlots[i];
                break;
            }
        }

        if (matchingSlot != null && matchingSlot.weaponObject != null)
        {
            matchingSlot.weaponObject.SetActive(true);
            currentWeapon = matchingSlot.weaponObject.GetComponent<Weapon>();
            currentWeaponType = data.weaponType;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWeaponSwitch();
            }
            if (gameUI != null)
            {
                gameUI.UpdateWeaponDisplay(data.weaponType);
            }

            Debug.Log($"Equipped: {data.weaponName}");
        }
        else
        {
            Debug.LogWarning($"No weapon slot found for {data.weaponName}");
        }
    }

    public void EquipWeapon(WeaponType weaponType)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i].weaponData != null && weaponSlots[i].weaponData.weaponType == weaponType)
            {
                EquipWeapon(weaponSlots[i].weaponData);
                return;
            }
        }

        Debug.LogWarning($"No weapon slot found for WeaponType: {weaponType}");
    }

    private void DisableAllWeapons()
    {
        if (weaponSlots == null) return;

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i].weaponObject != null)
            {
                weaponSlots[i].weaponObject.SetActive(false);
            }
        }
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;

        if (!canShoot && currentWeapon != null)
        {
            currentWeapon.StopFiring();
        }
    }

    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
