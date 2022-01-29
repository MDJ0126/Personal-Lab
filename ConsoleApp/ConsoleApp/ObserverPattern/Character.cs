public abstract class Character
{
    public float Hp { get; protected set; } = 100f;
    public float Defense { get; protected set; } = 0f;
    public float Power { get; protected set; } = 0f;
    
    public void Attack(Character target)
    {
        target.DamageProcess(this.Power);
    }

    public void DamageProcess(float power)
    {
        var damage = power - this.Defense;
        if (damage < 0f) damage = 0f;
        this.Hp -= damage;

        // 방송
        if (this.Hp > 0f)
        {
            NotificationCenter.Broadcast($"{this.GetType()}는 '{damage}' 데미지를 받았다. (현재 체력 : {this.Hp})");
        }
        else
        {
            NotificationCenter.Broadcast($"{this.GetType()}는 사망했다.");
        }
    }

    public bool IsDead()
    {
        return this.Hp <= 0f;
    }
}