#if PUN_2_OR_NEWER
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Complete character selection UI with Next/Previous navigation and full character data display.
/// 100% functional implementation.
/// </summary>
public class CharacterSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterDescriptionText;
    [SerializeField] private TextMeshProUGUI characterAbilityText;
    [SerializeField] private TextMeshProUGUI characterStatsText;
    [SerializeField] private Image characterIconImage;
    
    [Header("Navigation Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;
    
    [Header("Character Index Display")]
    [SerializeField] private TextMeshProUGUI characterIndexText; // "1 / 5"
    
    [Header("Debug")]
    [SerializeField] private bool logUIEvents = true;
    
    private List<CharacterData> unlockedCharacters = new List<CharacterData>();
    private int currentCharacterIndex = 0; // Index in unlockedCharacters list
    
    private void Start()
    {
        Log("🔵 [INIT] CharacterSelectionUI starting...");
        
        // Wire all buttons
        if (previousButton != null)
        {
            previousButton.onClick.AddListener(OnPreviousClicked);
            Log("✅ [WIRE] PreviousButton → OnPreviousClicked");
        }
        
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextClicked);
            Log("✅ [WIRE] NextButton → OnNextClicked");
        }
        
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectClicked);
            Log("✅ [WIRE] SelectButton → OnSelectClicked");
        }
        
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            Log("✅ [WIRE] ConfirmButton → OnConfirmClicked");
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
            Log("✅ [WIRE] BackButton → OnBackClicked");
        }
        
        // Load characters and display first one
        RefreshCharacterList();
        
        Log("✅ [INIT] CharacterSelectionUI initialized");
    }
    
    /// <summary>
    /// Refresh character list and display current character.
    /// </summary>
    public void RefreshCharacterList()
    {
        Log("🔄 [REFRESH] Refreshing character list...");
        
        if (CharacterSelectionManager.Instance == null)
        {
            Log("❌ [ERROR] CharacterSelectionManager.Instance is NULL!");
            return;
        }
        
        // Get unlocked characters
        unlockedCharacters = CharacterSelectionManager.Instance.GetUnlockedCharacters();
        Log($"📋 [DATA] Found {unlockedCharacters.Count} unlocked characters");
        
        if (unlockedCharacters.Count == 0)
        {
            Log("⚠️ [WARN] No unlocked characters available!");
            return;
        }
        
        // Clamp current index
        if (currentCharacterIndex >= unlockedCharacters.Count)
        {
            currentCharacterIndex = 0;
        }
        
        // Display current character
        DisplayCharacter(currentCharacterIndex);
        
        Log("✅ [REFRESH] Character list refreshed");
    }
    
    /// <summary>
    /// Display character at index in unlockedCharacters list.
    /// </summary>
    private void DisplayCharacter(int index)
    {
        if (index < 0 || index >= unlockedCharacters.Count)
        {
            Log($"❌ [ERROR] Invalid character index: {index}");
            return;
        }
        
        CharacterData character = unlockedCharacters[index];
        if (character == null)
        {
            Log($"❌ [ERROR] Character at index {index} is NULL!");
            return;
        }
        
        currentCharacterIndex = index;
        
        Log($"📋 [DISPLAY] Character: {character.characterName} (Index: {index})");
        
        // Update all UI elements with character data
        if (characterNameText != null)
        {
            characterNameText.text = character.characterName;
            Log($"✅ [UI] Name: {character.characterName}");
        }
        
        if (characterDescriptionText != null)
        {
            characterDescriptionText.text = character.description;
            Log($"✅ [UI] Description: {character.description}");
        }
        
        if (characterAbilityText != null)
        {
            characterAbilityText.text = $"Special Ability: {character.specialAbility}";
            Log($"✅ [UI] Ability: {character.specialAbility}");
        }
        
        if (characterStatsText != null)
        {
            characterStatsText.text = $"Speed: {character.speedMultiplier}x | Jump: {character.jumpMultiplier}x | Resistance: {(character.knockbackResistance * 100):F0}%";
            Log($"✅ [UI] Stats: Speed {character.speedMultiplier}x, Jump {character.jumpMultiplier}x, Resistance {character.knockbackResistance * 100}%");
        }
        
        if (characterIconImage != null)
        {
            if (character.icon != null)
            {
                characterIconImage.sprite = character.icon;
                Log($"✅ [UI] Icon set");
            }
            else
            {
                Log("⚠️ [WARN] Character icon is NULL!");
            }
        }
        
        // Update character index display (e.g., "1 / 5")
        if (characterIndexText != null)
        {
            characterIndexText.text = $"{index + 1} / {unlockedCharacters.Count}";
        }
        
        // Update navigation button states
        UpdateNavigationButtons();
        
        Log($"✅ [DISPLAY] Character display updated: {character.characterName}");
    }
    
    /// <summary>
    /// Update navigation button states (enable/disable based on position).
    /// </summary>
    private void UpdateNavigationButtons()
    {
        if (previousButton != null)
        {
            previousButton.interactable = currentCharacterIndex > 0;
        }
        
        if (nextButton != null)
        {
            nextButton.interactable = currentCharacterIndex < unlockedCharacters.Count - 1;
        }
    }
    
    /// <summary>
    /// Previous character button clicked.
    /// </summary>
    private void OnPreviousClicked()
    {
        Log("🖱️ [CLICK] Previous button clicked");
        
        if (currentCharacterIndex > 0)
        {
            currentCharacterIndex--;
            DisplayCharacter(currentCharacterIndex);
            Log($"✅ [NAV] Moved to previous character: {unlockedCharacters[currentCharacterIndex].characterName}");
        }
        else
        {
            Log("⚠️ [WARN] Already at first character!");
        }
    }
    
    /// <summary>
    /// Next character button clicked.
    /// </summary>
    private void OnNextClicked()
    {
        Log("🖱️ [CLICK] Next button clicked");
        
        if (currentCharacterIndex < unlockedCharacters.Count - 1)
        {
            currentCharacterIndex++;
            DisplayCharacter(currentCharacterIndex);
            Log($"✅ [NAV] Moved to next character: {unlockedCharacters[currentCharacterIndex].characterName}");
        }
        else
        {
            Log("⚠️ [WARN] Already at last character!");
        }
    }
    
    /// <summary>
    /// Select button clicked - select current character.
    /// </summary>
    private void OnSelectClicked()
    {
        Log($"🖱️ [CLICK] Select button clicked for character index {currentCharacterIndex}");
        
        if (CharacterSelectionManager.Instance == null)
        {
            Log("❌ [ERROR] CharacterSelectionManager.Instance is NULL!");
            return;
        }
        
        if (currentCharacterIndex < 0 || currentCharacterIndex >= unlockedCharacters.Count)
        {
            Log($"❌ [ERROR] Invalid character index: {currentCharacterIndex}");
            return;
        }
        
        CharacterData character = unlockedCharacters[currentCharacterIndex];
        if (character == null)
        {
            Log($"❌ [ERROR] Character at index {currentCharacterIndex} is NULL!");
            return;
        }
        
        // Find actual index in availableCharacters list
        List<CharacterData> availableCharacters = CharacterSelectionManager.Instance.GetAvailableCharacters();
        int actualIndex = availableCharacters.IndexOf(character);
        
        if (actualIndex < 0)
        {
            Log($"❌ [ERROR] Character {character.characterName} not found in available characters list!");
            return;
        }
        
        Log($"🚀 [ACTION] Selecting character: {character.characterName} (Actual Index: {actualIndex})");
        CharacterSelectionManager.Instance.SelectCharacter(actualIndex);
        Log($"✅ [ACTION] CharacterSelectionManager.SelectCharacter({actualIndex}) called");
    }
    
    /// <summary>
    /// Confirm button clicked - proceed to game mode selection.
    /// </summary>
    private void OnConfirmClicked()
    {
        Log("🖱️ [CLICK] Confirm button clicked");
        
        // Ensure character is selected
        OnSelectClicked();
        
        // Hide character selection and show game mode selection
        Log("🔄 [FLOW] Character confirmed - moving to game mode selection");
        
        gameObject.SetActive(false);
        
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnCharacterSelectionConfirmed();
            Log("✅ [ACTION] LobbyManager.OnCharacterSelectionConfirmed() called");
        }
        else
        {
            Log("❌ [ERROR] LobbyManager.Instance is NULL!");
        }
    }
    
    /// <summary>
    /// Back button clicked - return to room creation.
    /// </summary>
    private void OnBackClicked()
    {
        Log("🖱️ [CLICK] Back button clicked - returning to room creation");
        
        gameObject.SetActive(false);
        
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnBackToRoomCreation();
            Log("✅ [ACTION] LobbyManager.OnBackToRoomCreation() called");
        }
        else
        {
            Log("❌ [ERROR] LobbyManager.Instance is NULL!");
        }
    }
    
    private void Log(string message)
    {
        if (logUIEvents)
        {
            Debug.Log($"[CharacterSelectionUI] {message}");
        }
    }
}
#else
using UnityEngine;

public class CharacterSelectionUI : MonoBehaviour
{
    private void Start()
    {
        Debug.LogWarning("[CharacterSelectionUI] Photon PUN2 not installed!");
    }
}
#endif
