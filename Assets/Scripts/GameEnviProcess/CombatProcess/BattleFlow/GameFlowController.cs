using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GameFlowState
{
    MainMenu,
    LevelSelect,
    LoadingBattle,
    InBattle,
    BattleResult
}

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private BattleLevelLoader levelLoader;
    [SerializeField] private BattleFlowController battleFlowController;

    [Header("UI Pages")]
    [SerializeField] private GameObject mainMenuPage;
    [SerializeField] private GameObject levelSelectPage;
    [SerializeField] private GameObject battleHudPage;
    [SerializeField] private GameObject battleResultPage;

    [Header("Runtime")]
    [SerializeField] private GameFlowState currentState = GameFlowState.MainMenu;
    [SerializeField] private EnemyAIController enemyAIController;

    public GameFlowState CurrentState => currentState;

    public event Action<GameFlowState> OnGameFlowStateChanged;
    private Coroutine startLevelRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (battleFlowController != null)
        {
            battleFlowController.OnBattleVictory += HandleBattleVictory;
            battleFlowController.OnBattleDefeat += HandleBattleDefeat;
        }

        ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (battleFlowController != null)
        {
            battleFlowController.OnBattleVictory -= HandleBattleVictory;
            battleFlowController.OnBattleDefeat -= HandleBattleDefeat;
        }
    }

    public void ShowMainMenu()
    {
        SetState(GameFlowState.MainMenu);

        SetPage(mainMenuPage, true);
        SetPage(levelSelectPage, false);
        SetPage(battleHudPage, false);
        SetPage(battleResultPage, false);
    }

    public void ShowLevelSelect()
    {
        SetState(GameFlowState.LevelSelect);

        SetPage(mainMenuPage, false);
        SetPage(levelSelectPage, true);
        SetPage(battleHudPage, false);
        SetPage(battleResultPage, false);
    }

    public void StartLevel(LevelDefinition level)
    {
        if (level == null)
            return;

        if (startLevelRoutine != null)
            StopCoroutine(startLevelRoutine);

        startLevelRoutine = StartCoroutine(StartLevelRoutine(level));
    }

    private IEnumerator StartLevelRoutine(LevelDefinition level)
    {
        SetState(GameFlowState.LoadingBattle);

        SetPage(mainMenuPage, false);
        SetPage(levelSelectPage, false);
        SetPage(battleResultPage, false);
        SetPage(battleHudPage, true);

        yield return WaitForBattleDependencies();

        if (levelLoader != null)
            levelLoader.LoadLevel(level);

        if (WarFogManager.Instance != null)
            WarFogManager.Instance.RefreshPlayerVision();

        if (battleFlowController != null)
            battleFlowController.StartBattle();

        SetState(GameFlowState.InBattle);

        startLevelRoutine = null;
    }

    private IEnumerator WaitForBattleDependencies()
    {
        yield return new WaitUntil(() => UnitsManager.Instance != null);
        yield return new WaitUntil(() => CampManager.Instance != null);
        yield return new WaitUntil(() => HexGridMapManager.Instance != null);

        yield return new WaitUntil(() =>
            HexGridMapManager.Instance.IsInitialized
        );

        yield return null;
    }

    public void ReturnToLevelSelect()
    {
        CleanupBattle();

        ShowLevelSelect();
    }

    public void ReturnToMainMenu()
    {
        CleanupBattle();

        ShowMainMenu();
    }

    private void HandleBattleVictory()
    {
        ShowBattleResult();
    }

    private void HandleBattleDefeat()
    {
        ShowBattleResult();
    }

    private void ShowBattleResult()
    {
        SetState(GameFlowState.BattleResult);

        SetPage(battleHudPage, true);
        SetPage(battleResultPage, true);
    }

    private void CleanupBattle()
    {
        if (enemyAIController != null)
            enemyAIController.StopAI();

        if (battleFlowController != null)
            battleFlowController.StopBattleAndCleanupState();

        if (levelLoader != null)
            levelLoader.ClearCurrentLevel();

        if (WarFogManager.Instance != null)
            WarFogManager.Instance.RefreshPlayerVision();
    }

    private void SetState(GameFlowState state)
    {
        if (currentState == state)
            return;

        currentState = state;
        OnGameFlowStateChanged?.Invoke(currentState);

        Debug.Log($"[GameFlowController] State: {currentState}");
    }

    private void SetPage(GameObject page, bool active)
    {
        if (page != null)
            page.SetActive(active);
    }
}