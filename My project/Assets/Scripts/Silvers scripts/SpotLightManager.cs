using UnityEngine;

public class SpotLightManager : MonoBehaviour
{
    public GameObject[] spotlightPrefabs;
    public Vector3 spotlightOffset = new Vector3(0, 10, 0);

    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            SpawnSpotlightForPlayer(players[i], spotlightPrefabs[i]);
        }
    }

    void SpawnSpotlightForPlayer(GameObject player, GameObject spotlightPrefab)
    {
        GameObject spotlight = Instantiate(spotlightPrefab, player.transform.position + spotlightOffset, Quaternion.identity);
        ShowLights follower = spotlight.AddComponent<ShowLights>();
        follower.SetTarget(player.transform);
    }
}