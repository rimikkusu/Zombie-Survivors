namespace Vampire_Survivors.Entities
{
    public class Enemy
    {
        public Enemy(EnemyType type = EnemyType.ZombieFast, int playerLevel = 1, int spawnWave = 1)
        {
            Type = type;
            Stats = EnemyStats.GetStats(type);
            SpawnWave = Math.Max(1, spawnWave);
            MaxHealth = Stats.GetScaledMaxHealth(playerLevel);
            Health = MaxHealth;
        }

        public float X { get; set; }
        public float Y { get; set; }

        public float Angle { get; set; }

        public EnemyType Type { get; }
        public EnemyStats Stats { get; }
        public int SpawnWave { get; }

        public int MaxHealth { get; }
        public int Health { get; set; }

        // How long the enemy flashes red after being hit
        public int HitFlashTimer { get; set; } = 0;

        public int RenderSize => Stats.RenderSize;

        public PointF GetCenter() => new(X + RenderSize / 2f, Y + RenderSize / 2f);

        public RectangleF GetHitbox()
        {
            return new RectangleF(
                X + (RenderSize - Stats.HitboxWidth) / 2f,
                Y + (RenderSize - Stats.HitboxHeight) / 2f,
                Stats.HitboxWidth,
                Stats.HitboxHeight
            );
        }

        public int RollExperienceReward()
        {
            int baseXp = Random.Shared.Next(Stats.MinXp, Stats.MaxXp + 1);
            float multiplier = Math.Min(4f, 1f + (SpawnWave - 1) * 0.10f);
            return Math.Max(1, (int)MathF.Round(baseXp * multiplier));
        }
    }
}
