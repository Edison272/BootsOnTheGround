using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ItemActionEnum {Projectile, Linecast, MeleeAttack}
public abstract class ItemAction
{
    public Character user;
    public GameObject instance;

    public abstract void Action();
}
public class Projectile : ItemAction
{
    AttackData atk_data;

    public GameObject projectile;
    public float projectile_speed;
    public override void Action()
    {
        throw new System.NotImplementedException();
    }

}
public class Linecast : ItemAction
{
    public override void Action()
    {
        throw new System.NotImplementedException();
    }
}



public class MeleeAttack : ItemAction
{
    public override void Action()
    {
        throw new System.NotImplementedException();
    }
}
public class StatChanger : ItemAction
{
    public override void Action()
    {
        throw new System.NotImplementedException();
    }
}
