namespace HermitCrab.Character
{
    public interface IDamageable 
    {
        void ReceiveDamage(DamageType damageType, int damage);
    }
}