using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitListener : MonoBehaviour
{
  public static QuitListener instance = null;

  private void Awake()
  {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
    }
    DontDestroyOnLoad(gameObject);
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape) | Input.GetKeyDown(KeyCode.Q)) {
      Application.Quit();
    }
  }
}
