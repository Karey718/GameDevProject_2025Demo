using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnitBase : MonoBehaviour
{
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public int speed;

    public UnitBase sourceUnit;

    public void InitFromMainUnit(UnitBase unit)
    {
        sourceUnit = unit;
        maxHP = unit.maxHP;
        currentHP = unit.currentHP;
        attack = unit.attackDamage;
        defense = unit.defense;
        speed = unit.speed;
    }

    public bool IsAlive()
    {
        return currentHP > 0;
    }

    public IEnumerator Attack(BattleUnitBase target)
    {
        yield return new WaitForSeconds(0.5f);

        int damage = Mathf.Max(0, attack - target.defense);
        target.currentHP -= damage;

        yield return new WaitForSeconds(0.5f);
    }
}
