using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private LevelDefinition level;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(HandleClicked);
    }

    public void Bind(LevelDefinition level)
    {
        this.level = level;

        if (nameText != null)
            nameText.text = level != null ? level.levelName : "Unknown Level";

        if (descriptionText != null)
            descriptionText.text = level != null ? level.description : "";
    }

    private void HandleClicked()
    {
        if (level == null)
            return;

        if (GameFlowController.Instance != null)
            GameFlowController.Instance.StartLevel(level);
    }
}