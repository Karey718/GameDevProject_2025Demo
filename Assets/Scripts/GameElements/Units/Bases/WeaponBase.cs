using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    //单兵枪械
    Rifle,
    //单兵反人员
    AntiPersonnel,
    //单兵反载具
    AntiVehicle,
    //单兵工程
    Engineer,
    //单兵支援
    Support,
    //单兵侦察
    Reconnaissance
}

public class WeaponBase 
{

    public WeaponBase(WeaponType weaponType, int damage, int range, int ammoCount)
    {
        this.weaponType = weaponType;
        this.damage = damage;
        this.range = range;
        this.ammoCount = ammoCount;
    }

    public WeaponType weaponType;
    public int damage;
    public int range;

    public int ammoCount; 
    
}
