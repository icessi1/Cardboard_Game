using System;

public interface IDamageable
{
    event Action OnDied;
    void Damage(int damage);
}
