using UnityEngine;

public enum EffectDurationType {Timer, Call}
public interface IEffectDuration
{
    bool effect_ended {get;}
    void Update();
    void StopEffect();
}

public struct EffectTimer : IEffectDuration
{
    float duration_time;
    public bool effect_ended {get; private set;}
    public EffectTimer(float duration_time)
    {
        this.duration_time = duration_time;
        effect_ended = false;
    }
    public void Update()
    {
        duration_time -= Time.deltaTime;
        if (duration_time <= 0)
        {
            StopEffect();
        }
    }
    public void StopEffect()
    {
        duration_time = 0;
        effect_ended = true;
    }
}

public struct EffectCall : IEffectDuration
{
    public bool effect_ended {get; private set;}
    public void Update()
    {
        throw new System.NotImplementedException();
    }
    public void StopEffect()
    {
        throw new System.NotImplementedException();
    }
}