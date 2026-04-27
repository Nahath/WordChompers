using UnityEngine;
using System.Collections;

// Attach to a GameObject in the Game scene.
// Wire all references in the inspector, then this script calls GameManager.BeginLevel()
// once Unity's layout has settled and data is loaded.
public class GameSceneLoader : MonoBehaviour
{
    [Header("Scene Components")]
    [SerializeField] private GridManager      gridManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MonsterSpawner   monsterSpawner;

    [Header("Clipping")]
    [SerializeField] private RectTransform gridPanel;
    [SerializeField] private RectTransform monsterContainer;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[GameSceneLoader] GameManager not found. " +
                           "Always start the game from the MainMenu scene.");
            return;
        }

        GameManager.Instance.RegisterSceneComponents(gridManager, playerController, monsterSpawner);
        StartCoroutine(WaitThenBegin());
    }

    private IEnumerator WaitThenBegin()
    {
        // One frame so Unity's layout system computes GridLayoutGroup positions.
        yield return null;

        // Match MonsterContainer to GridPanel so RectMask2D clips exactly at the grid boundary.
        if (gridPanel != null && monsterContainer != null)
        {
            monsterContainer.anchorMin        = gridPanel.anchorMin;
            monsterContainer.anchorMax        = gridPanel.anchorMax;
            monsterContainer.pivot            = gridPanel.pivot;
            monsterContainer.sizeDelta        = gridPanel.sizeDelta;
            monsterContainer.anchoredPosition = gridPanel.anchoredPosition;
        }

        // Wait for data if we somehow started before it finished loading.
        while (GameDataLoader.Instance == null || !GameDataLoader.Instance.IsLoaded)
            yield return null;

        GameManager.Instance.BeginLevel();
    }
}
