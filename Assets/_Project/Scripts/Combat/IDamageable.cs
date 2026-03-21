namespace Mistborn.Combat
{
    public interface IDamageable
    {
        void TakeDamage(DamageData damage);
        float health { get; }
        bool isDead { get; }
    }

    public struct DamageData
    {
        public float amount;
        public DamageType type;
        public object source;
        public bool isCritical;

        public DamageData(float amount, DamageType type = DamageType.Standard, object source = null)
        {
            this.amount = amount;
            this.type = type;
            this.source = source;
            this.isCritical = false;
        }
    }

    public enum DamageType
    {
        Standard,
        PewterEnhanced,
        Allomantic,
        MetalPush,
        MetalPull
    }
}
