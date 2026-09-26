namespace Vampire_Survivors.Entities
{
    public sealed class Bandage
    {
        public const int Size = 28;
        private const int HitboxPadding = 5;

        public Bandage(PointF worldCenter)
        {
            X = worldCenter.X - Size / 2f;
            Y = worldCenter.Y - Size / 2f;
        }

        public float X { get; }
        public float Y { get; }

        public RectangleF GetHitbox()
        {
            return new RectangleF(
                X - HitboxPadding,
                Y - HitboxPadding,
                Size + HitboxPadding * 2,
                Size + HitboxPadding * 2);
        }
    }
}
