using UnityEngine;
using UnityEngine.UI;

// Attach to the pause menu panel (child of the Game scene canvas).
// Panel should be inactive by default.
public class PauseMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button returnToGameButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        volumeSlider.value = AudioManager.Instance.GetVolume();
        volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetVolume);

        returnToGameButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            GameManager.Instance.ResumeGame();
        });

        quitButton.onClick.AddListener(() => Application.Quit());

        GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        gameObject.SetActive(state == GameState.Paused);
    }
}
