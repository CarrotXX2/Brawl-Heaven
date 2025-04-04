using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class FullscreenToggle : MonoBehaviour
{
    private int savedWidth;
    private int savedHeight;

    void Start()
    {
        // Sla de huidige resolutie op bij het opstarten
        savedWidth = Screen.width;
        savedHeight = Screen.height;

        GetComponent<Button>().onClick.AddListener(ToggleFullscreen);
    }

    public void ToggleFullscreen()
    {
        if (Screen.fullScreen)
        {
            // Schakel terug naar windowed mode en herstel de vorige resolutie
            Screen.SetResolution(savedWidth, savedHeight, false);
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
        else
        {
            // Sla de huidige resolutie op voordat je fullscreen gaat
            savedWidth = Screen.width;
            savedHeight = Screen.height;

            // Schakel naar fullscreen
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
    }
}
