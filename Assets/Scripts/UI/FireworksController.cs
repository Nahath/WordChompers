using UnityEngine;
using Coffee.UIExtensions;
using System.Collections;

// Attach to the FireworksUI GameObject (the UIParticle host).
// Fires 4 sequential bursts at random canvas positions when a level or game completes.
public class FireworksController : MonoBehaviour
{
    private UIParticle uiParticle;
    private Transform burstTransform;
    private RectTransform rectTransform;

    void Awake()
    {
        uiParticle      = GetComponent<UIParticle>();
        burstTransform  = transform.Find("Burst");
        rectTransform   = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.LevelComplete || state == GameState.GameComplete)
            StartCoroutine(FireSequence());
    }

    private IEnumerator FireSequence()
    {
        // Middle half of canvas in each axis (25%–75% of screen).
        // Pivot is at center, so local X runs -fullWidth/2 to +fullWidth/2.
        // 25%–75% → random range ±(fullWidth * 0.25).
        float hw = rectTransform.rect.width  * 0.25f;  // e.g. 320 for 1280-wide canvas
        float hh = rectTransform.rect.height * 0.25f;  // e.g. 180 for 720-tall canvas

        for (int i = 0; i < 4; i++)
        {
            burstTransform.localPosition = new Vector3(
                Random.Range(-hw, hw),
                Random.Range(-hh, hh),
                0f
            );

            uiParticle.Play();

            if (i < 3)
                yield return new WaitForSeconds(Random.Range(0.25f, 0.75f));
        }
    }
}
