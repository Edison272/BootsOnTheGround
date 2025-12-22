using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class FuncModule
{    

}


/*
Gun Platforms
- Rely on ammo to shoot (usually)
- Have Recoil that throws off aim
*/
public class Melee : FuncModule
/*
Melee Platforms
- use combo patterns to do different attacks
*/
{
    
}
public class Shield : FuncModule
/*
Shield Platforms
- block attacks to trigger attacks
- blocking/damage reduction is based on direction
*/
{
    
}
public class Conduit : FuncModule
/*
Conduit Platforms
- Switch between opposite states
- affect a target area varying in diameter
*/
{
    
}