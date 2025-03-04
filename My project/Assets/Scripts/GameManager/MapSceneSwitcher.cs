using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSceneSwitcher : MonoBehaviour
{
    public string sceneName;

    public void SwitchScene()
    {
        foreach (var player in PlayerManager.Instance.players)
        {
            player.OnGameStart();
        }
        
        SceneManager.LoadScene(sceneName);
        
    }
    
}
