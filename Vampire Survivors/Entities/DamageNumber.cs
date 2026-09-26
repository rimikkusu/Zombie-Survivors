namespace Vampire_Survivors.Entities
{
    public enum CombatTextType
    {
        NormalDamage,
        CriticalDamage,
        KillDamage,
        CriticalKillDamage,
        Experience,
        Healing
    }

    public class DamageNumber
    {
        public float X { get; set; }
        public float Y { get; set; }

        // Damage dealt, XP awarded, or HP restored depending on Type.
        public int Damage { get; set; }

        public CombatTextType Type { get; set; } = CombatTextType.NormalDamage;

        public int Life { get; set; } = 45;

        // Counts down via the game loop. While above 0 the text
        // is hidden and motionless (used for delayed XP feedback).
        public int DelayRemainingMs { get; set; } = 0;
    }
}
