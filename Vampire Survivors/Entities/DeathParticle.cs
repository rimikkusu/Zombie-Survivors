namespace Vampire_Survivors.Entities
{
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
