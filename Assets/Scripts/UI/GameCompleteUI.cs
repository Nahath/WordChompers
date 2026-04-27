using UnityEngine;
using TMPro;
using System.Collections;

// Attach to the Game Complete panel (inactive by default).
// GameplayUI activates/deactivates this panel based on game state.
public class GameCompleteUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text       congratsText;
    [SerializeField] private TMP_Text       returnPromptText;
    [SerializeField] private ParticleSystem fireworks;

    void OnEnable()
    {
        returnPromptText.gameObject.SetActive(false);
        if (fireworks != null) fireworks.Play();
        StartCoroutine(ShowPromptAfterDelay());
    }

    void OnDisable()
    {
        if (fireworks != null) fireworks.Stop();
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
