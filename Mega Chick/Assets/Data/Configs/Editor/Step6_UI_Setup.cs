#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Reflection; // For BindingFlags
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // For List

/// <summary>
/// Complete UI setup script - recreates all lobby UI with proper button wiring.
/// Why? Ensures 100% working buttons via onClick.AddListener in code.
/// </summary>
public class Step6_UI_Setup : EditorWindow
{
    private const string UI_PREFABS_PATH = "Assets/Prefabs/UI";
    private const string LOBBY_SCENE_PATH = "Assets/Scenes/Lobby.unity";
    
    [MenuItem("Mega Chick/Step 6 UI Setup/Complete Setup (Recreate All UI)")]
    public static void CompleteSetup()
    {
        Debug.Log("🚀 Starting Complete UI Setup - Recreating ALL UI...");
        
        // Load or create Lobby scene
        Scene lobbyScene = LoadOrCreateLobbyScene();
        if (!lobbyScene.IsValid())
        {
            Debug.LogError("❌ Failed to load/create Lobby scene!");
            return;
        }
        
        // Clean up old UI
        CleanupOldUI();
        
        // Clean up old managers
        CleanupOldManagers();
        
        // Create Canvas
        Canvas canvas = FindOrCreateCanvas();
        if (canvas == null)
        {
            Debug.LogError("❌ Failed to create Canvas!");
            return;
        }
        
        // Create all UI panels
        CreateRoomCreationUI(canvas.transform);
        CreateRoomListUI(canvas.transform); // NEW: Separate Room List panel
        CreatePlayerListUI(canvas.transform);
        CreateCharacterSelectionUI(canvas.transform);
        CreateGameModeSelectionUI(canvas.transform);
        CreateReadyPanel(canvas.transform);
        
        // Create Managers
        CreateGameModeSelectionManager();
        CreateCharacterSelectionManager();
        
        // Auto-load CharacterData
        AutoLoadCharacterData();
        
        // Create LobbyManager and wire everything
        CreateLobbyManager(canvas.transform);
        
        // Save scene
        EditorSceneManager.SaveScene(lobbyScene);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✅ Complete UI Setup Done! All UI recreated with proper button wiring.");
        EditorUtility.DisplayDialog("Complete Setup Done",
            "All UI recreated successfully!\n\n" +
            "✅ RoomCreationUI (Create Room)\n" +
            "✅ RoomListUI (NEW - Separate room list panel)\n" +
            "✅ PlayerListUI (Updated - horizontal format, no titles)\n" +
            "✅ CharacterSelectionUI\n" +
            "✅ GameModeSelectionUI\n" +
            "✅ GameModeSelectionManager (NEW)\n" +
            "✅ CharacterSelectionManager (with auto-loaded data)\n" +
            "✅ LobbyManager (all wired)\n\n" +
            "All buttons wired via onClick.AddListener!",
            "OK");
    }
    
