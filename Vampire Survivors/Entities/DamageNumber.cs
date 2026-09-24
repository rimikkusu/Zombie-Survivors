namespace Vampire_Survivors.Entities
{
    public class DamageNumber
    {
        public float X { get; set; }
        public float Y { get; set; }

        public int Damage { get; set; }

        public bool IsCritical { get; set; }

        public int Life { get; set; } = 45;
    }
}
