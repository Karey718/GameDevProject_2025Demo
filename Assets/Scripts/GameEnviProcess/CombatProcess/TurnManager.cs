using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public Button endTurnButton;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        endTurnButton.onClick.AddListener(EndCurrentTurn);
    }

    public void EndCurrentTurn()
    {
        // 恢复所有单位的AP
        foreach (UnitBase unit in FindObjectsOfType<UnitBase>())
        {
            unit.ResetAP();
        }
        
        // 这里可以添加其他回合结束逻辑
        Debug.Log("Turn Ended. AP Reset!");
    }
}
