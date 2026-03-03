using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class APDisplayController : MonoBehaviour
{
    [SerializeField] private UnitBase targetUnit;
    [SerializeField] private TextMeshProUGUI apText;

    private void Update()
    {
        if (targetUnit != null && apText != null)
        {
            // 跟随单位位置
            Vector3 worldPosition = targetUnit.transform.position + new Vector3(0, 2f, 0);
            apText.transform.position = Camera.main.WorldToScreenPoint(worldPosition);
        }
    }
}
