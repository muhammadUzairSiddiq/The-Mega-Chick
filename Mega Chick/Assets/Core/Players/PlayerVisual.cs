using UnityEngine;
using System.Collections;

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
    
    private void Start()
    {
        // Auto-load character if not already loaded and CharacterLoader exists
        if (currentCharacterModel == null && CharacterLoader.Instance != null)
        {
            // Let CharacterLoader handle it (it will call SetCharacter)
            // This prevents duplicate loading
        }
        else if (currentCharacterModel == null && characterManager != null)
        {
            // Fallback: Try to load character directly if CharacterLoader not available
            TryLoadCharacterFromPhoton();
        }
    }
    
    /// <summary>
    /// Try to load character from Photon player properties.
    /// </summary>
    private void TryLoadCharacterFromPhoton()
    {
#if PUN_2_OR_NEWER
        if (!Photon.Pun.PhotonNetwork.IsConnected) return;
        
        Photon.Pun.PhotonView pv = GetComponent<Photon.Pun.PhotonView>();
        if (pv != null && pv.Owner != null)
        {
            CharacterData charData = characterManager.GetCharacterData(pv.Owner);
            if (charData != null)
            {
                SetCharacter(charData);
            }
        }
#else
        // Photon not installed - skip
#endif
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
            
            // Add PlayerAnimatorController to character model if it has Animator
            Animator charAnimator = currentCharacterModel.GetComponent<Animator>();
            if (charAnimator != null)
            {
                // CRITICAL: Disable root motion - it conflicts with rigidbody movement
                charAnimator.applyRootMotion = false;
                
                // Set idle animation IMMEDIATELY on animator (before adding component) - ID=1 for idle
                charAnimator.SetInteger("animation", 1);
                
                PlayerAnimatorController animController = currentCharacterModel.GetComponent<PlayerAnimatorController>();
                if (animController == null)
                {
                    animController = currentCharacterModel.AddComponent<PlayerAnimatorController>();
                }
                
                // Set idle animation IMMEDIATELY (don't wait for coroutine)
                if (animController != null)
                {
                    animController.SetIdle();
                }
                
                // Also set idle after one frame (ensures animator is fully initialized)
                StartCoroutine(SetIdleAfterLoad(animController));
                
                // Notify PlayerController that animator is ready
                PlayerController playerController = GetComponent<PlayerController>();
                if (playerController != null && animController != null)
                {
                    playerController.SetAnimatorController(animController);
                    // Re-initialize player when character loads (ensures gravity/controls work)
                    playerController.OnCharacterLoaded();
                }
            }
            
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
    
    /// <summary>
    /// Set idle animation after character model loads (waits for animator to initialize).
    /// </summary>
    private IEnumerator SetIdleAfterLoad(PlayerAnimatorController animController)
    {
        yield return new WaitForEndOfFrame(); // Wait one frame for animator to initialize
        if (animController != null)
        {
            animController.SetIdle();
        }
    }
}

