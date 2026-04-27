using UnityEngine;
using TMPro;
using System.Collections;

// Attach to the Game Over panel (inactive by default).
public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text gameOverText;       // "Game Over"
    [SerializeField] private TMP_Text levelReachedText;   // "You made it to level X"
    [SerializeField] private TMP_Text pressAnyButtonText; // shown after 4 seconds

    void Start()
    {
        pressAnyButtonText.gameObject.SetActive(false);
        GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            gameObject.SetActive(true);
            levelReachedText.text = $"You made it to level {GameManager.Instance.CurrentLevel}";
            pressAnyButtonText.gameObject.SetActive(false);
            StartCoroutine(ShowPromptAfterDelay());
        }
        else
        {
            gameObject.SetActive(false);
        }
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
        // Wait one frame so the key that triggered game over doesn't count.
        yield return null;
        while (!Input.anyKeyDown)
            yield return null;
    }
}
