using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FullscreenToggle : MonoBehaviour
{
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        if (!Screen.fullScreen)
        {
            Screen.SetResolution(1280, 720, false);
        }
    }

}