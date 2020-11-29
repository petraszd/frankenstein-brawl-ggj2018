using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FrankController : MonoBehaviour
{
  const float LIMB_DROP_DELAY = 0.3f;
  const float ROOT_DROP_DELAY = 0.4f; // Must: ROOT > LIMB
  const float CELEBRATION_DELAY = 0.5f;

  Animator animator;

  public Limb ForeArm;
  public Limb BackArm;
  public Limb ForeLeg;
  public Limb BackLeg;

  public Limb Root;

  Dictionary<LimbType, Limb> limbs;

  public int Health;

  [HideInInspector]
  public bool IsAlive;

  void Start()
  {
    animator = GetComponent<Animator>();

    limbs = new Dictionary<LimbType, Limb>();
    limbs[LimbType.BACK_ARM] = BackArm;
    limbs[LimbType.FORE_ARM] = ForeArm;
    limbs[LimbType.BACK_LEG] = BackLeg;
    limbs[LimbType.FORE_LEG] = ForeLeg;

    IsAlive = true;
  }

  public void Die()
  {
    IsAlive = false;

    var keys = limbs.Keys;
    var types = new LimbType[keys.Count];
    var index = 0;
    foreach (LimbType t in keys) {
      types[index++] = t;
    }

    for (int i = 0; i < types.Length; ++i) {
      DropLimb(types[i]);
    }

    DropRoot();
  }

  public void Celebrate()
  {
    animator.SetTrigger("Win");
  }

  public bool HasStrikeLimb(MoveType move)
  {
    if (!Moves.IsStrike(move)) {
      return false;
    }

    return limbs[GetStrikeLimbType(move)] != null;
  }

  public bool HasDefenseLimb(MoveType move)
  {
    if (Moves.IsStrike(move)) {
      return false;
    }

    return limbs[GetDefenseLimbType(move)] != null;
  }

  public void Strike(MoveType selfMove, MoveType oponentMove, bool canOponentDefend)
  {
    LimbType type;
    if (Moves.IsStrike(selfMove)) {
      type = GetStrikeLimbType(selfMove);
    } else {
      type = GetDefenseLimbType(selfMove);
    }
    Limb limb = limbs[type];

    if (limb != null) {
      animator.SetTrigger(Moves.ComboMoveToAnimatorTrigger(selfMove));

      if (canOponentDefend && Moves.Negates(oponentMove, selfMove)) {
        if (limb != null) {
          DropLimb(type);
          if (limbs[LimbType.BACK_LEG] == null && limbs[LimbType.FORE_LEG] == null) {
            Die();
          }
        }
      }
    }
  }

  public void ReactTo(MoveType selfMove, MoveType oponentMove)
  {
    if (Moves.IsStrike(oponentMove) && !(HasDefenseLimb(selfMove) && Moves.Negates(selfMove, oponentMove))) {
      animator.SetTrigger(Moves.DamageToAnimatorTrigger(oponentMove));

      DecHealth();
    }
  }

  public int GetHealth()
  {
    return Health;
  }

  public void DecHealth()
  {
    Health = Mathf.Max(Health - 1, 0);
    if (Health == 0 && IsAlive) {
      Die();
    }
  }

  public void PlayHeadFX()
  {
    AudioManager.PlayHead();
  }

  public void PlayBallsFX()
  {
    AudioManager.PlayBalls();
  }

  public void PlayLimbFX()
  {
    AudioManager.PlayLimb();
  }

  public void PlayCelebrateFX()
  {
    AudioManager.PlayCelebrate();
  }

  LimbType GetStrikeLimbType(MoveType move)
  {
    switch (move) {
      case MoveType.STRIKE_BACK_ARM:
        return LimbType.BACK_ARM;
      case MoveType.STRIKE_FORE_ARM:
        return LimbType.FORE_ARM;
      case MoveType.STRIKE_BACK_LEG:
        return LimbType.BACK_LEG;
      case MoveType.STRIKE_FORE_LEG:
        return LimbType.FORE_LEG;
      default:
        Debug.LogError(string.Format("GetStrikeLimbType got wrong move {0}", move));
        return LimbType.FORE_ARM; // Returning whatever
    }
  }

  LimbType GetDefenseLimbType(MoveType move)
  {
    if (move == MoveType.DEFEND_HEAD) {
      if (limbs[LimbType.BACK_ARM] != null) {
        return LimbType.BACK_ARM;
      }
      return LimbType.FORE_ARM;
    }
    if (move == MoveType.DEFEND_BALLS) {
      if (limbs[LimbType.BACK_LEG] != null) {
        return LimbType.BACK_LEG;
      }
      return LimbType.FORE_LEG;
    }

    Debug.LogError(string.Format("GetDefenseLimbType got wrong move {0}", move));
    return LimbType.FORE_ARM;
  }

  void DropRoot()
  {
    StartCoroutine(DropLimbDelayed(Root, ROOT_DROP_DELAY));
  }

  void DropLimb(LimbType limbType)
  {
    Limb limb = limbs[limbType];
    limbs[limbType] = null;
    if (limb != null) {
      StartCoroutine(DropLimbDelayed(limb, LIMB_DROP_DELAY));
    }


  }

  IEnumerator DropLimbDelayed(Limb limb, float delay)
  {
    yield return new WaitForSeconds(delay);

    PlayLimbFX();

    Limb copy = Instantiate(limb, limb.transform.parent);
    copy.gameObject.name = "Detached_Limb";
    copy.DropSelf(transform.localScale.x > 0);
    copy.transform.parent = null;

    limb.gameObject.SetActive(false);
  }

  IEnumerator CelebrateDelayed()
  {
    yield return new WaitForSeconds(CELEBRATION_DELAY);
    animator.SetTrigger("Win");
  }
}
