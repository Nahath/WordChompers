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

        // Wait for data if we somehow started before it finished loading.
        while (GameDataLoader.Instance == null || !GameDataLoader.Instance.IsLoaded)
            yield return null;

        GameManager.Instance.BeginLevel();
    }
}
