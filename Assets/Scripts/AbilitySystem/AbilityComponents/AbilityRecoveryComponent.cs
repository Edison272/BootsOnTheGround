using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class AbilityRecoveryComponent
{
    private int max_ability_points;
    private float curr_ability_points;
    private int max_charges;
    private int curr_charges;
    private AbilityRecoveryType ability_recovery_type;
    public AbilityRecoveryComponent(AbilityRecoveryType ability_recovery_type, Operator user, float max_ability_points)
    {
        this.ability_recovery_type = ability_recovery_type;
        Debug.Log(ability_recovery_type);
        switch (ability_recovery_type)
        {
            case AbilityRecoveryType.Time:
                // 1 sec = 1 ability points
                user.OnActiveUpdate += TimerRecovery;
                break;
            case AbilityRecoveryType.Kills:
                user.OnEnemyKilled += KillRecovery;
                break;
            case AbilityRecoveryType.DamageTaken:
                user.OnDamageTaken += DamageTakenRecovery;
                break;
        }
        this.max_ability_points = (int)max_ability_points;
    }
    public void ResetRecovery()
    {
        curr_ability_points = 0;
    }
    public virtual float AbilityRecoveryCompletion()
    {
        return (float)curr_ability_points/max_ability_points;
    }

    #region recovery fx types
    void TimerRecovery(float deltaTime)
    {
        curr_ability_points += deltaTime;
    }
    void KillRecovery(float target_value)
    {
        curr_ability_points += 1 * target_value;
    }
    void DamageTakenRecovery(float damage_taken)
    {
        //Debug.Log(string.Format("OW I TOOK {0} DAMAGE", damage_taken));
        curr_ability_points += (int)damage_taken;
    }
    #endregion
    #region SetRecovery

    #endregion
}