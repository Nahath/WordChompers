using UnityEngine;
using System.Collections;

// All audio clips must be placed under Assets/Resources/Audio/
// e.g. Assets/Resources/Audio/SFX/sfx_player_move.wav
// e.g. Assets/Resources/Audio/Words/word_apple.wav
// e.g. Assets/Resources/Audio/Letters/letter_a.wav
// e.g. Assets/Resources/Audio/Categories/header_food.wav
// e.g. Assets/Resources/Audio/Spoken/spoken_is_not_a.wav
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource spokenSource;

    private float masterVolume = 1f;
    private Coroutine spokenCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetVolume(float vol)
    {
        masterVolume = Mathf.Clamp01(vol);
        sfxSource.volume    = masterVolume;
        spokenSource.volume = masterVolume;
    }

    public float GetVolume() => masterVolume;

    public void PlaySFX(string resourcePath)
    {
        var clip = Load(resourcePath);
        if (clip != null)
            sfxSource.PlayOneShot(clip, masterVolume);
    }

    // Plays clips in sequence, waiting for each to finish before starting the next.
    public void PlaySequence(string[] resourcePaths)
    {
        if (spokenCoroutine != null) StopCoroutine(spokenCoroutine);
        spokenCoroutine = StartCoroutine(PlaySequenceCoroutine(resourcePaths));
    }

    private IEnumerator PlaySequenceCoroutine(string[] paths)
    {
        spokenSource.Stop();
        foreach (string path in paths)
        {
            var clip = Load(path);
            if (clip == null) continue;
            spokenSource.clip = clip;
            spokenSource.Play();
            yield return new WaitForSeconds(clip.length);
        }
    }

    private AudioClip Load(string resourcePath)
    {
        var clip = Resources.Load<AudioClip>("Audio/" + resourcePath);
        if (clip == null)
            Debug.LogWarning($"[AudioManager] Missing clip: Resources/Audio/{resourcePath}");
        return clip;
    }
}
