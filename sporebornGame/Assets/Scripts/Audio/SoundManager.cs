using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Sources (assign in Inspector)")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource bossMusicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("Sound Effect Clips")]
    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioClip pickupSound;
    [SerializeField] AudioClip nectarPickupSound;
    [SerializeField] AudioClip doorSound;
    [SerializeField] AudioClip footstepSound;
    [SerializeField] AudioClip damageSound;
    [SerializeField] AudioClip enemyHitSound;
    [SerializeField] AudioClip deathSound;

    [Header("Crossfade")]
    [SerializeField, Range(0f, 2f)] float crossfadeSeconds = 1.0f;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.8f;

    [Header("SFX Volumes")]
    [SerializeField, Range(0f, 1f)] float masterSFXVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] float attackVolume = 1f;
    [SerializeField, Range(0f, 1f)] float pickupVolume = 1f;
    [SerializeField, Range(0f, 1f)] float nectarVolume = 1f;
    [SerializeField, Range(0f, 1f)] float doorVolume = 1f;
    [SerializeField, Range(0f, 1f)] float footstepVolume = 1f;
    [SerializeField, Range(0f, 1f)] float damageVolume = 1f;
    [SerializeField, Range(0f, 1f)] float enemyHitVolume = 1f;
    [SerializeField, Range(0f, 1f)] float deathVolume = 1f;

    [Header("State")]
    [SerializeField] bool musicEnabled = true;

    Coroutine xfading;
    AudioSource current;
    AudioSource other;

    float lastEnemyHitTime;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // sanity defaults
        if (musicSource) { musicSource.loop = true; }
        if (bossMusicSource) { bossMusicSource.loop = true; bossMusicSource.playOnAwake = false; }

        SceneManager.sceneLoaded += OnSceneLoaded;

        // start basic track if enabled
        if (musicEnabled)
        {
            StartBasicLoop();
        }
        else
        {
            // keep them muted if music starts disabled
            if (musicSource) musicSource.volume = 0f;
            if (bossMusicSource) bossMusicSource.volume = 0f;
            current = musicSource;
            other = bossMusicSource;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Restart music when loading game scene (scene index 1)
        if (scene.buildIndex == 1 && musicEnabled)
        {
            StartBasicLoop();
        }
    }

    // ---------- Public API ----------

    public void EnterBossRoom()
    {
        if (!musicEnabled) return;
        Crossfade(musicSource, bossMusicSource, crossfadeSeconds);
    }

    public void BossDefeated()
    {
        if (!musicEnabled) return;
        Crossfade(bossMusicSource, musicSource, crossfadeSeconds);
    }

    public void PlayAttackSound() => sfxSource.PlayOneShot(attackSound, masterSFXVolume * attackVolume);
    public void PlayPickupSound() => sfxSource.PlayOneShot(pickupSound, masterSFXVolume * pickupVolume);
    public void PlayNectarPickupSound() => sfxSource.PlayOneShot(nectarPickupSound, masterSFXVolume * nectarVolume);
    public void PlayDoorSound() => sfxSource.PlayOneShot(doorSound, masterSFXVolume * doorVolume);
    public void PlayFootstepSound() => sfxSource.PlayOneShot(footstepSound, masterSFXVolume * footstepVolume);
    public void PlayDamageSound() => sfxSource.PlayOneShot(damageSound, masterSFXVolume * damageVolume);
    public void PlayDeathSound() => sfxSource.PlayOneShot(deathSound, masterSFXVolume * deathVolume);

    public void PlayEnemyHitSound()
    {
        if (Time.time - lastEnemyHitTime < 0.05f) return;
        sfxSource.PlayOneShot(enemyHitSound, masterSFXVolume * enemyHitVolume);
        lastEnemyHitTime = Time.time;
    }

    public void StopAllMusic()
    {
        if (xfading != null) StopCoroutine(xfading);
        musicSource.Stop();
        bossMusicSource.Stop();
        musicSource.volume = 0f;
        bossMusicSource.volume = 0f;
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!musicEnabled);
    }

    /// On/Off from Settings UI (or save/load).
    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;

        if (current == null) current = musicSource;
        if (other == null) other = bossMusicSource;

        if (enabled)
        {
            // restore the active source to target volume
            if (!current.isPlaying) current.Play();
            current.volume = musicVolume;
            other.volume = 0f;
        }
        else
        {
            // mute both (keeps position; use .Pause() if you prefer)
            if (xfading != null) StopCoroutine(xfading);
            if (musicSource) musicSource.volume = 0f;
            if (bossMusicSource) bossMusicSource.volume = 0f;
        }
    }

    // ---------- Internals ----------

    void StartBasicLoop()
    {
        if (musicSource == null) return;
        current = musicSource;
        other = bossMusicSource;

        if (!musicSource.isPlaying) musicSource.Play();
        musicSource.volume = musicVolume;

        if (bossMusicSource)
        {
            bossMusicSource.Stop();
            bossMusicSource.volume = 0f;
        }
    }

    void Crossfade(AudioSource from, AudioSource to, float seconds)
    {
        if (xfading != null) StopCoroutine(xfading);
        xfading = StartCoroutine(CoCrossfade(from, to, seconds));
        current = to;
        other = from;
    }

    IEnumerator CoCrossfade(AudioSource from, AudioSource to, float seconds)
    {
        if (to != null && !to.isPlaying) to.Play();

        float t = 0f;
        float fromStart = from ? from.volume : 0f;
        float toStart = to ? to.volume : 0f;
        float toTarget = musicVolume;

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime; // unscaled so it still fades during pauses
            float k = Mathf.Clamp01(t / seconds);

            if (from) from.volume = Mathf.Lerp(fromStart, 0f, k);
            if (to) to.volume = Mathf.Lerp(toStart, toTarget, k);

            yield return null;
        }

        if (from) { from.volume = 0f; from.Stop(); } // stop to free CPU
        if (to) { to.volume = musicVolume; }
        xfading = null;
    }
}
