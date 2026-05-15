using System.Collections.Generic;
using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private List<LevelDefinition> levels = new();

    [Header("UI")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private LevelSelectButtonUI buttonPrefab;

    private readonly List<LevelSelectButtonUI> spawnedButtons = new();

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        ClearButtons();

        foreach (LevelDefinition level in levels)
        {
            if (level == null)
                continue;

            LevelSelectButtonUI button = Instantiate(buttonPrefab, buttonContainer, false);
            button.Bind(level);
            spawnedButtons.Add(button);
        }
    }

    private void ClearButtons()
    {
        foreach (LevelSelectButtonUI button in spawnedButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        spawnedButtons.Clear();
    }
}