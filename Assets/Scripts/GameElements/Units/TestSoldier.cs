using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSoldier : UnitBase
{

    void Start()
    {
        InitAttibutes(
            10, 10,
            ObservationType.Optical, 1, 4, 8, 3, 2,
            3, 5,
            3, 5);

        // 刷新战争迷雾
        WarFogManager.Instance.RefreshVision(GetAllFriendlyUnits());
    }



}
