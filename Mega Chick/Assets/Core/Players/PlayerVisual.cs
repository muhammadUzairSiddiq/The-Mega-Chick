using UnityEngine;

/// <summary>
/// Handles player visual representation - swaps character model.
/// Why separate? Visual logic isolated from gameplay.
/// Simple but extendable - can add more visual customization later.
/// </summary>
public class PlayerVisual : MonoBehaviour
{
    [Header("Character Model")]
    [SerializeField] private Transform modelParent; // Where character model is attached
    [SerializeField] private GameObject currentCharacterModel;
    
    [Header("Config")]
    [SerializeField] private CharacterSelectionManager characterManager;
    
    private CharacterData currentCharacterData;
    
    private void Awake()
    {
        if (modelParent == null)
        {
            // Auto-find or create model parent
            modelParent = transform.Find("Model");
            if (modelParent == null)
            {
                GameObject modelObj = new GameObject("Model");
                modelObj.transform.SetParent(transform);
                modelObj.transform.localPosition = Vector3.zero;
                modelObj.transform.localRotation = Quaternion.identity;
                modelParent = modelObj.transform;
            }
        }
        
        if (characterManager == null)
        {
            characterManager = CharacterSelectionManager.Instance;
        }
    }
    
    /// <summary>
    /// Set character visual based on character data.
    /// Called when player spawns or changes character.
    /// </summary>
    public void SetCharacter(CharacterData characterData)
    {
        if (characterData == null)
        {
            Debug.LogWarning("[PlayerVisual] Character data is null!");
            return;
        }
        
        // Remove old model
        if (currentCharacterModel != null)
        {
            Destroy(currentCharacterModel);
        }
        
        // Instantiate new model
        if (characterData.characterPrefab != null)
        {
            currentCharacterModel = Instantiate(characterData.characterPrefab, modelParent);
            currentCharacterModel.transform.localPosition = Vector3.zero;
            currentCharacterModel.transform.localRotation = Quaternion.identity;
            
            // Apply character stat multipliers if needed
            ApplyCharacterStats(characterData);
        }
        else
        {
            Debug.LogWarning($"[PlayerVisual] Character {characterData.characterName} has no prefab!");
        }
        
        currentCharacterData = characterData;
    }
    
    /// <summary>
    /// Apply character stat multipliers to player controller.
    /// </summary>
    private void ApplyCharacterStats(CharacterData characterData)
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Apply speed/jump multipliers via MovementConfig
            // This will be handled by PlayerController when we pass character data
            // For now, just store reference
        }
    }
    
    /// <summary>
    /// Get current character data.
    /// </summary>
    public CharacterData GetCurrentCharacter()
    {
        return currentCharacterData;
    }
}

