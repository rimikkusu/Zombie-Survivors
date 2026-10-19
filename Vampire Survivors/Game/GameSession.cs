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
        public AbilitySystem Abilities { get; } = new();
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
            Abilities.Reset(Player);
            Enemies.Clear();
            Bandages.Clear();
            Bullets.Clear();
            DamageNumbers.Clear();
            DeathParticles.Clear();
        }

        public void Update(Point cursor, PointF direction, bool noAbilityCooldowns, bool infiniteHealth)
        {
            if (Player.IsDead)
                return;

            int previousLevel = Player.Level;

            // The arena and the mouse both use the form's client coordinates.
            Abilities.Update(16, Player, noAbilityCooldowns);
            Player.Move(direction.X, direction.Y, ArenaSize);
            Player.UpdateAngle(cursor.X, cursor.Y);

            float forcefieldRadius = Abilities.IsActive(MajorAbilityType.Forcefield)
                ? AbilitySystem.ForcefieldRadius : 0f;
            EnemyManager.Update(Enemies, Player.GetCenter(), ArenaSize, forcefieldRadius,
                Player.GetHitbox(), 16);
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

        public MajorAbilityType? ActivateAbility(int slot, PointF cursorScreen, bool noCooldowns)
        {
            MajorAbilityType? selected = Abilities.GetAbilityAtSlot(slot);
            if (selected is not MajorAbilityType ability ||
                !Abilities.TryActivate(ability, Player, noCooldowns))
                return null;

            switch (ability)
            {
                case MajorAbilityType.Teleportation:
                    TeleportTowardCursor(cursorScreen);
                    break;
                case MajorAbilityType.Forcefield:
                    EnemyManager.PushEnemiesOutsideRadius(Enemies, Player.GetCenter(),
                        AbilitySystem.ForcefieldRadius, ArenaSize);
                    break;
                case MajorAbilityType.Shockwave:
                    int previousLevel = Player.Level;
                    if (ApplyShockwave() > 0)
                        QueueGainedLevels(previousLevel);
                    break;
            }
            return ability;
        }

        private void TeleportTowardCursor(PointF cursor)
        {
            PointF center = Player.GetCenter();
            float dx = cursor.X - center.X;
            float dy = cursor.Y - center.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);
            float fraction = distance > 450f ? 450f / distance : 1f;
            float targetX = Math.Clamp(center.X + dx * fraction - Player.Width / 2f,
                0, Math.Max(0, ArenaSize.Width - Player.Width));
            float targetY = Math.Clamp(center.Y + dy * fraction - Player.Height / 2f,
                0, Math.Max(0, ArenaSize.Height - Player.Height));
            // If the destination is occupied, try shorter jumps along the same line.
            for (int step = 10; step > 0; step--)
            {
                float x = Player.X + (targetX - Player.X) * step / 10f;
                float y = Player.Y + (targetY - Player.Y) * step / 10f;
                bool blocked = false;
                foreach (Enemy enemy in Enemies)
                {
                    if (Player.GetHitbox(x, y).IntersectsWith(enemy.GetHitbox()))
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked)
                    continue;
                Player.X = x;
                Player.Y = y;
                break;
            }
            Player.UpdateAngle(cursor.X, cursor.Y);
        }

        private int ApplyShockwave()
        {
            PointF center = Player.GetCenter();
            int xpAwarded = 0;
            // Go backwards because a shockwave can remove enemies it kills.
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = Enemies[i];
                if (EnemyManager.Distance(center, enemy.GetCenter()) > 300f)
                    continue;

                EnemyManager.PushEnemyAway(enemy, center, 180f, ArenaSize);
                xpAwarded += Combat.ApplyEnemyDamage(enemy, 20, false);
                if (enemy.Health <= 0)
                    Enemies.RemoveAt(i);
            }
            return xpAwarded;
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
