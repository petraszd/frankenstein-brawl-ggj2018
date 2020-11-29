using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
  const string MESSAGE_FIRST_MATCH = "Select your moves";
  const string MESSAGE_REPEAT_FIGHT = "He is still alive!";
  const string MESSAGE_PREPARE_FOR_NEXT_FIGHT = "Good.\nBut next is comming";
  const string MESSAGE_YOU_WON = "Good.\nYou are winner!";
  const string MESSAGE_YOU_LOST = "Bad.\nYou are looser!";
  const float TIMOUT_BETWEEN_MESSAGES = 2.5f;
  const float TIMOUT_BEFORE_CHANGING_SCENES = 3.0f;

  public delegate void UIMoveSelectedEvent(int index, MoveType move);
  public delegate void UIEvent();

  public static event UIEvent OnPrepareForMatch;
  public static event UIEvent OnStartFighting;
  public static event UIMoveSelectedEvent OnPlayerMoveSelected;

  public CircleCollider2D[] buttons;
  public SpriteRenderer[] selected;
  public SpriteRenderer[] playerCombo;
  public SpriteRenderer[] computerCombo;
  public BoxCollider2D startButton;
  GameObject startButtonObj;

  public GameObject buttonsWrapper;
  public GameObject panel;

  public UnityEngine.UI.Text message;
  public UnityEngine.UI.Text playerHealth;
  public UnityEngine.UI.Text computerHealth;

  Animator animator;

  int uiLayerMask;
  int currentSelected;

  bool isOver;
  bool[] visibleComputerMoves;

  MoveType[] movesMap;

  void OnEnable()
  {
    BrawlManager.OnComputerMoveSelectedEvent += OnComputerMoveSelectedEvent;
    BrawlManager.OnPlayerHealthChanged += OnPlayerHealthChanged;
    BrawlManager.OnComputerHealthChanged += OnComputerHealthChanged;

    BrawlManager.OnRepeatWithSameOponent += OnRepeatWithSameOponent;
    BrawlManager.OnOneOfOponentDefeated += OnOneOfOponentDefeated;
    BrawlManager.OnLastOponentDefeated += OnLastOponentDefeated;
    BrawlManager.OnPlayerDefeated += OnPlayerDefeated;
  }

  void OnDisable()
  {
    BrawlManager.OnComputerMoveSelectedEvent -= OnComputerMoveSelectedEvent;
    BrawlManager.OnPlayerHealthChanged -= OnPlayerHealthChanged;
    BrawlManager.OnComputerHealthChanged -= OnComputerHealthChanged;

    BrawlManager.OnRepeatWithSameOponent -= OnRepeatWithSameOponent;
    BrawlManager.OnOneOfOponentDefeated -= OnOneOfOponentDefeated;
    BrawlManager.OnLastOponentDefeated -= OnLastOponentDefeated;
    BrawlManager.OnPlayerDefeated -= OnPlayerDefeated;
  }

  void Start()
  {
    isOver = false;
    animator = GetComponent<Animator>();

    InitVisibleComputerMoves();
    uiLayerMask = LayerMask.GetMask("UI");
    startButtonObj = startButton.gameObject;

    movesMap = new MoveType[] {
      MoveType.DEFEND_BALLS,
      MoveType.DEFEND_HEAD,
      MoveType.STRIKE_FORE_LEG,
      MoveType.STRIKE_BACK_LEG,
      MoveType.STRIKE_FORE_ARM,
      MoveType.STRIKE_BACK_ARM,
    };

    ShowMessage(MESSAGE_FIRST_MATCH);
    PreprareForMatch();
  }

  void Update()
  {
    if (isOver) {
      return;
    }

    if (IsEnoughSelected()) {
      ProcessStartButtonClick();
    } else {
      ProcessComboSelection();
    }
  }

  void PreprareForMatch()
  {
    HideAllSelected();
    HideAllComputerMoves();
    HideAllPlayerMoves();
    buttonsWrapper.SetActive(true);
    panel.SetActive(true);
    startButtonObj.SetActive(false);

    currentSelected = 0;

    StartCoroutine(EmitPrepareForMatch());
  }

  void ProcessComboSelection()
  {
    if (IsPointInput()) {
      Vector2 mousePosition = GetPointInputPosition();
      Collider2D coll = Physics2D.OverlapPoint(mousePosition, uiLayerMask);

      if (coll != null) {
        for (int i = 0; i < buttons.Length; ++i) {
          if (coll == buttons[i]) {
            SpriteRenderer buttonRenderer = coll.GetComponent<SpriteRenderer>();
            selected[currentSelected].sprite = buttonRenderer.sprite;
            selected[currentSelected].flipX = buttonRenderer.flipX;
            playerCombo[currentSelected].sprite = buttonRenderer.sprite;
            playerCombo[currentSelected].flipX = buttonRenderer.flipX;

            ShowMove(playerCombo[currentSelected]);
            EmitPlayerMoveSelected(currentSelected, movesMap[i]);

            currentSelected++;

            if (IsEnoughSelected()) {
              OnEnoughSelected();
            }
            break;
          }
        }
      }
    }
  }

  bool IsPointInput()
  {
    if (Input.GetMouseButtonUp(0)) {
      return true;
    }
    for (int i = 0; i < Input.touches.Length; ++i) {
      if (Input.touches[i].phase == TouchPhase.Ended) {
        return true;
      }
    }
    return false;
  }

  Vector2 GetPointInputPosition()
  {
    Vector2 result = Vector2.zero;
    if (Input.GetMouseButtonUp(0)) {
      result = Input.mousePosition;
    }

    for (int i = 0; i < Input.touches.Length; ++i) {
      if (Input.touches[i].phase == TouchPhase.Ended) {
        result = Input.touches[i].position;
      }
    }

    return Camera.main.ScreenToWorldPoint(result);
  }

  void ProcessStartButtonClick()
  {
    if (IsPointInput()) {
      Vector2 mousePosition = GetPointInputPosition();
      Collider2D coll = Physics2D.OverlapPoint(mousePosition, uiLayerMask);

      if (coll != null && coll == startButton) {
        OnStartClicked();
      }
    }
  }

  bool IsEnoughSelected()
  {
    return currentSelected >= selected.Length;
  }

  void OnEnoughSelected()
  {
    buttonsWrapper.SetActive(false);
    startButtonObj.SetActive(true);
  }

  void OnStartClicked()
  {
    panel.SetActive(false);
    UnhideAllComputerMoves();
    EmitStartFighting();
  }

  IEnumerator EmitPrepareForMatch()
  {
    yield return new WaitForSeconds(0.1f); // TODO: to constant
    if (OnPrepareForMatch != null) {
      OnPrepareForMatch();
    }
  }

  void OnComputerMoveSelectedEvent(int index, MoveType move)
  {
    int moveIndex = 0;
    for (int i = 0; i < movesMap.Length; ++i) {
      if (movesMap[i] == move) {
        moveIndex = i;
        break;
      }
    }
    SpriteRenderer buttonRenderer = buttons[moveIndex].GetComponent<SpriteRenderer>();
    computerCombo[index].sprite = buttonRenderer.sprite;
    computerCombo[index].flipX = buttonRenderer.flipX;

    UnhideAvailableComputerMoves();
  }

  void EmitStartFighting()
  {
    if (OnStartFighting != null) {
      OnStartFighting();
    }
  }

  void EmitPlayerMoveSelected(int index, MoveType move)
  {
    if (OnPlayerMoveSelected != null) {
      OnPlayerMoveSelected(index, move);
    }
  }

  void OnPlayerHealthChanged(int newHealth)
  {
    UpdateHealthText(playerHealth, newHealth);
  }

  void OnComputerHealthChanged(int newHealth)
  {
    UpdateHealthText(computerHealth, newHealth);
  }

  void UpdateHealthText(UnityEngine.UI.Text text, int health)
  {
    // Hack to get a delay cause UI is out of sycn with anim
    StartCoroutine(UpdateHealthTextDelayed(text, health));
  }

  IEnumerator UpdateHealthTextDelayed(UnityEngine.UI.Text text, int health)
  {
    yield return new WaitForSeconds(0.3f);
    string newText = string.Format("Health: {0}", health);
    text.text = newText;
  }

  void HideAllSelected()
  {
    for (int i = 0; i < selected.Length; ++i) {
      selected[i].sprite = null;
    }
  }

  void HideAllComputerMoves()
  {
    for (int i = 0; i < computerCombo.Length; ++i) {
      HideMove(computerCombo[i]);
    }
  }

  void HideAllPlayerMoves()
  {
    for (int i = 0; i < playerCombo.Length; ++i) {
      HideMove(playerCombo[i]);
    }
  }

  void UnhideAllComputerMoves()
  {
    for (int i = 0; i < computerCombo.Length; ++i) {
      ShowMove(computerCombo[i]);
    }
  }

  void UnhideAvailableComputerMoves()
  {
    for (int i = 0; i < computerCombo.Length; ++i) {
      if (visibleComputerMoves[i]) {
        ShowMove(computerCombo[i]);
      }
    }
  }

  void HideMove(SpriteRenderer r)
  {
    r.color = Color.black;
  }

  void ShowMove(SpriteRenderer r)
  {
    r.color = Color.white;
  }

  void InitVisibleComputerMoves()
  {
    visibleComputerMoves = new bool[computerCombo.Length];
    for (int i = 0; i < computerCombo.Length; ++i) {
      visibleComputerMoves[i] = true;
    }
  }

  void MarkAsHiddenRandomComputerMove()
  {
    int nVisible = 0;
    for (int i = 0; i < visibleComputerMoves.Length; ++i) {
      if (visibleComputerMoves[i]) {
        nVisible++;
      }
    }

    if (nVisible == 0) {
      return;
    }

    int toHide = Random.Range(0, nVisible);
    int visibleIndex = 0;
    for (int i = 0; i < visibleComputerMoves.Length; ++i) {
      if (visibleComputerMoves[i]) {
        if (visibleIndex == toHide) {
          visibleComputerMoves[i] = false;
          return;
        }
        visibleIndex++;
      }
    }
  }

  void OnRepeatWithSameOponent()
  {
    ShowMessage(MESSAGE_REPEAT_FIGHT);
    StartCoroutine(PreprareForMatchDelayed());
  }

  IEnumerator PreprareForMatchDelayed()
  {
    yield return new WaitForSeconds(TIMOUT_BETWEEN_MESSAGES);
    PreprareForMatch();
  }

  void OnOneOfOponentDefeated()
  {
    MarkAsHiddenRandomComputerMove();
    ShowMessage(MESSAGE_PREPARE_FOR_NEXT_FIGHT);
    StartCoroutine(PreprareForMatchDelayed());
  }

  void OnLastOponentDefeated()
  {
    isOver = true;
    ShowMessage(MESSAGE_YOU_WON);

    StartCoroutine(GoToSuccessScreen());
  }

  void OnPlayerDefeated()
  {
    isOver = true;
    ShowMessage(MESSAGE_YOU_LOST);

    StartCoroutine(GoToFailureScreen());
  }

  void ShowMessage(string newText)
  {
    message.text = newText;
    animator.SetTrigger("Message");
  }

  IEnumerator GoToFailureScreen()
  {
    yield return new WaitForSeconds(TIMOUT_BEFORE_CHANGING_SCENES);
    UnityEngine.SceneManagement.SceneManager.LoadScene("End_Failure");
  }

  IEnumerator GoToSuccessScreen()
  {
    yield return new WaitForSeconds(TIMOUT_BEFORE_CHANGING_SCENES);
    UnityEngine.SceneManagement.SceneManager.LoadScene("End_Success");
  }
}
