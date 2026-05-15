using TMPro;
using UnityEngine;

public class TargetingHintUI : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private ActionTargetingController targetingController;

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI hintText;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (targetingController != null)
        {
            targetingController.OnTargetingStarted += HandleTargetingStarted;
            targetingController.OnTargetingEnded += HandleTargetingEnded;
            targetingController.OnTargetingStepChanged += HandleTargetingStepChanged;
        }
    }

    private void OnDisable()
    {
        if (targetingController != null)
        {
            targetingController.OnTargetingStarted -= HandleTargetingStarted;
            targetingController.OnTargetingEnded -= HandleTargetingEnded;
            targetingController.OnTargetingStepChanged -= HandleTargetingStepChanged;
        }
    }

    private void HandleTargetingStarted(PlannedActionData action)
    {
        SetVisible(true);
    }

    private void HandleTargetingStepChanged(
        PlannedActionData action,
        TargetingStepType step,
        int stepIndex,
        int stepCount)
    {
        if (hintText != null)
            hintText.text = BuildStepHintText(action, step, stepIndex, stepCount);

        SetVisible(true);
    }

    private void HandleTargetingEnded()
    {
        SetVisible(false);
    }

    private string BuildStepHintText(
        PlannedActionData action,
        TargetingStepType step,
        int stepIndex,
        int stepCount)
    {
        string cardName = action != null && action.card != null
            ? action.card.cardName
            : "行动";

        string stepPrefix = stepCount > 1
            ? $"[{stepIndex + 1}/{stepCount}] "
            : "";

        switch (step)
        {
            case TargetingStepType.SelectMoveTile:
                return $"{stepPrefix}{cardName}: 请选择移动目标格。右键返回上一步, Esc取消。";

            case TargetingStepType.SelectEnemyUnit:
                return $"{stepPrefix}{cardName}: 请选择攻击目标。右键返回上一步, Esc取消。";

            case TargetingStepType.SelectFriendlyUnit:
                return $"{stepPrefix}{cardName}: 请选择友方目标。右键返回上一步, Esc取消。";

            case TargetingStepType.SelectArea:
                return $"{stepPrefix}{cardName}: 请选择区域中心。右键返回上一步, Esc取消。";

            default:
                return $"{stepPrefix}{cardName}: 请选择目标。右键返回上一步, Esc取消。";
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}