    private static Scene LoadOrCreateLobbyScene()
    {
        // Try to load existing scene
        Scene scene = EditorSceneManager.OpenScene(LOBBY_SCENE_PATH, OpenSceneMode.Single);
        if (scene.IsValid())
        {
            Debug.Log("✅ Loaded existing Lobby scene");
            return scene;
        }
        
        // Create new scene
        scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, LOBBY_SCENE_PATH);
        Debug.Log("✅ Created new Lobby scene");
        return scene;
    }
    
    private static void CleanupOldUI()
    {
        Debug.Log("🧹 Cleaning up old UI...");
        
        // Find and destroy old UI objects
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        System.Collections.Generic.List<GameObject> toDestroy = new System.Collections.Generic.List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && (obj.name.Contains("RoomCreation") || 
                obj.name.Contains("RoomList") ||
                obj.name.Contains("PlayerList") || 
                obj.name.Contains("CharacterSelection") ||
                obj.name.Contains("GameModeSelection") ||
                obj.name.Contains("LobbyManager")))
            {
                toDestroy.Add(obj);
            }
        }
        
        // Destroy collected objects
        foreach (GameObject obj in toDestroy)
        {
            if (obj != null)
            {
                string objName = obj.name; // Store name before destroying
                Object.DestroyImmediate(obj);
                Debug.Log($"🗑️ Deleted old UI: {objName}");
            }
        }
        
        // Destroy old Canvas if exists (but keep EventSystem)
        Canvas oldCanvas = Object.FindObjectOfType<Canvas>();
        if (oldCanvas != null)
        {
            Object.DestroyImmediate(oldCanvas.gameObject);
            Debug.Log("🗑️ Deleted old Canvas");
        }
    }
    
    private static void CleanupOldManagers()
    {
        Debug.Log("🧹 Cleaning up old managers...");
        
        // Find and destroy old managers
        GameModeSelectionManager oldGameModeManager = Object.FindObjectOfType<GameModeSelectionManager>();
        if (oldGameModeManager != null)
        {
            Object.DestroyImmediate(oldGameModeManager.gameObject);
            Debug.Log("🗑️ Deleted old GameModeSelectionManager");
        }
        
        CharacterSelectionManager oldCharManager = Object.FindObjectOfType<CharacterSelectionManager>();
        if (oldCharManager != null)
        {
            Object.DestroyImmediate(oldCharManager.gameObject);
            Debug.Log("🗑️ Deleted old CharacterSelectionManager");
        }
    }
    
    private static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Debug.Log("✅ Found existing Canvas");
            return canvas;
        }
        
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        Debug.Log("✅ Created new Canvas and EventSystem");
        return canvas;
    }
    
    private static void CreateRoomCreationUI(Transform parent)
    {
        Debug.Log("🔨 Creating RoomCreationUI...");
        
        // Main panel
        GameObject panel = CreatePanel("RoomCreationPanel", parent, new Vector2(0, 0), new Vector2(1920, 1080));
        
        // Create Room Button
        GameObject createButton = CreateButton("CreateRoomButton", panel.transform, "Create Room", new Vector2(0, 200), new Vector2(300, 60));
        SetButtonColor(createButton.GetComponent<Button>(), new Color(0.2f, 0.4f, 0.8f));
        
        // Refresh Room List Button
        GameObject refreshButton = CreateButton("RefreshButton", panel.transform, "Refresh Room List", new Vector2(0, 100), new Vector2(300, 60));
        SetButtonColor(refreshButton.GetComponent<Button>(), new Color(0.3f, 0.6f, 0.3f));
        
        // Status Text
        GameObject statusText = CreateText("StatusText", panel.transform, "Ready to create room", new Vector2(0, -100), 24);
        
        // Room List Panel
        GameObject roomListPanel = CreatePanel("RoomListPanel", panel.transform, new Vector2(0, -200), new Vector2(600, 400));
        roomListPanel.SetActive(false);
        
        // Room List Parent (ScrollView)
        GameObject scrollView = CreateScrollView("RoomListScrollView", roomListPanel.transform, new Vector2(0, 0), new Vector2(580, 380));
        Transform contentParent = scrollView.transform.Find("Viewport/Content");
        
        // Create Room Entry Prefab with proper structure (similar to PlayerEntry)
        // Structure: Horizontal layout with Room Name, Host, Game Mode, Player Count, Join Button
        GameObject roomEntryTemplate = CreatePanel("RoomEntryTemplate", contentParent, Vector2.zero, new Vector2(560, 60));
        Image panelImage = roomEntryTemplate.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color(0.7f, 0.6f, 0.8f, 1f); // Light purple
        }
        
        // Add Horizontal Layout Group
        HorizontalLayoutGroup layoutGroup = roomEntryTemplate.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 10f;
        layoutGroup.padding = new RectOffset(10, 10, 5, 5);
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        
        // Room Name Text (left, flexible width)
        GameObject roomNameText = CreateText("RoomNameText", roomEntryTemplate.transform, "Epic Race Arena", Vector2.zero, 18);
        roomNameText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        RectTransform roomNameRect = roomNameText.GetComponent<RectTransform>();
        roomNameRect.sizeDelta = new Vector2(150, 50);
        roomNameRect.anchorMin = new Vector2(0, 0.5f);
        roomNameRect.anchorMax = new Vector2(0, 0.5f);
        roomNameRect.pivot = new Vector2(0, 0.5f);
        
        // Host Text (left-center)
        GameObject hostText = CreateText("HostText", roomEntryTemplate.transform, "Host: BeanMaster", Vector2.zero, 16);
        RectTransform hostRect = hostText.GetComponent<RectTransform>();
        hostRect.sizeDelta = new Vector2(120, 50);
        hostRect.anchorMin = new Vector2(0, 0.5f);
        hostRect.anchorMax = new Vector2(0, 0.5f);
        hostRect.pivot = new Vector2(0, 0.5f);
        
        // Game Mode Text (center)
        GameObject gameModeText = CreateText("GameModeText", roomEntryTemplate.transform, "Race", Vector2.zero, 18);
        gameModeText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        RectTransform gameModeRect = gameModeText.GetComponent<RectTransform>();
        gameModeRect.sizeDelta = new Vector2(80, 50);
        gameModeRect.anchorMin = new Vector2(0, 0.5f);
        gameModeRect.anchorMax = new Vector2(0, 0.5f);
        gameModeRect.pivot = new Vector2(0, 0.5f);
        
        // Player Count Text (right-center)
        GameObject playerCountText = CreateText("PlayerCountText", roomEntryTemplate.transform, "3/12", Vector2.zero, 18);
        RectTransform playerCountRect = playerCountText.GetComponent<RectTransform>();
        playerCountRect.sizeDelta = new Vector2(80, 50);
        playerCountRect.anchorMin = new Vector2(0, 0.5f);
        playerCountRect.anchorMax = new Vector2(0, 0.5f);
        playerCountRect.pivot = new Vector2(0, 0.5f);
        
        // Join Button (right side) - shows game mode name
        GameObject joinButton = CreateButton("JoinButton", roomEntryTemplate.transform, "Race", Vector2.zero, new Vector2(100, 50));
        SetButtonColor(joinButton.GetComponent<Button>(), new Color(0.5f, 0.2f, 0.8f)); // Purple
        RectTransform joinButtonRect = joinButton.GetComponent<RectTransform>();
        joinButtonRect.anchorMin = new Vector2(0, 0.5f);
        joinButtonRect.anchorMax = new Vector2(0, 0.5f);
        joinButtonRect.pivot = new Vector2(0, 0.5f);
        
        // Save room entry as prefab
        string prefabPath = "Assets/Prefabs/UI/RoomEntryPrefab.prefab";
        EnsureDirectoryExists(Path.GetDirectoryName(prefabPath));
        GameObject roomEntryPrefab = PrefabUtility.SaveAsPrefabAsset(roomEntryTemplate, prefabPath);
        Object.DestroyImmediate(roomEntryTemplate); // Remove template from scene
        
        Debug.Log($"✅ Created RoomEntryPrefab at {prefabPath}");
        
        // Add RoomCreationUI script
        RoomCreationUI roomCreationUI = panel.AddComponent<RoomCreationUI>();
        
        // Assign references via reflection (since fields are private)
        var createRoomPanelField = typeof(RoomCreationUI).GetField("createRoomPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var createRoomButtonField = typeof(RoomCreationUI).GetField("createRoomButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var statusTextField = typeof(RoomCreationUI).GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var roomListPanelField = typeof(RoomCreationUI).GetField("roomListPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var roomListParentField = typeof(RoomCreationUI).GetField("roomListParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var refreshButtonField = typeof(RoomCreationUI).GetField("refreshButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var roomEntryPrefabField = typeof(RoomCreationUI).GetField("roomEntryPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        createRoomPanelField?.SetValue(roomCreationUI, panel);
        createRoomButtonField?.SetValue(roomCreationUI, createButton.GetComponent<Button>());
        statusTextField?.SetValue(roomCreationUI, statusText.GetComponent<TextMeshProUGUI>());
        roomListPanelField?.SetValue(roomCreationUI, roomListPanel);
        roomListParentField?.SetValue(roomCreationUI, contentParent);
        refreshButtonField?.SetValue(roomCreationUI, refreshButton.GetComponent<Button>());
        roomEntryPrefabField?.SetValue(roomCreationUI, roomEntryPrefab);
        
        Debug.Log("✅ RoomCreationUI created and wired");
    }
    
    private static void CreateRoomListUI(Transform parent)
    {
        Debug.Log("🔨 Creating RoomListUI (NEW - Separate room list panel)...");
        
        GameObject panel = CreatePanel("RoomListPanel", parent, new Vector2(600, 0), new Vector2(400, 600));
        panel.SetActive(true); // Active by default to show room list
        
        // Room List Parent (ScrollView)
        GameObject scrollView = CreateScrollView("RoomListScrollView", panel.transform, new Vector2(0, 0), new Vector2(380, 550));
        Transform contentParent = scrollView.transform.Find("Viewport/Content");
        
        // Add Vertical Layout Group to Content for proper spacing
        if (contentParent != null)
        {
            VerticalLayoutGroup layoutGroup = contentParent.gameObject.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = contentParent.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            
            // Add Content Size Fitter to auto-size content
            ContentSizeFitter sizeFitter = contentParent.gameObject.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = contentParent.gameObject.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            Debug.Log("✅ Added VerticalLayoutGroup and ContentSizeFitter to Content");
        }
        
        // Add RoomListUI script
        RoomListUI roomListUI = panel.AddComponent<RoomListUI>();
        
        // Assign references using reflection
        var roomListParentField = typeof(RoomListUI).GetField("roomListParent", BindingFlags.NonPublic | BindingFlags.Instance);
        var roomEntryPrefabField = typeof(RoomListUI).GetField("roomEntryPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        
        roomListParentField?.SetValue(roomListUI, contentParent);
        
        // Try to load room entry prefab (must be RoomEntryPrefab created above)
        GameObject roomEntryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/RoomEntryPrefab.prefab");
        if (roomEntryPrefab == null)
        {
            // Try alternative path
            roomEntryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UI_PREFABS_PATH}/RoomEntryPrefab.prefab");
        }
        
        if (roomEntryPrefab == null)
        {
            Debug.LogError($"❌ Could not find RoomEntryPrefab.prefab! Room list will not display entries.");
            Debug.LogError($"💡 The prefab should have been created in CreateRoomCreationUI().");
            Debug.LogError($"💡 Please run 'Step 6 UI Setup → Complete Setup' again to create the prefab.");
        }
        else
        {
            Debug.Log($"✅ Found room entry prefab: {roomEntryPrefab.name}");
        }
        
        roomEntryPrefabField?.SetValue(roomListUI, roomEntryPrefab);
        
        Debug.Log("✅ RoomListUI created and wired");
    }
    
    private static void CreatePlayerListUI(Transform parent)
    {
        Debug.Log("🔨 Creating PlayerListUI (Updated - horizontal format, no titles)...");
        
        GameObject panel = CreatePanel("PlayerListPanel", parent, new Vector2(-600, 0), new Vector2(400, 600));
        
        // NO TITLES - User doesn't want them (they'll be hidden anyway)
        // But we still create them for reference (they'll be hidden in code)
        GameObject titleText = CreateText("TitleText", panel.transform, "", new Vector2(0, 280), 32);
        titleText.SetActive(false); // Hide immediately
        
        GameObject roomNameText = CreateText("RoomNameText", panel.transform, "", new Vector2(0, 250), 24);
        roomNameText.SetActive(false);
        
        GameObject roomCodeText = CreateText("RoomCodeText", panel.transform, "", new Vector2(0, 210), 18);
        roomCodeText.SetActive(false);
        
        GameObject statusText = CreateText("StatusText", panel.transform, "", new Vector2(0, 180), 18);
        statusText.SetActive(false);
        
        GameObject playerCountText = CreateText("PlayerCountText", panel.transform, "", new Vector2(0, 150), 18);
        playerCountText.SetActive(false);
        
        // Player List Parent (ScrollView) - Full height since no titles
        GameObject scrollView = CreateScrollView("PlayerListScrollView", panel.transform, new Vector2(0, 0), new Vector2(380, 550));
        Transform contentParent = scrollView.transform.Find("Viewport/Content");
        
        // Add PlayerListUI script
        PlayerListUI playerListUI = panel.AddComponent<PlayerListUI>();
        
        // Assign references using reflection
        var playerListParentField = typeof(PlayerListUI).GetField("playerListParent", BindingFlags.NonPublic | BindingFlags.Instance);
        var playerEntryPrefabField = typeof(PlayerListUI).GetField("playerEntryPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        var playerCountTextField = typeof(PlayerListUI).GetField("playerCountText", BindingFlags.NonPublic | BindingFlags.Instance);
        var roomCodeTextField = typeof(PlayerListUI).GetField("roomCodeText", BindingFlags.NonPublic | BindingFlags.Instance);
        var roomNameTextField = typeof(PlayerListUI).GetField("roomNameText", BindingFlags.NonPublic | BindingFlags.Instance);
        var statusTextField = typeof(PlayerListUI).GetField("statusText", BindingFlags.NonPublic | BindingFlags.Instance);
        var titleTextField = typeof(PlayerListUI).GetField("titleText", BindingFlags.NonPublic | BindingFlags.Instance);
        
        playerListParentField?.SetValue(playerListUI, contentParent);
        playerEntryPrefabField?.SetValue(playerListUI, AssetDatabase.LoadAssetAtPath<GameObject>($"{UI_PREFABS_PATH}/PlayerEntry.prefab"));
        playerCountTextField?.SetValue(playerListUI, playerCountText.GetComponent<TextMeshProUGUI>());
        roomCodeTextField?.SetValue(playerListUI, roomCodeText.GetComponent<TextMeshProUGUI>());
        roomNameTextField?.SetValue(playerListUI, roomNameText.GetComponent<TextMeshProUGUI>());
        statusTextField?.SetValue(playerListUI, statusText.GetComponent<TextMeshProUGUI>());
        titleTextField?.SetValue(playerListUI, titleText.GetComponent<TextMeshProUGUI>());
        
        Debug.Log("✅ PlayerListUI created and wired");
    }
    
    private static void CreateCharacterSelectionUI(Transform parent)
    {
        Debug.Log("🔨 Creating CharacterSelectionUI...");
        
        GameObject panel = CreatePanel("CharacterSelectionPanel", parent, new Vector2(0, 0), new Vector2(1920, 1080));
        panel.SetActive(false);
        
        GameObject title = CreateText("Title", panel.transform, "Select Character", new Vector2(0, 450), 36);
        
        // Character Preview Panel
        GameObject previewPanel = CreatePanel("PreviewPanel", panel.transform, new Vector2(0, 0), new Vector2(600, 500));
        
        // Character Icon
        GameObject iconObj = new GameObject("CharacterIcon");
        iconObj.transform.SetParent(previewPanel.transform);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 150);
        iconRect.sizeDelta = new Vector2(200, 200);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        // Character Name
        GameObject nameText = CreateText("CharacterNameText", previewPanel.transform, "Character Name", new Vector2(0, 50), 32);
        
        // Character Index (1 / 5)
        GameObject indexText = CreateText("CharacterIndexText", previewPanel.transform, "1 / 5", new Vector2(0, 0), 20);
        
        // Character Description
        GameObject descText = CreateText("CharacterDescriptionText", previewPanel.transform, "Description", new Vector2(0, -80), 18);
        RectTransform descRect = descText.GetComponent<RectTransform>();
        descRect.sizeDelta = new Vector2(550, 100);
        TextMeshProUGUI descTMP = descText.GetComponent<TextMeshProUGUI>();
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.enableWordWrapping = true;
        
        // Character Ability
        GameObject abilityText = CreateText("CharacterAbilityText", previewPanel.transform, "Special Ability: None", new Vector2(0, -150), 20);
        
        // Character Stats
        GameObject statsText = CreateText("CharacterStatsText", previewPanel.transform, "Speed: 1.0x | Jump: 1.0x | Attack: 10", new Vector2(0, -200), 18);
        
        // Navigation Buttons (Previous/Next)
        GameObject previousButton = CreateButton("PreviousButton", panel.transform, "◀ Previous", new Vector2(-300, -350), new Vector2(150, 60));
        GameObject nextButton = CreateButton("NextButton", panel.transform, "Next ▶", new Vector2(300, -350), new Vector2(150, 60));
        SetButtonColor(previousButton.GetComponent<Button>(), new Color(0.3f, 0.3f, 0.6f));
        SetButtonColor(nextButton.GetComponent<Button>(), new Color(0.3f, 0.3f, 0.6f));
        
        // Action Buttons (Select/Confirm/Back)
        GameObject selectButton = CreateButton("SelectButton", panel.transform, "Select", new Vector2(-200, -450), new Vector2(150, 60));
        GameObject confirmButton = CreateButton("ConfirmButton", panel.transform, "Confirm", new Vector2(0, -450), new Vector2(150, 60));
        GameObject backButton = CreateButton("BackButton", panel.transform, "Back", new Vector2(200, -450), new Vector2(150, 60));
        SetButtonColor(selectButton.GetComponent<Button>(), new Color(0.2f, 0.6f, 0.8f));
        SetButtonColor(confirmButton.GetComponent<Button>(), new Color(0.2f, 0.8f, 0.2f));
        SetButtonColor(backButton.GetComponent<Button>(), new Color(0.6f, 0.0f, 0.0f));
        
        // Add CharacterSelectionUI script
        CharacterSelectionUI characterSelectionUI = panel.AddComponent<CharacterSelectionUI>();
        
        // Assign all references via reflection
        var nameTextField = typeof(CharacterSelectionUI).GetField("characterNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var descTextField = typeof(CharacterSelectionUI).GetField("characterDescriptionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var abilityTextField = typeof(CharacterSelectionUI).GetField("characterAbilityText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var statsTextField = typeof(CharacterSelectionUI).GetField("characterStatsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var iconImageField = typeof(CharacterSelectionUI).GetField("characterIconImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var previousButtonField = typeof(CharacterSelectionUI).GetField("previousButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nextButtonField = typeof(CharacterSelectionUI).GetField("nextButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var selectButtonField = typeof(CharacterSelectionUI).GetField("selectButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var confirmButtonField = typeof(CharacterSelectionUI).GetField("confirmButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var backButtonField = typeof(CharacterSelectionUI).GetField("backButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var indexTextField = typeof(CharacterSelectionUI).GetField("characterIndexText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        nameTextField?.SetValue(characterSelectionUI, nameText.GetComponent<TextMeshProUGUI>());
        descTextField?.SetValue(characterSelectionUI, descText.GetComponent<TextMeshProUGUI>());
        abilityTextField?.SetValue(characterSelectionUI, abilityText.GetComponent<TextMeshProUGUI>());
        statsTextField?.SetValue(characterSelectionUI, statsText.GetComponent<TextMeshProUGUI>());
        iconImageField?.SetValue(characterSelectionUI, iconImage);
        previousButtonField?.SetValue(characterSelectionUI, previousButton.GetComponent<Button>());
        nextButtonField?.SetValue(characterSelectionUI, nextButton.GetComponent<Button>());
        selectButtonField?.SetValue(characterSelectionUI, selectButton.GetComponent<Button>());
        confirmButtonField?.SetValue(characterSelectionUI, confirmButton.GetComponent<Button>());
        backButtonField?.SetValue(characterSelectionUI, backButton.GetComponent<Button>());
        indexTextField?.SetValue(characterSelectionUI, indexText.GetComponent<TextMeshProUGUI>());
        
        Debug.Log("✅ CharacterSelectionUI created with ALL buttons and fields");
    }
    
    private static void CreateGameModeSelectionUI(Transform parent)
    {
        Debug.Log("🔨 Creating GameModeSelectionUI...");
        
        GameObject panel = CreatePanel("GameModeSelectionPanel", parent, new Vector2(0, 0), new Vector2(1920, 1080));
        panel.SetActive(false);
        
        GameObject title = CreateText("Title", panel.transform, "Select Game Mode", new Vector2(0, 400), 32);
        
        // Mode Buttons Parent
        GameObject buttonParent = new GameObject("ModeButtonParent");
        buttonParent.transform.SetParent(panel.transform);
        RectTransform buttonParentRect = buttonParent.AddComponent<RectTransform>();
        buttonParentRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonParentRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonParentRect.anchoredPosition = new Vector2(0, 100);
        buttonParentRect.sizeDelta = new Vector2(1200, 200);
        HorizontalLayoutGroup layout = buttonParent.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        
        // Mode Preview Panel (at bottom)
        GameObject previewPanel = CreatePanel("PreviewPanel", panel.transform, new Vector2(0, -200), new Vector2(600, 200));
        GameObject nameText = CreateText("ModeNameText", previewPanel.transform, "Mode Name", new Vector2(0, 50), 28);
        GameObject descText = CreateText("ModeDescriptionText", previewPanel.transform, "Select a game mode to see description", new Vector2(0, -30), 18);
        RectTransform descRect = descText.GetComponent<RectTransform>();
        descRect.sizeDelta = new Vector2(550, 80);
        TextMeshProUGUI descTMP = descText.GetComponent<TextMeshProUGUI>();
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.enableWordWrapping = true;
        
        // Add GameModeSelectionUI script FIRST (before creating buttons so we can wire them)
        GameModeSelectionUI gameModeSelectionUI = panel.AddComponent<GameModeSelectionUI>();
        
        // Create 5 mode buttons and wire them directly
        string[] modes = { "Race", "FFA", "Hunter", "Zone", "Carry" };
        
        for (int i = 0; i < modes.Length; i++)
        {
            GameObject modeButton = CreateButton($"ModeButton_{modes[i]}", buttonParent.transform, modes[i], Vector2.zero, new Vector2(200, 80));
            SetButtonColor(modeButton.GetComponent<Button>(), new Color(0.4f, 0.4f, 0.6f));
            
            // Wire button directly to SelectMode (now public)
            Button button = modeButton.GetComponent<Button>();
            if (button != null && gameModeSelectionUI != null)
            {
                int capturedIndex = i; // Capture index for lambda
                button.onClick.AddListener(() => gameModeSelectionUI.SelectMode(capturedIndex));
            }
        }
        
        // Confirm and Back Buttons
        GameObject confirmButton = CreateButton("ConfirmModeButton", panel.transform, "Start Game", new Vector2(-100, -400), new Vector2(200, 60));
        GameObject backButton = CreateButton("BackButton", panel.transform, "Back", new Vector2(100, -400), new Vector2(200, 60));
        SetButtonColor(confirmButton.GetComponent<Button>(), new Color(0.2f, 0.8f, 0.2f));
        SetButtonColor(backButton.GetComponent<Button>(), new Color(0.6f, 0.0f, 0.0f));
        
        // Assign references via reflection
        var modeButtonParentField = typeof(GameModeSelectionUI).GetField("modeButtonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nameTextField = typeof(GameModeSelectionUI).GetField("selectedModeNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var descTextField = typeof(GameModeSelectionUI).GetField("selectedModeDescriptionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var confirmButtonField = typeof(GameModeSelectionUI).GetField("confirmModeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var backButtonField = typeof(GameModeSelectionUI).GetField("backButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        modeButtonParentField?.SetValue(gameModeSelectionUI, buttonParent.transform);
        nameTextField?.SetValue(gameModeSelectionUI, nameText.GetComponent<TextMeshProUGUI>());
        descTextField?.SetValue(gameModeSelectionUI, descText.GetComponent<TextMeshProUGUI>());
        confirmButtonField?.SetValue(gameModeSelectionUI, confirmButton.GetComponent<Button>());
        backButtonField?.SetValue(gameModeSelectionUI, backButton.GetComponent<Button>());
        
        Debug.Log("✅ GameModeSelectionUI created");
    }
    
    private static void CreateReadyPanel(Transform parent)
    {
        Debug.Log("🔨 Creating ReadyPanel with Start Match button...");
        
        // Create Ready Panel
        GameObject readyPanel = CreatePanel("ReadyPanel", parent, new Vector2(0, -500), new Vector2(400, 150));
        readyPanel.SetActive(false); // Hidden initially
        
        // Create Start Match Button
        GameObject startMatchButton = CreateButton("StartMatchButton", readyPanel.transform, "Start Match", new Vector2(0, 0), new Vector2(300, 60));
        SetButtonColor(startMatchButton.GetComponent<Button>(), new Color(0.2f, 0.8f, 0.2f));
        
        // Add text below button (optional - shows player count requirement)
        GameObject infoText = CreateText("InfoText", readyPanel.transform, "Waiting for players...", new Vector2(0, -40), 16);
        TextMeshProUGUI infoTMP = infoText.GetComponent<TextMeshProUGUI>();
        infoTMP.color = new Color(0.8f, 0.8f, 0.8f);
        
        Debug.Log("✅ ReadyPanel created with StartMatchButton");
    }
    
    private static void CreateLobbyManager(Transform parent)
    {
        Debug.Log("🔨 Creating LobbyManager...");
        
        GameObject managerObj = new GameObject("LobbyManager");
        managerObj.transform.SetParent(parent);
        
        LobbyManager lobbyManager = managerObj.AddComponent<LobbyManager>();
        
        // Find UI components
        RoomCreationUI roomCreationUI = Object.FindObjectOfType<RoomCreationUI>();
        PlayerListUI playerListUI = Object.FindObjectOfType<PlayerListUI>();
        CharacterSelectionUI characterSelectionUI = Object.FindObjectOfType<CharacterSelectionUI>();
        GameModeSelectionUI gameModeSelectionUI = Object.FindObjectOfType<GameModeSelectionUI>();
        
        // Find Ready Panel and Start Match Button
        GameObject readyPanel = GameObject.Find("ReadyPanel");
        Button startMatchButton = null;
        if (readyPanel != null)
        {
            startMatchButton = readyPanel.GetComponentInChildren<Button>();
        }
        
        // Assign references
        var roomCreationUIField = typeof(LobbyManager).GetField("roomCreationUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerListUIField = typeof(LobbyManager).GetField("playerListUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var characterSelectionUIField = typeof(LobbyManager).GetField("characterSelectionUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gameModeSelectionUIField = typeof(LobbyManager).GetField("gameModeSelectionUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var startMatchButtonField = typeof(LobbyManager).GetField("startMatchButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var readyPanelField = typeof(LobbyManager).GetField("readyPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        roomCreationUIField?.SetValue(lobbyManager, roomCreationUI);
        playerListUIField?.SetValue(lobbyManager, playerListUI);
        characterSelectionUIField?.SetValue(lobbyManager, characterSelectionUI);
        gameModeSelectionUIField?.SetValue(lobbyManager, gameModeSelectionUI);
        startMatchButtonField?.SetValue(lobbyManager, startMatchButton);
        readyPanelField?.SetValue(lobbyManager, readyPanel);
        
        Debug.Log("✅ LobbyManager created and wired (including ReadyPanel and StartMatchButton)");
    }
    
    private static void CreateGameModeSelectionManager()
    {
        Debug.Log("🔨 Creating GameModeSelectionManager...");
        
        // Check if already exists
        GameModeSelectionManager existing = Object.FindObjectOfType<GameModeSelectionManager>();
        if (existing != null)
        {
            Debug.Log("✅ GameModeSelectionManager already exists!");
            return;
        }
        
        // Try to load prefab first
        string prefabPath = "Assets/Prefabs/Managers/GameModeSelectionManager.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        GameObject managerObj;
        if (prefab != null)
        {
            managerObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Debug.Log($"✅ Instantiated GameModeSelectionManager from prefab: {prefabPath}");
        }
        else
        {
            // Create new GameObject
            managerObj = new GameObject("GameModeSelectionManager");
            managerObj.AddComponent<GameModeSelectionManager>();
            Debug.Log("✅ Created new GameModeSelectionManager GameObject");
        }
        
        Debug.Log("✅ GameModeSelectionManager created");
    }
    
    private static void CreateCharacterSelectionManager()
    {
        Debug.Log("🔨 Creating CharacterSelectionManager...");
        
        // Check if already exists
        CharacterSelectionManager existing = Object.FindObjectOfType<CharacterSelectionManager>();
        if (existing != null)
        {
            Debug.Log("✅ CharacterSelectionManager already exists!");
            return;
        }
        
        // Try to load prefab first
        string prefabPath = "Assets/Prefabs/Managers/CharacterSelectionManager.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        GameObject managerObj;
        if (prefab != null)
        {
            managerObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Debug.Log($"✅ Instantiated CharacterSelectionManager from prefab: {prefabPath}");
        }
        else
        {
            // Create new GameObject
            managerObj = new GameObject("CharacterSelectionManager");
            managerObj.AddComponent<CharacterSelectionManager>();
            Debug.Log("✅ Created new CharacterSelectionManager GameObject");
        }
        
        Debug.Log("✅ CharacterSelectionManager created");
    }
    
    private static void AutoLoadCharacterData()
    {
        Debug.Log("🔨 Auto-loading CharacterData...");
        
        // Find all CharacterData assets
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        
        if (guids.Length == 0)
        {
            Debug.LogWarning("⚠️ No CharacterData assets found!");
            return;
        }
        
        // Find CharacterSelectionManager in scene
        CharacterSelectionManager manager = Object.FindObjectOfType<CharacterSelectionManager>();
        if (manager == null)
        {
            Debug.LogWarning("⚠️ CharacterSelectionManager not found in scene! Cannot auto-load characters.");
            return;
        }
        
        // Load characters using SerializedObject
        SerializedObject serializedManager = new SerializedObject(manager);
        SerializedProperty availableCharactersProp = serializedManager.FindProperty("availableCharacters");
        
        if (availableCharactersProp != null)
        {
            availableCharactersProp.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                string charPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                CharacterData charData = AssetDatabase.LoadAssetAtPath<CharacterData>(charPath);
                availableCharactersProp.GetArrayElementAtIndex(i).objectReferenceValue = charData;
            }
            serializedManager.ApplyModifiedProperties();
            Debug.Log($"✅ Auto-loaded {guids.Length} CharacterData assets into CharacterSelectionManager!");
        }
        else
        {
            Debug.LogWarning("⚠️ Could not find 'availableCharacters' property in CharacterSelectionManager!");
        }
    }
    
    // Helper methods
    private static GameObject CreatePanel(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        return panel;
    }
    
    private static GameObject CreateButton(string name, Transform parent, string text, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = Color.white;
        
        Button button = buttonObj.AddComponent<Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = 20;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;
        
        return buttonObj;
    }
    
    private static void SetButtonColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f);
        colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f);
        button.colors = colors;
    }
    
    private static GameObject CreateText(string name, Transform parent, string text, Vector2 position, float fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 50);
        
        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;
        
        return textObj;
    }
    
    private static GameObject CreateScrollView(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject scrollView = new GameObject(name);
        scrollView.transform.SetParent(parent);
        RectTransform rect = scrollView.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = scrollView.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        Mask mask = viewport.AddComponent<Mask>();
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        
        return scrollView;
    }
    
    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
#endif
