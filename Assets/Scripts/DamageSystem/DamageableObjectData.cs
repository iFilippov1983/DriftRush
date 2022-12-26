namespace RaceManager.DamageSystem
{
    public struct DamageableObjectData
    {
        public IDamageable DamageableObject;

        private float Damage;

        public DamageableObjectData(IDamageable damageableObject)
        {
            DamageableObject = damageableObject;
            Damage = 0;
        }

        public void TrySetMaxDamage(float damage)
        {
            if (Damage < damage)
            {
                Damage = damage;
            }
        }

        public void ApplyDamage()
        {
            if (Damage > 0)
            {
                DamageableObject.SetDamage(Damage);
            }
            Damage = 0;
        }
    }
}
