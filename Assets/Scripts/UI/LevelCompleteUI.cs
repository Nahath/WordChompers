using UnityEngine;
using TMPro;
using System.Collections;

// Attach to the Level Complete panel (inactive by default).
// GameplayUI activates/deactivates this panel based on game state.
public class LevelCompleteUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text levelCompleteText;
    [SerializeField] private TMP_Text proceedPromptText;

    void OnEnable()
    {
        proceedPromptText.gameObject.SetActive(false);
        StartCoroutine(ShowPromptAfterDelay());
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
