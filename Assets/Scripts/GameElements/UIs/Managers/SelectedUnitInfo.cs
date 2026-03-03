using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUnitInfo : MonoBehaviour
{
public TextMeshProUGUI UnitName;
    public TextMeshProUGUI UnitAP;

    [Header("AP Bar (Foreground Image)")]
    public Image appFrontFill;  

    public UnitBase currSelectedUnit;

    void Update()
    {
        if (!this.isActiveAndEnabled) return;

        if (currSelectedUnit != null)
        {
            SetInfo(currSelectedUnit);
        }
        else
        {
            ResetInfo();
        }
    }

    public void ResetInfo()
    {
        UnitName.text = "";
        UnitAP.text = "";

        if (appFrontFill != null)
            appFrontFill.fillAmount = 0f;
    }

    public void SetInfo(UnitBase unit)
    {
        UnitName.text = unit.unitName;
        UnitAP.text = unit.currentAP.ToString();

        float maxAP = Mathf.Max(1f, unit.maxAP);
        float percent = Mathf.Clamp01(unit.currentAP / maxAP);

        if (appFrontFill != null)
            appFrontFill.fillAmount = percent;
    }
}
