using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 预战斗部署 UI 总管理器。
/// 
/// 主要职责：
/// 1. 打开 / 关闭预战斗部署界面；
/// 2. 初始化战斗部署上下文；
/// 3. 协调单位列表、我方部署区、敌方部署区、战力预估面板刷新；
/// 4. 响应取消、快速战斗、详细战斗按钮。
/// 
/// 不直接负责具体拖拽逻辑、不直接计算战力、不直接执行战斗。
/// </summary>
public class PreBattleUIManager : MonoBehaviour
{
    public static PreBattleUIManager Instance;

    [Header("Root")]
    [SerializeField] private GameObject preBattleRoot;

    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI friendlyLimitText;
    [SerializeField] private TextMeshProUGUI enemyLimitText;
    [SerializeField] private TextMeshProUGUI battlefieldWidthText;

    [Header("Panels - Optional References")]
    [Tooltip("左侧我方可用单位列表面板")]
    [SerializeField] private DeployableUnitListPanel deployableUnitListPanel;

    [Tooltip("我方部署区面板")]
    [SerializeField] private PreBattleSidePanel friendlySidePanel;

    [Tooltip("敌方部署区面板")]
    [SerializeField] private PreBattleSidePanel enemySidePanel;

    [Tooltip("战力预估面板")]
    [SerializeField] private MonoBehaviour estimatePanel;

