
using UnityEngine;

public class CursorMode : MonoBehaviour
{
    private void Start()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
}
