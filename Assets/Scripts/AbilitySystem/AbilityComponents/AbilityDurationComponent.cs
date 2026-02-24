using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class AbilityDurationComponent
{
    private int max_duration;
    private float curr_duration;
    public bool duration_complete => curr_duration >= max_duration;
    private AbilityRecoveryType ability_recovery_type;
    private Operator user;
    public AbilityDurationComponent(AbilityRecoveryType ability_recovery_type, Operator user, float max_duration)
    {
        this.ability_recovery_type = ability_recovery_type;
        this.user = user;
        this.max_duration = (int)max_duration;
    }

    public void SetDurationActive(bool is_active)
    {
        switch (ability_recovery_type)
        {
            case AbilityRecoveryType.Time:
                // 1 sec = 1 ability points
                if (is_active) {user.OnActiveUpdate += TimerRecovery;}
                else {user.OnActiveUpdate -= TimerRecovery;}
                break;
            case AbilityRecoveryType.Kills:
                if (is_active) {user.OnEnemyKilled += KillRecovery;}
                else {user.OnEnemyKilled -= KillRecovery;}
                break;
            case AbilityRecoveryType.DamageTaken:
                if (is_active) {user.OnDamageTaken += DamageTakenRecovery;}
                else {user.OnDamageTaken -= DamageTakenRecovery;}
                break;
        }
    }
    public void ResetDuration()
    {
        curr_duration = 0;
    }
    public virtual float AbilityDurationCompletion()
    {
        return (float)curr_duration/max_duration;
    }

    #region recovery fx types
    void TimerRecovery(float deltaTime)
    {
        curr_duration += deltaTime;
    }
    void KillRecovery(float target_value)
    {
        curr_duration += 1 * target_value;
    }
    void DamageTakenRecovery(float damage_taken)
    {
        //Debug.Log(string.Format("OW I TOOK {0} DAMAGE", damage_taken));
        curr_duration += (int)damage_taken;
    }
    #endregion
    #region SetRecovery

    #endregion
}