    [Header("Buttons")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button quickBattleButton;
    [SerializeField] private Button detailBattleButton;

    [Header("Controllers")]
    [SerializeField] private PreBattleDeploymentController deploymentController;
    [SerializeField] private HexGridInputController inputController;

    public void ClosePreBattleUI()
    {
        // 原有关闭 UI 逻辑
        gameObject.SetActive(false);

        if (inputController != null)
            inputController.SyncAfterPreBattleUIClose();
    }

    private PreBattleSetupContext currentContext;

    public bool IsOpen
    {
        get
        {
            return preBattleRoot != null && preBattleRoot.activeSelf;
        }
    }

    /// <summary>
    /// 当点击快速战斗时触发。
    /// </summary>
    public event Action<PreBattleStartData> OnQuickBattleRequested;

    /// <summary>
    /// 当点击详细战斗时触发。
    /// </summary>
    public event Action<PreBattleStartData> OnDetailBattleRequested;

    private void Awake()
    {
        Instance = this;

        if (deploymentController == null)
        {
            deploymentController = GetComponent<PreBattleDeploymentController>();
        }

        if (preBattleRoot != null)
        {
            preBattleRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        BindButtons();

        if (deploymentController != null)
        {
            deploymentController.OnDeploymentChanged += HandleDeploymentChanged;
        }
    }

    private void OnDisable()
    {
        UnbindButtons();

        if (deploymentController != null)
        {
            deploymentController.OnDeploymentChanged -= HandleDeploymentChanged;
        }
    }


    private void BindButtons()
    {
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(OnCancelClicked);
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        if (quickBattleButton != null)
        {
            quickBattleButton.onClick.RemoveListener(OnQuickBattleClicked);
            quickBattleButton.onClick.AddListener(OnQuickBattleClicked);
        }

        if (detailBattleButton != null)
        {
            detailBattleButton.onClick.RemoveListener(OnDetailBattleClicked);
            detailBattleButton.onClick.AddListener(OnDetailBattleClicked);
        }
    }

    private void UnbindButtons()
    {
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(OnCancelClicked);

        if (quickBattleButton != null)
            quickBattleButton.onClick.RemoveListener(OnQuickBattleClicked);

        if (detailBattleButton != null)
            detailBattleButton.onClick.RemoveListener(OnDetailBattleClicked);
    }

    /// <summary>
    /// 之后可以逐步改成直接传 PreBattleSetupContext。
    /// </summary>
    public void ShowBattleUI(UnitBase attacker, UnitBase defender)
    {
        PreBattleSetupContext context = PreBattleSetupContext.CreateSimple(attacker, defender);
        ShowBattleUI(context);
    }

    /// <summary>
    /// 直接传入完整战前部署上下文。
    /// </summary>
    public void ShowBattleUI(PreBattleSetupContext context)
    {
        if (context == null)
        {
            Debug.LogError("PreBattleUIManager.ShowBattleUI failed: context is null.");
            return;
        }

        currentContext = context;

        if (preBattleRoot != null)
        {
            preBattleRoot.SetActive(true);
        }

        if (friendlySidePanel != null)
        {
            friendlySidePanel.SetData(
                deploymentController,
                PreBattleSide.Friendly,
                true
            );
        }

        if (enemySidePanel != null)
        {
            enemySidePanel.SetData(
                deploymentController,
                PreBattleSide.Enemy,
                false
            );
        }

        if (deploymentController == null)
        {
            Debug.LogError("PreBattleUIManager requires PreBattleDeploymentController.");
            return;
        }

        deploymentController.Initialize(context);
        SetupSubPanels();

        RefreshAll();
    }

    public void HideBattleUI()
    {
        currentContext = null;

        if (deploymentController != null)
        {
            deploymentController.Clear();
        }

        if (preBattleRoot != null)
        {
            preBattleRoot.SetActive(false);
        }
    }

    private void SetupSubPanels()
    {
        if (friendlySidePanel != null)
        {
            friendlySidePanel.SetData(
                deploymentController,
                PreBattleSide.Friendly,
                true
            );
        }

        if (enemySidePanel != null)
        {
            enemySidePanel.SetData(
                deploymentController,
                PreBattleSide.Enemy,
                false
            );
        }

        if (deployableUnitListPanel != null)
        {
            deployableUnitListPanel.SetData(
                deploymentController,
                friendlySidePanel
            );
        }
    }

    private void RefreshAll()
    {
        RefreshTopBar();
        RefreshDeployableUnitList();
        RefreshSidePanels();
        RefreshEstimatePanel();
        RefreshButtons();
    }

    private void RefreshTopBar()
    {
        if (currentContext == null || deploymentController == null)
            return;

        if (titleText != null)
            titleText.text = "预战斗部署";

        if (friendlyLimitText != null)
        {
            friendlyLimitText.text =
                $"我方上限 {deploymentController.FriendlyDeployedCount}/{currentContext.friendlyMaxDeployCount}";
        }

        if (enemyLimitText != null)
        {
            enemyLimitText.text =
                $"敌方上限 {deploymentController.EnemyDeployedCount}/{currentContext.enemyMaxDeployCount}";
        }

        if (battlefieldWidthText != null)
        {
            battlefieldWidthText.text = $"战场宽度 {currentContext.battlefieldWidth}";
        }
    }

    private void RefreshDeployableUnitList()
    {
        if (deployableUnitListPanel != null)
        {
            deployableUnitListPanel.Refresh();
        }
    }

    private void RefreshSidePanels()
    {
        if (friendlySidePanel != null)
            friendlySidePanel.Refresh();

        if (enemySidePanel != null)
            enemySidePanel.Refresh();
    }

    private void RefreshEstimatePanel()
    {
        //
        // BattleEstimateResult result = deploymentController.CalculateEstimate();
        // estimatePanel.SetData(result);
    }

    private void RefreshButtons()
    {
        if (deploymentController == null)
            return;

        bool canStart = deploymentController.CanStartBattle(out string reason);

        if (quickBattleButton != null)
            quickBattleButton.interactable = canStart;

        if (detailBattleButton != null)
            detailBattleButton.interactable = canStart;

        // TODO:
        // 可以把 reason 显示到按钮提示、错误提示或状态栏中。
        // 例如：当前未选择任何我方单位 / 超过战场宽度 / 阵位不合法。
    }

    private void HandleDeploymentChanged()
    {
        RefreshAll();
    }

    private void OnCancelClicked()
    {
        HideBattleUI();
    }

    public void HighlightUnitCard(UnitBase unit)
    {
        if (unit == null)
            return;

        // TODO:
        // 高亮对应单位卡片。

        Debug.Log($"Highlight unit in PreBattle UI: {unit.DisplayName}");

        // if (friendlySidePanel != null)
        // {
        //     friendlySidePanel.HighlightUnitCard(unit);
        // }

        // if (enemySidePanel != null)
        // {
        //     enemySidePanel.HighlightUnitCard(unit);
        // }

        // if (deployableUnitListPanel != null)
        // {
        //     deployableUnitListPanel.HighlightUnitCard(unit);
        // }
    }

    private void OnQuickBattleClicked()
    {
        if (deploymentController == null)
            return;

        if (!deploymentController.CanStartBattle(out string reason))
        {
            Debug.LogWarning($"Cannot start quick battle: {reason}");
            return;
        }

        PreBattleStartData startData = deploymentController.BuildStartData();

        Debug.Log("Quick Battle Requested");

        OnQuickBattleRequested?.Invoke(startData);

        // TODO:
        // BattleFlowController.Instance.StartQuickBattle(startData);

        HideBattleUI();
    }

    private void OnDetailBattleClicked()
    {
        if (deploymentController == null)
            return;

        if (!deploymentController.CanStartBattle(out string reason))
        {
            Debug.LogWarning($"Cannot start detail battle: {reason}");
            return;
        }

        PreBattleStartData startData = deploymentController.BuildStartData();

        Debug.Log("Detail Battle Requested");

        OnDetailBattleRequested?.Invoke(startData);

        // TODO:
        // BattleFlowController.Instance.StartDetailBattle(startData);

        HideBattleUI();
    }
}