using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(HandleStartClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(HandleQuitClicked);
    }

    private void HandleStartClicked()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.ShowLevelSelect();
    }

    private void HandleQuitClicked()
    {
        Application.Quit();
    }
}