namespace Vampire_Survivors.Entities
{
    public class Enemy
    {
        public float X { get; set; }
        public float Y { get; set; }

        public float Angle { get; set; }

        public int Health { get; set; } = 100;

        // How long the enemy flashes red after being hit
        public int HitFlashTimer { get; set; } = 0;

        public const int Size = 128;
        public const float Speed = 2f;
    }
}
