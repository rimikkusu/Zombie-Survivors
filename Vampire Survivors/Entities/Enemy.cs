namespace Vampire_Survivors.Entities
{
    public enum EnemyBehaviorState
    {
        Chase,
        Windup,
        Burst,
        Strafe,
        Retreat,
        Recover
    }

    public class Enemy
    {
        public Enemy(EnemyType type = EnemyType.ZombieFast, int playerLevel = 1, int spawnWave = 1)
        {
            Type = type;
            Stats = EnemyStats.GetStats(type);
            SpawnWave = Math.Max(1, spawnWave);
            MaxHealth = Stats.GetScaledMaxHealth(playerLevel);
            Health = MaxHealth;
            ProximityDelayRemainingMs = Random.Shared.Next(180, 850);
            SpecialCooldownRemainingMs = Random.Shared.Next(400, 1800);
            StrafeDirection = Random.Shared.Next(2) == 0 ? -1 : 1;
            WanderPhase = Random.Shared.NextSingle() * MathF.Tau;
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
        public bool CriticalHitFlash { get; set; }
        public int StaggerRemainingMs { get; set; }

        // The behavior state tells EnemyBehavior which part of an attack comes next.
        public EnemyBehaviorState BehaviorState { get; set; } = EnemyBehaviorState.Chase;
        public int BehaviorRemainingMs { get; set; }
        public int SpecialCooldownRemainingMs { get; set; }
        public int ProximityDelayRemainingMs { get; set; }
        public int StrafeDirection { get; set; }
        public float WanderPhase { get; set; }
        public float BurstDirectionX { get; set; }
        public float BurstDirectionY { get; set; }

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

        public void ClampToWorld(Size worldSize)
        {
            X = Math.Clamp(X, 0, Math.Max(0, worldSize.Width - RenderSize));
            Y = Math.Clamp(Y, 0, Math.Max(0, worldSize.Height - RenderSize));
        }

        public int RollExperienceReward()
        {
            // Later waves give more XP, up to four times the starting reward.
            int baseXp = Random.Shared.Next(Stats.MinXp, Stats.MaxXp + 1);
            float multiplier = Math.Min(4f, 1f + (SpawnWave - 1) * 0.10f);
            return Math.Max(1, (int)MathF.Round(baseXp * multiplier));
        }
    }
}
