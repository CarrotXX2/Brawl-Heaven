using UnityEngine;
using System.Collections.Generic;

public class PlayersFloatingUI : MonoBehaviour
{
    public GameObject player1UIPrefab;
    public GameObject player2UIPrefab;
    public GameObject player3UIPrefab;
    public GameObject player4UIPrefab;

    public Vector3 uiOffset = new Vector3(0, 2, 0);

    private Dictionary<int, GameObject> playerUIInstances = new Dictionary<int, GameObject>();

    void Start()
    {
        InitializePlayerUI();
    }

    void LateUpdate()
    {
        UpdateUIPositions();
    }

    void InitializePlayerUI()
    {
        List<GameObject> players = GetPlayers();

        for (int i = 0; i < 4; i++)
        {
            if (i < players.Count)
            {
                GameObject player = players[i];
                if (!playerUIInstances.ContainsKey(i + 1))
                {
                    GameObject uiInstance = Instantiate(GetUIPrefab(i + 1), transform);
                    uiInstance.name = "PlayerUI" + (i + 1);
                    playerUIInstances[i + 1] = uiInstance;
                }
                playerUIInstances[i + 1].SetActive(true);
            }
            else if (playerUIInstances.ContainsKey(i + 1))
            {
                playerUIInstances[i + 1].SetActive(false);
            }
        }
    }

    void UpdateUIPositions()
    {
        List<GameObject> players = GetPlayers();
        for (int i = 0; i < players.Count; i++)
        {
            if (playerUIInstances.ContainsKey(i + 1))
            {
                playerUIInstances[i + 1].transform.position = players[i].transform.position + uiOffset;
            }
        }
    }

    List<GameObject> GetPlayers()
    {
        return GameplayManager.Instance?.players ?? new List<GameObject>();
    }

    GameObject GetUIPrefab(int playerIndex)
    {
        switch (playerIndex)
        {
            case 1: return player1UIPrefab;
            case 2: return player2UIPrefab;
            case 3: return player3UIPrefab;
            case 4: return player4UIPrefab;
            default: return null;
        }
    }
}
