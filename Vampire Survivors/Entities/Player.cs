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

        // Smaller than the full 128x128 sprite so contact
        // feels fair around the visible character.
        public const int HitboxSize = 80;

        public float X { get; set; }
        public float Y { get; set; }

        public float Angle { get; set; }

        public int MaxHealth { get; } = 100;
        public int Health { get; private set; } = 100;

        public bool IsDead => Health <= 0;

        public int Level { get; private set; } = 1;
        public int Experience { get; private set; } = 0;
        public int ExperienceToNextLevel { get; private set; } = 50;

        public int InvulnerabilityRemainingMs { get; private set; }
        public int HitFlashRemainingMs { get; private set; }

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
                center.X - HitboxSize / 2f,
                center.Y - HitboxSize / 2f,
                HitboxSize,
                HitboxSize
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

        public void Move(float directionX, float directionY, Size clientSize)
        {
            X += directionX * Speed;
            Y += directionY * Speed;

            X = Math.Clamp(
                X,
                0,
                clientSize.Width - Width
            );

            Y = Math.Clamp(
                Y,
                0,
                clientSize.Height - Height
            );
        }

        public void TakeDamage(int damage)
        {
            if (IsDead)
                return;

            if (IsInvulnerable)
                return;

            Health = Math.Max(0, Health - damage);

            InvulnerabilityRemainingMs = InvulnerabilityDurationMs;
            HitFlashRemainingMs = HitFlashDurationMs;
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
        }

        public void Reset(float x, float y)
        {
            X = x;
            Y = y;

            Health = MaxHealth;

            InvulnerabilityRemainingMs = 0;
            HitFlashRemainingMs = 0;

            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 50;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0)
                return;

            if (IsDead)
                return;

            Experience += amount;

            while (Experience >= ExperienceToNextLevel)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                ExperienceToNextLevel += 25;
            }
        }
    }
}
