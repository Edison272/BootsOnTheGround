public interface IHealth
{    HealthComponent health_component {get;}
    void ChangeHealth(int change_amt);
    void MaxHealthBoost(int boost_amt, float duration);
    void ChangeHealthTick(int change_amt, float duration, float tick_rate = 1);
    void ShieldBoost(int boost_amt);
}