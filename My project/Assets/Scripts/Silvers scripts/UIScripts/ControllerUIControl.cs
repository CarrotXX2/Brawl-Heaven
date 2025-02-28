using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerUIControl : MonoBehaviour

{
    public GameObject startFirst, CharacterSelect, StageSelect, Settings, Tutorial, Credits, DLC;

    // Start is called before the first frame update
    public void StartButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startFirst);
    }

    public void OpenCharacterSelect()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(CharacterSelect);
    }
    public void OpenStageSelect()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(StageSelect);
    }
    public void CloseStageSelect()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(CharacterSelect);
    }
    public void OpenSettings()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Settings);
    }
    public void OpenTutorial()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Tutorial);
    }
    public void OpenCredits()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Credits);
    }
    public void openDLC()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(DLC);
    }

}
