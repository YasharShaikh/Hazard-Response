using UnityEngine;
using MasterStylizedProjectile;

public enum WeaponType
{
    Rifle,
    Shotgun,
    MachineGun
}

public class Weapon : MonoBehaviour
{
    [Header("Weapon Data (ScriptableObject)")]
    public WeaponDataSO weaponData;

    [Header("Scene References")]
    public Transform muzzlePoint; // Where bullets spawn from

    [Header("Legacy Particle System (Optional)")]
    public ParticleSystem bulletParticles;

    // Runtime stats — populated from weaponData in Awake()
    private WeaponType weaponType;
    private float baseDamage;
    private float baseFireRate;
    private float range;
    private float colliderRadius;
    private bool isPiercing;
    private BulletDatas bulletData;
    private int effectIndex;

    // Runtime calculated values
    private float currentDamage;
    private float currentFireRate;
    private ParticleSystem.EmissionModule emissionModule;
    private bool isFiring = false;
    private float lastFireTime = 0f;

    private static Transform bulletContainer;

    void Awake()
    {
        // Load all stats from ScriptableObject
        if (weaponData != null)
        {
            weaponType = weaponData.weaponType;
            baseDamage = weaponData.baseDamage;
            baseFireRate = weaponData.baseFireRate;
            range = weaponData.range;
            colliderRadius = weaponData.colliderRadius;
            isPiercing = weaponData.isPiercing;
            bulletData = weaponData.bulletData;
            effectIndex = weaponData.effectIndex;
        }
        else
        {
            Debug.LogWarning($"Weapon on {gameObject.name} has no WeaponDataSO assigned!");
        }

        currentDamage = baseDamage;
        currentFireRate = baseFireRate;

        if (bulletParticles != null)
        {
            emissionModule = bulletParticles.emission;
            emissionModule.enabled = false;
        }

        // Auto-find muzzle point if not assigned
        if (muzzlePoint == null)
        {
            muzzlePoint = transform;
        }

        // Create bullet container if not exists
        if (bulletContainer == null)
        {
            GameObject container = new GameObject("_BulletContainer");
            bulletContainer = container.transform;
        }
    }

    void Start()
    {
        // Apply any existing multipliers from GameManager
        if (GameManager.Instance != null)
        {
            ApplyMultipliers(GameManager.Instance.damagePercentage, GameManager.Instance.fireRatePercentage);
        }
    }

    void Update()
    {
        // Handle continuous firing when isFiring is true
        if (isFiring && bulletData != null)
        {
            float fireInterval = 1f / currentFireRate;
            if (Time.time >= lastFireTime + fireInterval)
            {
                ShootBullet();
                lastFireTime = Time.time;
            }
        }
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMultipliersChanged.AddListener(ApplyMultipliers);
            
            ApplyMultipliers(GameManager.Instance.damagePercentage, GameManager.Instance.fireRatePercentage);
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMultipliersChanged.RemoveListener(ApplyMultipliers);
        }
        
