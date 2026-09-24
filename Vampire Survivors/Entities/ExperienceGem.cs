namespace Vampire_Survivors.Entities
{
    public class ExperienceGem
    {
        public float X { get; set; }
        public float Y { get; set; }

        public int Value { get; set; } = 10;

        public const int Size = 32;

        public RectangleF GetBounds()
        {
            return new RectangleF(X, Y, Size, Size);
        }
    }
}
