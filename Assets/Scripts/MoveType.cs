using UnityEngine;
using System.Collections;

[System.Flags]
public enum MoveType
{
  STRIKE_FORE_ARM = 2,
  STRIKE_BACK_ARM = 4,
  STRIKE_FORE_LEG = 8,
  STRIKE_BACK_LEG = 16,

  DEFEND_HEAD = 32,
  DEFEND_BALLS = 64,
};

public static class Moves
{
  public const int N_POSSIBLE_STRIKES = 6;
  public static readonly MoveType STRIKE_MOVES = MoveType.STRIKE_BACK_ARM
                                                         | MoveType.STRIKE_FORE_ARM
                                                         | MoveType.STRIKE_BACK_LEG
                                                         | MoveType.STRIKE_FORE_LEG;

  public static readonly MoveType HEAD_STRIKE_MOVES = MoveType.STRIKE_BACK_ARM | MoveType.STRIKE_FORE_ARM;
  public static readonly MoveType BALLS_STRIKE_MOVES = MoveType.STRIKE_BACK_LEG | MoveType.STRIKE_FORE_LEG;

  public static string ComboMoveToAnimatorTrigger(MoveType move)
  {
    switch (move) {

      case MoveType.STRIKE_BACK_ARM:
        return "Frank_Back_Arm_Strike";
      case MoveType.STRIKE_FORE_ARM:
        return "Frank_Fore_Arm_Strike";
      case MoveType.STRIKE_BACK_LEG:
        return "Frank_Back_Leg_Strike";
      case MoveType.STRIKE_FORE_LEG:
        return "Frank_Fore_Leg_Strike";

      case MoveType.DEFEND_HEAD:
        return "Frank_Defend_Head";
      case MoveType.DEFEND_BALLS:
        return "Frank_Defend_Balls";

      default:
        Debug.LogError("Wrong ProActive move");
        return "";
    }
  }

  public static bool IsStrike(MoveType move)
  {
    return (move & STRIKE_MOVES) != 0;
  }

  public static bool Negates(MoveType defenseMove, MoveType attackMove)
  {
    if ((attackMove & HEAD_STRIKE_MOVES) != 0) {
      return defenseMove == MoveType.DEFEND_HEAD;
    }
    if ((attackMove & BALLS_STRIKE_MOVES) != 0) {
      return defenseMove == MoveType.DEFEND_BALLS;
    }

    return false;
  }

  public static string DamageToAnimatorTrigger(MoveType oponentMove)
  {
    switch (oponentMove) {
      case MoveType.STRIKE_BACK_ARM:
      case MoveType.STRIKE_FORE_ARM:
        return "Frank_Damage_Head";

      case MoveType.STRIKE_BACK_LEG:
      case MoveType.STRIKE_FORE_LEG:
        return "Frank_Damage_Balls";

      default:
        Debug.LogError("Wrong Damage move");
        return "";
    }
  }

  public static MoveType GetRandom()
  {
    int number = Random.Range(0, N_POSSIBLE_STRIKES);
    int move = 2;
    for (int i = 0; i < number; ++i) {
      move *= 2;
    }
    return (MoveType)move;
  }
}
