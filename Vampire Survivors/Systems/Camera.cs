namespace Vampire_Survivors.Systems
{
    public static class GameWorld
    {
        public const int WorldWidth = 5000;
        public const int WorldHeight = 3500;
    }

    // X and Y are the top-left corner of the world currently visible on screen.
    public sealed class Camera
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        public void Follow(PointF targetCenter, Size viewportSize)
        {
            // Keep the player near the center without showing anything past the map edge.
            float maxX = Math.Max(0, GameWorld.WorldWidth - viewportSize.Width);
            float maxY = Math.Max(0, GameWorld.WorldHeight - viewportSize.Height);

            X = Math.Clamp(targetCenter.X - viewportSize.Width / 2f, 0, maxX);
            Y = Math.Clamp(targetCenter.Y - viewportSize.Height / 2f, 0, maxY);
        }

        public PointF ScreenToWorld(PointF screenPosition)
        {
            // Mouse clicks use screen coordinates; gameplay uses world coordinates.
            return new PointF(screenPosition.X + X, screenPosition.Y + Y);
        }

        public RectangleF GetVisibleWorldBounds(Size viewportSize)
        {
            return new RectangleF(X, Y, viewportSize.Width, viewportSize.Height);
        }

    }
}
