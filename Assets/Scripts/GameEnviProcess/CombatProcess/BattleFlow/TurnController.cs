using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleTurnSide
{
    None,
    Player,
    Enemy
}

public enum BattleState
{
    None,
    Preparing,
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}

public class TurnController : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private int currentTurnNumber = 0;
    [SerializeField] private BattleTurnSide currentSide = BattleTurnSide.None;

    [Header("Enemy Turn Placeholder")]
    [SerializeField] private bool autoEndEnemyTurnWithoutAI = false;
    [SerializeField] private float enemyTurnPlaceholderDelay = 0.5f;

    public int CurrentTurnNumber => currentTurnNumber;
    public BattleTurnSide CurrentSide => currentSide;

    public bool IsPlayerTurn => currentSide == BattleTurnSide.Player;
    public bool IsEnemyTurn => currentSide == BattleTurnSide.Enemy;

    public int PlayerCampId
    {
        get
        {
            if (CampManager.Instance == null)
                return 1;

            return CampManager.Instance.PlayerCampId;
        }
    }

    public event Action<int> OnRoundStarted;
    public event Action<BattleTurnSide> OnTurnSideChanged;
    public event Action OnPlayerTurnStarted;
    public event Action OnPlayerTurnEnded;
    public event Action OnEnemyTurnStarted;
    public event Action OnEnemyTurnEnded;

    public void StartBattleTurns()
    {
        StopAllCoroutines();

        currentTurnNumber = 1;
        currentSide = BattleTurnSide.None;

        OnRoundStarted?.Invoke(currentTurnNumber);

        StartPlayerTurn();
    }
    
    public void StartPlayerTurn()
    {
        currentSide = BattleTurnSide.Player;

        RestoreAPForPlayerFriendlyUnits();

        OnPlayerTurnStarted?.Invoke();
        OnTurnSideChanged?.Invoke(currentSide);

        Debug.Log($"[TurnController] Player Turn Start. Round {currentTurnNumber}");
    }

    public void EndPlayerTurn()
    {
        if (!IsPlayerTurn)
            return;

        EndTurnForPlayerFriendlyUnits();

        OnPlayerTurnEnded?.Invoke();

        Debug.Log("[TurnController] Player Turn End.");

        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        currentSide = BattleTurnSide.Enemy;

        RestoreAPForPlayerEnemyUnits();

        OnTurnSideChanged?.Invoke(currentSide);
        OnEnemyTurnStarted?.Invoke();

        Debug.Log($"[TurnController] Enemy Turn Start. Round {currentTurnNumber}");

        if (autoEndEnemyTurnWithoutAI)
            StartCoroutine(AutoEndEnemyTurnRoutine());
    }

    public void EndEnemyTurn()
    {
        if (!IsEnemyTurn)
            return;

        EndTurnForPlayerEnemyUnits();

        OnEnemyTurnEnded?.Invoke();

        Debug.Log("[TurnController] Enemy Turn End.");

        currentTurnNumber++;

        OnRoundStarted?.Invoke(currentTurnNumber);

        StartPlayerTurn();
    }

    private IEnumerator AutoEndEnemyTurnRoutine()
    {
        yield return new WaitForSeconds(enemyTurnPlaceholderDelay);

        if (IsEnemyTurn)
            EndEnemyTurn();
    }

    private void RestoreAPForPlayerFriendlyUnits()
    {
        if (UnitsManager.Instance == null)
            return;

        List<UnitBase> units = UnitsManager.Instance.GetPlayerFriendlyUnits();

        foreach (UnitBase unit in units)
        {
            if (unit == null)
                continue;

            unit.OnTurnStarted();
        }
    }

    private void RestoreAPForPlayerEnemyUnits()
    {
        if (UnitsManager.Instance == null)
            return;

        List<UnitBase> units = UnitsManager.Instance.GetPlayerEnemyUnits();

        foreach (UnitBase unit in units)
        {
            if (unit == null)
                continue;

            unit.OnTurnStarted();
        }
    }

    private void EndTurnForPlayerFriendlyUnits()
    {
        if (UnitsManager.Instance == null)
            return;

        List<UnitBase> units = UnitsManager.Instance.GetPlayerFriendlyUnits();

        foreach (UnitBase unit in units)
        {
            if (unit == null)
                continue;

            unit.OnTurnEnded();
        }
    }

    private void EndTurnForPlayerEnemyUnits()
    {
        if (UnitsManager.Instance == null)
            return;

        List<UnitBase> units = UnitsManager.Instance.GetPlayerEnemyUnits();

        foreach (UnitBase unit in units)
        {
            if (unit == null)
                continue;

            unit.OnTurnEnded();
        }
    }

    public void ResetTurns()
    {
        StopAllCoroutines();

        currentTurnNumber = 0;
        currentSide = BattleTurnSide.None;

        OnTurnSideChanged?.Invoke(currentSide);
    }
}