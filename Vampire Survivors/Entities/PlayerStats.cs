namespace Vampire_Survivors.Entities
{
    public class PlayerStats
    {
        public const float MaxDamageResistance = 0.60f; // 60% cap
        public const float MaxCritChance = 0.35f;        // 35% cap

        // Damage: Base 5-10, +5% additive per DamageBoost stack
        public int BaseDamageMin { get; set; } = 5;
        public int BaseDamageMax { get; set; } = 10;
        public float DamageBonusPercent { get; set; } = 0f;
        public float TemporaryDamageBonusPercent { get; set; } = 0f;

        // Move speed: Base 6.0, +3% additive per SpeedBoost stack
        public float BaseMoveSpeed { get; set; } = 6f;
        public float MoveSpeedBonusPercent { get; set; } = 0f;
        public float TemporaryMoveSpeedBonusPercent { get; set; } = 0f;

        // Crit chance: Base 2.5%, +1 percentage point additive per CriticalTraining stack
        public float BaseCritChance { get; set; } = 0.025f;
        public float CritChanceBonus { get; set; } = 0f;

        // Bullet speed: Base 14.0, +5% additive per BulletSpeed stack
        public float BaseBulletSpeed { get; set; } = 14f;
        public float BulletSpeedBonusPercent { get; set; } = 0f;

        // Damage resistance: Base 0%, +3 percentage points additive per Durability stack (capped at 60%)
        public float DamageResistancePercent { get; set; } = 0f;
        public float TemporaryDamageResistancePercent { get; set; } = 0f;

        // Health: Base 100, +10 additive per HealthBoost stack
        public int BaseMaxHealth { get; set; } = 100;
        public int BonusMaxHealth { get; set; } = 0;

        // Fire rate cooldown: Base 250ms (~4 shots/sec), +4% additive fire rate per RapidFire stack
        public float BaseFireCooldownMs { get; set; } = 250f;
        public float FireRateBonusPercent { get; set; } = 0f;

        public int MaxHealth => BaseMaxHealth + BonusMaxHealth;

        public float MoveSpeed => BaseMoveSpeed * (1f + MoveSpeedBonusPercent + TemporaryMoveSpeedBonusPercent);

        public float BulletSpeed => BaseBulletSpeed * (1f + BulletSpeedBonusPercent);

        public float EffectiveCritChance => Math.Min(BaseCritChance + CritChanceBonus, MaxCritChance);

        public float EffectiveDamageResistance => Math.Min(
            DamageResistancePercent + TemporaryDamageResistancePercent,
            MaxDamageResistance);

        public float FireCooldownMs => BaseFireCooldownMs / (1f + FireRateBonusPercent);

        public int CalculateDamage(int rawBaseDamage)
        {
            float finalDamage = rawBaseDamage * (1f + DamageBonusPercent + TemporaryDamageBonusPercent);
            return Math.Max(1, (int)MathF.Round(finalDamage));
        }

        public void Reset()
        {
            BaseDamageMin = 5;
            BaseDamageMax = 10;
            DamageBonusPercent = 0f;
            TemporaryDamageBonusPercent = 0f;

            BaseMoveSpeed = 6f;
            MoveSpeedBonusPercent = 0f;
            TemporaryMoveSpeedBonusPercent = 0f;

            BaseCritChance = 0.025f;
            CritChanceBonus = 0f;

            BaseBulletSpeed = 14f;
            BulletSpeedBonusPercent = 0f;

            DamageResistancePercent = 0f;
            TemporaryDamageResistancePercent = 0f;

            BaseMaxHealth = 100;
            BonusMaxHealth = 0;

            BaseFireCooldownMs = 250f;
            FireRateBonusPercent = 0f;
        }
    }
}
