using UnityEngine;
using UnityEngine.UI;

public class CommandPointButtonUI : MonoBehaviour
{
    [Header("Command")]
    [SerializeField] private CommandPointType pointType;

    [Header("Hotkey")]
    [SerializeField] private KeyCode hotkey = KeyCode.None;
    [SerializeField] private KeyCode alternativeHotkey = KeyCode.None;

    [Header("Controller")]
    [SerializeField] private CommandPointController commandPointController;

    [Header("UI")]
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    private void Update()
    {
        if (hotkey != KeyCode.None && Input.GetKeyDown(hotkey))
        {
            TryAddCommandPoint();
            return;
        }

        if (alternativeHotkey != KeyCode.None && Input.GetKeyDown(alternativeHotkey))
        {
            TryAddCommandPoint();
        }
    }

    private void HandleClick()
    {
        TryAddCommandPoint();
    }

    private void TryAddCommandPoint()
    {
        if (commandPointController == null)
            return;

        commandPointController.AddCommandPoint(pointType);
    }
}