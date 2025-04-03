
using UnityEngine;

public class CursorMode : MonoBehaviour
{
    private void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
}
