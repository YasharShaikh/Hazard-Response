using UnityEngine;
using MasterStylizedProjectile;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game Data/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public WeaponType weaponType;

    [Header("Stats")]
    public float baseDamage = 1f;
    public float baseFireRate = 3f;
    public float range = 20f;
    public float colliderRadius = 0.3f;
    public bool isPiercing = false;

    [Header("Bullet Effects (MasterStylizedProjectiles)")]
    public BulletDatas bulletData;
    public int effectIndex = 0;
}
