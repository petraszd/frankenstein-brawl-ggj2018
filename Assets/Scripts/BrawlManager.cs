using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrawlManager : MonoBehaviour
{
  public delegate void ComputerMoveSelectedEvent(int index, MoveType move);
  public delegate void HealthEvent(int newHealth);
  public delegate void MatchEvent();

  public static event ComputerMoveSelectedEvent OnComputerMoveSelectedEvent;
  public static event HealthEvent OnPlayerHealthChanged;
  public static event HealthEvent OnComputerHealthChanged;
  public static event MatchEvent OnRepeatWithSameOponent;
  public static event MatchEvent OnOneOfOponentDefeated;
  public static event MatchEvent OnLastOponentDefeated;
  public static event MatchEvent OnPlayerDefeated;

  const int N_OF_STRIKES_PER_COMBINATION = 6;
  readonly YieldInstruction TIME_BETWEEN_STRIKES = new WaitForSeconds(0.75f);
  const float MOVE_DELAY_MIN = 0.05f;
  const float MOVE_DELAY_MAX = 0.1f;
  const float COMPUTER_MOVE_TIMER = 2.0f;

  public FrankController Player;
  public FrankController[] Oponents;
  int nOponent = 0;

  [HideInInspector]
  public FrankController Computer;

  MoveType[] playerMoves = new MoveType[N_OF_STRIKES_PER_COMBINATION];
  MoveType[] computerMoves = new MoveType[N_OF_STRIKES_PER_COMBINATION];

  void OnEnable()
  {
    UIManager.OnPrepareForMatch += OnPrepareForMatch;
    UIManager.OnStartFighting += OnStartFighting;
    UIManager.OnPlayerMoveSelected += OnPlayerMoveSelected;
  }

  void OnDisable()
  {
    UIManager.OnPrepareForMatch -= OnPrepareForMatch;
    UIManager.OnStartFighting -= OnStartFighting;
    UIManager.OnPlayerMoveSelected -= OnPlayerMoveSelected;
  }

  void Start()
  {
    nOponent = 0;
    Computer = Oponents[nOponent];
    EmitPlayerHealthChanged();
  }

  void PrepareComputerOpenent()
  {
    Computer.gameObject.SetActive(true);
    EmitComputerHealthChanged();

    for (int i = 0; i < computerMoves.Length; ++i) {
      MoveType move = Moves.GetRandom();
      computerMoves[i] = move;
      EmitComputerMoveSelectedEvent(i, move);
    }

    StartCoroutine(MoveOpenentInStage());
  }

  IEnumerator MoveOpenentInStage()
  {
    Vector3 position = Computer.transform.position;
    float startX = position.x;
    float endX = Player.transform.position.x * -1.0f;

    float t = 0.0f;
    while (t < COMPUTER_MOVE_TIMER) {
      yield return null;

      position.x = Mathf.SmoothStep(startX, endX, t / COMPUTER_MOVE_TIMER);
      Computer.transform.position = position;

      t += Time.deltaTime;
    }
  }

  IEnumerator Fight()
  {
    yield return null;
    for (int i = 0; i < N_OF_STRIKES_PER_COMBINATION; ++i) {
      yield return TIME_BETWEEN_STRIKES;

      MoveType playerMove = playerMoves[i];
      bool canPlayerStrike = Player.HasStrikeLimb(playerMove);
      bool canPlayerDefend = Player.HasDefenseLimb(playerMove);

      MoveType computerMove = computerMoves[i];
      bool canComputerStrike = Computer.HasStrikeLimb(computerMove);
      bool canComputerDefend = Computer.HasDefenseLimb(computerMove);

      // TODO: add random order of delaying
      if (canPlayerStrike | canPlayerDefend) {
        Player.Strike(playerMove, computerMove, canComputerDefend);
        Computer.ReactTo(computerMove, playerMove);
        EmitComputerHealthChanged();
      }
      if (!Computer.IsAlive) {
        Player.Celebrate();
        break;
      }

      yield return GetRandomDelay();

      if (canComputerStrike | canComputerDefend) {
        Computer.Strike(computerMove, playerMove, canPlayerDefend);
        Player.ReactTo(playerMove, computerMove);
        EmitPlayerHealthChanged();
      }
      if (!Player.IsAlive) {
        Computer.Celebrate();
        break;
      }
    }

    if (!Player.IsAlive) {
      OnPlayerIsDead();
    } else if (!Computer.IsAlive) {
      OnComputerIsDead();
    } else {
      OnRoundEndWithoutDeath();
    }
  }

  YieldInstruction GetRandomDelay()
  {
    return new WaitForSeconds(Random.Range(MOVE_DELAY_MIN, MOVE_DELAY_MAX));
  }

  void OnPrepareForMatch()
  {
    PrepareComputerOpenent();
  }

  void OnStartFighting()
  {
    StartCoroutine(Fight());
  }

  void OnPlayerMoveSelected(int index, MoveType move)
  {
    playerMoves[index] = move;
  }

  void EmitComputerMoveSelectedEvent(int index, MoveType move)
  {
    if (OnComputerMoveSelectedEvent != null) {
      OnComputerMoveSelectedEvent(index, move);
    }
  }

  void EmitPlayerHealthChanged()
  {
    if (OnPlayerHealthChanged != null) {
      OnPlayerHealthChanged(Player.GetHealth());
    }
  }

  void EmitComputerHealthChanged()
  {
    if (OnComputerHealthChanged != null) {
      OnComputerHealthChanged(Computer.GetHealth());
    }
  }

  void OnPlayerIsDead()
  {
    EmitPlayerIsDefeated();
  }

  void EmitPlayerIsDefeated()
  {
    if (OnPlayerDefeated != null) {
      OnPlayerDefeated();
    }
  }

  void OnComputerIsDead()
  {
    if (nOponent == Oponents.Length - 1) {
      EmitLastOponentDefeated();
    } else {
      EmitOneOfOponentDefeated();
      Computer = Oponents[++nOponent];
    }
  }

  void OnRoundEndWithoutDeath()
  {
    EmitRepeatWithSameOponent();
  }

  void EmitRepeatWithSameOponent()
  {
    if (OnRepeatWithSameOponent != null) {
      OnRepeatWithSameOponent();
    }
  }

  void EmitOneOfOponentDefeated()
  {
    if (OnOneOfOponentDefeated != null) {
      OnOneOfOponentDefeated();
    }
  }

  void EmitLastOponentDefeated()
  {
    OnLastOponentDefeated();
  }
}
