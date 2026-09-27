namespace Vampire_Survivors.Entities
{
    public sealed class Bandage
    {
        public const int Size = 28;
        private const int HitboxPadding = 5;

        public Bandage(PointF worldCenter)
        {
            // Store the pickup in world space so it stays in place as the camera moves.
            X = worldCenter.X - Size / 2f;
            Y = worldCenter.Y - Size / 2f;
        }

        public float X { get; }
        public float Y { get; }

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
}
