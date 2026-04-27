using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to the GridCell prefab. Requires: Image (background), TMP_Text (word text).
public class GridCell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text wordText;
    [SerializeField] private Image    background;

    public string DisplayText    { get; private set; }
    public bool   HasContent     { get; private set; }
    public bool   IsValidTarget  { get; private set; }

    // ── Content ───────────────────────────────────────────────────────────────

    public void SetWord(string word, bool isValid, float fontSize = 18f)
    {
        DisplayText       = word;
        HasContent        = true;
        IsValidTarget     = isValid;
        wordText.fontSize = fontSize;
        wordText.text     = word;
        wordText.gameObject.SetActive(true);
        SetBackground(false);
    }

    public void SetLetter(char letter, bool isValid, float fontSize = 36f)
    {
        SetWord(letter.ToString(), isValid, fontSize);
    }

    public void ClearContent()
    {
        DisplayText   = string.Empty;
        HasContent    = false;
        IsValidTarget = false;
        wordText.text = string.Empty;
        wordText.gameObject.SetActive(false);
        SetBackground(false);
        SetPlayerOccupied(false);
    }

    // ── Visual State ──────────────────────────────────────────────────────────

    // Call when the player enters/leaves this cell so the border highlights.
    public void SetPlayerOccupied(bool occupied) { }

    private void SetBackground(bool isEmpty)
    {
        if (background == null) return;
        background.color = Color.clear;
    }
}
