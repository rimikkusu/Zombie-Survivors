namespace Vampire_Survivors.Entities
{
    public enum EnemyType
    {
        ZombieFast,
        ZombieHeavy,
        Vampire
    }

    public readonly record struct EnemyStats(
        int BaseHealth,
        float Speed,
        int ContactDamage,
        int MinXp,
        int MaxXp,
        int RenderSize,
        int HitboxWidth,
        int HitboxHeight)
    {
        public static EnemyStats GetStats(EnemyType type)
        {
            return type switch
            {
                EnemyType.ZombieFast => new EnemyStats(35, Player.Speed, 10, 12, 18, 100, 40, 36),
                EnemyType.ZombieHeavy => new EnemyStats(100, Player.Speed * 0.60f, 20, 22, 32, 150, 58, 62),
                EnemyType.Vampire => new EnemyStats(160, Player.Speed * 1.10f, 30, 40, 55, 130, 52, 58),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public int GetScaledMaxHealth(int playerLevel)
        {
            float multiplier = 1f + Math.Max(0, playerLevel - 1) * 0.06f;
            return Math.Max(1, (int)MathF.Round(BaseHealth * multiplier));
        }
    }
}
