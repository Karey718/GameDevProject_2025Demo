using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;


public enum UnitControlMode
{
    None,
    QuickMove,
    AttackPrepare,
    CardPlanning
}

public class HexGridInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HexGridMapManager hexGridMapManager;

    [Tooltip("选中单位信息面板")]
    [SerializeField] private GameObject unitInfoDisplayer;

    [Header("Control Mode")]
    [SerializeField] private UnitControlMode currentControlMode = UnitControlMode.None;

    [Header("Quick Move")]
    [SerializeField] private float doubleClickThreshold = 0.28f;

    private HexGridTile_Base lastClickedTile;
    private float lastClickTime;

    [Header("Input Lock")]
    [SerializeField] private bool playerInputEnabled = true;

    public bool PlayerInputEnabled => playerInputEnabled;

    [Header("Camp")]
    [SerializeField] private int playerCampId = 1;


    [Header("Options")]
    [SerializeField] private bool allowAutoEnterAttackModeWhenClickEnemy = true;
    [SerializeField] private bool enableTestSpawnHotkeys = true;


    private UnitBase currentSelectedUnit;

    public UnitBase CurrentSelectedUnit => currentSelectedUnit;

    private void Awake()
    {
        if (hexGridMapManager == null)
        {
            hexGridMapManager = HexGridMapManager.Instance;
        }

        if (hexGridMapManager == null)
        {
            hexGridMapManager = GetComponent<HexGridMapManager>();
        }

        if (unitInfoDisplayer != null)
        {
            unitInfoDisplayer.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInputEnabled)
            return;

        HandleKeyboardInput();
        HandleMouseInput();

        if (enableTestSpawnHotkeys)
        {
            HandleTestSpawn();
        }
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleEKey();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleQKey();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    private void HandleMouseInput()
    {
        if (IsPointerOverUI())
            return;

        if (currentControlMode == UnitControlMode.CardPlanning)
        {
            // 卡牌模式下，地图点击交给 ActionTargetingController。
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    private void HandleLeftClick()
    {
        if (hexGridMapManager == null)
            return;

        HexGridTile_Base clickedTile = hexGridMapManager.GetHexTileFromMouseClick();

        if (clickedTile == null)
            return;

        UnitBase clickedUnit = clickedTile.currUnit;

        if (currentSelectedUnit == null)
        {
            if (IsFriendlyUnit(clickedUnit))
            {
                SelectUnit(clickedUnit);
            }
            else if (IsEnemyUnit(clickedUnit))
            {
                Debug.Log("不能选择敌方单位。");
            }

            return;
        }

        if (clickedTile.currUnit != null)
        {
            if (IsFriendlyToSelectedUnit(clickedUnit))
            {
                HandleLeftClickFriendlyUnit(clickedUnit);
            }
            else if (IsEnemyToSelectedUnit(clickedUnit))
            {
                HandleLeftClickEnemyUnit(clickedUnit, clickedTile);
            }
            else
            {
                Debug.Log("中立单位，暂不处理。");
            }
        }
        else
        {
            HandleLeftClickEmptyTile(clickedTile);
        }
    }

    private void HandleLeftClickEmptyTile(HexGridTile_Base clickedTile)
    {
        if (currentSelectedUnit == null)
        {
            Debug.Log("Clicked tile position: " + clickedTile.GetCoordinates());
            return;
        }

        if (currentControlMode == UnitControlMode.QuickMove && currentSelectedUnit.OperationState == UnitOperationState.Selected)
        {
            HandleQuickMoveTileClick(clickedTile);
            return;
        }

        if (currentControlMode == UnitControlMode.AttackPrepare ||
            currentSelectedUnit.OperationState == UnitOperationState.AttackSelecting)
        {
            Debug.Log("当前处于攻击选择模式，点击空地不会移动。");
            return;
        }
    }

    private void HandleQuickMoveTileClick(HexGridTile_Base clickedTile)
    {
        if (clickedTile == null || currentSelectedUnit == null)
            return;

        bool isDoubleClick =
            lastClickedTile == clickedTile &&
            Time.time - lastClickTime <= doubleClickThreshold;

        lastClickedTile = clickedTile;
        lastClickTime = Time.time;

        if (!isDoubleClick)
            return;

        currentSelectedUnit.TryMoveTo(clickedTile);
    }

    private void HandleLeftClickFriendlyUnit(UnitBase clickedUnit)
    {
        if (clickedUnit == null)
            return;

        if (currentSelectedUnit == null)
        {
            SelectUnit(clickedUnit);
            return;
        }

        if (currentControlMode == UnitControlMode.QuickMove)
        {
            SelectUnit(clickedUnit);
            return;
        }

        if (currentControlMode == UnitControlMode.AttackPrepare)
        {
            if (IsPreBattleUIOpen())
            {
                HighlightUnitInPreBattleUI(clickedUnit);
                return;
            }

            SelectUnit(clickedUnit);
            return;
        }
    }

    private void HandleLeftClickEnemyUnit(UnitBase clickedUnit, HexGridTile_Base clickedTile)
    {
        if (clickedUnit == null)
            return;

        if (currentSelectedUnit == null)
        {
            Debug.Log("未选择我方单位，不能选择敌方单位。");
            return;
        }

        if (currentControlMode == UnitControlMode.AttackPrepare ||
            currentSelectedUnit.OperationState == UnitOperationState.AttackSelecting)
        {
            if (currentSelectedUnit.IsTileInAttackRange(clickedTile))
            {
                OpenPreBattleUI(currentSelectedUnit, clickedUnit);
            }
            else
            {
                Debug.Log("敌方单位不在攻击范围内。");
            }

            return;
        }

        if (currentControlMode == UnitControlMode.QuickMove)
        {
            if (allowAutoEnterAttackModeWhenClickEnemy)
            {
                EnterAttackPrepareMode();

                if (currentSelectedUnit.IsTileInAttackRange(clickedTile))
                {
                    OpenPreBattleUI(currentSelectedUnit, clickedUnit);
                }
                else
                {
                    Debug.Log("已进入攻击模式，但目标不在攻击范围内。");
                }
            }
            else
            {
                Debug.Log("请按 Q 进入攻击模式后再选择敌方单位。");
            }
        }
    }

    private void HandleQKey()
    {
        if (currentSelectedUnit == null)
            return;

        if (IsPreBattleUIOpen())
        {
            Debug.Log("预战斗 UI 已打开，Q 键暂不切换攻击模式。");
            return;
        }

        switch (currentControlMode)
        {
            case UnitControlMode.CardPlanning:
                ExitCardPlanningModeOnly();
                EnterAttackPrepareMode();
                return;

            case UnitControlMode.QuickMove:
                EnterAttackPrepareMode();
                return;

            case UnitControlMode.AttackPrepare:
                EnterQuickMoveMode();
                return;

            default:
                EnterQuickMoveMode();
                return;
        }
    }

    private void HandleEKey()
    {
        if (currentSelectedUnit == null)
            return;

        if (IsPreBattleUIOpen())
            return;

        if (currentControlMode == UnitControlMode.CardPlanning)
        {
            ExitCardPlanningModeToQuickMove();
            return;
        }

        EnterCardPlanningMode();
    }

    private void HandleEscapeKey()
    {
        if (currentSelectedUnit == null)
            return;

        if (currentControlMode == UnitControlMode.CardPlanning)
        {
            ExitCardPlanningModeToQuickMove();
            return;
        }
    }

    private void HandleRightClick()
    {
        if (currentSelectedUnit == null)
            return;

        if (currentControlMode == UnitControlMode.CardPlanning)
        {
            // 卡牌模式下右键交给 ActionTargetingController
            return;
        }

        if (currentControlMode == UnitControlMode.AttackPrepare)
        {
            EnterQuickMoveMode();
            return;
        }

        if (currentControlMode == UnitControlMode.QuickMove)
        {
            DeselectCurrentUnit();
            return;
        }
    }

    public void SyncAfterPreBattleUIClose()
    {
        if (currentSelectedUnit == null)
        {
            currentControlMode = UnitControlMode.None;
            return;
        }

        // 从预战斗 UI 退出后，仍然保持攻击准备模式
        // 让右键 / Q / E 都继续按 AttackPrepare 逻辑工作
        currentControlMode = UnitControlMode.AttackPrepare;

        currentSelectedUnit.ClearMoveRange();
        currentSelectedUnit.CalculateShowAttackRange();
        currentSelectedUnit.EnterAttackSelecting();
    }

    private void EnterQuickMoveMode()
    {
        if (currentSelectedUnit == null)
            return;

        currentControlMode = UnitControlMode.QuickMove;

        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.ExitCardMode();

        currentSelectedUnit.EnterQuickMoveMode();
    }

    private void EnterAttackPrepareMode()
    {
        if (currentSelectedUnit == null)
            return;

        currentControlMode = UnitControlMode.AttackPrepare;

        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.ExitCardMode();

        currentSelectedUnit.ClearMoveRange();
        currentSelectedUnit.EnterAttackSelecting();
        currentSelectedUnit.SetOperationStateForAttackPrepare();
    }

    private void EnterCardPlanningMode()
    {
        if (currentSelectedUnit == null)
            return;

        currentControlMode = UnitControlMode.CardPlanning;

        currentSelectedUnit.EnterCardPlanningMode();

        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.EnterCardMode(currentSelectedUnit);
    }

    private void ExitCardPlanningModeOnly()
    {
        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.ExitCardMode();

        if (currentSelectedUnit != null)
            currentSelectedUnit.EnterCardPlanningMode();
    }

    private void ExitCardPlanningModeToQuickMove()
    {
        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.ExitCardMode();

        EnterQuickMoveMode();
    }

    public void ReturnToQuickMoveAfterCardExecution()
    {
        if (currentSelectedUnit == null)
        {
            currentControlMode = UnitControlMode.None;
            return;
        }

        EnterQuickMoveMode();
    }

    public void SelectUnit(UnitBase unit)
    {
        if (!CanPlayerSelectUnit(unit))
            return;

        if (unit == null)
            return;

        if (currentSelectedUnit != null && currentSelectedUnit != unit)
        {
            currentSelectedUnit.Deselect();
        }

        currentSelectedUnit = unit;

        if (BattleCardSystem.Instance != null)
        {
            BattleCardSystem.Instance.SetCurrentUnit(unit);
            BattleCardSystem.Instance.ExitCardMode();
        }

        EnterQuickMoveMode();

        RefreshSelectedUnitInfo();

        Debug.Log("Selected unit: " + currentSelectedUnit.DisplayName);

    }

    public void DeselectCurrentUnit()
    {
        if (currentSelectedUnit != null)
        {
            currentSelectedUnit.Deselect();
        }

        currentSelectedUnit = null;
        currentControlMode = UnitControlMode.None;

        lastClickedTile = null;
        lastClickTime = 0f;

        RefreshSelectedUnitInfo();

        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.ClearCurrentUnit();
    }

    private void RefreshSelectedUnitInfo()
    {
        if (unitInfoDisplayer == null)
            return;

        SelectedUnitInfo selectedUnitInfo = unitInfoDisplayer.GetComponent<SelectedUnitInfo>();

        if (selectedUnitInfo == null)
            return;

        if (currentSelectedUnit == null)
        {
            selectedUnitInfo.ClearSelectedUnit();
            unitInfoDisplayer.SetActive(false);
        }
        else
        {
            unitInfoDisplayer.SetActive(true);
            selectedUnitInfo.SetSelectedUnit(currentSelectedUnit);
        }
    }

    private bool IsFriendlyUnit(UnitBase unit)
    {
        if (unit == null)
            return false;

        return CampManager.Instance.IsFriendlyToPlayer(unit.CampId);
    }

    private bool IsEnemyUnit(UnitBase unit)
    {
        if (unit == null)
            return false;

        return CampManager.Instance.IsEnemyToPlayer(unit.CampId);
    }

    private bool IsFriendlyToSelectedUnit(UnitBase unit)
    {
        if (currentSelectedUnit == null || unit == null)
            return false;

        if (CampManager.Instance != null)
            return CampManager.Instance.IsCampFriendly(currentSelectedUnit.CampId, unit.CampId);

        return currentSelectedUnit.CampId == unit.CampId;
    }

    private bool IsEnemyToSelectedUnit(UnitBase unit)
    {
        if (currentSelectedUnit == null || unit == null)
            return false;

        if (CampManager.Instance != null)
            return CampManager.Instance.IsCampEnemy(currentSelectedUnit.CampId, unit.CampId);

        return currentSelectedUnit.CampId != unit.CampId;
    }

    private void OpenPreBattleUI(UnitBase attacker, UnitBase defender)
    {
        if (PreBattleUIManager.Instance == null)
        {
            Debug.LogWarning("PreBattleUIManager.Instance 不存在，无法打开预战斗 UI。");
            return;
        }

        if (attacker == null || defender == null)
            return;

        attacker.RequestPreBattleAttack(defender);
    }

    private bool IsPreBattleUIOpen()
    {
        return PreBattleUIManager.Instance != null &&
               PreBattleUIManager.Instance.IsOpen;
    }

    private void HighlightUnitInPreBattleUI(UnitBase unit)
    {
        if (PreBattleUIManager.Instance == null)
            return;

        PreBattleUIManager.Instance.HighlightUnitCard(unit);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void HandleTestSpawn()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            UnitsManager.Instance.SpawnUnitById("soldier_basic", 1, new Vector2(5, 5));
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            UnitsManager.Instance.SpawnUnitById("soldier_basic", 2, new Vector2(5, 7));
        }
    }

    public void SetPlayerInputEnabled(bool enabled)
    {
        playerInputEnabled = enabled;

        if (!playerInputEnabled)
        {
            ForceClearPlayerControlState();
        }
    }

    private void ForceClearPlayerControlState()
    {
        if (currentSelectedUnit != null)
        {
            currentSelectedUnit.ClearAllRangeHighlights();
            currentSelectedUnit.Deselect();
        }

        currentSelectedUnit = null;
        currentControlMode = UnitControlMode.None;

        if (BattleCardSystem.Instance != null)
            BattleCardSystem.Instance.ClearCurrentUnit();

        RefreshSelectedUnitInfo();
    }

    public void SetPlayerCampId(int campId)
    {
        playerCampId = campId;
    }

    private bool CanPlayerSelectUnit(UnitBase unit)
    {
        if (!playerInputEnabled)
            return false;

        if (unit == null)
            return false;

        if (unit.IsDestroyed)
            return false;

        if (CampManager.Instance != null)
            return CampManager.Instance.IsFriendlyUnit(unit);

        return unit.CampId == playerCampId;
    }

}