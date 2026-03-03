using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum HexGridTileType
{
    Grass,
    Dune,
    Snow,
    Sand,
    Mountain,
    Water,
    OutBounds // 界外
}

public enum FogState
{
    Unexplored, // 未探索
    Explored,   // 已探索
    Visible     // 当前可见
}

public class HexGridTile_Base : MonoBehaviour
{


    public HexGridTileType tileType;
    public bool isWalkable;

    public UnitBase currUnit;

    // 移动花费
    public int moveCost { get; private set; }
    // 反观测指数
    [NonSerialized]
    public int antiObservationIndex;

    [Header("Highlights")]
    // 移动指示高亮层
    public GameObject moveRangeIndicator;
    public GameObject attackRangeIndicator;


    #region 坐标系统

    public int q;
    public int r;

    [Header("Coordinates")]
    public TextMeshPro CoordinatesText;
    public TextMeshPro MoveCostText;

    public void SetCoordinates(int x, int y)
    {
        q = x;
        r = y;
    }

    public Vector2Int GetCoordinates()
    {
        return new Vector2Int(q, r);
    }


    // 转换为立方体坐标
    // 将轴向坐标系（Axial Coordinates）(q, r)转换为立方体坐标系（Cube Coordinates）(x, y, z), 满足 x + y + z = 0
    public Vector3Int ToCube()
    {
        return new Vector3Int(q, r, -q - r);
    }

    // 计算六边形距离
    public static int Distance(HexGridTile_Base a, HexGridTile_Base b)
    {
        var ac = a.ToCube();
        var bc = b.ToCube();
        return (Mathf.Abs(ac.x - bc.x) + Mathf.Abs(ac.y - bc.y) + Mathf.Abs(ac.z - bc.z)) / 2;
    }
    #endregion

    #region 战争迷雾
    [Header("WarFog")]
    public FogState fogState = FogState.Unexplored;
    public GameObject fogOverlay;

    public void SetFogState(FogState state)
    {
        fogState = state;

        switch (state)
        {
            case FogState.Unexplored:
                fogOverlay.SetActive(true);
                fogOverlay.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                MoveCostText.gameObject.SetActive(false);
                break;

            case FogState.Explored:
                fogOverlay.SetActive(true);
                fogOverlay.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 0.3f);
                MoveCostText.gameObject.SetActive(true);
                break;

            case FogState.Visible:
                fogOverlay.SetActive(false);
                MoveCostText.gameObject.SetActive(true);
                break;
        }
    }
    #endregion

    #region 地形初始化
    public void InitTile(HexGridTileType type)
    {
        tileType = type;

        switch (tileType)
        {
            case HexGridTileType.Grass:
                isWalkable = true;
                moveCost = 1 + UnityEngine.Random.Range(0, 2);
                antiObservationIndex = 0;
                break;

            case HexGridTileType.Dune:
                isWalkable = true;
                moveCost = 3 + UnityEngine.Random.Range(0, 2);
                antiObservationIndex = 1;
                break;

            case HexGridTileType.Mountain:
                isWalkable = true;
                moveCost = 8 + UnityEngine.Random.Range(0, 2);
                antiObservationIndex = 4;
                break;

            case HexGridTileType.Water:
                isWalkable = false;
                moveCost = 999;
                antiObservationIndex = 0;
                break;

            case HexGridTileType.OutBounds:
                isWalkable = false;
                moveCost = 999;
                antiObservationIndex = 999;
                break;
        }

        if (MoveCostText != null)
            if (moveCost != 999)
            {
                MoveCostText.text = moveCost.ToString();
            }
            else
            {
                MoveCostText.text = "/";
            }

        MoveCostText.gameObject.SetActive(false);

    }
    #endregion

    #region 高亮
    public void SetMoveRangeHighlight(bool active)
    {
        moveRangeIndicator.SetActive(active);
    }

    public void SetAttackRangeHighlight(bool active)
    {
        attackRangeIndicator.SetActive(active);
    }
    #endregion



}
