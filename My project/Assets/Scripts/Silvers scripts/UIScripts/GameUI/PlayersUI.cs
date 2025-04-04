using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerUIManager : MonoBehaviour
{
    public GameObject layout2Players;
    public GameObject layout3Players;
    public GameObject layout4Players;

    [Header("Player UI References")]
    public GameObject p1UI;
    public GameObject p2UI;
    public GameObject p3UI;
    public GameObject p4UI;

    [Header("Test Mode Settings")]
    public bool useTestMode = false;
    [Range(1, 4)] public int testPlayerCount = 2;
    public KeyCode testRefreshKey = KeyCode.R;
    
    void Start()
    {
        SetPlayerUI();
    }

    void Update()
    {
        if (useTestMode && Input.GetKeyDown(testRefreshKey))
        {
            SetPlayerUI();
        }
    }
    public void SetPlayerUI()
    {
        int playerCount = GetPlayerCount();
        SetAllUIActive(false);
        SetAllLayoutsActive(false);

        if (playerCount == 1)
        {
            layout2Players.SetActive(true);
            p1UI.SetActive(true);
            PositionUIElement(p1UI, "Pos1", layout2Players);
            return;
        }
        switch (playerCount)
        {
            case 2:
                ActivateLayout(layout2Players, 2);
                break;
            case 3:
                ActivateLayout(layout3Players, 3);
                break;
            case 4:
                ActivateLayout(layout4Players, 4);
                break;
        }
    }

    public void UpdatePlayerHealthUI(int playerID, float damageTaken)
    {
        print("Change UI");
        if (playerID == 0)
        {
            p1UI.GetComponentInChildren<EnhancedDamageDisplay>().UpdateDamage(damageTaken);
        }
        else if (playerID == 1)
        {
            p2UI.GetComponentInChildren<EnhancedDamageDisplay>().UpdateDamage(damageTaken);
        }
        else if (playerID == 2)
        {
            p3UI.GetComponentInChildren<EnhancedDamageDisplay>().UpdateDamage(damageTaken);
        }
        else if (playerID == 3)
        {
            p4UI.GetComponentInChildren<EnhancedDamageDisplay>().UpdateDamage(damageTaken);
        }
    }
    
    public void UpdatePlayerStockUI(int playerID)
    {
        if (playerID == 1)
        {
            p1UI.GetComponentInChildren<EnhancedDamageDisplay>().DecrementLife();
        }
        else if (playerID == 2)
        {
            p2UI.GetComponentInChildren<EnhancedDamageDisplay>().DecrementLife();
        }
        else if (playerID == 3)
        {
            p3UI.GetComponentInChildren<EnhancedDamageDisplay>().DecrementLife();
        }
        else if (playerID == 4)
        {
            p4UI.GetComponentInChildren<EnhancedDamageDisplay>().DecrementLife();
        }
    }

    int GetPlayerCount()
    {
        if ( GameplayManager.Instance != null && GameplayManager.Instance.players != null)
        {
            return GameplayManager.Instance.players.Count;
        }
        return 0;
    }
    void SetAllUIActive(bool active)
    {
        if (p1UI != null) p1UI.SetActive(active);
        if (p2UI != null) p2UI.SetActive(active);
        if (p3UI != null) p3UI.SetActive(active);
        if (p4UI != null) p4UI.SetActive(active);
    }

    void SetAllLayoutsActive(bool active)
    {
        if (layout2Players != null) layout2Players.SetActive(active);
        if (layout3Players != null) layout3Players.SetActive(active);
        if (layout4Players != null) layout4Players.SetActive(active);
    }

    void ActivateLayout(GameObject layout, int playerCount)
    {
        if (layout == null) return;

        layout.SetActive(true);

        for (int i = 1; i <= playerCount; i++)
        {
            GameObject uiElement = GetUIElement(i);
            if (uiElement != null)
            {
                uiElement.SetActive(true);
                PositionUIElement(uiElement, "Pos" + i, layout);
            }
        }
    }

    GameObject GetUIElement(int playerIndex)
    {
        switch (playerIndex)
        {
            case 1: return p1UI;
            case 2: return p2UI;
            case 3: return p3UI;
            case 4: return p4UI;
            default: return null;
        }
    }

    void PositionUIElement(GameObject uiElement, string positionName, GameObject layout)
    {
        if (uiElement == null || layout == null) return;

        Transform posTransform = layout.transform.Find(positionName);
        if (posTransform != null)
        {
            uiElement.transform.position = posTransform.position;
            uiElement.transform.rotation = posTransform.rotation;
        }

    }
}
