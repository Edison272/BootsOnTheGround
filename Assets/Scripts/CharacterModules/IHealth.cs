public interface IHealth
{
    int curr_health {get;}
    int max_health {get;}
    void TakeDamage(int damage_amt);
}