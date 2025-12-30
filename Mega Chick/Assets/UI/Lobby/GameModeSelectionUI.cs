#if PUN_2_OR_NEWER
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// UI for selecting game mode (Race, FFA, Hunter, Zone, Carry).
/// Each mode = different scene.
/// </summary>
public class GameModeSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class GameMode
    {
        public string modeName;
        public string sceneName;
        public string description;
        public Sprite icon;
    }
    
    [Header("Game Modes")]
    [SerializeField] private List<GameMode> gameModes = new List<GameMode>
    {
        new GameMode { modeName = "Race", sceneName = "Race", description = "Race to the finish!" },
        new GameMode { modeName = "FFA", sceneName = "FFA", description = "Free For All - Last chick standing!" },
        new GameMode { modeName = "Hunter", sceneName = "Hunter", description = "King Mega Chick mode!" },
        new GameMode { modeName = "Zone", sceneName = "Zone", description = "Control the zones!" },
        new GameMode { modeName = "Carry", sceneName = "Carry", description = "Carry the egg!" }
    };
    
    [Header("UI References")]
    [SerializeField] private Transform modeButtonParent;
    [SerializeField] private GameObject modeButtonPrefab;
    [SerializeField] private TextMeshProUGUI selectedModeNameText;
    [SerializeField] private TextMeshProUGUI selectedModeDescriptionText;
    [SerializeField] private Image selectedModeIconImage;
    [SerializeField] private Button confirmModeButton;
    [SerializeField] private Button backButton; // Back to character selection
    
    [Header("Debug")]
    [SerializeField] private bool logUIEvents = true;
    
    private List<GameObject> modeButtons = new List<GameObject>();
    private int selectedModeIndex = 0;
    private string selectedSceneName = "";
    
    private void Start()
    {
        LogState("🔵 [INIT] GameModeSelectionUI starting...");
        
        if (confirmModeButton != null)
        {
            confirmModeButton.onClick.AddListener(OnConfirmModeClicked);
            LogState("✅ [WIRE] ConfirmModeButton → OnConfirmModeClicked");
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
            LogState("✅ [WIRE] BackButton → OnBackClicked");
        }
        
        RefreshModeList();
        
        // Wire existing buttons if they exist
        WireExistingButtons();
        
        LogState("✅ [INIT] GameModeSelectionUI initialized");
    }
    
    /// <summary>
    /// Wire existing mode buttons in scene.
    /// </summary>
    private void WireExistingButtons()
    {
        if (modeButtonParent == null)
        {
            LogState("⚠️ [WARN] modeButtonParent is NULL, cannot wire buttons");
            return;
        }
        
        Button[] buttons = modeButtonParent.GetComponentsInChildren<Button>();
        int wiredCount = 0;
        
        foreach (Button btn in buttons)
        {
            if (btn.name.StartsWith("ModeButton_"))
            {
                string modeName = btn.name.Replace("ModeButton_", "");
                int index = gameModes.FindIndex(m => m.modeName == modeName);
                
                if (index >= 0)
                {
                    btn.onClick.RemoveAllListeners();
                    int capturedIndex = index;
                    btn.onClick.AddListener(() => {
                        LogState($"[BUTTON] {btn.name} clicked, selecting mode index {capturedIndex}");
                        SelectMode(capturedIndex);
                    });
                    wiredCount++;
                    LogState($"✅ [WIRE] Wired {btn.name} → SelectMode({index}) [{gameModes[index].modeName}]");
                }
                else
                {
                    LogState($"⚠️ [WARN] Mode '{modeName}' not found in gameModes list!");
                }
            }
        }
        
        if (wiredCount > 0)
        {
            LogState($"✅ [WIRE] Wired {wiredCount} existing mode buttons");
        }
    }
    
    /// <summary>
    /// Refresh mode list UI.
    /// </summary>
    public void RefreshModeList()
    {
        LogState("🔄 [UI] Refreshing game mode list...");
        
        if (modeButtonParent == null)
        {
            LogState("❌ [ERROR] modeButtonParent is NULL! Cannot refresh mode list.");
            return;
        }
        
        // First, check if buttons already exist in scene (created by setup script)
        Button[] existingButtons = modeButtonParent.GetComponentsInChildren<Button>(true); // Include inactive
        bool hasExistingButtons = false;
        
        foreach (Button btn in existingButtons)
        {
            if (btn != null && btn.name.StartsWith("ModeButton_"))
            {
                hasExistingButtons = true;
                break;
            }
        }
        
        // If buttons exist in scene, wire them (don't destroy or recreate)
        if (hasExistingButtons)
        {
            LogState($"✅ [FOUND] Found existing mode buttons in scene, wiring them...");
            
            // Clear the modeButtons list first to avoid duplicates
            modeButtons.Clear();
            
            // Find and wire existing buttons
            foreach (Button btn in existingButtons)
            {
                if (btn != null && btn.name.StartsWith("ModeButton_"))
                {
                    modeButtons.Add(btn.gameObject);
                    // Extract mode name from button name
                    string modeName = btn.name.Replace("ModeButton_", "");
                    int index = gameModes.FindIndex(m => m.modeName == modeName);
                    if (index >= 0)
                    {
                        int capturedIndex = index;
                        btn.onClick.RemoveAllListeners(); // Clear any existing listeners
                        btn.onClick.AddListener(() => {
                            LogState($"[BUTTON] {btn.name} clicked, selecting mode index {capturedIndex}");
                            SelectMode(capturedIndex);
                        });
                        LogState($"✅ [WIRE] Wired existing button: {btn.name} → SelectMode({index}) [{gameModes[index].modeName}]");
                    }
                    else
                    {
                        LogState($"⚠️ [WARN] Mode '{modeName}' not found in gameModes list!");
                    }
                }
            }
            
            LogState($"✅ [WIRE] Successfully wired {modeButtons.Count} existing mode buttons");
        }
        else if (modeButtonPrefab != null)
        {
            // No existing buttons, create new ones using prefab
            LogState("📦 [CREATE] No existing buttons found, creating from prefab...");
            
            // Clear existing buttons
            foreach (var button in modeButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            modeButtons.Clear();
            
            // Create buttons for each mode
            for (int i = 0; i < gameModes.Count; i++)
            {
                CreateModeButton(gameModes[i], i);
            }
            
            LogState($"✅ [CREATE] Created {modeButtons.Count} mode buttons from prefab");
        }
        else
        {
            LogState("❌ [ERROR] No existing buttons found AND modeButtonPrefab is NULL! Cannot create buttons.");
            LogState("💡 [TIP] Either create buttons manually in scene OR assign modeButtonPrefab in inspector.");
        }
        
        // Select first mode by default
        if (gameModes.Count > 0 && modeButtons.Count > 0)
        {
            SelectMode(0);
            LogState("✅ [SELECT] Selected first mode by default");
        }
        else
        {
            LogState("⚠️ [WARN] Cannot select first mode - no modes or buttons available!");
        }
    }
    
    /// <summary>
    /// Create mode button.
    /// </summary>
    private void CreateModeButton(GameMode mode, int index)
    {
        if (modeButtonPrefab == null || modeButtonParent == null)
        {
            LogState("❌ [ERROR] Mode button prefab or parent not assigned!");
            return;
        }
        
        GameObject buttonObj = Instantiate(modeButtonPrefab, modeButtonParent);
        modeButtons.Add(buttonObj);
        
        // Setup button
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            int capturedIndex = index;
            button.onClick.AddListener(() => SelectMode(capturedIndex));
            LogState($"✅ [WIRE] ModeButton[{index}] → SelectMode({index})");
        }
        
        // Setup button UI
        TextMeshProUGUI nameText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = mode.modeName;
        }
        
        Image iconImage = buttonObj.GetComponentsInChildren<Image>()[1]; // Second image is icon
        if (iconImage != null && mode.icon != null)
        {
            iconImage.sprite = mode.icon;
        }
    }
    
    /// <summary>
    /// Select game mode (preview). Made public for button wiring.
    /// </summary>
    public void SelectMode(int index)
    {
        LogState($"🖱️ [SELECT] Game mode index {index} selected");
        
        if (index < 0 || index >= gameModes.Count)
        {
            LogState($"❌ [ERROR] Invalid mode index: {index} (gameModes.Count = {gameModes.Count})");
            return;
        }
        
        selectedModeIndex = index;
        GameMode mode = gameModes[index];
        selectedSceneName = mode.sceneName;
        
        LogState($"📋 [MODE] Name: {mode.modeName} | Scene: {mode.sceneName} | Description: {mode.description}");
        LogState($"📋 [STATE] selectedModeIndex = {selectedModeIndex}, selectedSceneName = '{selectedSceneName}'");
        
        // Update preview UI - FORCE UPDATE
        if (selectedModeNameText != null)
        {
            selectedModeNameText.text = mode.modeName;
            LogState($"✅ [UI] Name text updated: {mode.modeName}");
        }
        else
        {
            LogState("❌ [ERROR] selectedModeNameText is NULL!");
        }
        
        if (selectedModeDescriptionText != null)
        {
            selectedModeDescriptionText.text = mode.description;
            LogState($"✅ [UI] Description text updated: {mode.description}");
        }
        else
        {
            LogState("❌ [ERROR] selectedModeDescriptionText is NULL!");
        }
        
        if (selectedModeIconImage != null && mode.icon != null)
        {
            selectedModeIconImage.sprite = mode.icon;
            LogState("✅ [UI] Icon updated");
        }
        else if (selectedModeIconImage != null && mode.icon == null)
        {
            LogState("⚠️ [WARN] Icon is NULL for this mode");
        }
        
        LogState($"✅ [SELECT] Preview updated for: {mode.modeName} (Scene: {selectedSceneName})");
    }
    
    /// <summary>
    /// Confirm mode selection - store selection and show ready panel (don't load scene yet).
    /// </summary>
    private void OnConfirmModeClicked()
    {
        LogState("🖱️ [CLICK] Confirm Mode button clicked");
        
        if (string.IsNullOrEmpty(selectedSceneName))
        {
            LogState("❌ [ERROR] No mode selected!");
            return;
        }
        
        LogState($"📋 [CHECK] Selected mode index: {selectedModeIndex}");
        LogState($"📋 [CHECK] Game mode: {gameModes[selectedModeIndex].modeName}");
        LogState($"📋 [CHECK] Scene: {selectedSceneName}");
        
        // Store game mode selection in Photon player properties
        if (GameModeSelectionManager.Instance != null)
        {
            GameModeSelectionManager.Instance.SelectGameMode(selectedModeIndex);
            LogState($"✅ [STORE] Game mode selection stored in Photon properties");
        }
        else
        {
            LogState("⚠️ [WARN] GameModeSelectionManager.Instance is NULL! Selection not stored.");
        }
        
        // Hide game mode selection panel
        gameObject.SetActive(false);
        LogState("✅ [UI] GameModeSelectionUI deactivated");
        
        // Show ready panel (via LobbyManager)
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnGameModeSelectionConfirmed(selectedSceneName);
            LogState($"✅ [ACTION] Requested LobbyManager to show ready panel");
        }
        else
        {
            LogState("❌ [ERROR] LobbyManager.Instance is NULL! Cannot show ready panel.");
        }
    }
    
    /// <summary>
    /// Get selected scene name (for LobbyManager to load).
    /// </summary>
    public string GetSelectedSceneName()
    {
        return selectedSceneName;
    }
    
    /// <summary>
    /// Back button clicked - return to character selection.
    /// </summary>
    private void OnBackClicked()
    {
        LogState("🖱️ [CLICK] Back button clicked - returning to character selection");
        
        // Hide game mode selection
        gameObject.SetActive(false);
        
        // Show character selection
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnBackToCharacterSelection();
        }
        
        LogState("✅ [ACTION] Returned to character selection");
    }
    
    /// <summary>
    /// Show/hide this panel.
    /// </summary>
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        LogState($"🔄 [UI] GameModeSelectionUI {(active ? "activated" : "deactivated")}");
        
        if (active)
        {
            // Ensure buttons are visible and wired when panel is activated
            if (modeButtonParent != null)
            {
                // Make sure all child buttons are active
                Button[] buttons = modeButtonParent.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn != null && btn.name.StartsWith("ModeButton_"))
                    {
                        btn.gameObject.SetActive(true);
                    }
                }
                LogState($"✅ [UI] Ensured {buttons.Length} mode buttons are visible");
            }
            
            // Refresh mode list to ensure everything is wired
            RefreshModeList();
        }
    }
    
    private void LogState(string message)
    {
        if (logUIEvents)
        {
            Debug.Log($"[GameModeSelectionUI] {message}");
        }
    }
}
#else
using UnityEngine;

public class GameModeSelectionUI : MonoBehaviour
{
    private void Start()
    {
        Debug.LogWarning("[GameModeSelectionUI] Photon PUN2 not installed!");
    }
}
#endif

