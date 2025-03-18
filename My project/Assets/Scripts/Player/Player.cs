using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Player Info")]
    public int playerID;

    [Header("Character Info")]
    public GameObject characterPrefab; // Selected character
    private PlayerController playerController; // Handles character input

    [Header("Cursor Variables")]
    [SerializeField] private RectTransform cursorPrefab; // Cursor prefab
    private RectTransform cursorInstance; // Cursor instance
    
    [SerializeField] private float cursorSpeed = 500f; // Cursor movement speed
    
    private Mouse virtualMouse; // Virtual mouse instance
    private Vector2 cursorPosition;
    private Gamepad gamepad; // Reference to player's gamepad

    [Header("Component References")]
    private Canvas canvas;

    void Awake()
    {
        PlayerManager.Instance.AddPlayer(gameObject);
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (ControllerUIControl.instance.characterSelect)
        {
            OnCharacterSelectStart();
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        GameplayManager gameplayManager = GameplayManager.Instance;

        if (gameplayManager != null)
        {
            gameObject.GetComponentInChildren<PlayerController>(true).GameStartSetup();
        }
    }
    #region Character Selection

    public void OnCharacterSelectStart()
    {
        AddVirtualMouseForGamepad();
    }

    private void AddVirtualMouseForGamepad()
    {
        if (cursorInstance != null) return; // Prevent duplicates

        virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        InputUser.PerformPairingWithDevice(virtualMouse); // Ensure UI recognizes it

        gamepad = Gamepad.current; // Assign current gamepad to this player

        cursorInstance = Instantiate(cursorPrefab, canvas.transform);
        cursorPosition = new Vector2(Screen.width / 2, Screen.height / 2); // Set initial position
        cursorInstance.position = cursorPosition;
    }

    public void RemoveVirtualMouseForGamepad()
    {
        if (cursorInstance == null) return;

        Destroy(cursorInstance.gameObject);
        cursorInstance = null;

        if (virtualMouse != null)
        {
            InputSystem.RemoveDevice(virtualMouse);
            virtualMouse = null;
        }
    }

    #endregion

    public void OnGameStart()
    {
        GameObject playerCharacter = Instantiate(characterPrefab, transform);
        playerController = playerCharacter.GetComponent<PlayerController>();
    }

    private void Update() // TODO: can control multiple cursors at once in some cases fix ;3
    {
        if (cursorInstance == null || virtualMouse == null || gamepad == null) return;

        // Read input from left stick
        Vector2 moveInput = gamepad.leftStick.ReadValue();
        cursorPosition += moveInput * cursorSpeed * Time.deltaTime;

        // Clamp cursor position to stay within screen bounds
        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);

        // Move the cursor in UI
        cursorInstance.position = cursorPosition;

        // Handle clicking
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            ClickUIElement();
        }

        // Update virtual mouse position
        InputState.Change(virtualMouse.position, cursorPosition);
    }
    
    private void ClickUIElement()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = cursorInstance.position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            GameObject clickedObject = result.gameObject;

            // If a text or child element is clicked, get the parent button
            Button button = clickedObject.GetComponentInParent<Button>();
            if (button != null)
            {
                clickedObject = button.gameObject;
            }

            // If the button implements the CharacterButton logic, pass playerID
            if (clickedObject.TryGetComponent(out CharacterButton characterButton))
            {
                characterButton.OnButtonClicked(playerID);
                Debug.Log($"Player {playerID} selected {characterButton.characterPrefab.name}");
            }
            // If the button has a generic event, invoke it
            else if (clickedObject.TryGetComponent(out ICustomButtonHandler buttonHandler))
            {
                buttonHandler.OnButtonClicked();
                Debug.Log($"Player {playerID} clicked {clickedObject.name}");
            }

            break; // Stop after first valid UI element
        }
    }
    #region Input Handling

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (cursorInstance != null) return; // Ignore movement if in character select

        if (CanPerformCharacterActions()) // Move character if game started
        {
            playerController.OnMove(ctx);
        }
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!CanPerformCharacterActions()) return;
        playerController.OnJump(ctx);
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (!CanPerformCharacterActions()) return;
        playerController.OnDash(ctx);
    }

    public void OnBlock(InputAction.CallbackContext ctx)
    {
        if (!CanPerformCharacterActions()) return;
        playerController.OnBlock(ctx);
    }

    public void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (!CanPerformCharacterActions()) return;
        playerController.OnLightAttack(ctx);
    }

    public void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        if (!CanPerformCharacterActions()) return;
        playerController.OnHeavytAttack(ctx);
    }

    public void OnUltimateCast(InputAction.CallbackContext ctx) 
    {
        if (!CanPerformCharacterActions()) return;
        playerController.OnUltimateCast(ctx);
    }

    public void OnRightAnalogStickMove(InputAction.CallbackContext ctx)
    {
        if (!CanPerformCharacterActions()) return;
        playerController.OnRightAnalogStickMove(ctx);
    }

    private bool CanPerformCharacterActions()
    {
        return playerController != null;
    }

    #endregion
}
