#if PUN_2_OR_NEWER
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement; // For Application.CanStreamedLevelBeLoaded

/// <summary>
/// Coordinates lobby flow: connection, room creation, character selection, match start.
/// Why separate? Centralizes lobby logic, makes UI scripts simpler.
/// </summary>
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private RoomCreationUI roomCreationUI;
    [SerializeField] private PlayerListUI playerListUI;
    [SerializeField] private CharacterSelectionUI characterSelectionUI;
    [SerializeField] private GameModeSelectionUI gameModeSelectionUI;
    
    [Header("Start Match UI")]
    [SerializeField] private Button startMatchButton;
    [SerializeField] private GameObject readyPanel;
    
    [Header("Config")]
    [SerializeField] private MatchConfig matchConfig;
    
    [Header("Debug")]
    [SerializeField] private bool logLobbyEvents = true;
    [SerializeField] private bool verboseLogging = true;
    
    private enum LobbyState { Disconnected, Connecting, Connected, InRoom, MatchStarting, MatchActive }
    private LobbyState currentState = LobbyState.Disconnected;
    private string selectedSceneName = ""; // Store selected game mode scene
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        LogState("🔵 [INIT] LobbyManager starting...");
        
        // Validate references
        bool allValid = true;
        if (roomCreationUI == null) { LogState("⚠️ [WARN] roomCreationUI is NULL!"); }
        if (playerListUI == null) { LogState("⚠️ [WARN] playerListUI is NULL!"); }
        if (characterSelectionUI == null) { LogState("⚠️ [WARN] characterSelectionUI is NULL!"); }
        if (matchConfig == null) { LogState("⚠️ [WARN] matchConfig is NULL!"); }
        
        if (allValid)
        {
            LogState("✅ [INIT] All references valid");
        }
        
        // Subscribe to events
        GameEventBus.OnConnectedToMaster += OnConnectedToMaster;
        GameEventBus.OnJoinedRoom += OnJoinedRoom;
        GameEventBus.OnMatchStateChanged += OnMatchStateChanged;
        LogState("✅ [EVENT] Subscribed to network events");
        
        if (startMatchButton != null)
        {
            startMatchButton.onClick.AddListener(OnStartMatchClicked);
            startMatchButton.interactable = false; // Only master can start
            LogState("✅ [WIRE] StartMatchButton → OnStartMatchClicked");
        }
        else
        {
            LogState("⚠️ [WARN] startMatchButton is NULL!");
        }
        
        // Initialize connection
        SetState(LobbyState.Connecting);
        ConnectToPhoton();
        
        LogState("✅ [INIT] LobbyManager initialized");
    }
    
    private void OnDestroy()
    {
        GameEventBus.OnConnectedToMaster -= OnConnectedToMaster;
        GameEventBus.OnJoinedRoom -= OnJoinedRoom;
        GameEventBus.OnMatchStateChanged -= OnMatchStateChanged;
    }
    
    private void Update()
    {
        // Update start button state (only master client can start)
        if (startMatchButton != null && PhotonNetwork.InRoom)
        {
            bool isMaster = PhotonNetwork.IsMasterClient;
            int playerCount = RoomManager.Instance != null ? RoomManager.Instance.GetPlayerCount() : 0;
            int minPlayers = matchConfig != null ? matchConfig.minPlayersToStart : 2;
            bool canStart = isMaster && playerCount >= minPlayers;
            
            if (startMatchButton.interactable != canStart && verboseLogging)
            {
                LogState($"🔄 [BUTTON] StartMatchButton: {(canStart ? "ENABLED ✅" : "DISABLED ❌")} | Master: {isMaster} | Players: {playerCount}/{minPlayers}");
            }
            
            startMatchButton.interactable = canStart;
        }
    }
    
    /// <summary>
    /// Connect to Photon servers.
    /// </summary>
    private void ConnectToPhoton()
    {
        LogState("🔌 [NETWORK] Connecting to Photon...");
        
        if (NetworkBootstrap.Instance == null)
        {
            LogState("❌ [ERROR] NetworkBootstrap.Instance is NULL!");
            SetState(LobbyState.Disconnected);
            return;
        }
        
        bool isConnected = NetworkBootstrap.Instance.IsConnected();
        LogState($"🔌 [NETWORK] Current connection status: {(isConnected ? "CONNECTED ✅" : "NOT CONNECTED ❌")}");
        
        if (!isConnected)
        {
            LogState("🚀 [ACTION] Calling NetworkBootstrap.Connect()...");
            NetworkBootstrap.Instance.Connect();
            SetState(LobbyState.Connecting);
        }
        else
        {
            LogState("✅ [NETWORK] Already connected!");
            SetState(LobbyState.Connected);
        }
    }
    
    /// <summary>
    /// Called when connected to Photon master server.
    /// </summary>
    private void OnConnectedToMaster()
    {
        LogState("🎉 [EVENT] OnConnectedToMaster fired!");
        LogState("✅ [NETWORK] Connected to Photon Master Server");
        SetState(LobbyState.Connected);
        LogState("✅ [STATE] Ready to create/join rooms");
    }
    
    /// <summary>
    /// Called when joined a room.
    /// </summary>
    private void OnJoinedRoom()
    {
        LogState("🎉 [EVENT] OnJoinedRoom fired!");
        
        bool isMaster = PhotonNetwork.IsMasterClient;
        bool isJoiningExistingRoom = false;
        
        // Check if room already has a game mode (means we're joining, not creating)
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameModeName"))
        {
            isJoiningExistingRoom = true;
            string existingGameMode = PhotonNetwork.CurrentRoom.CustomProperties["GameModeName"].ToString();
            LogState($"🏠 [ROOM] Joining existing room with game mode: {existingGameMode}");
            
            // Store the game mode scene for later use
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameModeScene"))
            {
                selectedSceneName = PhotonNetwork.CurrentRoom.CustomProperties["GameModeScene"].ToString();
                LogState($"✅ [STORE] Stored game mode scene from room: {selectedSceneName}");
            }
        }
        else
        {
            LogState("🏠 [ROOM] Creating new room (no game mode set yet)");
        }
        
        if (RoomManager.Instance != null)
        {
            string roomCode = RoomManager.Instance.GetCurrentRoomCode();
            int playerCount = RoomManager.Instance.GetPlayerCount();
            int maxPlayers = RoomManager.Instance.GetMaxPlayers();
            
            LogState($"🏠 [ROOM] Room Code: {roomCode}");
            LogState($"👥 [ROOM] Players: {playerCount}/{maxPlayers}");
            LogState($"👑 [ROOM] Is Master Client: {(isMaster ? "YES ✅" : "NO")}");
        }
        
        // Hide room creation UI
        if (roomCreationUI != null)
        {
            roomCreationUI.gameObject.SetActive(false);
            LogState("✅ [UI] RoomCreationUI deactivated");
        }
        
        // Show player list
        if (playerListUI != null)
        {
            playerListUI.gameObject.SetActive(true);
            playerListUI.RefreshPlayerList();
            LogState("✅ [UI] PlayerListUI activated and refreshed");
        }
        
        // Show character selection UI immediately - FORCE IT
        if (characterSelectionUI != null)
        {
            characterSelectionUI.gameObject.SetActive(true);
            LogState("✅ [UI] CharacterSelectionUI GameObject activated");
            
            // Force enable all child objects
            foreach (Transform child in characterSelectionUI.transform)
            {
                child.gameObject.SetActive(true);
            }
            
            // Wait a frame then refresh (ensures CharacterSelectionManager is ready)
            StartCoroutine(DelayedCharacterRefresh());
        }
        else
        {
            LogState("❌ [ERROR] characterSelectionUI is NULL! Trying to find it...");
            
            // Try to find by GameObject name first
            GameObject panelObj = GameObject.Find("CharacterSelectionPanel");
            if (panelObj == null)
            {
                panelObj = GameObject.Find("characterselectionpanem"); // Try with typo
            }
            if (panelObj == null)
            {
                panelObj = GameObject.Find("CharacterSelectionPanel"); // Try exact case
            }
            
            if (panelObj != null)
            {
                CharacterSelectionUI foundUI = panelObj.GetComponent<CharacterSelectionUI>();
                if (foundUI != null)
                {
                    characterSelectionUI = foundUI;
                    characterSelectionUI.gameObject.SetActive(true);
                    LogState($"✅ [FIX] Found CharacterSelectionUI by GameObject name '{panelObj.name}' and activated!");
                    StartCoroutine(DelayedCharacterRefresh());
                }
                else
                {
                    LogState($"❌ [ERROR] GameObject '{panelObj.name}' found but CharacterSelectionUI component missing!");
                }
            }
            else
            {
                // Try FindObjectOfType as fallback
                CharacterSelectionUI foundUI = FindObjectOfType<CharacterSelectionUI>(true); // Include inactive
                if (foundUI != null)
                {
                    characterSelectionUI = foundUI;
                    characterSelectionUI.gameObject.SetActive(true);
                    LogState("✅ [FIX] Found CharacterSelectionUI via FindObjectOfType (including inactive) and activated!");
                    StartCoroutine(DelayedCharacterRefresh());
                }
                else
                {
                    LogState("❌ [ERROR] CharacterSelectionUI not found in scene by name or type!");
                }
            }
        }
        
        // Hide game mode selection (only show if master client creating new room)
        if (gameModeSelectionUI != null)
        {
            gameModeSelectionUI.SetActive(false);
            if (isJoiningExistingRoom)
            {
                LogState("✅ [UI] GameModeSelectionUI deactivated (joining existing room - skip game mode selection)");
            }
            else
            {
                LogState("✅ [UI] GameModeSelectionUI deactivated (will show after character selection)");
            }
        }
        
        // Hide ready panel initially
        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }
        
        SetState(LobbyState.InRoom);
        LogState("✅ [STATE] Now in room - character selection available");
    }
    
    /// <summary>
    /// Called when match state changes.
    /// </summary>
    private void OnMatchStateChanged()
    {
        LogState("🎉 [EVENT] OnMatchStateChanged fired!");
        
        if (MatchFlowController.Instance == null)
        {
            LogState("❌ [ERROR] MatchFlowController.Instance is NULL!");
            return;
        }
        
        MatchState matchState = MatchFlowController.Instance.GetCurrentState();
        float elapsedTime = MatchFlowController.Instance.GetStateElapsedTime();
        float remainingTime = MatchFlowController.Instance.GetStateRemainingTime();
        
        LogState($"🎮 [MATCH] State: {matchState} | Elapsed: {elapsedTime:F1}s | Remaining: {(remainingTime < 0 ? "∞" : remainingTime.ToString("F1") + "s")}");
        
        // Hide lobby UI when match starts
        if (matchState == MatchState.Countdown || matchState == MatchState.Playing)
        {
            LogState("🔄 [UI] Hiding lobby UI (match active)");
            if (readyPanel != null)
            {
                readyPanel.SetActive(false);
                LogState("✅ [UI] ReadyPanel deactivated");
            }
            if (characterSelectionUI != null)
            {
                characterSelectionUI.gameObject.SetActive(false);
                LogState("✅ [UI] CharacterSelectionUI deactivated");
            }
            
            if (matchState == MatchState.Countdown)
            {
                SetState(LobbyState.MatchStarting);
            }
            else if (matchState == MatchState.Playing)
            {
                SetState(LobbyState.MatchActive);
            }
        }
        // Show lobby UI when returning to lobby
        else if (matchState == MatchState.Lobby)
        {
            LogState("🔄 [UI] Showing lobby UI (returned to lobby)");
            if (readyPanel != null)
            {
                readyPanel.SetActive(true);
                LogState("✅ [UI] ReadyPanel activated");
            }
            if (characterSelectionUI != null)
            {
                characterSelectionUI.gameObject.SetActive(true);
                LogState("✅ [UI] CharacterSelectionUI activated");
            }
            
            SetState(LobbyState.InRoom);
        }
    }
    
    /// <summary>
    /// Force show character selection (called from RoomCreationUI).
    /// </summary>
    public void ForceShowCharacterSelection()
    {
        LogState("🚀 [FORCE] ForceShowCharacterSelection() called");
        
        // Hide room creation UI
        if (roomCreationUI != null)
        {
            roomCreationUI.gameObject.SetActive(false);
            LogState("✅ [UI] RoomCreationUI deactivated");
        }
        
        // Show player list
        if (playerListUI != null)
        {
            playerListUI.gameObject.SetActive(true);
            playerListUI.RefreshPlayerList();
            LogState("✅ [UI] PlayerListUI activated");
        }
        
        // FORCE SHOW CHARACTER SELECTION
        if (characterSelectionUI != null)
        {
            characterSelectionUI.gameObject.SetActive(true);
            LogState("✅ [UI] CharacterSelectionUI FORCE activated");
            
            // Force enable all child objects
            foreach (Transform child in characterSelectionUI.transform)
            {
                child.gameObject.SetActive(true);
            }
            
            StartCoroutine(DelayedCharacterRefresh());
        }
        else
        {
            LogState("❌ [ERROR] characterSelectionUI is NULL! Trying to find it...");
            
            // Try to find by GameObject name first (multiple variations)
            GameObject panelObj = GameObject.Find("CharacterSelectionPanel");
            if (panelObj == null) panelObj = GameObject.Find("characterselectionpanem");
            if (panelObj == null) panelObj = GameObject.Find("CharacterSelectionPanel");
            
            if (panelObj != null)
            {
                CharacterSelectionUI foundUI = panelObj.GetComponent<CharacterSelectionUI>();
                if (foundUI != null)
                {
                    characterSelectionUI = foundUI;
                    characterSelectionUI.gameObject.SetActive(true);
                    LogState($"✅ [FIX] Found CharacterSelectionUI by GameObject name '{panelObj.name}' and activated!");
                    StartCoroutine(DelayedCharacterRefresh());
                }
                else
                {
                    LogState($"❌ [ERROR] GameObject '{panelObj.name}' found but CharacterSelectionUI component missing!");
                }
            }
            else
            {
                // Try FindObjectOfType as fallback (including inactive)
                CharacterSelectionUI foundUI = FindObjectOfType<CharacterSelectionUI>(true);
                if (foundUI != null)
                {
                    characterSelectionUI = foundUI;
                    characterSelectionUI.gameObject.SetActive(true);
                    LogState("✅ [FIX] Found CharacterSelectionUI via FindObjectOfType (including inactive) and activated!");
                    StartCoroutine(DelayedCharacterRefresh());
                }
                else
                {
                    // Last resort: search all GameObjects in scene
                    LogState("🔍 [SEARCH] Searching all GameObjects in scene...");
                    GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
                    foreach (GameObject obj in allObjects)
                    {
                        string objNameLower = obj.name.ToLower();
                        if (objNameLower.Contains("character") && objNameLower.Contains("selection"))
                        {
                            CharacterSelectionUI ui = obj.GetComponent<CharacterSelectionUI>();
                            if (ui != null)
                            {
                                characterSelectionUI = ui;
                                characterSelectionUI.gameObject.SetActive(true);
                                LogState($"✅ [FIX] Found CharacterSelectionUI on GameObject '{obj.name}' and activated!");
                                StartCoroutine(DelayedCharacterRefresh());
                                return;
                            }
                        }
                    }
                    LogState("❌ [ERROR] CharacterSelectionUI not found in scene by any method!");
                }
            }
        }
        
        // Hide game mode selection
        if (gameModeSelectionUI != null)
        {
            gameModeSelectionUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called when character selection is confirmed - show game mode selection (only if master client creating room).
    /// </summary>
    public void OnCharacterSelectionConfirmed()
    {
        LogState("🎉 [EVENT] Character selection confirmed!");
        
        // Check if room already has a game mode (means we're joining, not creating)
        bool isJoiningExistingRoom = false;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameModeName"))
        {
            isJoiningExistingRoom = true;
            string existingGameMode = PhotonNetwork.CurrentRoom.CustomProperties["GameModeName"].ToString();
            LogState($"🏠 [ROOM] Room already has game mode: {existingGameMode} - skipping game mode selection");
            
            // Get the game mode scene
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameModeScene"))
            {
                selectedSceneName = PhotonNetwork.CurrentRoom.CustomProperties["GameModeScene"].ToString();
                LogState($"✅ [STORE] Stored game mode scene from room: {selectedSceneName}");
            }
            
            // Skip game mode selection - go directly to ready panel
            if (readyPanel != null)
            {
                readyPanel.SetActive(true);
                LogState("✅ [UI] ReadyPanel activated (skipped game mode selection)");
            }
            
            // Hide character selection
            if (characterSelectionUI != null)
            {
                characterSelectionUI.gameObject.SetActive(false);
                LogState("✅ [UI] CharacterSelectionUI deactivated");
            }
            
            return; // Don't show game mode selection
        }
        
        // Only master client creating a new room should see game mode selection
        if (!PhotonNetwork.IsMasterClient)
        {
            LogState("⚠️ [WARN] Not master client - should not see game mode selection");
            // Still hide character selection and show ready panel
            if (characterSelectionUI != null)
            {
                characterSelectionUI.gameObject.SetActive(false);
            }
            if (readyPanel != null)
            {
                readyPanel.SetActive(true);
            }
            return;
        }
        
        // Hide character selection
        if (characterSelectionUI != null)
        {
            characterSelectionUI.gameObject.SetActive(false);
            LogState("✅ [UI] CharacterSelectionUI deactivated");
        }
        
        // Find GameModeSelectionUI if reference is null
        if (gameModeSelectionUI == null)
        {
            LogState("⚠️ [WARN] gameModeSelectionUI is NULL! Attempting to find it...");
            
            // Try to find by GameObject name first
            GameObject panelObj = GameObject.Find("GameModeSelectionPanel");
            if (panelObj == null) panelObj = GameObject.Find("GameModeSelectionUI");
            
            if (panelObj != null)
            {
                GameModeSelectionUI foundUI = panelObj.GetComponent<GameModeSelectionUI>();
                if (foundUI != null)
                {
                    gameModeSelectionUI = foundUI;
                    LogState($"✅ [FIX] Found GameModeSelectionUI by GameObject name '{panelObj.name}'!");
                }
                else
                {
                    LogState($"❌ [ERROR] GameObject '{panelObj.name}' found but GameModeSelectionUI component missing!");
                }
            }
            else
            {
                // Try FindObjectOfType as fallback (including inactive)
                GameModeSelectionUI foundUI = FindObjectOfType<GameModeSelectionUI>(true);
                if (foundUI != null)
                {
                    gameModeSelectionUI = foundUI;
                    LogState("✅ [FIX] Found GameModeSelectionUI via FindObjectOfType (including inactive)!");
                }
                else
                {
                    LogState("❌ [ERROR] GameModeSelectionUI not found in scene by any method!");
                    return; // Cannot proceed without UI
                }
            }
        }
        
        // Show game mode selection (only for master client creating new room)
        if (gameModeSelectionUI != null)
        {
            gameModeSelectionUI.SetActive(true);
            LogState("✅ [UI] GameModeSelectionUI activated");
            
            // Refresh mode list to ensure buttons are wired
            gameModeSelectionUI.RefreshModeList();
            LogState("✅ [UI] GameModeSelectionUI refreshed");
        }
        else
        {
            LogState("❌ [ERROR] gameModeSelectionUI is still NULL after all attempts!");
        }
    }
    
    /// <summary>
    /// Called when back button clicked from character selection - return to room creation.
    /// </summary>
    public void OnBackToRoomCreation()
    {
        LogState("🔄 [ACTION] Back to room creation");
        
        // Leave room if in one
        if (PhotonNetwork.InRoom)
        {
            if (RoomManager.Instance != null)
            {
                RoomManager.Instance.LeaveRoom();
            }
        }
        
        // Hide character selection and game mode selection
        if (characterSelectionUI != null) characterSelectionUI.gameObject.SetActive(false);
        if (gameModeSelectionUI != null) gameModeSelectionUI.SetActive(false);
        if (playerListUI != null) playerListUI.gameObject.SetActive(false);
        
        // Show room creation UI
        if (roomCreationUI != null)
        {
            roomCreationUI.gameObject.SetActive(true);
            roomCreationUI.ShowCreateRoomPanel();
            LogState("✅ [UI] RoomCreationUI activated");
        }
    }
    
    /// <summary>
    /// Called when game mode selection is confirmed - show ready panel.
    /// </summary>
    public void OnGameModeSelectionConfirmed(string sceneName)
    {
        LogState($"🎉 [EVENT] Game mode selection confirmed! Scene: {sceneName}");
        
        selectedSceneName = sceneName;
        
        // Hide game mode selection
        if (gameModeSelectionUI != null)
        {
            gameModeSelectionUI.gameObject.SetActive(false);
            LogState("✅ [UI] GameModeSelectionUI deactivated");
        }
        
        // Show ready panel
        if (readyPanel != null)
        {
            readyPanel.SetActive(true);
            LogState("✅ [UI] ReadyPanel activated");
        }
        else
        {
            LogState("⚠️ [WARN] readyPanel is NULL!");
        }
    }
    
    /// <summary>
    /// Called when back button clicked from game mode selection - return to character selection.
    /// </summary>
    public void OnBackToCharacterSelection()
    {
        LogState("🔄 [ACTION] Back to character selection");
        
        // Hide game mode selection
        if (gameModeSelectionUI != null)
        {
            gameModeSelectionUI.SetActive(false);
        }
        
        // Show character selection
        if (characterSelectionUI != null)
        {
            characterSelectionUI.gameObject.SetActive(true);
            LogState("✅ [UI] CharacterSelectionUI activated");
        }
    }
    
    /// <summary>
    /// Start match button clicked (Master Client only).
    /// </summary>
    private void OnStartMatchClicked()
    {
        LogState("🖱️ [CLICK] Start Match button clicked");
        
        bool isMaster = PhotonNetwork.IsMasterClient;
        LogState($"👑 [CHECK] Is Master Client: {(isMaster ? "YES ✅" : "NO ❌")}");
        
        if (!isMaster)
        {
            LogState("❌ [ERROR] Only Master Client can start the match!");
            return;
        }
        
        if (MatchFlowController.Instance == null)
        {
            LogState("❌ [ERROR] MatchFlowController.Instance is NULL!");
            return;
        }
        
        // Check minimum players
        if (RoomManager.Instance != null)
        {
            int playerCount = RoomManager.Instance.GetPlayerCount();
            int maxPlayers = RoomManager.Instance.GetMaxPlayers();
            int minPlayers = matchConfig != null ? matchConfig.minPlayersToStart : 2;
            
            LogState($"👥 [CHECK] Players: {playerCount}/{maxPlayers} | Min Required: {minPlayers}");
            
            if (playerCount < minPlayers)
            {
                LogState($"❌ [ERROR] Need at least {minPlayers} players to start! Current: {playerCount}");
                return;
            }
            
            LogState($"✅ [CHECK] Player count valid: {playerCount} >= {minPlayers}");
        }
        
        // Load the selected game mode scene
        if (string.IsNullOrEmpty(selectedSceneName))
        {
            LogState("❌ [ERROR] No game mode scene selected! Cannot start match.");
            return;
        }

        LogState($"🚀 [ACTION] Loading game mode scene: {selectedSceneName}");
        
        // Verify scene exists
        if (Application.CanStreamedLevelBeLoaded(selectedSceneName))
        {
            LogState($"✅ [CHECK] Scene '{selectedSceneName}' exists and can be loaded");
        }
        else
        {
            LogState($"⚠️ [WARN] Scene '{selectedSceneName}' may not exist! Will attempt to load anyway.");
        }
        
        // Load the scene using Photon (syncs all players)
        PhotonNetwork.LoadLevel(selectedSceneName);
        LogState($"✅ [ACTION] Scene load requested: {selectedSceneName}");
    }
    
    /// <summary>
    /// Set current lobby state and log transition.
    /// </summary>
    private void SetState(LobbyState newState)
    {
        if (currentState != newState)
        {
            LogState($"🔄 [STATE] {currentState} → {newState}");
            currentState = newState;
        }
    }
    
    /// <summary>
    /// Delayed character refresh to ensure CharacterSelectionManager is ready.
    /// </summary>
    private IEnumerator DelayedCharacterRefresh()
    {
        yield return null; // Wait one frame
        
        if (characterSelectionUI != null)
        {
            characterSelectionUI.RefreshCharacterList();
            LogState("✅ [UI] CharacterSelectionUI refreshed after delay");
        }
    }
    
    /// <summary>
    /// Log with state information.
    /// </summary>
    private void LogState(string message)
    {
        if (logLobbyEvents)
        {
            string statePrefix = $"[{currentState}]";
            Debug.Log($"[LobbyManager] {statePrefix} {message}");
        }
    }
}
#else
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Photon not installed - stub.
/// </summary>
public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button startMatchButton;
    
    private void Start()
    {
        if (startMatchButton != null)
        {
            startMatchButton.interactable = false;
        }
        
        Debug.LogWarning("[LobbyManager] Photon PUN2 not installed!");
    }
}
#endif

