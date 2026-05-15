using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleFlowController battleFlowController;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button returnToLevelSelectButton;
    [SerializeField] private Button returnToMainMenuButton;

    private void Awake()
    {
        if (returnToLevelSelectButton != null)
            returnToLevelSelectButton.onClick.AddListener(ReturnToLevelSelect);

        if (returnToMainMenuButton != null)
            returnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (titleText == null || battleFlowController == null)
            return;

        switch (battleFlowController.CurrentState)
        {
            case BattleState.Victory:
                titleText.text = "胜利";
                break;

            case BattleState.Defeat:
                titleText.text = "失败";
                break;

            default:
                titleText.text = "战斗结束";
                break;
        }
    }

    private void ReturnToLevelSelect()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.ReturnToLevelSelect();
    }

    private void ReturnToMainMenu()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.ReturnToMainMenu();
    }
}