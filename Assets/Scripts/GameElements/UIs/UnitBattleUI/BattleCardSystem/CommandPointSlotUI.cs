using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandPointSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Button removeButton;

    private CommandPointController controller;
    private int slotIndex;

    private void Awake()
    {
        if (removeButton != null)
            removeButton.onClick.AddListener(HandleRemoveClicked);
    }

    public void Bind(
        CommandPointController controller,
        int slotIndex,
        bool hasPoint,
        CommandPointType pointType)
    {
        this.controller = controller;
        this.slotIndex = slotIndex;

        if (!hasPoint)
        {
            SetEmpty();
            return;
        }

        SetPoint(pointType);
    }

    private void SetPoint(CommandPointType pointType)
    {
        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.color = GetColor(pointType);
        }

        if (labelText != null)
            labelText.text = GetShortName(pointType);

        if (removeButton != null)
            removeButton.gameObject.SetActive(true);
    }

    private void SetEmpty()
    {
        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.color = new Color(1f, 1f, 1f, 0.15f);
        }

        if (labelText != null)
            labelText.text = "";

        if (removeButton != null)
            removeButton.gameObject.SetActive(false);
    }

    private void HandleRemoveClicked()
    {
        if (controller == null)
            return;

        controller.RemoveCommandPointAt(slotIndex);
    }

    private Color GetColor(CommandPointType type)
    {
        switch (type)
        {
            case CommandPointType.Mobility:
                return Color.green;

            case CommandPointType.Attack:
                return Color.red;

            case CommandPointType.Utility:
                return Color.yellow;

            case CommandPointType.Defense:
                return Color.blue;

            default:
                return Color.white;
        }
    }

    private string GetShortName(CommandPointType type)
    {
        switch (type)
        {
            case CommandPointType.Mobility:
                return "绿";

            case CommandPointType.Attack:
                return "红";

            case CommandPointType.Utility:
                return "黄";

            case CommandPointType.Defense:
                return "蓝";

            default:
                return "?";
        }
    }
}