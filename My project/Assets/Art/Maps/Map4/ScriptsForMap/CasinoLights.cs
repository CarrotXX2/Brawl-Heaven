using System.Collections;
using UnityEngine;

public class CasinoLights : MonoBehaviour
{
    public GameObject[] gameObjects; // Array van GameObjects
    public Material[] materials;    // Array van Materials
    public float switchInterval = 1.0f; // Tijd in seconden tussen elke material switch

    private void Start()
    {
        if (gameObjects.Length == 0 || materials.Length == 0)
        {
            Debug.LogError("Zorg ervoor dat de arrays voor GameObjects en Materials niet leeg zijn!");
            return;
        }

        StartCoroutine(SwitchMaterials());
    }

    private IEnumerator SwitchMaterials()
    {
        while (true)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                // Bereken de material index op basis van de cyclische volgorde
                int materialIndex = i % materials.Length;

                // Haal de Renderer component op van het GameObject
                Renderer renderer = gameObjects[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Wissel het material
                    renderer.material = materials[materialIndex];
                }
            }

            // Wacht voor de opgegeven interval
            yield return new WaitForSeconds(switchInterval);

            // Verschuif de materials naar rechts (optioneel, voor een animatie-effect)
            Material lastMaterial = materials[materials.Length - 1];
            for (int i = materials.Length - 1; i > 0; i--)
            {
                materials[i] = materials[i - 1];
            }
            materials[0] = lastMaterial;
        }
    }
}