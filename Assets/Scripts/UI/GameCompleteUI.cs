using UnityEngine;
using TMPro;
using System.Collections;

// Attach to the Game Complete panel (inactive by default).
// Assign a ParticleSystem for the fireworks effect.
public class GameCompleteUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text       congratsText;
    [SerializeField] private TMP_Text       returnPromptText;
    [SerializeField] private ParticleSystem fireworks;

    void Start()
    {
        returnPromptText.gameObject.SetActive(false);
        GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.GameComplete)
        {
            gameObject.SetActive(true);
            if (fireworks != null) fireworks.Play();
            returnPromptText.gameObject.SetActive(false);
            StartCoroutine(ShowPromptAfterDelay());
        }
        else
        {
            gameObject.SetActive(false);
            if (fireworks != null) fireworks.Stop();
        }
    }

    private IEnumerator ShowPromptAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        returnPromptText.text = Application.isMobilePlatform
            ? "Tap to return to main menu"
            : "Press any key to return to the main menu";
        returnPromptText.gameObject.SetActive(true);

        yield return StartCoroutine(WaitForAnyInput());
        GameManager.Instance.ReturnToMainMenu();
    }

    private IEnumerator WaitForAnyInput()
    {
        yield return null;
        while (!Input.anyKeyDown && Input.touchCount <= 0)
            yield return null;
    }
}
