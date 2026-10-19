using Vampire_Survivors.Entities;

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
        public bool CriticalHitFlash { get; set; }
        public int StaggerRemainingMs { get; set; }

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

        public void ClampToArena(Size arenaSize)
        {
            X = Math.Clamp(X, 0, Math.Max(0, arenaSize.Width - RenderSize));
            Y = Math.Clamp(Y, 0, Math.Max(0, arenaSize.Height - RenderSize));
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

namespace Vampire_Survivors.Systems
{
    public class EnemyManager
    {
        public void Update(List<Enemy> enemies, PointF playerCenter, Size arenaSize,
            RectangleF playerHitbox, int elapsedMs)
        {
            // All three types chase directly; their stats make them feel different.
            foreach (Enemy enemy in enemies)
            {
                PointF center = enemy.GetCenter();
                float dx = playerCenter.X - center.X;
                float dy = playerCenter.Y - center.Y;
                float distance = MathF.Sqrt(dx * dx + dy * dy);
                enemy.Angle = MathF.Atan2(dy, dx) * 180f / MathF.PI;
                enemy.StaggerRemainingMs = Math.Max(0, enemy.StaggerRemainingMs - elapsedMs);
                float speed = enemy.Stats.Speed;
                if (enemy.StaggerRemainingMs > 0)
                    speed *= enemy.Stats.StaggerSpeedFactor;
                if (distance > 0.001f)
                {
                    enemy.X += dx / distance * Math.Min(speed, distance);
                    enemy.Y += dy / distance * Math.Min(speed, distance);
                }
                enemy.ClampToArena(arenaSize);
                enemy.HitFlashTimer = Math.Max(0, enemy.HitFlashTimer - 1);
            }

            SeparateEnemies(enemies, arenaSize);
            foreach (Enemy enemy in enemies)
                SeparateFromPlayer(enemy, playerHitbox, arenaSize);
        }

        public bool TrySpawnEnemy(List<Enemy> enemies, Size arenaSize, PointF playerCenter,
            int playerLevel, int wave, int spawnIndex)
        {
            Enemy enemy = new(ChooseEnemyType(wave, spawnIndex), playerLevel, wave);
            int maxX = arenaSize.Width - enemy.RenderSize;
            int maxY = arenaSize.Height - enemy.RenderSize;
            if (maxX < 0 || maxY < 0)
                return false;

            float clearance = Math.Min(300f, Math.Min(arenaSize.Width, arenaSize.Height) * 0.45f);
            for (int attempt = 0; attempt < 80; attempt++)
            {
                enemy.X = Random.Shared.Next(maxX + 1);
                enemy.Y = Random.Shared.Next(maxY + 1);
                // One coordinate is on an edge, but the whole enemy stays inside.
                switch (Random.Shared.Next(4))
                {
                    case 0: enemy.X = 0; break;
                    case 1: enemy.X = maxX; break;
                    case 2: enemy.Y = 0; break;
                    case 3: enemy.Y = maxY; break;
                }
                if (Distance(enemy.GetCenter(), playerCenter) < clearance)
                    continue;
                bool crowded = false;
                foreach (Enemy existing in enemies)
                {
                    if (enemy.GetHitbox().IntersectsWith(existing.GetHitbox()))
                    {
                        crowded = true;
                        break;
                    }
                }
                if (crowded)
                    continue;
                enemies.Add(enemy);
                return true;
            }
            return false; // A crowded edge can be retried on the next frame.
        }

        public static EnemyType ChooseEnemyType(int wave, int spawnIndex)
        {
            if (wave >= 10 && (spawnIndex == 0 || Random.Shared.Next(5) == 0))
                return EnemyType.Vampire;
            if (wave >= 3 && (spawnIndex == 0 || Random.Shared.Next(3) == 0))
                return EnemyType.ZombieHeavy;
            return EnemyType.ZombieFast;
        }

        private static void SeparateEnemies(List<Enemy> enemies, Size arenaSize)
        {
            foreach (Enemy enemy in enemies)
            {
                float pushX = 0f;
                float pushY = 0f;
                foreach (Enemy other in enemies)
                {
                    if (enemy == other)
                        continue;
                    RectangleF overlap = RectangleF.Intersect(enemy.GetHitbox(), other.GetHitbox());
                    if (overlap.Width <= 0 || overlap.Height <= 0)
                        continue;
                    float push = Math.Min(2f, Math.Min(overlap.Width, overlap.Height) / 2f);
                    if (overlap.Width < overlap.Height)
                        pushX += enemy.X < other.X ? -push : push;
                    else
                        pushY += enemy.Y < other.Y ? -push : push;
                }
                // Even a large crowd cannot shove one enemy across the player in a frame.
                enemy.X += Math.Clamp(pushX, -3f, 3f);
                enemy.Y += Math.Clamp(pushY, -3f, 3f);
                enemy.ClampToArena(arenaSize);
            }
        }
        private static void SeparateFromPlayer(Enemy enemy, RectangleF player, Size arenaSize)
        {
            RectangleF body = enemy.GetHitbox();
            if (!body.IntersectsWith(player))
                return;

            // Keep one pixel of contact for damage. Try another side if an edge blocks a push.
            float bestMove = float.MaxValue;
            PointF position = new(enemy.X, enemy.Y);
            for (int side = 0; side < 4; side++)
            {
                float x = enemy.X;
                float y = enemy.Y;
                switch (side)
                {
                    case 0: x += player.Left - body.Right + 1; break;
                    case 1: x += player.Right - body.Left - 1; break;
                    case 2: y += player.Top - body.Bottom + 1; break;
                    case 3: y += player.Bottom - body.Top - 1; break;
                }
                if (x < 0 || y < 0 || x > arenaSize.Width - enemy.RenderSize ||
                    y > arenaSize.Height - enemy.RenderSize)
                    continue;
                float move = MathF.Abs(x - enemy.X) + MathF.Abs(y - enemy.Y);
                if (move < bestMove)
                {
                    bestMove = move;
                    position = new PointF(x, y);
                }
            }
            enemy.X = position.X;
            enemy.Y = position.Y;
        }

        public static float Distance(PointF first, PointF second)
        {
            float dx = first.X - second.X;
            float dy = first.Y - second.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }
    }

    // Count wave enemies here; EnemyManager chooses their positions.
    public sealed class WaveManager
    {
        public const int IntermissionDurationMs = 2000;
        private int spawnDelayMs = 650;
        public int CurrentWave { get; private set; } = 1;
        public int EnemiesSpawned { get; private set; }
        public int EnemiesRequired => 3 + (CurrentWave - 1) * 2;
        public int IntermissionRemainingMs { get; private set; }
        public bool IsWaveActive => IntermissionRemainingMs == 0;

        public int GetEnemiesRemaining(int livingEnemies)
        {
            return Math.Max(0, EnemiesRequired - EnemiesSpawned + livingEnemies);
        }

        public void Update(int elapsedMs, int livingEnemies, Func<int, int, bool> spawnEnemy)
        {
            // Finish the short break before adding enemies from the next wave.
            if (!IsWaveActive)
            {
                IntermissionRemainingMs = Math.Max(0, IntermissionRemainingMs - elapsedMs);
                if (IntermissionRemainingMs == 0)
                    AdvanceToNextWave();
                return;
            }
            if (EnemiesSpawned < EnemiesRequired)
            {
                spawnDelayMs -= elapsedMs;
                if (spawnDelayMs <= 0 && spawnEnemy(CurrentWave, EnemiesSpawned))
                {
                    EnemiesSpawned++;
                    spawnDelayMs = 650;
                }
            }
            else if (livingEnemies == 0)
                IntermissionRemainingMs = IntermissionDurationMs;
        }

        private void StartWave(int wave)
        {
            CurrentWave = wave;
            EnemiesSpawned = 0;
            spawnDelayMs = 650;
            IntermissionRemainingMs = 0;
        }

        public void Reset() => StartWave(1);
        public void AdvanceToNextWave() => StartWave(CurrentWave + 1);
        public void CompleteCurrentWave() => EnemiesSpawned = EnemiesRequired;
    }
}
