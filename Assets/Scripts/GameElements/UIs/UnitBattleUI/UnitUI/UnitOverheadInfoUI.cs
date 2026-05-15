using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitOverheadInfoUI : MonoBehaviour
{
    [Header("Unit")]
    [SerializeField] private UnitBase unit;

    [Header("Camp")]
    [SerializeField] private CampVisualConfig campVisualConfig;
    [SerializeField] private bool showAPForPlayerFriendlyOnly = true;

    [Header("Icon")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image campFrameImage;
    [SerializeField] private Image campBackgroundImage;

    [Header("HP Bar")]
    [SerializeField] private GameObject hpBarRoot;
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image hpPreviewFillImage;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("AP Bar")]
    [SerializeField] private GameObject apBarRoot;
    [SerializeField] private Image apFillImage;
    [SerializeField] private Image apPreviewFillImage;
    [SerializeField] private TextMeshProUGUI apText;

    [Header("Options")]
    [SerializeField] private bool refreshEveryFrame = true;
    [SerializeField] private bool showText = false;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup;

    private int previewHP = -1;
    private int previewAP = -1;

    private void Awake()
    {
        if (unit == null)
            unit = GetComponentInParent<UnitBase>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Debug.Log($"[UnitOverheadInfoUI] UnitBase found = {unit != null}, GameObject = {(unit != null ? unit.gameObject.name : "NULL")}", this);
    }

    private void Start()
    {
        RefreshStaticInfo();
        RefreshRuntimeInfo();
    }

    private void Update()
    {
        if (refreshEveryFrame)
            RefreshRuntimeInfo();
    }

    public void Bind(UnitBase targetUnit)
    {
        unit = targetUnit;

        RefreshStaticInfo();
        RefreshRuntimeInfo();
    }

    private bool IsFriendlyToPlayer(UnitBase targetUnit)
    {
        if (targetUnit == null)
            return false;

        if (CampManager.Instance != null)
            return CampManager.Instance.IsFriendlyUnit(targetUnit);

        return false;
    }

    public void SetHPPreview(int previewValue)
    {
        previewHP = previewValue;
        RefreshRuntimeInfo();
    }

    public void ClearHPPreview()
    {
        previewHP = -1;
        RefreshRuntimeInfo();
    }

    public void SetAPPreview(int previewValue)
    {
        previewAP = previewValue;
        RefreshRuntimeInfo();
    }

    public void ClearAPPreview()
    {
        previewAP = -1;
        RefreshRuntimeInfo();
    }

    private void RefreshStaticInfo()
    {
        if (unit == null)
            return;

        RefreshIcon();
        RefreshCampColor();
    }

    private void RefreshIcon()
    {
        if (iconImage == null)
            return;

        Sprite icon = null;

        if (unit.Definition != null)
        {
            icon = unit.Definition.unitIcon;
        }

        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
    }

    private void RefreshCampColor()
    {
        if (unit == null)
            return;

        Color campColor = campVisualConfig != null
            ? campVisualConfig.GetCampColor(unit.CampId)
            : Color.white;

        if (campFrameImage != null)
            campFrameImage.color = campColor;

        if (campBackgroundImage != null)
            campBackgroundImage.color = new Color(
                campColor.r,
                campColor.g,
                campColor.b,
                0.35f
            );
    }

    private void RefreshRuntimeInfo()
    {
        if (unit == null)
        {
            SetVisible(false);
            return;
        }

        if (!unit.IsRuntimeDataReady)
        {
            SetVisible(false);
            return;
        }

        if (unit.IsDestroyed || unit.CurrentHP <= 0)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        RefreshStaticInfo();

        bool isFriendly = IsFriendlyToPlayer(unit);

        RefreshHPBar();

        bool showAP = !showAPForPlayerFriendlyOnly || isFriendly;
        RefreshAPBar(showAP);
    }

    private void RefreshHPBar()
    {
        if (hpBarRoot != null)
            hpBarRoot.SetActive(true);

        int currentHP = Mathf.Max(0, unit.CurrentHP);
        int maxHP = Mathf.Max(1, unit.MaxHP);

        float hpPercent = Mathf.Clamp01((float)currentHP / maxHP);

        if (hpFillImage != null)
            hpFillImage.fillAmount = hpPercent;

        if (hpPreviewFillImage != null)
        {
            if (previewHP >= 0)
            {
                int clampedPreviewHP = Mathf.Clamp(previewHP, 0, maxHP);
                hpPreviewFillImage.gameObject.SetActive(true);
                hpPreviewFillImage.fillAmount = Mathf.Clamp01((float)clampedPreviewHP / maxHP);
            }
            else
            {
                hpPreviewFillImage.gameObject.SetActive(false);
            }
        }

        if (hpText != null)
        {
            hpText.gameObject.SetActive(showText);
            hpText.text = $"{currentHP}/{maxHP}";
        }
    }

    private void RefreshAPBar(bool isFriendly)
    {
        if (apBarRoot != null)
            apBarRoot.SetActive(isFriendly);

        if (!isFriendly)
            return;

        int currentAP = Mathf.Max(0, unit.CurrentAP);
        int maxAP = Mathf.Max(1, unit.MaxAP);

        float apPercent = Mathf.Clamp01((float)currentAP / maxAP);

        if (apFillImage != null)
            apFillImage.fillAmount = apPercent;

        if (apPreviewFillImage != null)
        {
            if (previewAP >= 0)
            {
                int clampedPreviewAP = Mathf.Clamp(previewAP, 0, maxAP);
                apPreviewFillImage.gameObject.SetActive(true);
                apPreviewFillImage.fillAmount = Mathf.Clamp01((float)clampedPreviewAP / maxAP);
            }
            else
            {
                apPreviewFillImage.gameObject.SetActive(false);
            }
        }

        if (apText != null)
        {
            apText.gameObject.SetActive(showText);
            apText.text = $"{currentAP}/{maxAP}";
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}