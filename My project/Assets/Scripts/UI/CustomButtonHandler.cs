using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CustomButtonHandler : MonoBehaviour, ICustomButtonHandler
{
  public UnityEvent unityEvent;
  public void OnButtonClicked()
  {
    unityEvent?.Invoke();
  }
  
}
