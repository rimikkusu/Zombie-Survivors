namespace Vampire_Survivors.Entities
{
    public class Player
    {
        public const int Width = 128;
        public const int Height = 128;
        public const float Speed = 6f;

        public const float PivotX = 32f;
        public const float PivotY = 20f;

        public const float GunMuzzleX = 54f;
        public const float GunMuzzleY = 28f;

        public const int InvulnerabilityDurationMs = 750;
        public const int HitFlashDurationMs = 150;

        // Player art is drawn around the source pivot (32,20); the torso
        // occupies a small area below it, while most of the sprite is empty.
        public const int HitboxWidth = 46;
        public const int HitboxHeight = 36;

        public float X { get; set; }
        public float Y { get; set; }

        public float Angle { get; set; }

        public PlayerStats Stats { get; private set; } = new();

        public int MaxHealth => Stats.MaxHealth;
        public int Health { get; private set; } = 100;

        public bool IsDead => Health <= 0;

        public int Level { get; private set; } = 1;
        public int Experience { get; private set; } = 0;
        public int ExperienceToNextLevel { get; private set; } = 20;

        public int InvulnerabilityRemainingMs { get; private set; }
        public int HitFlashRemainingMs { get; private set; }
        public float ShootCooldownRemainingMs { get; set; }
        public int ShotFeedbackRemainingMs { get; private set; }

        public const int ShotFeedbackDurationMs = 64;

        public bool IsInvulnerable => InvulnerabilityRemainingMs > 0;

        public void RegisterShot() => ShotFeedbackRemainingMs = ShotFeedbackDurationMs;

        public PointF GetCenter() => new(X + Width / 2f, Y + Height / 2f);

        public RectangleF GetHitbox() => GetHitbox(X, Y);

        // The collision box covers the player's body, not the empty parts of the sprite.
        public static RectangleF GetHitbox(float x, float y) => new(
            x + Width / 2f - HitboxWidth / 2f,
            y + Height / 2f + 5f - HitboxHeight / 2f,
            HitboxWidth, HitboxHeight);

        public PointF GetGunMuzzlePosition()
        {
            PointF center = GetCenter();

            float scaleX = Width / 64f;
            float scaleY = Height / 64f;

            float localMuzzleX = (GunMuzzleX - PivotX) * scaleX;
            float localMuzzleY = (GunMuzzleY - PivotY) * scaleY;

            float angleRadians = Angle * MathF.PI / 180f;

            float cos = MathF.Cos(angleRadians);
            float sin = MathF.Sin(angleRadians);

            return new PointF(
                center.X + localMuzzleX * cos - localMuzzleY * sin,
                center.Y + localMuzzleX * sin + localMuzzleY * cos
            );
        }

        public void UpdateAngle(float mouseX, float mouseY)
        {
            PointF center = GetCenter();

            float dx = mouseX - center.X;
            float dy = mouseY - center.Y;

            Angle = MathF.Atan2(dy, dx) * 180f / MathF.PI;
        }

        public void Move(float directionX, float directionY, Size arenaSize)
        {
            float speed = Stats.MoveSpeed;
            X += directionX * speed;
            Y += directionY * speed;

            ClampToArena(arenaSize);
        }

        public void ClampToArena(Size arenaSize)
        {
            X = Math.Clamp(X, 0, Math.Max(0, arenaSize.Width - Width));
            Y = Math.Clamp(Y, 0, Math.Max(0, arenaSize.Height - Height));
        }

        public void TakeDamage(int rawDamage)
        {
            // A recent hit gives the player a short break from contact damage.
            if (IsDead || IsInvulnerable)
                return;

            float resistance = Stats.EffectiveDamageResistance;
            int finalDamage = (int)MathF.Round(rawDamage * (1f - resistance));
            finalDamage = Math.Max(1, finalDamage);

            Health = Math.Max(0, Health - finalDamage);

            InvulnerabilityRemainingMs = InvulnerabilityDurationMs;
            HitFlashRemainingMs = HitFlashDurationMs;
        }

        public int Heal(int amount)
        {
            if (IsDead || amount <= 0)
                return 0;

            int healed = Math.Min(amount, Math.Max(0, MaxHealth - Health));
            Health += healed;
            return healed;
        }

        public void UpdateDamageTimers(int elapsedMs)
        {
            InvulnerabilityRemainingMs = Math.Max(0, InvulnerabilityRemainingMs - elapsedMs);
            HitFlashRemainingMs = Math.Max(0, HitFlashRemainingMs - elapsedMs);
            ShootCooldownRemainingMs = Math.Max(0, ShootCooldownRemainingMs - elapsedMs);
            ShotFeedbackRemainingMs = Math.Max(0, ShotFeedbackRemainingMs - elapsedMs);
        }

        public void Reset(float x, float y)
        {
            X = x;
            Y = y;

            Stats = new PlayerStats();
            Health = MaxHealth;

            InvulnerabilityRemainingMs = 0;
            HitFlashRemainingMs = 0;
            ShootCooldownRemainingMs = 0;
            ShotFeedbackRemainingMs = 0;

            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 20;
        }

        public int AddExperience(int amount)
        {
            if (amount <= 0 || IsDead)
                return 0;

            Experience += amount;
            int levelsGained = 0;

            // Keep leftover XP, even if one reward gains several levels.
            while (Experience >= ExperienceToNextLevel)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                ExperienceToNextLevel += 8;
                levelsGained++;
            }

            return levelsGained;
        }
    }

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

    // Bullets, pickups and combat effects are small objects owned by GameSession.
    public class Bullet
    {
        public float X { get; set; }
        public float Y { get; set; }

        public float VelocityX { get; set; }
        public float VelocityY { get; set; }

        public float Angle { get; set; }

        public const int Size = 64;
        public const int HitboxWidth = 10;
        public const int HitboxHeight = 14;

    }

    public sealed class Bandage
    {
        public const int Size = 28;
        private const int HitboxPadding = 5;

        public Bandage(PointF center)
        {
            X = center.X - Size / 2f;
            Y = center.Y - Size / 2f;
        }

        public float X { get; private set; }
        public float Y { get; private set; }

        public void ClampToArena(System.Drawing.Size arenaSize)
        {
            X = Math.Clamp(X, 0, Math.Max(0, arenaSize.Width - Size));
            Y = Math.Clamp(Y, 0, Math.Max(0, arenaSize.Height - Size));
        }

        public RectangleF GetHitbox()
        {
            // A little extra padding makes the pickup easier to collect.
            return new RectangleF(
                X - HitboxPadding,
                Y - HitboxPadding,
                Size + HitboxPadding * 2,
                Size + HitboxPadding * 2);
        }
    }

    public enum CombatTextType
    {
        NormalDamage,
        CriticalDamage,
        KillDamage,
        CriticalKillDamage,
        Experience,
        Healing
    }

    public class DamageNumber
    {
        public float X { get; set; }
        public float Y { get; set; }

        // Damage dealt, XP awarded, or HP restored depending on Type.
        public int Damage { get; set; }

        public CombatTextType Type { get; set; } = CombatTextType.NormalDamage;

        public int Life { get; set; } = 45;

        // Counts down via the game loop. While above 0 the text
        // is hidden and motionless (used for delayed XP feedback).
        public int DelayRemainingMs { get; set; } = 0;
    }

    public sealed class DeathParticle
    {
        public const int LifetimeMs = 280;

        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Size { get; set; }
        public bool IsDark { get; set; }
        public int RemainingMs { get; private set; } = LifetimeMs;

        public void Update(int elapsedMs)
        {
            // Slow each piece and count down its lifetime; the renderer fades it.
            float frames = elapsedMs / 16f;
            X += VelocityX * frames;
            Y += VelocityY * frames;
            float drag = MathF.Pow(0.91f, frames);
            VelocityX *= drag;
            VelocityY *= drag;
            RemainingMs = Math.Max(0, RemainingMs - elapsedMs);
        }
    }
}
