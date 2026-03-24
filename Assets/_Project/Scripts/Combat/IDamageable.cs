/// <summary>
/// Interface for any object that can receive damage.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
    float GetMaxHealth();
    float GetCurrentHealth();
}
