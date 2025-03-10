using UnityEngine;

public class SpotLightManager : MonoBehaviour
{
    public GameObject spotlightPrefab; // Prefab van de spotlight
    public Vector3 spotlightOffset = new Vector3(0, 10, 0); // Offset voor de spotlight (boven de speler)

    void Start()
    {
        // Zoek alle objecten met de tag "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Loop door alle spelers en spawn een spotlight voor elke speler
        foreach (GameObject player in players)
        {
            SpawnSpotlightForPlayer(player);
        }
    }

    void SpawnSpotlightForPlayer(GameObject player)
    {
        if (spotlightPrefab == null)
        {
            Debug.LogError("Spotlight prefab is not assigned in the PlayerSpotlightManager script.");
            return;
        }

        // Spawn de spotlight op de positie van de speler + offset
        GameObject spotlight = Instantiate(spotlightPrefab, player.transform.position + spotlightOffset, Quaternion.identity);

        // Voeg het SpotlightFollower script toe aan de spotlight en koppel de speler
        ShowLights follower = spotlight.AddComponent<ShowLights>();
        follower.SetTarget(player.transform);
    }
}