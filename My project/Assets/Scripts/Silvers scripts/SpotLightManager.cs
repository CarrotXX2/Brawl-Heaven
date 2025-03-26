using UnityEngine;

public class SpotLightManager : MonoBehaviour
{
    public GameObject[] spotlightPrefabs;
    public Vector3 spotlightOffset = new Vector3(0, 10, 0);

    void Start()
    {
        if (GameplayManager.Instance == null || GameplayManager.Instance.playersAlive == null)
        {
            return;
        }

        int spotlightIndex = 0;

        foreach (var player in GameplayManager.Instance.playersAlive)
        {
            SpawnSpotlightForPlayer(player, spotlightPrefabs[spotlightIndex]);
            spotlightIndex++;
        }
    }

    void SpawnSpotlightForPlayer(GameObject player, GameObject spotlightPrefab)
    {
        GameObject spotlight = Instantiate(spotlightPrefab, player.transform.position + spotlightOffset, Quaternion.identity);
        ShowLights follower = spotlight.AddComponent<ShowLights>();
        follower.SetTarget(player.transform);
    }
}