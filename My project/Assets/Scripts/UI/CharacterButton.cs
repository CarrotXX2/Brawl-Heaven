using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    public GameObject characterPrefab;
    public UnityEvent onButtonPressed;

    // UI Instellingen
    public GameObject imagePrefab;  // Prefab van de UI-afbeelding
    public Transform[] spawnPositions;

    public void OnButtonClicked(GameObject playerObject)
    {
        // Haal de Player-component op
        Player player = playerObject.GetComponent<Player>();
        int playerID = player.playerID;

        // Stel het karakter in voor de speler
        player.characterPrefab = characterPrefab;
        print($"Character Button Pressed door Player {playerID}");

        // Controleer of er al een afbeelding aanwezig is en verwijder deze
        foreach (Transform child in spawnPositions[playerID])
        {
            Destroy(child.gameObject);
        }

        // Instantiate de afbeelding op de juiste UI-locatie
        GameObject imageInstance = Instantiate(imagePrefab, spawnPositions[playerID]);
        imageInstance.transform.localPosition = Vector3.zero; // Zorgt voor juiste uitlijning
        imageInstance.transform.localScale = Vector3.one;     // Behoud originele schaal
    }
}