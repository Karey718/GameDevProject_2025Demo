using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/UI/Camp Visual Config")]
public class CampVisualConfig : ScriptableObject
{
    [System.Serializable]
    public class CampColorData
    {
        public int campId;
        public Color color = Color.white;
    }

    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private List<CampColorData> campColors = new();

    public Color GetCampColor(int campId)
    {
        foreach (CampColorData data in campColors)
        {
            if (data.campId == campId)
                return data.color;
        }

        return defaultColor;
    }
}