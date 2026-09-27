namespace Vampire_Survivors.Entities
{
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

        public RectangleF GetHitbox()
        {
            // The visible bullet image is larger than the part that can hit an enemy.
            return new RectangleF(
                X + (Size - HitboxWidth) / 2f,
                Y + (Size - HitboxHeight) / 2f,
                HitboxWidth,
                HitboxHeight
            );
        }
    }
}
