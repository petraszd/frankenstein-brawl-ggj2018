using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunMainManager : MonoBehaviour
{
  public float Timout = 0.1f;

  bool isOn = false;

  void Start()
  {
    isOn = false;
    StartCoroutine(TurnOnUIDelayed());
  }

  void Update()
  {
    if (!isOn) {
      return;
    }

    if (Input.touches.Length > 0 || Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return)) {
      UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
  }

  IEnumerator TurnOnUIDelayed()
  {
    yield return new WaitForSeconds(Timout);
    isOn = true;
  }
}
