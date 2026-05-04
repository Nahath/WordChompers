using UnityEngine;
using System.Collections;

// Audio channel design:
//
//   SFX (sfxSource) — fire-and-forget, overlapping.
//     Use for: movement, chomp, monster sounds, level jingles, UI clicks.
//     Multiple SFX can and should stack simultaneously.
//
//   Reminder (categorySource) — one at a time, interruptible, stoppable.
//     Use for: spoken category/letter reminders that play on a loop timer.
//     Stopped immediately on player death so it doesn't talk over the death sequence.
//
//   Spoken (spokenSource) — sequential, one clip finishes before the next starts.
//     Use for: multi-clip spoken sequences (currently unused but wired for future use).
//
// All clips live under Assets/Resources/Audio/ and are loaded by relative path at runtime.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;      // overlapping one-shots
    [SerializeField] private AudioSource categorySource; // stoppable reminder voice
    [SerializeField] private AudioSource spokenSource;   // sequential spoken clips

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
        sfxSource.volume      = masterVolume;
        categorySource.volume = masterVolume;
        spokenSource.volume   = masterVolume;
    }

    public float GetVolume() => masterVolume;

    // SFX channel — overlapping, cannot be stopped individually.
    public void PlaySFX(string resourcePath, float volumeScale = 1f)
    {
        var clip = Load(resourcePath);
        if (clip != null)
            sfxSource.PlayOneShot(clip, masterVolume * volumeScale);
    }

    // Plays a character-specific SFX: appends _girl to the path when the girl is selected.
    public void PlayCharacterSFX(string resourcePath)
    {
        if (GameManager.Instance?.SelectedCharacter == CharacterType.Girl)
            PlaySFX(resourcePath + "_girl", 0.1f);
        else
            PlaySFX(resourcePath);
    }

    // Reminder channel — one at a time, interrupts any currently playing reminder.
    public void PlayReminderSFX(string resourcePath)
    {
        if (categorySource == null) { Debug.LogError("[AudioManager] categorySource not assigned in Inspector"); return; }
        var clip = Load(resourcePath);
        if (clip == null) return;
        categorySource.clip = clip;
        categorySource.Play();
    }

    // Stops the reminder channel immediately (called on player death).
    public void StopReminderAudio()
    {
        if (categorySource != null) categorySource.Stop();
    }

    // Spoken channel — plays clips in order, waiting for each to finish.
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
