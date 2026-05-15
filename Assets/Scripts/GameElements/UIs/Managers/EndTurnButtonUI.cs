using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnController turnController;
    [SerializeField] private BattleFlowController battleFlowController;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI reasonText;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(HandleClicked);
    }

    private void Start()
    {
        if (turnController == null)
            turnController = FindObjectOfType<TurnController>();

        if (battleFlowController == null)
            battleFlowController = FindObjectOfType<BattleFlowController>();

        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool canClick = false;
        string reason = "";

        if (battleFlowController != null)
            canClick = battleFlowController.CanEndPlayerTurn(out reason);
        else
            reason = "BattleFlowController 未绑定";

        if (button != null)
            button.interactable = canClick;

        if (labelText != null)
            labelText.text = canClick ? "结束回合" : "等待中";

        if (reasonText != null)
            reasonText.text = canClick ? "" : reason;
    }

    private void HandleClicked()
    {
        if (battleFlowController == null)
            return;

        battleFlowController.RequestEndPlayerTurn();
    }
}