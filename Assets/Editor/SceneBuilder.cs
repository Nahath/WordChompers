using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class SceneBuilder
{
    private const string PrefabsPath    = "Assets/Prefabs";
    private const string MonstersPath   = "Assets/Prefabs/Monsters";
    private const string MainMenuPath   = "Assets/Scenes/MainMenu.unity";
    private const string GamePath       = "Assets/Scenes/Game.unity";

    private const float CellWidth    = 160f;
    private const float CellHeight   = 100f;
    private const int   GridCols     = 6;
    private const int   GridRows     = 6;
    private const float HeaderHeight = 80f;
    private const float LivesHeight  = 40f;

    // ── Entry Points (commented out — use Unity Editor directly instead) ────────

    // [MenuItem("Word Chompers/Build All (Scenes Only)")]
    // public static void BuildAll()
    // {
    //     EnsureFolders();
    //     BuildMainMenuScene();
    //     BuildGameScene();
    //     UpdateBuildSettings();
    //     AssetDatabase.SaveAssets();
    //     AssetDatabase.Refresh();
    //     Debug.Log("[SceneBuilder] Build complete.");
    // }

    // [MenuItem("Word Chompers/Build MainMenu Scene")]
    // public static void BuildMainMenuScene()
    // {
    //     EnsureFolders();
    //     var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    //     PopulateMainMenu();
    //     EditorSceneManager.SaveScene(scene, MainMenuPath);
    //     Debug.Log("[SceneBuilder] MainMenu scene saved.");
    // }

    // [MenuItem("Word Chompers/Build Game Scene")]
    // public static void BuildGameScene()
    // {
    //     EnsureFolders();
    //     var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    //     PopulateGame();
    //     EditorSceneManager.SaveScene(scene, GamePath);
    //     Debug.Log("[SceneBuilder] Game scene saved.");
    // }

    // [MenuItem("Word Chompers/Create Prefabs Only")]
    // public static void CreatePrefabsOnly()
    // {
    //     EnsureFolders();
    //     CreatePrefabs();
    //     AssetDatabase.SaveAssets();
    //     AssetDatabase.Refresh();
    // }

    // ── Folder Setup ──────────────────────────────────────────────────────────

    private static void EnsureFolders()
    {
        EnsureFolder("Assets",          "Prefabs");
        EnsureFolder("Assets/Prefabs",  "Monsters");
        EnsureFolder("Assets",          "Scenes");
        EnsureFolder("Assets",          "Resources");
        EnsureFolder("Assets/Resources","Audio");
        EnsureFolder("Assets/Resources/Audio", "SFX");
        EnsureFolder("Assets/Resources/Audio", "Spoken");
        EnsureFolder("Assets/Resources/Audio", "Categories");
        EnsureFolder("Assets/Resources/Audio", "Words");
        EnsureFolder("Assets/Resources/Audio", "Letters");
        EnsureFolder("Assets",               "StreamingAssets");
        EnsureFolder("Assets/StreamingAssets","Data");
        EnsureFolder("Assets",               "Shaders");
        EnsureFolder("Assets",               "Materials");
        AssetDatabase.Refresh();
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    // ── Prefab Creation ───────────────────────────────────────────────────────

    private static void CreatePrefabs()
    {
        CreateGridCellPrefab();
        CreateLifeIconPrefab();
        const string MA = "Assets/Animations/Monsters/";
        CreateMonsterPrefab<SquigglerMonster> ("SquigglerMonster",  GameColors.SkyBlue,        MA + "SquigglerAnimationController.overrideController");
        CreateMonsterPrefab<GorblerMonster>   ("GorblerMonster",    GameColors.CoralOrange,    MA + "MonsterAnimator.controller");
        CreateMonsterPrefab<ScaredyMonster>   ("ScaredyMonster",    GameColors.SunshineYellow, MA + "ScaredyAnimationController.overrideController");
        CreateMonsterPrefab<BlagwerrMonster>  ("BlagwerrMonster",   GameColors.PlayfulPurple,  MA + "BlagwerrAnimationController.overrideController");
        CreateMonsterPrefab<GallumpherMonster>("GallumpherMonster", GameColors.SoftTeal,       MA + "GallumpherAnimationController.overrideController");
        CreateMonsterPrefab<ZabyssMonster>    ("ZabyssMonster",     GameColors.DeepNavy,       null); // create ZabyssAnimationController.overrideController to wire this
        Debug.Log("[SceneBuilder] Prefabs created.");
    }

    private static void CreateGridCellPrefab()
    {
        string path = $"{PrefabsPath}/GridCell.prefab";

        var root = new GameObject("GridCell");
        var img  = root.AddComponent<Image>();
        img.color = Color.black;

        var textGO = new GameObject("WordText");
        textGO.transform.SetParent(root.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.color     = GameColors.CreamWhite;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = 18f;
        StretchFull(textGO.GetComponent<RectTransform>());

        var cell = root.AddComponent<GridCell>();
        Wire(cell, "wordText",   tmp);
        Wire(cell, "background", img);

        root.GetComponent<RectTransform>().sizeDelta = new Vector2(CellWidth, CellHeight);

        SavePrefab(root, path);
    }

    private static void CreateLifeIconPrefab()
    {
        string path = $"{PrefabsPath}/LifeIcon.prefab";
        var root = new GameObject("LifeIcon");
        var img  = root.AddComponent<Image>();
        img.color = GameColors.CoralOrange;
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
        SavePrefab(root, path);
    }

    private static void CreateMonsterPrefab<T>(string name, Color color, string controllerPath) where T : MonsterBase
    {
        string path = $"{MonstersPath}/{name}.prefab";

        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            // Prefab already exists — update only color and controller, leave everything else intact.
            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = scope.prefabContentsRoot;
                var img  = root.GetComponent<Image>();
                if (img != null) img.color = color;
                var anim = root.GetComponent<Animator>();
                if (anim != null && controllerPath != null)
                {
                    var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                    if (ctrl != null) anim.runtimeAnimatorController = ctrl;
                    else Debug.LogWarning($"[SceneBuilder] AnimatorController not found at {controllerPath} — assign manually.");
                }
            }
            return;
        }

        // Prefab doesn't exist — create it fresh.
        var newRoot  = new GameObject(name);
        var newImg   = newRoot.AddComponent<Image>();
        newImg.color = color;
        var animator = newRoot.AddComponent<Animator>();
        if (controllerPath != null)
        {
            var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (ctrl != null) animator.runtimeAnimatorController = ctrl;
            else Debug.LogWarning($"[SceneBuilder] AnimatorController not found at {controllerPath} — assign manually.");
        }
        var monster  = newRoot.AddComponent<T>();
        var rt       = newRoot.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80f, 80f);
        Wire(monster, "animator",      animator);
        Wire(monster, "rectTransform", rt);
        SavePrefab(newRoot, path);
    }

    private static void SavePrefab(GameObject root, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    // ── MainMenu Scene ────────────────────────────────────────────────────────

    private static void PopulateMainMenu()
    {
        // Camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.backgroundColor  = GameColors.DeepNavy;
        cam.clearFlags       = CameraClearFlags.SolidColor;

        CreateEventSystem();

        // Persistent singletons — each calls DontDestroyOnLoad(gameObject) in Awake.
        new GameObject("GameManager").AddComponent<GameManager>();

        var amGO         = new GameObject("AudioManager");
        var am           = amGO.AddComponent<AudioManager>();
        var sfxSrc       = amGO.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;
        var spokenSrc    = amGO.AddComponent<AudioSource>();
        spokenSrc.playOnAwake = false;
        Wire(am, "sfxSource",    sfxSrc);
        Wire(am, "spokenSource", spokenSrc);

        new GameObject("GameDataLoader").AddComponent<GameDataLoader>();

        // Canvas
        var canvas = MakeCanvas("Canvas");

        // Full-screen background
        Img(canvas.transform, "Background", GameColors.DeepNavy, stretch: true);

        // Title
        var title    = TMP(canvas.transform, "TitleText", "WORD CHOMPERS", 80f, GameColors.SunshineYellow);
        var titleRT  = title.GetComponent<RectTransform>();
        SetAnchors(titleRT, 0f, 0.7f, 1f, 1f);

        // Subtitle
        var sub   = TMP(canvas.transform, "SubtitleText", "Press 1 for Letters   \u00b7   Press 2 for Words", 24f, GameColors.CreamWhite);
        var subRT = sub.GetComponent<RectTransform>();
        SetAnchors(subRT, 0.1f, 0.25f, 0.9f, 0.45f);

        // Button row
        var row    = new GameObject("ButtonsPanel");
        row.transform.SetParent(canvas.transform, false);
        var hg     = row.AddComponent<HorizontalLayoutGroup>();
        hg.spacing                 = 40f;
        hg.childAlignment          = TextAnchor.MiddleCenter;
        hg.childForceExpandWidth   = false;
        hg.childForceExpandHeight  = false;
        SetAnchors(row.GetComponent<RectTransform>(), 0.1f, 0.45f, 0.9f, 0.7f);

        var lettersBtn = Btn(row.transform, "ChompLettersButton", "CHOMP LETTERS", GameColors.PlayfulPurple, 300f, 80f);
        var wordsBtn   = Btn(row.transform, "ChompWordsButton",   "CHOMP WORDS",   GameColors.CoralOrange,   300f, 80f);

        var ui = canvas.AddComponent<MainMenuUI>();
        Wire(ui, "chompLettersButton", lettersBtn.GetComponent<Button>());
        Wire(ui, "chompWordsButton",   wordsBtn.GetComponent<Button>());
    }

    // ── Game Scene ────────────────────────────────────────────────────────────

    private static void PopulateGame()
    {
        // Camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam   = camGO.AddComponent<Camera>();
        cam.orthographic    = true;
        cam.backgroundColor = Color.black;
        cam.clearFlags      = CameraClearFlags.SolidColor;

        CreateEventSystem();

        // Canvas
        var canvas = MakeCanvas("Canvas");

        // Background
        Img(canvas.transform, "Background", Color.black, stretch: true);

        // Header strip (top 80 px)
        var header = new GameObject("HeaderPanel", typeof(RectTransform));
        header.transform.SetParent(canvas.transform, false);
        var headerRT = header.GetComponent<RectTransform>();
        headerRT.anchorMin       = new Vector2(0f, 1f);
        headerRT.anchorMax       = new Vector2(1f, 1f);
        headerRT.pivot           = new Vector2(0.5f, 1f);
        headerRT.anchoredPosition = Vector2.zero;
        headerRT.sizeDelta       = new Vector2(0f, HeaderHeight);

        var levelNumTMP = TMP(header.transform, "LevelNumberText", "Level 1",
            28f, GameColors.CreamWhite, TextAlignmentOptions.MidlineLeft);
        SetAnchors(levelNumTMP.GetComponent<RectTransform>(), 0f, 0f, 0.25f, 1f);

        var levelHdrTMP = TMP(header.transform, "LevelHeaderText", "FOOD",
            28f, GameColors.SunshineYellow, TextAlignmentOptions.Center);
        SetAnchors(levelHdrTMP.GetComponent<RectTransform>(), 0.25f, 0f, 0.85f, 1f);

        // Gear button (top-right corner)
        var gearGO = Btn(canvas.transform, "GearButton", "\u2699", GameColors.SoftGray, 60f, 60f);
        var gearRT = gearGO.GetComponent<RectTransform>();
        gearRT.anchorMin = gearRT.anchorMax = new Vector2(1f, 1f);
        gearRT.pivot              = new Vector2(1f, 1f);
        gearRT.anchoredPosition   = new Vector2(-5f, -5f);
        gearRT.sizeDelta          = new Vector2(60f, 60f);

        // Grid panel — always exactly GridCols×CellWidth by GridRows×CellHeight so the
        // GridLayoutGroup's fixed cellSize and the GridLines shader UV divisions stay in sync
        // regardless of the Game view resolution.
        float gridW       = CellWidth  * GridCols;                       // 960
        float gridH       = CellHeight * GridRows;                       // 600
        float gridCenterY = (LivesHeight - HeaderHeight) * 0.5f;        // -20 (bias toward header)
        var gridPanel = new GameObject("GridPanel");
        gridPanel.transform.SetParent(canvas.transform, false);
        var gl = gridPanel.AddComponent<GridLayoutGroup>();
        gl.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = GridCols;
        gl.cellSize        = new Vector2(CellWidth, CellHeight);
        gl.spacing         = Vector2.zero;
        var gridRT = gridPanel.GetComponent<RectTransform>();
        gridRT.anchorMin        = gridRT.anchorMax = new Vector2(0.5f, 0.5f);
        gridRT.pivot            = new Vector2(0.5f, 0.5f);
        gridRT.sizeDelta        = new Vector2(gridW, gridH);
        gridRT.anchoredPosition = new Vector2(0f, gridCenterY);

        // Grid lines overlay — transparent except on the lines, renders above cells
        var gridLinesMat = GetOrCreateGridLineMaterial();
        if (gridLinesMat != null)
        {
            var glGO  = new GameObject("GridLines");
            glGO.transform.SetParent(canvas.transform, false);
            var glImg = glGO.AddComponent<Image>();
            glImg.material      = gridLinesMat;
            glImg.raycastTarget = false;
            var glRT = glGO.GetComponent<RectTransform>();
            glRT.anchorMin        = glRT.anchorMax = new Vector2(0.5f, 0.5f);
            glRT.pivot            = new Vector2(0.5f, 0.5f);
            glRT.sizeDelta        = new Vector2(gridW, gridH);
            glRT.anchoredPosition = new Vector2(0f, gridCenterY);
            var glSync = glGO.AddComponent<GridLinesMaterialSync>();
            Wire(glSync, "gridLayoutGroup", gl);
        }

        // Monster container (full canvas, behind UI overlays)
        var monsterCont = new GameObject("MonsterContainer", typeof(RectTransform));
        monsterCont.transform.SetParent(canvas.transform, false);
        StretchFull(monsterCont.GetComponent<RectTransform>());

        // Player (UI Image + PlayerController)
        var playerGO  = new GameObject("Player");
        playerGO.transform.SetParent(canvas.transform, false);
        var playerImg = playerGO.AddComponent<Image>();
        playerImg.color = Color.white;
        playerImg.raycastTarget = false;
        var playerRT  = playerGO.GetComponent<RectTransform>();
        playerRT.sizeDelta = new Vector2(80f, 80f);
        var playerAnim = playerGO.AddComponent<Animator>();
        var playerController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Animations/Muncher/MuncherAnimator.controller");
        if (playerController != null)
            playerAnim.runtimeAnimatorController = playerController;
        else
            Debug.LogError("[SceneBuilder] MuncherAnimator.controller not found — assign it manually to the Player's Animator.");
        var playerCtrl = playerGO.AddComponent<PlayerController>();
        Wire(playerCtrl, "animator",      playerAnim);
        Wire(playerCtrl, "playerImage",   playerImg);
        Wire(playerCtrl, "rectTransform", playerRT);

        // Lives panel (bottom strip)
        var livesPanel = new GameObject("LivesPanel");
        livesPanel.transform.SetParent(canvas.transform, false);
        var lg = livesPanel.AddComponent<HorizontalLayoutGroup>();
        lg.spacing               = 8f;
        lg.childAlignment        = TextAnchor.MiddleCenter;
        lg.childForceExpandWidth = lg.childForceExpandHeight = false;
        var livesRT = livesPanel.GetComponent<RectTransform>();
        livesRT.anchorMin        = new Vector2(0f, 0f);
        livesRT.anchorMax        = new Vector2(1f, 0f);
        livesRT.pivot            = new Vector2(0.5f, 0f);
        livesRT.anchoredPosition = Vector2.zero;
        livesRT.sizeDelta        = new Vector2(0f, LivesHeight);

        // D-pad panel (left strip, 160 px wide, mobile only)
        var dpadPanel = new GameObject("DPadPanel", typeof(RectTransform));
        dpadPanel.transform.SetParent(canvas.transform, false);
        dpadPanel.SetActive(false);
        var dpadRT = dpadPanel.GetComponent<RectTransform>();
        dpadRT.anchorMin        = dpadRT.anchorMax = new Vector2(0.5f, 0.5f);
        dpadRT.pivot            = new Vector2(1f, 0.5f);
        dpadRT.sizeDelta        = new Vector2(CellWidth, gridH);
        dpadRT.anchoredPosition = new Vector2(-gridW * 0.5f, gridCenterY);
        MakeDPadBtn(dpadPanel.transform, "Up",    new Vector2(0.5f,  0.75f), DPadButton.Direction.Up);
        MakeDPadBtn(dpadPanel.transform, "Down",  new Vector2(0.5f,  0.25f), DPadButton.Direction.Down);
        MakeDPadBtn(dpadPanel.transform, "Left",  new Vector2(0.15f, 0.5f),  DPadButton.Direction.Left);
        MakeDPadBtn(dpadPanel.transform, "Right", new Vector2(0.85f, 0.5f),  DPadButton.Direction.Right);

        // Chomp button panel (right strip, mobile only)
        var chompPanel = new GameObject("ChompButtonPanel", typeof(RectTransform));
        chompPanel.transform.SetParent(canvas.transform, false);
        chompPanel.SetActive(false);
        var chompRT = chompPanel.GetComponent<RectTransform>();
        chompRT.anchorMin        = chompRT.anchorMax = new Vector2(0.5f, 0.5f);
        chompRT.pivot            = new Vector2(0f, 0.5f);
        chompRT.sizeDelta        = new Vector2(CellWidth, gridH);
        chompRT.anchoredPosition = new Vector2(gridW * 0.5f, gridCenterY);
        var chompBtnGO = new GameObject("ChompButton");
        chompBtnGO.transform.SetParent(chompPanel.transform, false);
        var chompImg = chompBtnGO.AddComponent<Image>();
        chompImg.color = GameColors.CoralOrange;
        chompBtnGO.AddComponent<Button>();
        chompBtnGO.AddComponent<ChompButton>();
        StretchFull(chompBtnGO.GetComponent<RectTransform>());

        // ── Overlay panels (inactive by default) ──────────────────────────────

        var pausePanel = MakeOverlay(canvas.transform, "PauseMenuPanel", new Color(0f, 0f, 0f, 0.85f));
        var pauseUI    = pausePanel.AddComponent<PauseMenuUI>();
        TMP(pausePanel.transform, "PauseTitleText", "Paused", 48f, GameColors.CreamWhite)
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 120f);
        var volSliderGO = MakeSlider(pausePanel.transform, "VolumeSlider", new Vector2(0f, 30f));
        var returnBtn   = Btn(pausePanel.transform, "ReturnToGameButton", "Resume", GameColors.SkyBlue,  200f, 60f);
        returnBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);
        var quitBtn     = Btn(pausePanel.transform, "QuitButton",          "Quit",   GameColors.CoralOrange, 200f, 60f);
        quitBtn.GetComponent<RectTransform>().anchoredPosition   = new Vector2(0f, -120f);
        Wire(pauseUI, "volumeSlider",       volSliderGO.GetComponent<Slider>());
        Wire(pauseUI, "returnToGameButton", returnBtn.GetComponent<Button>());
        Wire(pauseUI, "quitButton",         quitBtn.GetComponent<Button>());

        var gameOverPanel = MakeOverlay(canvas.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.85f));
        var gameOverUI    = gameOverPanel.AddComponent<GameOverUI>();
        var goText   = CenteredTMP(gameOverPanel.transform, "GameOverText",      "Game Over",               48f, GameColors.CoralOrange,   new Vector2(0f,  80f));
        var goLevel  = CenteredTMP(gameOverPanel.transform, "LevelReachedText",  "You made it to level 1",  28f, GameColors.CreamWhite,    new Vector2(0f,   0f));
        var goPress  = CenteredTMP(gameOverPanel.transform, "PressAnyButtonText","Press any key to return", 20f, GameColors.SoftGray,      new Vector2(0f, -80f));
        goPress.gameObject.SetActive(false);
        Wire(gameOverUI, "gameOverText",       goText);
        Wire(gameOverUI, "levelReachedText",   goLevel);
        Wire(gameOverUI, "pressAnyButtonText", goPress);

        var lvlCompletePanel = MakeOverlay(canvas.transform, "LevelCompletePanel", new Color(0f, 0f, 0f, 0.8f));
        var lvlCompleteUI    = lvlCompletePanel.AddComponent<LevelCompleteUI>();
        var lcText  = CenteredTMP(lvlCompletePanel.transform, "LevelCompleteText", "Level Complete!",          48f, GameColors.SkyBlue,     new Vector2(0f,  60f));
        var lcPress = CenteredTMP(lvlCompletePanel.transform, "ProceedPromptText", "Press any button to proceed", 20f, GameColors.CreamWhite, new Vector2(0f, -60f));
        lcPress.gameObject.SetActive(false);
        Wire(lvlCompleteUI, "levelCompleteText", lcText);
        Wire(lvlCompleteUI, "proceedPromptText", lcPress);

        var gameCompletePanel = MakeOverlay(canvas.transform, "GameCompletePanel", new Color(0f, 0f, 0f, 0.85f));
        var gameCompleteUI    = gameCompletePanel.AddComponent<GameCompleteUI>();
        var gcCongrats  = CenteredTMP(gameCompletePanel.transform, "CongratsText",    "You Won!",              72f, GameColors.SunshineYellow, new Vector2(0f,  80f));
        var gcReturn    = CenteredTMP(gameCompletePanel.transform, "ReturnPromptText","Press any key to return",20f, GameColors.CreamWhite,    new Vector2(0f, -80f));
        gcReturn.gameObject.SetActive(false);
        var fireworksGO = new GameObject("Fireworks");
        fireworksGO.transform.SetParent(gameCompletePanel.transform, false);
        var fireworks = fireworksGO.AddComponent<ParticleSystem>();
        Wire(gameCompleteUI, "congratsText",    gcCongrats);
        Wire(gameCompleteUI, "returnPromptText",gcReturn);
        Wire(gameCompleteUI, "fireworks",       fireworks);

        // ── Non-canvas managers ───────────────────────────────────────────────

        var gridManagerGO = new GameObject("GridManager");
        var gridMgr = gridManagerGO.AddComponent<GridManager>();
        var cellPrefab = AssetDatabase.LoadAssetAtPath<GridCell>($"{PrefabsPath}/GridCell.prefab");
        Wire(gridMgr, "cellPrefab",    cellPrefab);
        Wire(gridMgr, "gridContainer", gridRT);

        var monsterSpawnerGO = new GameObject("MonsterSpawner");
        var spawner = monsterSpawnerGO.AddComponent<MonsterSpawner>();
        Wire(spawner, "squigglerPrefab",  LoadMonster("SquigglerMonster"));
        Wire(spawner, "gorblerPrefab",    LoadMonster("GorblerMonster"));
        Wire(spawner, "scaredyPrefab",    LoadMonster("ScaredyMonster"));
        Wire(spawner, "blagwerrPrefab",   LoadMonster("BlagwerrMonster"));
        Wire(spawner, "gallumpherPrefab", LoadMonster("GallumpherMonster"));
        Wire(spawner, "zabyssprefab",     LoadMonster("ZabyssMonster"));
        Wire(spawner, "monsterContainer", monsterCont.transform);

        var loaderGO = new GameObject("GameSceneLoader");
        var loader   = loaderGO.AddComponent<GameSceneLoader>();
        Wire(loader, "gridManager",      gridMgr);
        Wire(loader, "playerController", playerCtrl);
        Wire(loader, "monsterSpawner",   spawner);

        // GameplayUI (on canvas root — manages HUD)
        var lifeIconPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/LifeIcon.prefab");
        var gameplayUI = canvas.AddComponent<GameplayUI>();
        Wire(gameplayUI, "levelNumberText",  levelNumTMP);
        Wire(gameplayUI, "levelHeaderText",  levelHdrTMP);
        Wire(gameplayUI, "livesContainer",   livesPanel.transform);
        Wire(gameplayUI, "lifeIconPrefab",   lifeIconPrefab);
        Wire(gameplayUI, "gearButton",       gearGO.GetComponent<Button>());
        Wire(gameplayUI, "dpadPanel",        dpadPanel);
        Wire(gameplayUI, "chompButtonPanel", chompPanel);
    }

    // ── Build Settings ────────────────────────────────────────────────────────

    private static void UpdateBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuPath, true),
            new EditorBuildSettingsScene(GamePath,     true),
        };
    }

    // ── Factory Helpers ───────────────────────────────────────────────────────

    private static MonsterBase LoadMonster(string name) =>
        AssetDatabase.LoadAssetAtPath<MonsterBase>($"{MonstersPath}/{name}.prefab");

    private static GameObject MakeCanvas(string name)
    {
        var go     = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private static void CreateEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    private static Image Img(Transform parent, string name, Color color, bool stretch = false)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        if (stretch) StretchFull(go.GetComponent<RectTransform>());
        return img;
    }

    private static TextMeshProUGUI TMP(Transform parent, string name, string text,
        float fontSize, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t   = go.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.color     = color;
        t.alignment = align;
        return t;
    }

    private static TextMeshProUGUI CenteredTMP(Transform parent, string name, string text,
        float fontSize, Color color, Vector2 anchoredPos)
    {
        var t  = TMP(parent, name, text, fontSize, color);
        var rt = t.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot             = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition  = anchoredPos;
        rt.sizeDelta         = new Vector2(900f, 80f);
        return t;
    }

    private static GameObject Btn(Transform parent, string name, string label,
        Color bg, float w, float h)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bg;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var t = textGO.AddComponent<TextMeshProUGUI>();
        t.text      = label;
        t.fontSize  = 22f;
        t.color     = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        StretchFull(textGO.GetComponent<RectTransform>());
        return go;
    }

    private static GameObject MakeOverlay(Transform parent, string name, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.SetActive(false);
        var img = go.AddComponent<Image>();
        img.color = bg;
        StretchFull(go.GetComponent<RectTransform>());
        return go;
    }

    private static GameObject MakeSlider(Transform parent, string name, Vector2 pos)
    {
        var resources = new DefaultControls.Resources();
        var sliderGO  = DefaultControls.CreateSlider(resources);
        sliderGO.name = name;
        sliderGO.transform.SetParent(parent, false);
        var rt = sliderGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300f, 20f);
        rt.anchoredPosition = pos;
        var slider = sliderGO.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = 1f;
        return sliderGO;
    }

    private static void MakeDPadBtn(Transform parent, string dir, Vector2 anchor, DPadButton.Direction direction)
    {
        var go = new GameObject($"DPad_{dir}");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.25f);
        go.AddComponent<Button>();
        var dpad = go.AddComponent<DPadButton>();
        var so   = new SerializedObject(dpad);
        so.FindProperty("direction").enumValueIndex = (int)direction;
        so.ApplyModifiedPropertiesWithoutUndo();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot              = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition   = Vector2.zero;
        rt.sizeDelta          = new Vector2(60f, 60f);
    }

    // ── Material Helpers ──────────────────────────────────────────────────────

    private static Material GetOrCreateGridLineMaterial()
    {
        const string matPath    = "Assets/Materials/GridLines.mat";
        const string shaderPath = "Assets/Shaders/GridLines.shader";

        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null) return mat;

        var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
        if (shader == null)
        {
            Debug.LogError($"[SceneBuilder] GridLines shader not found at {shaderPath}");
            return null;
        }

        mat = new Material(shader);
        mat.SetColor("_BaseColor",    new Color(0.20f, 0.05f, 0.35f, 1f));
        mat.SetColor("_CrestColor",   new Color(0.70f, 0.45f, 0.95f, 1f));
        mat.SetFloat("_Cols",         6f);
        mat.SetFloat("_Rows",         6f);
        mat.SetFloat("_CellWidthPx",  CellWidth);
        mat.SetFloat("_CellHeightPx", CellHeight);
        mat.SetFloat("_LineWidthPx",  8f);
        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();
        return mat;
    }

    // ── Layout Helpers ────────────────────────────────────────────────────────

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
    }

    private static void SetAnchors(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ── Serialized Field Wiring ───────────────────────────────────────────────

    private static void Wire(Object target, string field, Object value)
    {
        var so   = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop == null)
        {
            Debug.LogError($"[SceneBuilder] '{field}' not found on {target.GetType().Name}");
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
