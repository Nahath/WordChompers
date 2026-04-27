using UnityEngine;
using TMPro;
using System.Collections;

// Attach to the Level Complete panel (inactive by default).
public class LevelCompleteUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text levelCompleteText;
    [SerializeField] private TMP_Text proceedPromptText; // shown after 3 seconds

    void Start()
    {
        proceedPromptText.gameObject.SetActive(false);
        GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.LevelComplete)
        {
            gameObject.SetActive(true);
            proceedPromptText.gameObject.SetActive(false);
            StartCoroutine(ShowPromptAfterDelay());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowPromptAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        proceedPromptText.text = Application.isMobilePlatform
            ? "Tap to proceed"
            : "Press any button to proceed";
        proceedPromptText.gameObject.SetActive(true);

        yield return StartCoroutine(WaitForAnyInput());
        GameManager.Instance.ProceedToNextLevel();
    }

    private IEnumerator WaitForAnyInput()
    {
        yield return null;
        while (!Input.anyKeyDown && Input.touchCount <= 0)
            yield return null;
    }
}
