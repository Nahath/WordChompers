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

    public void SetWord(string word, bool isValid)
    {
        DisplayText   = word;
        HasContent    = true;
        IsValidTarget = isValid;
        wordText.text = word;
        wordText.gameObject.SetActive(true);
        SetBackground(false);
    }

    public void SetLetter(char letter, bool isValid)
    {
        SetWord(letter.ToString(), isValid);
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
        background.color = Color.black;
    }
}
