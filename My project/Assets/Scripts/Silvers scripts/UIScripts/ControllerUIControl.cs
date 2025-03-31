using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControllerUIControl : MonoBehaviour
{
    public static ControllerUIControl instance;
    
    public GameObject startFirst, CharacterSelect, StageSelect, Settings, Tutorial, Credits, DLC, IngameSettings, pausebutton;
    
    public bool characterSelect = false;
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    public void StartButton()
    {
        RemovePlayerCursor();
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startFirst);
    }

    public void OpenCharacterSelect()
    {
        characterSelect = true;
        foreach (Player player in PlayerManager.Instance.players)
        {
            player.OnCharacterSelectStart();
        }
        
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void OpenStageSelect()
    {
        
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void CloseStageSelect()
    {
        
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void OpenSettings()
    {
        RemovePlayerCursor();
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Settings);
    }
    public void OpenTutorial()
    {
        RemovePlayerCursor();
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Tutorial);
    }
    public void OpenCredits()
    {
        RemovePlayerCursor();
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Credits);
    }
    public void openDLC()
    {
        RemovePlayerCursor();
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(DLC);
    }

    public void OpenPlayerSettings()
    {
        RemovePlayerCursor();

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(IngameSettings);
    }

    public void Pause()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pausebutton);
    }


    private void RemovePlayerCursor()
    {
        if (!characterSelect) return;
        
        characterSelect = false;

        foreach (Player player in PlayerManager.Instance.players)
        {
            player.RemoveVirtualMouseForGamepad();
        }
    }
}
