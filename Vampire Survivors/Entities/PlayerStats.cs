namespace Vampire_Survivors.Entities
{
    public class PlayerStats
    {
        public const float MaxDamageResistance = 0.60f; // 60% cap
        public const float MaxCritChance = 0.35f;        // 35% cap

        // A useful starting weapon; DamageBoost still adds 5% per stack.
        public int BaseDamageMin { get; set; } = 10;
        public int BaseDamageMax { get; set; } = 14;
        public float DamageBonusPercent { get; set; } = 0f;
        public float TemporaryDamageBonusPercent { get; set; } = 0f;

        // Move speed: Base 6.0, +3% additive per SpeedBoost stack
        public float BaseMoveSpeed { get; set; } = 6f;
        public float MoveSpeedBonusPercent { get; set; } = 0f;
        public float TemporaryMoveSpeedBonusPercent { get; set; } = 0f;

        // Crit chance: Base 2.5%, +1 percentage point additive per CriticalTraining stack
        public float BaseCritChance { get; set; } = 0.025f;
        public float CritChanceBonus { get; set; } = 0f;

        // Bullet speed: Base 20.0, +5% additive per BulletSpeed stack
        public float BaseBulletSpeed { get; set; } = 20f;
        public float BulletSpeedBonusPercent { get; set; } = 0f;

        // Damage resistance: Base 0%, +3 percentage points additive per Durability stack (capped at 60%)
        public float DamageResistancePercent { get; set; } = 0f;
        public float TemporaryDamageResistancePercent { get; set; } = 0f;

        // Health: Base 100, +10 additive per HealthBoost stack
        public int BaseMaxHealth { get; set; } = 100;
        public int BonusMaxHealth { get; set; } = 0;

        // Fire rate cooldown: Base 210ms, +4% additive fire rate per RapidFire stack
        public float BaseFireCooldownMs { get; set; } = 210f;
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

    }
}