        StopFiring();
    }

    public void ApplyMultipliers(float damagePercentage, float fireRatePercentage)
    {
        // Calculate new damage: baseDamage * (1 + percentage/100)
        currentDamage = baseDamage * (1f + damagePercentage / 100f);
        
        // Calculate new fire rate: baseFireRate * (1 + percentage/100)
        currentFireRate = baseFireRate * (1f + fireRatePercentage / 100f);

        // Update legacy particle system emission rate if used
        UpdateParticleEmission();

        Debug.Log($"{weaponType}: Damage={currentDamage:F2}, FireRate={currentFireRate:F2}");
    }

    private void UpdateParticleEmission()
    {
        if (bulletParticles != null)
        {
            emissionModule = bulletParticles.emission;
            emissionModule.rateOverTime = currentFireRate;
        }
    }

    public void StartFiring()
    {
        if (!isFiring)
        {
            isFiring = true;

            if (bulletParticles != null)
            {
                emissionModule.enabled = true;
                bulletParticles.Play();
            }

            // Fire first bullet only if cooldown has passed (prevents rapid click exploit)
            if (bulletData != null)
            {
                float fireInterval = 1f / currentFireRate;
                if (Time.time >= lastFireTime + fireInterval)
                {
                    ShootBullet();
                    lastFireTime = Time.time;
                }
            }
        }
    }

    public void StopFiring()
    {
        if (isFiring)
        {
            isFiring = false;

            if (bulletParticles != null)
            {
                emissionModule.enabled = false;
                bulletParticles.Stop();
            }
        }
    }

    private void ShootBullet()
    {
        if (bulletData == null || bulletData.Effects.Count == 0) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponShoot(weaponType);
        }

        int index = Mathf.Clamp(effectIndex, 0, bulletData.Effects.Count - 1);
        EffectsGroup effect = bulletData.Effects[index];

        // Get shoot direction (towards mouse/crosshair)
        Vector3 targetPos = GetMouseTargetPos();
        Vector3 targetDir = (targetPos - muzzlePoint.position).normalized;

        // Spawn muzzle flash (StartParticles)
        if (effect.StartParticles != null)
        {
            var muzzleFlash = Instantiate(effect.StartParticles, muzzlePoint.position, Quaternion.identity, bulletContainer);
            muzzleFlash.transform.forward = targetDir;
            
            // Auto-destroy muzzle flash after particle lifetime
            var ps = muzzleFlash.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float lifetime = ps.main.duration + ps.main.startLifetime.constantMax + 0.5f;
                Destroy(muzzleFlash.gameObject, lifetime);
            }
            else
            {
                Destroy(muzzleFlash.gameObject, 2f);
            }
            
            if (effect.startClip != null)
            {
                var audio = muzzleFlash.gameObject.AddComponent<AudioSource>();
                audio.clip = effect.startClip;
                audio.Play();
            }
        }

        // Spawn bullet projectile
        if (effect.BulletParticles != null)
        {
            var bulletObj = Instantiate(effect.BulletParticles, muzzlePoint.position, Quaternion.identity, bulletContainer);
            bulletObj.transform.forward = targetDir;

            // Add Bullet component for movement and hit detection
            var bullet = bulletObj.gameObject.AddComponent<Bullet>();
            bullet.OnHitEffect = effect.HitParticles;
            bullet.Speed = effect.Speed;
            bullet.isTargeting = effect.isTargeting;
            bullet.isFlatShoot = effect.isFlatShoot;
            bullet.maxDistance = range; // Set max travel distance
            bullet.isPiercing = isPiercing; // Shotgun pierces enemies

            if (effect.hitClip != null)
            {
                bullet.onHitClip = effect.hitClip;
            }
            if (effect.bulletClip != null)
            {
                bullet.bulletClip = effect.bulletClip;
            }

            // Add WeaponBullet component for damage
            var weaponBullet = bulletObj.gameObject.AddComponent<WeaponBullet>();
            weaponBullet.damage = currentDamage;
            weaponBullet.sourceWeapon = this;
            weaponBullet.isPiercing = isPiercing;

            // Add collider for hit detection
            var collider = bulletObj.gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = colliderRadius;
        }
    }

    private Vector3 GetMouseTargetPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f))
        {
            return hit.point;
        }
        
        // If nothing hit, shoot forward at same height
        return muzzlePoint.position + muzzlePoint.forward * 50f;
    }

    public float GetCurrentDamage()
    {
        return currentDamage;
    }

    public float GetCurrentFireRate()
    {
        return currentFireRate;
    }

    public bool IsFiring()
    {
        return isFiring;
    }
}

// Helper component added to bullets for damage dealing
public class WeaponBullet : MonoBehaviour
{
    public float damage = 1f;
    public Weapon sourceWeapon;
    public bool isPiercing = false;
    
    // Track enemies already hit (for piercing bullets)
    private System.Collections.Generic.HashSet<ZombieEnemy> hitEnemies = new System.Collections.Generic.HashSet<ZombieEnemy>();

    void OnTriggerEnter(Collider other)
    {
        // Check if hit enemy
        ZombieEnemy enemy = other.GetComponent<ZombieEnemy>();
        if (enemy != null)
        {
            // Skip if already hit this enemy (for piercing)
            if (hitEnemies.Contains(enemy)) return;
            
            hitEnemies.Add(enemy);
            enemy.TakeDamage(damage);
            Debug.Log($"Bullet hit enemy for {damage} damage!");
        }
    }
}
