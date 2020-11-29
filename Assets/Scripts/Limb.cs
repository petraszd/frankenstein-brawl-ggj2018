using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LimbType
{
  FORE_ARM,
  BACK_ARM,
  FORE_LEG,
  BACK_LEG,
};

[RequireComponent(typeof(Rigidbody2D))]
public class Limb : MonoBehaviour
{
  const float STOP_PHYSICS_DELAY = 5.0f;

  Rigidbody2D rb;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
  }

  public void DropSelf(bool frankIsLookingRight)
  {
    StartCoroutine(DropSelfSafe(frankIsLookingRight));
  }

  IEnumerator DropSelfSafe(bool frankIsLookingRight)
  {
    yield return null;
    Rigidbody2D[] bodies = GetComponentsInChildren<Rigidbody2D>();
    for (int i = 0; i < bodies.Length; ++i) {
      bodies[i].simulated = true;
    }
    rb.simulated = true;

    rb.AddForce(GetRandomAngle(frankIsLookingRight), ForceMode2D.Impulse);

    yield return new WaitForSeconds(STOP_PHYSICS_DELAY);

    for (int i = 0; i < bodies.Length; ++i) {
      bodies[i].simulated = false;
    }
    rb.simulated = false;
    rb = null;
  }

  Vector2 GetRandomAngle(bool frankIsLookingRight)
  {
    float x;
    if (frankIsLookingRight) {
      x = -1.0f;
    } else {
      x = 1.0f;
    }
    return (new Vector2(x, Random.value * 0.5f)).normalized * 12.0f; // TODO: to constant
  }
}
