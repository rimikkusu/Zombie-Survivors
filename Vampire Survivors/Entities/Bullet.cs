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
        public const float Speed = 14f;
    }
}
