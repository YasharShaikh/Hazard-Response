using UnityEngine;

public enum PowerupCategory
{
    Stat,
    Ability
}

[CreateAssetMenu(fileName = "NewPowerup", menuName = "Game Data/Powerup Data")]
public class PowerupDataSO : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public PowerupCategory category = PowerupCategory.Stat;

    [Header("Stat Powerup")]
    [Tooltip("Used by GameManager.ApplyPowerup() for stat-type powerups")]
    public PowerupType powerupType;

    [Tooltip("The value this powerup applies (e.g., 10 = +10% for speed/damage/fire rate, 30 = heal 30 HP, 10 = +10% max health)")]
    public float statModifierValue = 10f;

    [Header("Future Use")]
    public string description;
    public Sprite icon;

    [Tooltip("For future ability-type powerups (e.g., helper robot, rotating shield)")]
    public GameObject abilityPrefab;
}
