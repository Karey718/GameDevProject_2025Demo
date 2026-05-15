using UnityEngine;

public class UnitDisplayData
{
    public string displayName;
    public string categoryText;
    public string roleText;

    public string hpText;
    public string apText;
    public string powerText;

    public Sprite icon;
    public Sprite cardImage;
    public Sprite typeIcon;
    public Sprite campIcon;

    public int starLevel;
    public int combatPower;

    public bool isDestroyed;
    public bool isLocked;
    public bool isAvailableForBattle;

    public UnitBase sceneUnit;
    public UnitInstanceData instanceData;
    public UnitDefinition definition;
}