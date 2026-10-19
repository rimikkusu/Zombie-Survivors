using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public sealed class GameSession
    {
        private readonly Func<int, int, bool> spawnEnemy;
        public Size ArenaSize { get; private set; }

        public Player Player { get; } = new();
        public List<Enemy> Enemies { get; } = new();
        public List<Bullet> Bullets { get; } = new();
        public List<Bandage> Bandages { get; } = new();
        public List<DamageNumber> DamageNumbers { get; } = new();
        public List<DeathParticle> DeathParticles { get; } = new();
        public CombatSystem Combat { get; }
        public EnemyManager EnemyManager { get; } = new();
        public UpgradeSystem Upgrades { get; } = new();
        public WaveManager Waves { get; } = new();
        public Queue<int> PendingLevels { get; } = new();

        public GameSession(Size arenaSize)
        {
            Combat = new CombatSystem(this);
            spawnEnemy = (wave, index) => EnemyManager.TrySpawnEnemy(
                Enemies, ArenaSize,
                Player.GetCenter(), Player.Level, wave, index);
            Resize(arenaSize);
            Reset();
        }

        public void Resize(Size arenaSize)
        {
            // Minimize can briefly report an empty client area.
            if (arenaSize.Width <= 0 || arenaSize.Height <= 0)
                return;
            ArenaSize = arenaSize;
            Player.ClampToArena(ArenaSize);
            foreach (Enemy enemy in Enemies)
                enemy.ClampToArena(ArenaSize);
            foreach (Bandage bandage in Bandages)
                bandage.ClampToArena(ArenaSize);
            RectangleF bounds = new(PointF.Empty, ArenaSize);
            Bullets.RemoveAll(bullet => !bounds.Contains(bullet.X + Bullet.Size / 2f, bullet.Y + Bullet.Size / 2f));
            DamageNumbers.RemoveAll(number => !bounds.Contains(number.X, number.Y));
            DeathParticles.RemoveAll(particle => !bounds.Contains(particle.X, particle.Y));
        }

        public void Reset()
        {
            // A new run starts with no old enemies, pickups, bullets, or effects.
            PendingLevels.Clear();
            Upgrades.Reset();
            Waves.Reset();
            Player.Reset(Math.Max(0, (ArenaSize.Width - Player.Width) / 2f),
                Math.Max(0, (ArenaSize.Height - Player.Height) / 2f));
            Enemies.Clear();
            Bandages.Clear();
            Bullets.Clear();
            DamageNumbers.Clear();
            DeathParticles.Clear();
        }

        public void Update(Point cursor, PointF direction, bool infiniteHealth)
        {
            if (Player.IsDead)
                return;

            int previousLevel = Player.Level;

            // The arena and the mouse both use the form's client coordinates.
            Player.Move(direction.X, direction.Y, ArenaSize);
            Player.UpdateAngle(cursor.X, cursor.Y);

            EnemyManager.Update(Enemies, Player.GetCenter(), ArenaSize, Player.GetHitbox(), 16);
            CollectBandages();

            Combat.UpdateBullets();
            QueueGainedLevels(previousLevel);
            // Choosing an upgrade pauses the rest of this game frame.
            if (PendingLevels.Count > 0)
                return;

            Waves.Update(16, Enemies.Count, spawnEnemy);
            Combat.UpdateEffectsAndContact(16, infiniteHealth);
        }

        public void Shoot(PointF cursor)
        {
            int previousLevel = Player.Level;
            Combat.Shoot(cursor);
            QueueGainedLevels(previousLevel);
        }

        private void CollectBandages()
        {
            if (Player.IsDead || Player.Health >= Player.MaxHealth)
                return;

            // A bandage stays on the ground while health is full.
            RectangleF playerHitbox = Player.GetHitbox();
            for (int i = Bandages.Count - 1; i >= 0; i--)
            {
                if (!playerHitbox.IntersectsWith(Bandages[i].GetHitbox()))
                    continue;
                int healed = Player.Heal(20);
                if (healed <= 0)
                    continue;

                PointF playerCenter = Player.GetCenter();
                DamageNumbers.Add(new DamageNumber
                {
                    X = playerCenter.X, Y = Player.Y,
                    Damage = healed, Type = CombatTextType.Healing
                });
                Bandages.RemoveAt(i);
            }
        }

        private void QueueGainedLevels(int previousLevel)
        {
            // One reward can gain several levels; each level still earns one choice.
            for (int level = previousLevel + 1; level <= Player.Level; level++)
                PendingLevels.Enqueue(level);
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || Player.IsDead)
                return;
            int previousLevel = Player.Level;
            Player.AddExperience(amount);
            QueueGainedLevels(previousLevel);
        }

        public void GrantNextLevel()
        {
            AddExperience(Math.Max(0, Player.ExperienceToNextLevel - Player.Experience));
        }

        public void ClearDebugWave(bool advance)
        {
            Enemies.Clear();
            Bullets.Clear();
            if (advance)
                Waves.AdvanceToNextWave();
            else
                Waves.CompleteCurrentWave();
        }

        public int HealToFull() => Player.Heal(Player.MaxHealth);
    }
}
