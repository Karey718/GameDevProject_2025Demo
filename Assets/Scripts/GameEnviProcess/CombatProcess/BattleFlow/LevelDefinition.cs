using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Level Definition")]
public class LevelDefinition : ScriptableObject
{
    [System.Serializable]
    public class UnitSpawnData
    {
        public string unitId;
        public int campId;
        public Vector2 coord;
    }

    [Header("Basic")]
    public string levelId;
    public string levelName;

    [TextArea(3, 6)]
    public string description;

    [Header("Units")]
    public List<UnitSpawnData> playerUnits = new();
    public List<UnitSpawnData> enemyUnits = new();
}