using UnityEngine;

public class ExitGameButton : MonoBehaviour
{
    // Deze functie wordt opgeroepen via de Button OnClick() in de Inspector
    public void ExitGame()
    {
        Debug.Log("Game sluiten...");

        // Als je in de Editor zit, stopt dit de play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Sluit de game in een build
        Application.Quit();
#endif
    }
}
