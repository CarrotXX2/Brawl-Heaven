using System.Collections.Generic;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [Header("UI Layouts")]
    public GameObject layout2Players;
    public GameObject layout3Players;
    public GameObject layout4Players;
    
    [Header("UI Prefab")]
    public GameObject playerUIPrefab; // The UI prefab to instantiate for each player
    
    [Header("Test Mode Settings")]
    public bool useTestMode = false;
    [Range(1, 4)] public int testPlayerCount = 2;
    public KeyCode testRefreshKey = KeyCode.R;
    
    // Dictionary to track instantiated UI elements by player ID
    private Dictionary<int, GameObject> playerUIInstances = new Dictionary<int, GameObject>();
    
    void Start()
    {
        UpdatePlayerUI();
    }
    
    void Update()
    {
        if (useTestMode && Input.GetKeyDown(testRefreshKey))
        {
            UpdatePlayerUI();
        }
    }
    
    public void UpdatePlayerUI()
    {
        // Clean up any existing UI elements
        ClearPlayerUI();
        
        int playerCount = GetPlayerCount();
        SetAllLayoutsActive(false);
        
        if (playerCount <= 0) return;
        
        GameObject activeLayout;
        if (playerCount == 1 || playerCount == 2)
        {
            activeLayout = layout2Players;
        }
        else if (playerCount == 3)
        {
            activeLayout = layout3Players;
        }
        else
        {
            activeLayout = layout4Players;
        }
        
        if (activeLayout == null) return;
        
        activeLayout.SetActive(true);
        
        // Create UI for each player
        for (int i = 1; i <= playerCount; i++)
        {
            CreatePlayerUI(i, activeLayout);
        }
    }
    
    private void CreatePlayerUI(int playerIndex, GameObject layout)
    {
        if (playerUIPrefab == null || layout == null) return;
        
        Transform posTransform = layout.transform.Find("Pos" + playerIndex);
        if (posTransform == null) return;
        
        GameObject uiInstance = Instantiate(playerUIPrefab, posTransform.position, posTransform.rotation);
        uiInstance.name = "PlayerUI_" + playerIndex;
        
        // You can set up any player-specific UI customization here
        EnhancedDamageDisplay playerUI = uiInstance.GetComponent<EnhancedDamageDisplay>();
        if (playerUI != null)
        {
            playerUI.SetPlayerIndex(playerIndex);
        }
        
        // Store reference to UI in dictionary
        playerUIInstances[playerIndex] = uiInstance;
    }
    
    private void ClearPlayerUI()
    {
        foreach (var uiInstance in playerUIInstances.Values)
        {
            if (uiInstance != null)
            {
                Destroy(uiInstance);
            }
        }
        playerUIInstances.Clear();
    }
    
    int GetPlayerCount()
    {
        if (useTestMode)
        {
            return Mathf.Clamp(testPlayerCount, 1, 4);
        }
        else
        {
            return GameplayManager.Instance != null && GameplayManager.Instance.players != null
                ? GameplayManager.Instance.players.Count
                : 0;
        }
    }
    
    void SetAllLayoutsActive(bool active)
    {
        if (layout2Players != null) layout2Players.SetActive(active);
        if (layout3Players != null) layout3Players.SetActive(active);
        if (layout4Players != null) layout4Players.SetActive(active);
    }
    
    // Method to get a player's UI instance
    public GameObject GetPlayerUI(int playerIndex)
    {
        if (playerUIInstances.TryGetValue(playerIndex, out GameObject ui))
        {
            return ui;
        }
        return null;
    }
}
