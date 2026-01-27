public interface IHealth
{
    int curr_health {get;}
    int max_health {get;}
    void ChangeHealth(int change_amt);
}