using System;
using System.Collections.Generic;
using UnityEngine;

public enum CampRelation
{
    Neutral,
    Friendly,
    Enemy
}

public class CampManager : MonoBehaviour
{
    public static CampManager Instance;

    [Header("Player")]
    [SerializeField] private int playerCampId = 1;

    [Header("Default Setup")]
    [SerializeField] private bool initializeDefaultCamps = true;

    private readonly Dictionary<int, Camp> camps = new();

    public int PlayerCampId => playerCampId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Initialize();
    }

    private void Initialize()
    {
        camps.Clear();

        if (!initializeDefaultCamps)
            return;

        CreateCamp(1, "玩家阵营");
        CreateCamp(2, "敌方阵营");

        SetCampRelation(1, 1, CampRelation.Friendly);
        SetCampRelation(2, 2, CampRelation.Friendly);

        SetCampRelation(1, 2, CampRelation.Enemy);
    }

    public Camp CreateCamp(string campName)
    {
        int nextId = GetNextAvailableCampId();
        return CreateCamp(nextId, campName);
    }

    public Camp CreateCamp(int campId, string campName)
    {
        if (camps.ContainsKey(campId))
        {
            Debug.LogWarning($"Camp {campId} already exists.");
            return camps[campId];
        }

        Camp camp = new Camp(campId, campName);
        camps.Add(campId, camp);

        return camp;
    }

    public bool HasCamp(int campId)
    {
        return camps.ContainsKey(campId);
    }

    public Camp GetCamp(int campId)
    {
        if (camps.TryGetValue(campId, out Camp camp))
            return camp;

        Debug.LogWarning($"Camp {campId} does not exist.");
        return null;
    }

    public string GetCampName(int campId)
    {
        Camp camp = GetCamp(campId);
        return camp != null ? camp.CampName : $"Unknown Camp {campId}";
    }

    public void SetCampRelation(int campId1, int campId2, CampRelation relation)
    {
        if (!EnsureCampExists(campId1) || !EnsureCampExists(campId2))
            return;

        Camp camp1 = camps[campId1];
        Camp camp2 = camps[campId2];

        camp1.RemoveRelation(campId2);
        camp2.RemoveRelation(campId1);

        switch (relation)
        {
            case CampRelation.Friendly:
                camp1.AddFriendly(campId2);
                camp2.AddFriendly(campId1);
                break;

            case CampRelation.Enemy:
                camp1.AddEnemy(campId2);
                camp2.AddEnemy(campId1);
                break;

            case CampRelation.Neutral:
                break;
        }
    }

    public CampRelation GetRelation(int campId1, int campId2)
    {
        if (campId1 == campId2)
            return CampRelation.Friendly;

        if (!camps.TryGetValue(campId1, out Camp camp1))
            return CampRelation.Neutral;

        if (!camps.ContainsKey(campId2))
            return CampRelation.Neutral;

        if (camp1.IsFriendlyWith(campId2))
            return CampRelation.Friendly;

        if (camp1.IsEnemyWith(campId2))
            return CampRelation.Enemy;

        return CampRelation.Neutral;
    }

    public bool IsCampFriendly(int campId1, int campId2)
    {
        return GetRelation(campId1, campId2) == CampRelation.Friendly;
    }

    public bool IsCampEnemy(int campId1, int campId2)
    {
        return GetRelation(campId1, campId2) == CampRelation.Enemy;
    }

    public bool IsPlayerCamp(int campId)
    {
        return campId == playerCampId;
    }

    public bool IsFriendlyToPlayer(int campId)
    {
        return IsCampFriendly(playerCampId, campId);
    }

    public bool IsEnemyToPlayer(int campId)
    {
        return IsCampEnemy(playerCampId, campId);
    }

    public bool IsFriendlyUnit(UnitBase unit)
    {
        if (unit == null)
            return false;

        return IsFriendlyToPlayer(unit.CampId);
    }

    public bool IsEnemyUnit(UnitBase unit)
    {
        if (unit == null)
            return false;

        return IsEnemyToPlayer(unit.CampId);
    }

    private bool EnsureCampExists(int campId)
    {
        if (camps.ContainsKey(campId))
            return true;

        Debug.LogWarning($"Camp {campId} does not exist.");
        return false;
    }

    private int GetNextAvailableCampId()
    {
        int id = 1;

        while (camps.ContainsKey(id))
        {
            id++;
        }

        return id;
    }

    [ContextMenu("Debug Print Camps")]
    private void DebugPrintCamps()
    {
        foreach (Camp camp in camps.Values)
        {
            Debug.Log(camp.ToDebugString());
        }
    }

    [Serializable]
    public class Camp
    {
        [SerializeField] private int id;
        [SerializeField] private string campName;

        private readonly HashSet<int> friendlyCampIds = new();
        private readonly HashSet<int> enemyCampIds = new();

        public int Id => id;
        public string CampName => campName;

        public IReadOnlyCollection<int> FriendlyCampIds => friendlyCampIds;
        public IReadOnlyCollection<int> EnemyCampIds => enemyCampIds;

        public Camp(int id, string campName)
        {
            this.id = id;
            this.campName = campName;
        }

        public bool IsFriendlyWith(int otherCampId)
        {
            return friendlyCampIds.Contains(otherCampId);
        }

        public bool IsEnemyWith(int otherCampId)
        {
            return enemyCampIds.Contains(otherCampId);
        }

        public void AddFriendly(int otherCampId)
        {
            enemyCampIds.Remove(otherCampId);
            friendlyCampIds.Add(otherCampId);
        }

        public void AddEnemy(int otherCampId)
        {
            friendlyCampIds.Remove(otherCampId);
            enemyCampIds.Add(otherCampId);
        }

        public void RemoveRelation(int otherCampId)
        {
            friendlyCampIds.Remove(otherCampId);
            enemyCampIds.Remove(otherCampId);
        }

        public string ToDebugString()
        {
            string friendly = string.Join(", ", friendlyCampIds);
            string enemy = string.Join(", ", enemyCampIds);

            return $"Camp {id} / {campName} | Friendly: [{friendly}] | Enemy: [{enemy}]";
        }
    }
}