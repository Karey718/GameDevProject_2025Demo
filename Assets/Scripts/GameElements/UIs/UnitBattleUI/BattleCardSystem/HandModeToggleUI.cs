using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandModeToggleUI : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private HandCardController handCardController;

    [Header("Buttons")]
    [SerializeField] private Button allAvailableButton;
    [SerializeField] private Button recommendedButton;

    [Header("Optional Labels")]
    [SerializeField] private TextMeshProUGUI allAvailableLabel;
    [SerializeField] private TextMeshProUGUI recommendedLabel;

    [Header("Visual")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(1f, 1f, 1f, 0.45f);

    private void Awake()
    {
        if (allAvailableButton != null)
            allAvailableButton.onClick.AddListener(SelectAllAvailable);

        if (recommendedButton != null)
            recommendedButton.onClick.AddListener(SelectRecommended);
    }

    private void OnEnable()
    {
        if (handCardController != null)
        {
            handCardController.OnDisplayModeChanged += Refresh;
            Refresh(handCardController.DisplayMode);
        }
    }

    private void OnDisable()
    {
        if (handCardController != null)
            handCardController.OnDisplayModeChanged -= Refresh;
    }

    private void SelectAllAvailable()
    {
        if (handCardController != null)
            handCardController.SetDisplayMode(CardDisplayMode.AllAvailable);
    }

    private void SelectRecommended()
    {
        if (handCardController != null)
            handCardController.SetDisplayMode(CardDisplayMode.Recommended);
    }

    private void Refresh(CardDisplayMode mode)
    {
        bool isAll = mode == CardDisplayMode.AllAvailable;
        bool isRecommended = mode == CardDisplayMode.Recommended;

        if (allAvailableLabel != null)
            allAvailableLabel.color = isAll ? selectedColor : unselectedColor;

        if (recommendedLabel != null)
            recommendedLabel.color = isRecommended ? selectedColor : unselectedColor;
    }
}