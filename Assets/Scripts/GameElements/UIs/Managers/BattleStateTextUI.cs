using TMPro;
using UnityEngine;

public class BattleStateTextUI : MonoBehaviour
{
    [SerializeField] private BattleFlowController battleFlowController;
    [SerializeField] private TurnController turnController;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (battleFlowController != null)
            battleFlowController.OnBattleStateChanged += HandleBattleStateChanged;

        if (turnController != null)
        {
            turnController.OnRoundStarted += HandleRoundStarted;
            turnController.OnTurnSideChanged += HandleTurnSideChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (battleFlowController != null)
            battleFlowController.OnBattleStateChanged -= HandleBattleStateChanged;

        if (turnController != null)
        {
            turnController.OnRoundStarted -= HandleRoundStarted;
            turnController.OnTurnSideChanged -= HandleTurnSideChanged;
        }
    }

    private void HandleBattleStateChanged(BattleState state)
    {
        Refresh();
    }

    private void HandleRoundStarted(int round)
    {
        Refresh();
    }

    private void HandleTurnSideChanged(BattleTurnSide side)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (text == null || battleFlowController == null || turnController == null)
            return;

        string sideText = "";

        switch (turnController.CurrentSide)
        {
            case BattleTurnSide.Player:
                sideText = "玩家回合";
                break;

            case BattleTurnSide.Enemy:
                sideText = "敌方回合";
                break;

            default:
                sideText = "准备中";
                break;
        }

        if (battleFlowController.CurrentState == BattleState.Victory)
            sideText = "胜利";

        if (battleFlowController.CurrentState == BattleState.Defeat)
            sideText = "失败";

        text.text = $"第 {turnController.CurrentTurnNumber} 回合 - {sideText}";
    }
}