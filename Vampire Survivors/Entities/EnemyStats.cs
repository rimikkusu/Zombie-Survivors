namespace Vampire_Survivors.Entities
{
    public enum EnemyType
    {
        ZombieFast,
        ZombieHeavy,
        Vampire
    }

    public sealed class EnemyStats
    {
        public int BaseHealth { get; private set; }
        public float Speed { get; private set; }
        public int ContactDamage { get; private set; }
        public int MinXp { get; private set; }
        public int MaxXp { get; private set; }
        public int RenderSize { get; private set; }
        public int HitboxWidth { get; private set; }
        public int HitboxHeight { get; private set; }
        public float Knockback { get; private set; }
        public int StaggerMs { get; private set; }
        public float StaggerSpeedFactor { get; private set; }

        private EnemyStats() { }

        public static EnemyStats GetStats(EnemyType type)
        {
            // Each enemy has its own size and hitbox. These are its starting values.
            switch (type)
            {
                case EnemyType.ZombieFast:
                    return new EnemyStats
                    {
                        BaseHealth = 30, Speed = Player.Speed, ContactDamage = 10,
                        MinXp = 12, MaxXp = 18, RenderSize = 100,
                        HitboxWidth = 40, HitboxHeight = 36,
                        Knockback = 14f, StaggerMs = 96, StaggerSpeedFactor = 0.2f
                    };
                case EnemyType.ZombieHeavy:
                    return new EnemyStats
                    {
                        BaseHealth = 80, Speed = Player.Speed * 0.60f, ContactDamage = 20,
                        MinXp = 22, MaxXp = 32, RenderSize = 150,
                        HitboxWidth = 58, HitboxHeight = 62,
                        Knockback = 5f, StaggerMs = 64, StaggerSpeedFactor = 0.55f
                    };
                case EnemyType.Vampire:
                    return new EnemyStats
                    {
                        BaseHealth = 110, Speed = Player.Speed * 1.10f, ContactDamage = 30,
                        MinXp = 40, MaxXp = 55, RenderSize = 130,
                        HitboxWidth = 52, HitboxHeight = 58,
                        Knockback = 8f, StaggerMs = 80, StaggerSpeedFactor = 0.2f
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public int GetScaledMaxHealth(int playerLevel)
        {
            // Enemies gain four percent health for each player level after level one.
            float multiplier = 1f + Math.Max(0, playerLevel - 1) * 0.04f;
            return Math.Max(1, (int)MathF.Round(BaseHealth * multiplier));
        }
    }
}
