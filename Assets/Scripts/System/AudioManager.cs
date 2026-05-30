using UnityEngine;

// Centralized audio manager - loads clips from Resources and plays them
// Audio files must be placed in Assets/Resources/Audio/ for this to work
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private static AudioSource sfxSource;
    private static AudioSource sfxSource2;

    // All50+ audio clips (lazy-loaded from Resources)
    private static System.Collections.Generic.Dictionary<string, AudioClip> clips = new();

    private void Awake()
    {
        Instance = this;
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource2 = gameObject.AddComponent<AudioSource>();
        sfxSource2.volume = 0.2f;
    }

    // Play a sound by name. If clip not loaded, tries Resources.Load
    public static void Play(string name)
    {
        if (Instance == null) return;

        if (!clips.ContainsKey(name))
        {
            var clip = Resources.Load<AudioClip>($"Audio/{name}");
            if (clip == null)
            {
                // Audio files not available; silent fail
                return;
            }
            clips[name] = clip;
        }

        sfxSource.PlayOneShot(clips[name]);
    }

    // Play on secondary (quieter) channel
    public static void Play2(string name)
    {
        if (Instance == null) return;
        if (!clips.ContainsKey(name))
        {
            var clip = Resources.Load<AudioClip>($"Audio/{name}");
            if (clip == null) return;
            clips[name] = clip;
        }
        sfxSource2.PlayOneShot(clips[name]);
    }

    public static void PlayRandom(string prefix, int count)
    {
        Play($"{prefix}{Random.Range(1, count + 1)}");
    }
}
