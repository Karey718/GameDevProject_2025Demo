using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 左侧候选单位列表面板。
/// 
/// 1. 显示我方所有可参与本次战斗的单位；
/// 2. 区分已部署 / 未部署状态；
/// 3. 点击单位卡时选中单位；
/// 4. 通知我方部署区高亮可放置槽位；
/// 5. 为后续单位筛选、单位详情、拖拽部署预留接口。
/// </summary>
public class DeployableUnitListPanel : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private PreBattleDeploymentController deploymentController;

    [Header("Related UI")]
    [Tooltip("我方部署区，用于高亮合法槽位")]
    [SerializeField] private PreBattleSidePanel friendlySidePanel;

    [Header("List")]
    [Tooltip("单位卡片生成的父节点，通常是 ScrollView/Viewport/Content")]
    [SerializeField] private Transform contentRoot;

    [Tooltip("左侧候选单位卡片 prefab")]
    [SerializeField] private PreBattleUnitCard unitCardPrefab;

    [Header("Optional UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI countText;

    [Header("Filter Buttons - Optional")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button groundButton;
    [SerializeField] private Button armorButton;
    [SerializeField] private Button artilleryButton;
    [SerializeField] private Button airButton;

    [Header("Options")]
    [SerializeField] private bool allowDragging = true;
    [SerializeField] private bool hideDeployedUnits = false;

    private readonly List<PreBattleUnitCard> spawnedCards = new();

    private UnitBase selectedUnit;
    private DeployableUnitFilter currentFilter = DeployableUnitFilter.All;

    public UnitBase SelectedUnit => selectedUnit;

    private void Awake()
    {
        if (deploymentController == null)
        {
            deploymentController = FindObjectOfType<PreBattleDeploymentController>();
        }

        BindFilterButtons();
    }

    private void OnEnable()
    {
        if (deploymentController != null)
        {
            deploymentController.OnDeploymentChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (deploymentController != null)
        {
            deploymentController.OnDeploymentChanged -= Refresh;
        }
    }

    private void BindFilterButtons()
    {
        if (allButton != null)
        {
            allButton.onClick.RemoveListener(() => SetFilter(DeployableUnitFilter.All));
            allButton.onClick.AddListener(() => SetFilter(DeployableUnitFilter.All));
        }

        if (groundButton != null)
        {
            groundButton.onClick.RemoveListener(() => SetFilter(DeployableUnitFilter.Ground));
            groundButton.onClick.AddListener(() => SetFilter(DeployableUnitFilter.Ground));
        }

        if (armorButton != null)
        {
            armorButton.onClick.RemoveListener(() => SetFilter(DeployableUnitFilter.Armor));
            armorButton.onClick.AddListener(() => SetFilter(DeployableUnitFilter.Armor));
        }

        if (artilleryButton != null)
        {
            artilleryButton.onClick.RemoveListener(() => SetFilter(DeployableUnitFilter.Artillery));
            artilleryButton.onClick.AddListener(() => SetFilter(DeployableUnitFilter.Artillery));
        }

        if (airButton != null)
        {
            airButton.onClick.RemoveListener(() => SetFilter(DeployableUnitFilter.Air));
            airButton.onClick.AddListener(() => SetFilter(DeployableUnitFilter.Air));
        }
    }

    /// <summary>
    /// 外部初始化接口。
    /// 可以由 PreBattleUIManager 在 ShowBattleUI 时调用。
    /// </summary>
    public void SetData(
        PreBattleDeploymentController controller,
        PreBattleSidePanel sidePanel
    )
    {
        deploymentController = controller;
        friendlySidePanel = sidePanel;

        Refresh();
    }

    public void Refresh()
    {
        if (deploymentController == null)
        {
            ClearCards();
            return;
        }

        RefreshTitle();

        List<UnitBase> allUnits = deploymentController.GetAllFriendlyUnitsForDeployList();
        List<UnitBase> filteredUnits = ApplyFilter(allUnits);

        if (hideDeployedUnits)
        {
            filteredUnits = filteredUnits
                .Where(unit => !deploymentController.IsUnitDeployed(unit, PreBattleSide.Friendly))
                .ToList();
        }

        RebuildCards(filteredUnits);
        RefreshCount(allUnits, filteredUnits);
    }

    private void RefreshTitle()
    {
        if (titleText != null)
        {
            titleText.text = "我方单位预选";
        }
    }

    private void RefreshCount(List<UnitBase> allUnits, List<UnitBase> visibleUnits)
    {
        if (countText == null || deploymentController == null)
            return;

        int deployedCount = deploymentController.FriendlyDeployedCount;
        int totalCount = allUnits != null ? allUnits.Count : 0;

        countText.text = $"{deployedCount}/{totalCount}";
    }

    private void RebuildCards(List<UnitBase> units)
    {
        ClearCards();

        if (contentRoot == null)
        {
            Debug.LogWarning("DeployableUnitListPanel missing contentRoot.");
            return;
        }

        if (unitCardPrefab == null)
        {
            Debug.LogWarning("DeployableUnitListPanel missing unitCardPrefab.");
            return;
        }

        foreach (UnitBase unit in units)
        {
            if (unit == null)
                continue;

            PreBattleUnitCard card = Instantiate(unitCardPrefab, contentRoot);

            bool isDeployed = deploymentController.IsUnitDeployed(
                unit,
                PreBattleSide.Friendly
            );

            card.SetData(
                unit,
                isDeployed,
                allowDragging && !isDeployed
            );

            card.SetClickCallback(OnUnitCardClicked);

            spawnedCards.Add(card);
        }

        RefreshCardSelection();
    }

    private void ClearCards()
    {
        foreach (PreBattleUnitCard card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        spawnedCards.Clear();
    }

    private List<UnitBase> ApplyFilter(List<UnitBase> units)
    {
        if (units == null)
            return new List<UnitBase>();

        if (currentFilter == DeployableUnitFilter.All)
            return units;

        return units
            .Where(unit => MatchFilter(unit, currentFilter))
            .ToList();
    }

    private bool MatchFilter(UnitBase unit, DeployableUnitFilter filter)
    {
        if (unit == null)
            return false;

        // 当前 UnitBase 暂时没有 unitType / unitCategory 字段，
        // 所以这里先通过名字做临时判断，方便你用假数据测试。
        //
        // 后续建议改成：
        // UnitCategory category = unit.UnitDefinition.category;
        // UnitRole role = unit.UnitDefinition.role;

        string name = unit.DisplayName != null ? unit.DisplayName : "";

        switch (filter)
        {
            case DeployableUnitFilter.Ground:
                return !name.Contains("战机") &&
                       !name.Contains("直升机") &&
                       !name.Contains("航空");

            case DeployableUnitFilter.Armor:
                return name.Contains("坦克") ||
                       name.Contains("步战车") ||
                       name.Contains("装甲");

            case DeployableUnitFilter.Artillery:
                return name.Contains("火箭炮") ||
                       name.Contains("火炮") ||
                       name.Contains("榴弹炮") ||
                       name.Contains("远火");

            case DeployableUnitFilter.Air:
                return name.Contains("战机") ||
                       name.Contains("直升机") ||
                       name.Contains("航空");

            default:
                return true;
        }
    }

    public void SetFilter(DeployableUnitFilter filter)
    {
        currentFilter = filter;
        Refresh();
    }

    private void OnUnitCardClicked(PreBattleUnitCard card)
    {
        if (card == null || card.Unit == null)
            return;

        selectedUnit = card.Unit;

        Debug.Log($"Selected deployable unit: {selectedUnit.DisplayName}");

        RefreshCardSelection();

        if (friendlySidePanel != null)
        {
            friendlySidePanel.HighlightValidSlots(selectedUnit);
        }

        // TODO:
        // 后续可以在这里刷新左下角单位详情面板：
        // unitDetailPanel.SetData(selectedUnit);
    }

    private void RefreshCardSelection()
    {
        foreach (PreBattleUnitCard card in spawnedCards)
        {
            if (card == null)
                continue;

            bool selected = card.Unit != null && card.Unit == selectedUnit;
            card.SetSelected(selected);
        }
    }

    /// <summary>
    /// 供 PreBattleSlot 点击空槽时调用。
    /// 如果当前左侧列表已有选中单位，则尝试部署到该槽位。
    /// </summary>
    public bool TryDeploySelectedUnitToSlot(PreBattleSlotKey slotKey)
    {
        if (selectedUnit == null)
            return false;

        if (deploymentController == null)
            return false;

        bool success = deploymentController.TryDeployUnit(
            selectedUnit,
            slotKey,
            out string failReason
        );

        if (!success)
        {
            Debug.LogWarning($"Deploy selected unit failed: {failReason}");
            return false;
        }

        selectedUnit = null;

        if (friendlySidePanel != null)
        {
            friendlySidePanel.ClearHighlights();
        }

        Refresh();

        return true;
    }

    public void ClearSelection()
    {
        selectedUnit = null;

        if (friendlySidePanel != null)
        {
            friendlySidePanel.ClearHighlights();
        }

        RefreshCardSelection();
    }
}

public enum DeployableUnitFilter
{
    All,
    Ground,
    Armor,
    Artillery,
    Air
}