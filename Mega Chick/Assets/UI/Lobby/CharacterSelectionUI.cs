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
    [SerializeField] private Image characterIconImage; // For Sprite icons
    [SerializeField] private UnityEngine.UI.RawImage characterIconRawImage; // For Texture2D icons
    
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
        
        // Reset icon references to force re-discovery
        characterIconImage = null;
        characterIconRawImage = null;
        
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
        
        // Display current character (this will load the icon)
        DisplayCharacter(currentCharacterIndex);
        
        Log("✅ [REFRESH] Character list refreshed");
    }
    
    /// <summary>
    /// Called when this panel is enabled - refresh to load icons.
    /// </summary>
    private void OnEnable()
    {
        // Refresh when panel is enabled to ensure icons load
        if (Application.isPlaying)
        {
            RefreshCharacterList();
        }
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
        
        // Handle icon - support both Sprite (Image) and Texture2D (RawImage)
        // ALWAYS try to find icon components if not assigned (fallback)
        if (characterIconImage == null)
        {
            // Try multiple paths to find CharacterIcon
            Transform iconObj = null;
            
            // Try PreviewPanel/CharacterIcon first
            Transform previewPanel = transform.Find("PreviewPanel");
            if (previewPanel != null)
            {
                iconObj = previewPanel.Find("CharacterIcon");
            }
            
            // Try direct CharacterIcon
            if (iconObj == null)
            {
                iconObj = transform.Find("CharacterIcon");
            }
            
            // Try searching in all children
            if (iconObj == null)
            {
                iconObj = transform.GetComponentInChildren<Transform>().Find("CharacterIcon");
            }
            
            // Try recursive search
            if (iconObj == null)
            {
                iconObj = FindChildRecursive(transform, "CharacterIcon");
            }
            
            if (iconObj != null)
            {
                characterIconImage = iconObj.GetComponent<Image>();
                if (characterIconImage != null)
                {
                    string path = iconObj != null ? GetPath(iconObj) : "unknown";
                    Log($"✅ [FOUND] Found characterIconImage via fallback: {iconObj.name} at path: {path}");
                }
                else
                {
                    Log($"⚠️ [WARN] Found CharacterIcon GameObject but no Image component!");
                }
            }
            else
            {
                Log("❌ [ERROR] Could not find CharacterIcon GameObject! Searched in PreviewPanel/CharacterIcon, CharacterIcon, and recursively.");
            }
        }
        
        if (characterIconRawImage == null)
        {
            Transform iconObj = null;
            Transform previewPanel = transform.Find("PreviewPanel");
            if (previewPanel != null)
            {
                iconObj = previewPanel.Find("CharacterIcon");
            }
            if (iconObj == null) iconObj = transform.Find("CharacterIcon");
            if (iconObj == null) iconObj = FindChildRecursive(transform, "CharacterIcon");
            
            if (iconObj != null)
            {
                characterIconRawImage = iconObj.GetComponent<UnityEngine.UI.RawImage>();
                if (characterIconRawImage != null)
                {
                    Log($"✅ [FOUND] Found characterIconRawImage via fallback: {iconObj.name}");
                }
            }
        }
        
        // Debug: Log icon info
        Log($"🔍 [DEBUG] Character icon check for: {character.characterName}");
        Log($"🔍 [DEBUG] Icon is null: {character.icon == null}");
        if (character.icon != null)
        {
            Log($"🔍 [DEBUG] Icon type: {character.icon.GetType().Name}");
            Log($"🔍 [DEBUG] Icon name: {character.icon.name}");
        }
        Log($"🔍 [DEBUG] characterIconImage is null: {characterIconImage == null}");
        Log($"🔍 [DEBUG] characterIconRawImage is null: {characterIconRawImage == null}");
        
        if (character.icon != null)
        {
            // Check if it's a Sprite
            Sprite spriteIcon = character.icon as Sprite;
            if (spriteIcon != null)
            {
                Log($"✅ [TYPE] Icon is Sprite: {spriteIcon.name}");
                // Use Image component for Sprite
                if (characterIconImage != null)
                {
                    characterIconImage.sprite = spriteIcon;
                    characterIconImage.color = Color.white;
                    characterIconImage.enabled = true;
                    characterIconImage.gameObject.SetActive(true);
                    if (characterIconRawImage != null) 
                    {
                        characterIconRawImage.enabled = false;
                        characterIconRawImage.gameObject.SetActive(false);
                    }
                    // Ensure parent GameObject is also active
                    if (characterIconImage.transform != null && characterIconImage.transform.parent != null)
                    {
                        characterIconImage.transform.parent.gameObject.SetActive(true);
                    }
                    Log($"✅ [UI] Icon set (Sprite): {spriteIcon.name}");
                }
                else
                {
                    Log("❌ [ERROR] characterIconImage is NULL! Cannot display Sprite icon.");
                    // Try to create Image if RawImage exists - need to remove RawImage first
                    if (characterIconRawImage != null && spriteIcon != null)
                    {
                        Log("🔧 [FIX] Attempting to replace RawImage with Image component...");
                        GameObject iconObj = characterIconRawImage.gameObject;
                        if (iconObj != null)
                        {
                            // Remove the RawImage component first (can't have both Image and RawImage)
                            if (Application.isPlaying)
                            {
                                Destroy(characterIconRawImage);
                            }
                            else
                            {
                                DestroyImmediate(characterIconRawImage);
                            }
                            
                            // Now add Image component
                            Image newImage = iconObj.GetComponent<Image>();
                            if (newImage == null)
                            {
                                newImage = iconObj.AddComponent<Image>();
                            }
                            if (newImage != null)
                            {
                                newImage.sprite = spriteIcon;
                                newImage.color = Color.white;
                                newImage.enabled = true;
                                newImage.gameObject.SetActive(true);
                                characterIconImage = newImage;
                                characterIconRawImage = null; // Clear reference since we removed it
                                Log($"✅ [FIX] Replaced RawImage with Image component and set sprite: {spriteIcon.name}!");
                            }
                            else
                            {
                                Log("❌ [ERROR] Failed to create Image component!");
                            }
                        }
                        else
                        {
                            Log("❌ [ERROR] characterIconRawImage.gameObject is NULL!");
                        }
                    }
                    else
                    {
                        Log("❌ [ERROR] Cannot create Image component - no icon GameObject found!");
                    }
                }
            }
            // Check if it's a Texture2D
            else if (character.icon is Texture2D textureIcon)
            {
                Log($"✅ [TYPE] Icon is Texture2D: {textureIcon.name}");
                // Use RawImage component for Texture2D
                if (characterIconRawImage != null)
                {
                    characterIconRawImage.texture = textureIcon;
                    characterIconRawImage.color = Color.white;
                    characterIconRawImage.enabled = true;
                    characterIconRawImage.gameObject.SetActive(true);
                    if (characterIconImage != null) 
                    {
                        characterIconImage.enabled = false;
                        characterIconImage.gameObject.SetActive(false);
                    }
                    // Ensure parent GameObject is also active
                    if (characterIconRawImage.transform != null && characterIconRawImage.transform.parent != null)
                    {
                        characterIconRawImage.transform.parent.gameObject.SetActive(true);
                    }
                    Log($"✅ [UI] Icon set (Texture2D): {textureIcon.name}");
                }
                else
                {
                    Log("❌ [ERROR] characterIconRawImage is NULL! Cannot display Texture2D icon.");
                    // Try to create RawImage if Image exists - need to remove Image first
                    if (characterIconImage != null && textureIcon != null)
                    {
                        Log("🔧 [FIX] Attempting to replace Image with RawImage component...");
                        GameObject iconObj = characterIconImage.gameObject;
                        if (iconObj != null)
                        {
                            // Remove the Image component first (can't have both Image and RawImage)
                            if (Application.isPlaying)
                            {
                                Destroy(characterIconImage);
                            }
                            else
                            {
                                DestroyImmediate(characterIconImage);
                            }
                            
                            // Now add RawImage component
                            UnityEngine.UI.RawImage newRawImage = iconObj.GetComponent<UnityEngine.UI.RawImage>();
                            if (newRawImage == null)
                            {
                                newRawImage = iconObj.AddComponent<UnityEngine.UI.RawImage>();
                            }
                            if (newRawImage != null)
                            {
                                newRawImage.texture = textureIcon;
                                newRawImage.color = Color.white;
                                newRawImage.enabled = true;
                                newRawImage.gameObject.SetActive(true);
                                characterIconRawImage = newRawImage;
                                characterIconImage = null; // Clear reference since we removed it
                                Log($"✅ [FIX] Replaced Image with RawImage component and set texture: {textureIcon.name}!");
                            }
                            else
                            {
                                Log("❌ [ERROR] Failed to create RawImage component!");
                            }
                        }
                        else
                        {
                            Log("❌ [ERROR] characterIconImage.gameObject is NULL!");
                        }
                    }
                    else
                    {
                        if (characterIconImage == null) Log("❌ [ERROR] characterIconImage is NULL, cannot create RawImage!");
                        if (textureIcon == null) Log("❌ [ERROR] textureIcon is NULL!");
                    }
                }
            }
            else
            {
                Log($"⚠️ [WARN] Icon type not supported: {character.icon.GetType().Name}. Expected Sprite or Texture2D.");
                Log($"⚠️ [WARN] Icon value: {character.icon}");
            }
        }
        else
        {
            Log($"⚠️ [WARN] Character icon is NULL for: {character.characterName}");
            // Hide icon components if no icon
            if (characterIconImage != null) 
            {
                characterIconImage.enabled = false;
            }
            if (characterIconRawImage != null) 
            {
                characterIconRawImage.enabled = false;
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
            if (currentCharacterIndex >= 0 && currentCharacterIndex < unlockedCharacters.Count && unlockedCharacters[currentCharacterIndex] != null)
            {
                DisplayCharacter(currentCharacterIndex);
                Log($"✅ [NAV] Moved to previous character: {unlockedCharacters[currentCharacterIndex].characterName}");
            }
            else
            {
                Log($"❌ [ERROR] Invalid character index after decrement: {currentCharacterIndex}");
            }
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
    
    /// <summary>
    /// Recursively find a child by name.
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform found = FindChildRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get full path of a transform for debugging.
    /// </summary>
    private string GetPath(Transform transform)
    {
        if (transform == null) return "null";
        
        string path = transform.name;
        Transform current = transform;
        while (current != null && current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return path;
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
