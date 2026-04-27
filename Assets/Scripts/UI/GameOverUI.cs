using UnityEngine;
using TMPro;
using System.Collections;

// Attach to the Game Over panel (inactive by default).
// GameplayUI activates/deactivates this panel based on game state.
public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text levelReachedText;
    [SerializeField] private TMP_Text pressAnyButtonText;

    void OnEnable()
    {
        levelReachedText.text = $"You made it to level {GameManager.Instance.CurrentLevel}";
        pressAnyButtonText.gameObject.SetActive(false);
        StartCoroutine(ShowPromptAfterDelay());
    }

    private IEnumerator ShowPromptAfterDelay()
    {
        yield return new WaitForSeconds(4f);
        pressAnyButtonText.gameObject.SetActive(true);
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
