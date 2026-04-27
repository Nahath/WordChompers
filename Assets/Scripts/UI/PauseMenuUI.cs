using UnityEngine;
using UnityEngine.UI;

// Attach to the pause menu panel (inactive by default).
// GameplayUI activates/deactivates this panel based on game state.
public class PauseMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button returnToGameButton;
    [SerializeField] private Button quitButton;

    private bool initialized;

    void OnEnable()
    {
        if (initialized) return;
        initialized = true;

        volumeSlider.value = AudioManager.Instance.GetVolume();
        volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetVolume);
        returnToGameButton.onClick.AddListener(GameManager.Instance.ResumeGame);
        quitButton.onClick.AddListener(Application.Quit);
    }
}
