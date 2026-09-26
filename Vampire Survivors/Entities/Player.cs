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

        public PlayerStats Stats { get; } = new();

        public int MaxHealth => Stats.MaxHealth;
        public int Health { get; private set; } = 100;

        public bool IsDead => Health <= 0;

        public int Level { get; private set; } = 1;
        public int Experience { get; private set; } = 0;
        public int ExperienceToNextLevel { get; private set; } = 20;

        public int InvulnerabilityRemainingMs { get; private set; }
        public int HitFlashRemainingMs { get; private set; }
        public float ShootCooldownRemainingMs { get; set; }

        public bool IsInvulnerable => InvulnerabilityRemainingMs > 0;

        public PointF GetCenter()
        {
            return new PointF(
                X + Width / 2f,
                Y + Height / 2f
            );
        }

        public RectangleF GetHitbox()
        {
            PointF center = GetCenter();

            return new RectangleF(
                center.X - HitboxWidth / 2f,
                center.Y + 5f - HitboxHeight / 2f,
                HitboxWidth,
                HitboxHeight
            );
        }

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

        public void Move(float directionX, float directionY, Size worldSize)
        {
            float speed = Stats.MoveSpeed;
            X += directionX * speed;
            Y += directionY * speed;

            X = Math.Clamp(
                X,
                0,
                worldSize.Width - Width
            );

            Y = Math.Clamp(
                Y,
                0,
                worldSize.Height - Height
            );
        }

        public void TakeDamage(int rawDamage)
        {
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
            if (InvulnerabilityRemainingMs > 0)
            {
                InvulnerabilityRemainingMs = Math.Max(
                    0,
                    InvulnerabilityRemainingMs - elapsedMs
                );
            }

            if (HitFlashRemainingMs > 0)
            {
                HitFlashRemainingMs = Math.Max(
                    0,
                    HitFlashRemainingMs - elapsedMs
                );
            }

            if (ShootCooldownRemainingMs > 0)
            {
                ShootCooldownRemainingMs = Math.Max(
                    0,
                    ShootCooldownRemainingMs - elapsedMs
                );
            }
        }

        public void Reset(float x, float y)
        {
            X = x;
            Y = y;

            Stats.Reset();
            Health = MaxHealth;

            InvulnerabilityRemainingMs = 0;
            HitFlashRemainingMs = 0;
            ShootCooldownRemainingMs = 0;

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
}
