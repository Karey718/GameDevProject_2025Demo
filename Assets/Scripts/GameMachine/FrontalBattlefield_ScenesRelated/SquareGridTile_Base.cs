using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum SquareGridTileType
{
    Plain,
    Gravel,
    Mound,
    Dune,
    Snow,
    Sand,
    Mountain,
    Water,
    Default, // 默认
    OutBounds // 界外
}

public class SquareGridTile_Base : MonoBehaviour
{
    public SquareGridTileType tileType;
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

    #endregion

    #region 地形初始化
    public void InitTile(SquareGridTileType type)
    {
        tileType = type;

        switch (tileType)
        {
            case SquareGridTileType.Plain:
                isWalkable = true;
                moveCost = 1 + UnityEngine.Random.Range(0, 2);
                antiObservationIndex = 0;
                break;

            case SquareGridTileType.Gravel:
                isWalkable = true;
                moveCost = 3 + UnityEngine.Random.Range(0, 2);
                antiObservationIndex = 1;
                break;

            case SquareGridTileType.Mountain:
                isWalkable = true;
                moveCost = 8 + UnityEngine.Random.Range(0, 2);
                antiObservationIndex = 4;
                break;

            case SquareGridTileType.Water:
                isWalkable = false;
                moveCost = 999;
                antiObservationIndex = 0;
                break;

            case SquareGridTileType.OutBounds:
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