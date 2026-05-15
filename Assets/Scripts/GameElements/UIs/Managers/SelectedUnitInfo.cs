using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 选中单位信息卡。
/// 
/// - UnitBase 只作为地图实体；
/// - 单位名称、图标、类型、HP/AP、战力等显示信息通过 UnitDisplayDataFactory 获取；
/// - 脚本只负责 UI 显示
/// </summary>
public class SelectedUnitInfo : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI unitCategoryText;
    [SerializeField] private TextMeshProUGUI unitRoleText;

    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI apText;

    [Header("Images")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private Image typeIconImage;
    [SerializeField] private Image campIconImage;

    [Header("Bars")]
    [Tooltip("HP 血量条前景")]
    [SerializeField] private Image hpFrontFill;

    [Tooltip("AP 行动力条前景")]
    [SerializeField] private Image apFrontFill;

    [Header("State")]
    [SerializeField] private GameObject destroyedRoot;
    [SerializeField] private GameObject lockedRoot;

    private UnitBase currentSelectedUnit;

    public UnitBase CurrentSelectedUnit => currentSelectedUnit;

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        if (currentSelectedUnit != null)
        {
            Refresh();
        }
    }

    /// <summary>
    /// 外部调用：设置当前选中单位。
    /// </summary>
    public void SetSelectedUnit(UnitBase unit)
    {
        currentSelectedUnit = unit;
        Refresh();
    }

    /// <summary>
    /// 外部调用：清空当前显示。
    /// </summary>
    public void ClearSelectedUnit()
    {
        currentSelectedUnit = null;
        ResetInfo();
    }

    /// <summary>
    /// 刷新当前单位信息。
    /// </summary>
    public void Refresh()
    {
        if (!isActiveAndEnabled)
            return;

        if (currentSelectedUnit == null)
        {
            ResetInfo();
            return;
        }

        SetInfo(currentSelectedUnit);
    }

    private void ResetInfo()
    {
        SetText(unitNameText, "");
        SetText(unitCategoryText, "");
        SetText(unitRoleText, "");

        SetText(hpText, "");
        SetText(apText, "");

        if (unitIconImage != null)
        {
            unitIconImage.sprite = null;
            unitIconImage.enabled = false;
        }

        if (typeIconImage != null)
        {
            typeIconImage.sprite = null;
            typeIconImage.enabled = false;
        }

        if (campIconImage != null)
        {
            campIconImage.sprite = null;
            campIconImage.enabled = false;
        }

        if (hpFrontFill != null)
            hpFrontFill.fillAmount = 0f;

        if (apFrontFill != null)
            apFrontFill.fillAmount = 0f;

        if (destroyedRoot != null)
            destroyedRoot.SetActive(false);

        if (lockedRoot != null)
            lockedRoot.SetActive(false);
    }

    private void SetInfo(UnitBase unit)
    {
        if (unit == null)
        {
            ResetInfo();
            return;
        }

        UnitDisplayData displayData = UnitDisplayDataFactory.FromUnitBase(unit);

        if (displayData == null)
        {
            ResetInfo();
            return;
        }

        SetText(unitNameText, displayData.displayName);
        SetText(unitCategoryText, displayData.categoryText);
        SetText(unitRoleText, displayData.roleText);

        SetText(hpText, displayData.hpText);
        SetText(apText, displayData.apText);

        SetImage(unitIconImage, displayData.icon);
        SetImage(typeIconImage, displayData.typeIcon);
        SetImage(campIconImage, displayData.campIcon);

        RefreshBars(unit);
        RefreshState(displayData);
    }

    private void RefreshBars(UnitBase unit)
    {
        if (unit == null)
            return;

        if (hpFrontFill != null)
        {
            float maxHP = Mathf.Max(1f, unit.MaxHP);
            float hpPercent = Mathf.Clamp01(unit.CurrentHP / maxHP);
            hpFrontFill.fillAmount = hpPercent;
        }

        if (apFrontFill != null)
        {
            float maxAP = Mathf.Max(1f, unit.MaxAP);
            float apPercent = Mathf.Clamp01(unit.CurrentAP / maxAP);
            apFrontFill.fillAmount = apPercent;
        }
    }

    private void RefreshState(UnitDisplayData displayData)
    {
        if (destroyedRoot != null)
            destroyedRoot.SetActive(displayData.isDestroyed);

        if (lockedRoot != null)
            lockedRoot.SetActive(displayData.isLocked);
    }

    private void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.enabled = sprite != null;
    }
}