using UnityEngine;
using UnityEngine.UI;

// Drives the GridLines shader's _CellWidthPx / _CellHeightPx from the
// sibling GridLayoutGroup at Start so the shader never needs hardcoded
// cell-size constants separate from the layout definition.
[RequireComponent(typeof(Image))]
public class GridLinesMaterialSync : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    void Start()
    {
        if (gridLayoutGroup == null)
        {
            Debug.LogError("[GridLinesMaterialSync] GridLayoutGroup reference not set.");
            return;
        }
        var img     = GetComponent<Image>();
        var matInst = new Material(img.material);
        matInst.SetFloat("_CellWidthPx",  gridLayoutGroup.cellSize.x);
        matInst.SetFloat("_CellHeightPx", gridLayoutGroup.cellSize.y);
        img.material = matInst;
    }
}
