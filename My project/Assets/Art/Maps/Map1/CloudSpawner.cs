using UnityEngine;
using System.Collections;

public class CloudSpawner : MonoBehaviour
{
    public GameObject[] prefabs; // Array van prefabs die gespawnd kunnen worden
    public int maxPrefabs = 10; // Maximaal aantal prefabs dat gespawnd kan worden
    public float minSpawnInterval = 0.5f; // Minimale tijd tussen het spawnen van prefabs
    public float maxSpawnInterval = 2f; // Maximale tijd tussen het spawnen van prefabs
    public float moveSpeed = 5f; // Snelheid waarmee de prefabs bewegen
    public Vector3 moveDirection = Vector3.forward; // Richting waarin de prefabs bewegen
    public BoxCollider deletionCollider; // De box collider waarmee de prefabs moeten botsen om verwijderd te worden
    public float minY = -5f; // Minimale Y-waarde voor spawnpositie
    public float maxY = 5f; // Maximale Y-waarde voor spawnpositie
    public float minX = -10f; // Minimale X-waarde voor spawnpositie
    public float maxX = 10f; // Maximale X-waarde voor spawnpositie
    public int preloadCount = 3; // Aantal clouds dat wordt gepreload bij het starten

    private int currentPrefabCount = 0; // Huidig aantal gespawnde prefabs

    void Start()
    {
        // Normaliseer de bewegingrichting om consistent gedrag te garanderen
        moveDirection = moveDirection.normalized;

        // Preload een aantal clouds bij het starten
        PreloadClouds();

        // Start het spawnen van prefabs met een willekeurige vertraging
        StartCoroutine(SpawnPrefabsWithDelay());
    }

    void PreloadClouds()
    {
        for (int i = 0; i < preloadCount; i++)
        {
            SpawnPrefab(true); // true geeft aan dat dit een preload cloud is
        }
    }

    IEnumerator SpawnPrefabsWithDelay()
    {
        while (true)
        {
            if (currentPrefabCount < maxPrefabs)
            {
                SpawnPrefab(false); // false geeft aan dat dit geen preload cloud is
            }

            // Wacht een willekeurige tijd tussen de minimale en maximale spawninterval
            float delay = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    void SpawnPrefab(bool isPreload)
    {
        // Kies een willekeurige prefab uit de array
        GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];

        // Bepaal een willekeurige X- en Y-positie binnen het opgegeven bereik
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPosition;

        if (isPreload)
        {
            // Voor preload clouds: spawn op een willekeurige X- en Y-positie
            spawnPosition = new Vector3(randomX, randomY, transform.position.z);
        }
        else
        {
            // Voor niet-preload clouds: spawn op de spawner's X-positie en een willekeurige Y-positie
            spawnPosition = new Vector3(transform.position.x, randomY, transform.position.z);
        }

        // Spawn de prefab op de berekende positie
        GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // Verhoog het aantal gespawnde prefabs
        currentPrefabCount++;

        // Start de beweging van de prefab
        StartCoroutine(MovePrefab(spawnedPrefab));
    }

    IEnumerator MovePrefab(GameObject prefab)
    {
        while (prefab != null)
        {
            // Beweeg de prefab in de opgegeven richting
            prefab.transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

            // Controleer of de prefab botst met de deletion collider
            if (deletionCollider.bounds.Intersects(prefab.GetComponent<Collider>().bounds))
            {
                // Verwijder de prefab als deze botst met de collider
                Destroy(prefab);
                currentPrefabCount--;
                yield break;
            }

            yield return null;
        }
    }
}