using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem.Users;

public class MultiVirtualMouseManager : MonoBehaviour
{
    public static MultiVirtualMouseManager Instance;
    
    public RectTransform cursorPrefab; // Assign a cursor UI prefab
    public Canvas canvas; // Reference to the UI Canvas
    public float cursorSpeed = 500f;

    public Dictionary<Gamepad, VirtualMouseData> playerMice = new Dictionary<Gamepad, VirtualMouseData>();

    private void Awake()
    {
        Instance = this;
    }
    
    public class VirtualMouseData
    {
        public Mouse virtualMouse;
        public RectTransform cursorTransform;
        public Vector2 cursorPosition;
    }
    
    void Update()
    {
        foreach (var kvp in playerMice)
        {
            Gamepad gamepad = kvp.Key;
            VirtualMouseData data = kvp.Value;

            if (gamepad == null) continue;

            // Read input from left stick
            Vector2 moveInput = gamepad.leftStick.ReadValue();
            data.cursorPosition += moveInput * cursorSpeed * Time.deltaTime;

            // Clamp cursor position to screen
            data.cursorPosition.x = Mathf.Clamp(data.cursorPosition.x, 0, Screen.width);
            data.cursorPosition.y = Mathf.Clamp(data.cursorPosition.y, 0, Screen.height);

            // Move the UI cursor
            data.cursorTransform.position = data.cursorPosition;

            // Handle clicking (using proper input events)
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                InputSystem.QueueStateEvent(data.virtualMouse, new MouseState { buttons = 1 });
                InputSystem.Update(); // Force input system to process immediately
            }
            else if (gamepad.buttonSouth.wasReleasedThisFrame)
            {
                InputSystem.QueueStateEvent(data.virtualMouse, new MouseState { buttons = 0 });
                InputSystem.Update();
            }

            // Update the virtual mouse position
            InputState.Change(data.virtualMouse.position, data.cursorPosition);
        }
    }

    public void AddVirtualMouseForGamepad(Gamepad gamepad)
    {
        if (playerMice.ContainsKey(gamepad)) return;

        Mouse virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");

        // Ensure UI recognizes the virtual mouse
        InputUser.PerformPairingWithDevice(virtualMouse);

        RectTransform newCursor = Instantiate(cursorPrefab, canvas.transform);
            
        newCursor.position = new Vector2(Screen.width / 2, Screen.height / 2);
            
        VirtualMouseData data = new VirtualMouseData
        {
            virtualMouse = virtualMouse,
            cursorTransform = newCursor,
            cursorPosition = newCursor.position
        };

        playerMice[gamepad] = data;
    }

    public void RemoveVirtualMouseForGamepad(Gamepad gamepad)
    {
        if (!playerMice.ContainsKey(gamepad)) return;

        VirtualMouseData data = playerMice[gamepad];
        InputSystem.RemoveDevice(data.virtualMouse);
        Destroy(data.cursorTransform.gameObject);

        playerMice.Remove(gamepad);
    }
    
    private void OnDestroy()
    {
        foreach (var data in playerMice.Values)
        {
            InputSystem.RemoveDevice(data.virtualMouse);
            Destroy(data.cursorTransform.gameObject);
        }
        playerMice.Clear();
    }
}

