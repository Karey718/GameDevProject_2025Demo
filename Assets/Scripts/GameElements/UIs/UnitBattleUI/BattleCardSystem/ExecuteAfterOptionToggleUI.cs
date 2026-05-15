using UnityEngine;
using UnityEngine.UI;

public class ExecuteAfterOptionToggleUI : MonoBehaviour
{
    [SerializeField] private ActionExecutionController executionController;
    [SerializeField] private Toggle toggle;

    private void Awake()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (toggle != null)
            toggle.onValueChanged.AddListener(HandleToggleChanged);
    }

    private void OnEnable()
    {
        if (executionController != null && toggle != null)
            toggle.isOn = executionController.ExitCardModeAfterExecution;
    }

    private void HandleToggleChanged(bool value)
    {
        if (executionController != null)
            executionController.ExitCardModeAfterExecution = value;
    }
}