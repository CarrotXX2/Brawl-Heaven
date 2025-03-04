using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CharacterButton : MonoBehaviour
{
    public GameObject characterPrefab;
    public UnityEvent onButtonPressed;
    public void OnButtonClicked(int playerID)
    {
        PlayerManager.Instance.players[playerID].GetComponent<Player>().characterPrefab = characterPrefab;
        
        print("Character Button Pressed");
    }
}