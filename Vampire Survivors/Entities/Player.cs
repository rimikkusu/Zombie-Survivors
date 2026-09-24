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

        public float X { get; set; }
        public float Y { get; set; }

        public float Angle { get; set; }

        public PointF GetCenter()
        {
            return new PointF(
                X + Width / 2f,
                Y + Height / 2f
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
    }
}
