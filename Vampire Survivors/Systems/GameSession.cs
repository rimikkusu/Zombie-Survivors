using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public sealed class GameSession
    {
        private readonly Size worldSize = new(GameWorld.WorldWidth, GameWorld.WorldHeight);
        private readonly Func<int, int, bool> spawnEnemy;
        private Size viewport;

        public Player Player { get; } = new();
        public List<Enemy> Enemies { get; } = new();
        public List<Bullet> Bullets { get; } = new();
        public List<Bandage> Bandages { get; } = new();
        public List<DamageNumber> DamageNumbers { get; } = new();
        public List<DeathParticle> DeathParticles { get; } = new();
        public InputManager Input { get; } = new();
        public CombatSystem Combat { get; } = new();
        public ProjectileSystem Projectiles { get; }
        public EnemyManager EnemyManager { get; } = new();
        public UpgradeSystem Upgrades { get; } = new();
        public AbilitySystem Abilities { get; } = new();
        public WaveManager Waves { get; } = new();
        public Camera Camera { get; } = new();
        public LevelUpSelection LevelUps { get; }

        public GameSession(Size viewport)
        {
            this.viewport = viewport;
            LevelUps = new LevelUpSelection(Upgrades, Abilities, Player);
            Projectiles = new ProjectileSystem(Combat);
            Player.X = (GameWorld.WorldWidth - Player.Width) / 2f;
            Player.Y = (GameWorld.WorldHeight - Player.Height) / 2f;
            Camera.Follow(Player.GetCenter(), viewport);
            spawnEnemy = (wave, index) => EnemyManager.TrySpawnEnemy(
                Enemies, worldSize, Camera.GetVisibleWorldBounds(this.viewport),
                Player.GetCenter(), Player.Level, wave, index);
        }

        public void Resize(Size viewport)
        {
            this.viewport = viewport;
            Camera.Follow(Player.GetCenter(), viewport);
        }

        public void Reset()
        {
            // A new run starts with no old enemies, pickups, bullets, or effects.
            LevelUps.Reset();
            Upgrades.Reset();
            Waves.Reset();
            Player.Reset((GameWorld.WorldWidth - Player.Width) / 2f,
                (GameWorld.WorldHeight - Player.Height) / 2f);
            Abilities.Reset(Player);
            Camera.Follow(Player.GetCenter(), viewport);
            Input.Reset();
            Enemies.Clear();
            Bandages.Clear();
            Bullets.Clear();
            DamageNumbers.Clear();
            DeathParticles.Clear();
        }

        public bool Update(Point cursorScreen, bool noAbilityCooldowns, bool infiniteHealth)
        {
            if (Player.IsDead)
                return false;

            int previousExperience = Player.Experience;
            int previousLevel = Player.Level;

            // Movement and aiming use world positions, even though the mouse is on the screen.
            Abilities.Update(16, Player, noAbilityCooldowns);
            PointF direction = Input.GetMovementDirection();
            Player.Move(direction.X, direction.Y, worldSize);
            Camera.Follow(Player.GetCenter(), viewport);
            PointF mouseWorld = Camera.ScreenToWorld(cursorScreen);
            Player.UpdateAngle(mouseWorld.X, mouseWorld.Y);

            float forcefieldRadius = Abilities.IsActive(MajorAbilityType.Forcefield)
                ? AbilitySystem.ForcefieldRadius : 0f;
            EnemyManager.Update(Enemies, Player.GetCenter(), worldSize, forcefieldRadius,
                Player.GetHitbox(), 16);
            CollectBandages();

            int xpGained = Projectiles.UpdateBullets(Player, Bullets, Enemies, DamageNumbers,
                worldSize, Bandages, DeathParticles);
            if (xpGained > 0)
            {
                LevelUps.QueueGainedLevels(previousLevel);
                // Choosing an upgrade pauses the rest of this game frame.
                if (LevelUps.HasPending)
                    return true;
            }

            Combat.UpdateDamageNumbers(DamageNumbers, 16);
            Combat.UpdateDeathParticles(DeathParticles, 16);
            EnemyManager.UpdateHitFlashTimers(Enemies);
            Waves.Update(16, Enemies.Count, spawnEnemy);
            Combat.UpdateEnemyContactDamage(Player, Enemies, 16, infiniteHealth);
            return Player.Level != previousLevel || Player.Experience != previousExperience;
        }

        public void Shoot(PointF cursorScreen)
        {
            // Convert the click to world space before calculating the shot direction.
            PointF mouseWorld = Camera.ScreenToWorld(cursorScreen);
            Player.UpdateAngle(mouseWorld.X, mouseWorld.Y);
            int previousLevel = Player.Level;

            bool multishot = Abilities.IsActive(MajorAbilityType.Multishot);
            int shotCount = multishot ? 2 : 1;
            for (int shot = 0; shot < shotCount; shot++)
            {
                float angleOffset = multishot ? -2f + shot * 4f : 0f;
                if (!Projectiles.Shoot(Player, Bullets, mouseWorld.X, mouseWorld.Y, angleOffset,
                    bypassCooldown: shot > 0, enemies: Enemies, damageNumbers: DamageNumbers,
                    bandages: Bandages, deathParticles: DeathParticles, worldSize: worldSize))
                    break;
            }
            LevelUps.QueueGainedLevels(previousLevel);
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
                        AbilitySystem.ForcefieldRadius, worldSize);
                    break;
                case MajorAbilityType.Shockwave:
                    int previousLevel = Player.Level;
                    if (ApplyShockwave() > 0)
                        LevelUps.QueueGainedLevels(previousLevel);
                    break;
            }
            return ability;
        }

        private void TeleportTowardCursor(PointF cursorScreen)
        {
            PointF mouseWorld = Camera.ScreenToWorld(cursorScreen);
            PointF currentCenter = Player.GetCenter();
            float dx = mouseWorld.X - currentCenter.X;
            float dy = mouseWorld.Y - currentCenter.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);

            // Teleport goes at most 450 world pixels toward the cursor.
            if (distance > 0.001f && distance > 450f)
            {
                dx = dx / distance * 450f;
                dy = dy / distance * 450f;
                mouseWorld = new PointF(currentCenter.X + dx, currentCenter.Y + dy);
            }

            float desiredX = Math.Clamp(mouseWorld.X - Player.Width / 2f, 0f, worldSize.Width - Player.Width);
            float desiredY = Math.Clamp(mouseWorld.Y - Player.Height / 2f, 0f, worldSize.Height - Player.Height);
            PointF destination = FindSafeTeleportLocation(desiredX, desiredY, currentCenter);
            Player.X = destination.X;
            Player.Y = destination.Y;
            Player.UpdateAngle(mouseWorld.X, mouseWorld.Y);
            Camera.Follow(Player.GetCenter(), viewport);
        }

        private PointF FindSafeTeleportLocation(float desiredX, float desiredY, PointF originCenter)
        {
            if (IsTeleportLocationSafe(desiredX, desiredY))
                return new PointF(desiredX, desiredY);

            // Try nearby spots if an enemy is standing at the destination.
            for (int ring = 1; ring <= 3; ring++)
            {
                float radius = ring * 40f;
                for (int direction = 0; direction < 8; direction++)
                {
                    float angle = direction * MathF.PI / 4f;
                    float x = Math.Clamp(desiredX + MathF.Cos(angle) * radius, 0f, worldSize.Width - Player.Width);
                    float y = Math.Clamp(desiredY + MathF.Sin(angle) * radius, 0f, worldSize.Height - Player.Height);
                    PointF center = new(x + Player.Width / 2f, y + Player.Height / 2f);

                    if (EnemyManager.Distance(originCenter, center) <= 450f && IsTeleportLocationSafe(x, y))
                        return new PointF(x, y);
                }
            }

            return new PointF(desiredX, desiredY);
        }

        private bool IsTeleportLocationSafe(float x, float y)
        {
            RectangleF hitbox = Player.GetHitbox(x, y);
            foreach (Enemy enemy in Enemies)
            {
                if (hitbox.IntersectsWith(enemy.GetHitbox()))
                    return false;
            }
            return true;
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

                EnemyManager.PushEnemyAway(enemy, center, 180f, worldSize);
                xpAwarded += Combat.ApplyEnemyDamage(Player, enemy, 20, false,
                    DamageNumbers, Bandages, DeathParticles);
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

        public void AddExperience(int amount)
        {
            if (amount <= 0 || Player.IsDead)
                return;
            int previousLevel = Player.Level;
            Player.AddExperience(amount);
            LevelUps.QueueGainedLevels(previousLevel);
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
