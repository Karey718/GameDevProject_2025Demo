using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreBattleUIManager : MonoBehaviour
{
    public static PreBattleUIManager Instance;

    [Header("Battle UI Prefab")]
    public GameObject battleUIPrefab;

    private GameObject currentBattleUI;

    private UnitBase currentAttacker;
    private UnitBase currentDefender;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 显示战斗UI
    /// </summary>
    public void ShowBattleUI(UnitBase attacker, UnitBase defender)
    {
        HideBattleUI();

        currentAttacker = attacker;
        currentDefender = defender;

        Vector3 spawnPos = attacker.transform.position + Vector3.up * 5f;

        currentBattleUI = Instantiate(battleUIPrefab, spawnPos, Quaternion.identity);

        // currentBattleUI.transform.LookAt(Camera.main.transform);
        currentBattleUI.transform.Rotate(45f, 0, 0);

        SetupUI();
    }

    void SetupUI()
    {
        TextMeshProUGUI[] texts = currentBattleUI.GetComponentsInChildren<TextMeshProUGUI>();

        foreach (TextMeshProUGUI t in texts)
        {
            if (t.name == "AttackerName")
                t.text = currentAttacker.unitName;

            if (t.name == "DefenderName")
                t.text = currentDefender.unitName;
        }

        Button[] buttons = currentBattleUI.GetComponentsInChildren<Button>();

        foreach (Button btn in buttons)
        {
            if (btn.name == "Button_Quick")
                btn.onClick.AddListener(OnQuickBattle);

            if (btn.name == "Button_Detail")
                btn.onClick.AddListener(OnDetailBattle);
        }
    }

    public void HideBattleUI()
    {
        if (currentBattleUI != null)
        {
            Destroy(currentBattleUI);
        }
    }

    void OnQuickBattle()
    {
        Debug.Log("Quick Battle Start");

        // TODO: 快速战斗逻辑
        currentAttacker.TryAttack(currentDefender);

        HideBattleUI();
    }

    void OnDetailBattle()
    {
        Debug.Log("Detail Battle Start");

        // TODO: 详细战斗逻辑接口
        StartDetailBattle(currentAttacker, currentDefender);

        HideBattleUI();
    }

    void StartDetailBattle(UnitBase attacker, UnitBase defender)
    {
        // 预留接口
    }
}
