using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{

public static SoundManager instance;

    [Header("Sources (assign in Inspector)")]
    [SerializeField] AudioSource musicSource;      // basic loop (Play On Awake = ON, Loop = ON)
    [SerializeField] AudioSource bossMusicSource;  // boss loop (Play On Awake = OFF, Loop = ON)

    [Header("Crossfade")]
    [SerializeField, Range(0f, 2f)] float crossfadeSeconds = 1.0f;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.8f; // target volume when enabled

    [Header("State")]
    [SerializeField] bool musicEnabled = true;

    Coroutine xfading;
    AudioSource current;   // which source is currently “live”
    AudioSource other;     // the other source (for quick swaps)

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // sanity defaults
        if (musicSource)      { musicSource.loop = true; }
        if (bossMusicSource)  { bossMusicSource.loop = true; bossMusicSource.playOnAwake = false; }

        // start basic track if enabled
        if (musicEnabled) {
            StartBasicLoop();
        } else {
            // keep them muted if music starts disabled
            if (musicSource)      musicSource.volume = 0f;
            if (bossMusicSource)  bossMusicSource.volume = 0f;
            current = musicSource;
            other   = bossMusicSource;
        }
    }

    // ---------- Public API your game code can call ----------

    /// Call this when entering the boss room.
    public void EnterBossRoom()
    {
        if (!musicEnabled) return;
        if (musicSource == null || bossMusicSource == null) return;
        Crossfade(musicSource, bossMusicSource, crossfadeSeconds);
    }

    /// Call this when the boss is defeated (before/after loot/portal spawns).
    public void BossDefeated()
    {
        if (!musicEnabled) return;
        if (musicSource == null || bossMusicSource == null) return;
        Crossfade(bossMusicSource, musicSource, crossfadeSeconds);
    }

    /// Toggle from Settings UI (hook the button to this).
    public void ToggleMusic()
    {
        SetMusicEnabled(!musicEnabled);
    }

    /// On/Off from Settings UI (or save/load).
    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;

        if (current == null) current = musicSource;
        if (other   == null) other   = bossMusicSource;

        if (enabled)
        {
            // restore the active source to target volume
            if (!current.isPlaying) current.Play();
            current.volume = musicVolume;
            other.volume   = 0f;
        }
        else
        {
            // mute both (keeps position; use .Pause() if you prefer)
            if (xfading != null) StopCoroutine(xfading);
            if (musicSource)     musicSource.volume = 0f;
            if (bossMusicSource) bossMusicSource.volume = 0f;
        }
    }

    // ---------- Internals ----------

    void StartBasicLoop()
    {
        if (musicSource == null) return;
        current = musicSource;
        other   = bossMusicSource;

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
        other   = from;
    }

    IEnumerator CoCrossfade(AudioSource from, AudioSource to, float seconds)
    {
        if (to != null && !to.isPlaying) to.Play();

        float t = 0f;
        float fromStart = from ? from.volume : 0f;
        float toStart   = to   ? to.volume   : 0f;
        float toTarget  = musicVolume;

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime; // unscaled so it still fades during pauses
            float k = Mathf.Clamp01(t / seconds);

            if (from) from.volume = Mathf.Lerp(fromStart, 0f, k);
            if (to)   to.volume   = Mathf.Lerp(toStart,   toTarget, k);

            yield return null;
        }

        if (from) { from.volume = 0f; from.Stop(); } // stop to free CPU
        if (to)   { to.volume   = musicVolume; }
        xfading = null;
    }
}
