using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Player Sounds")]
    public AudioClip playerHurtSound;
    public AudioClip playerDiedSound;

    [Header("Weapon Sounds")]
    public AudioClip rifleShootSound;
    public AudioClip shotgunShootSound;
    public AudioClip machineGunShootSound;
    public AudioClip weaponSwitchSound;
    [Range(0f, 1f)] public float weaponVolume = 0.7f;

    [Header("Enemy Sounds")]
    public AudioClip zombieDeathSound;
    [Range(0f, 1f)] public float enemyVolume = 0.6f;

    [Header("Game Sounds")]
    public AudioClip levelUpSound;
    public AudioClip waveClearSound;

    void Awake()
    {
        // Singleton pattern (scene-based, not persistent)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlayPlayerHurt()
    {
        PlaySFX(playerHurtSound);
    }

    public void PlayPlayerDied()
    {
        PlaySFX(playerDiedSound);
    }

    public void PlayWeaponShoot(WeaponType weaponType)
    {
        AudioClip clip = null;
        switch (weaponType)
        {
            case WeaponType.Rifle:
                clip = rifleShootSound;
                break;
            case WeaponType.Shotgun:
                clip = shotgunShootSound;
                break;
            case WeaponType.MachineGun:
                clip = machineGunShootSound;
                break;
        }

        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, weaponVolume);
        }
    }

    public void PlayWeaponSwitch()
    {
        PlaySFX(weaponSwitchSound);
    }

    public void PlayZombieDeath()
    {
        if (zombieDeathSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(zombieDeathSound, enemyVolume);
        }
    }

    public void PlayLevelUp()
    {
        PlaySFX(levelUpSound);
    }

    public void PlayWaveClear()
    {
        PlaySFX(waveClearSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
