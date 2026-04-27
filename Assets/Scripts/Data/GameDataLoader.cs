using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class GameDataLoader : MonoBehaviour
{
    public static GameDataLoader Instance { get; private set; }

    public CategoryData[] Categories { get; private set; }
    public WordData[]     Words      { get; private set; }
    public LevelEntry[]   Levels     { get; private set; }
    public bool           IsLoaded   { get; private set; }

    public event System.Action OnDataLoaded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadAll());
    }

    private IEnumerator LoadAll()
    {
        Categories = new CategoryData[0];
        Words      = new WordData[0];
        Levels     = new LevelEntry[0];

        yield return StartCoroutine(LoadJson<CategoryDataList>("categories.json",
            r => Categories = r.categories));
        yield return StartCoroutine(LoadJson<WordDataList>("words.json",
            r => Words = r.words));
        yield return StartCoroutine(LoadJson<LevelEntryList>("levels.json",
            r => Levels = r.levels));

        IsLoaded = true;
        OnDataLoaded?.Invoke();
    }

    private IEnumerator LoadJson<T>(string fileName, System.Action<T> callback)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Data", fileName);

#if UNITY_ANDROID && !UNITY_EDITOR
        using (UnityWebRequest req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                callback(JsonUtility.FromJson<T>(req.downloadHandler.text));
            else
                Debug.LogError($"[GameDataLoader] Failed to load {fileName}: {req.error}");
        }
#else
        if (File.Exists(path))
        {
            callback(JsonUtility.FromJson<T>(File.ReadAllText(path)));
        }
        else
        {
            Debug.LogError($"[GameDataLoader] File not found: {path}");
        }
        yield return null;
#endif
    }
}
