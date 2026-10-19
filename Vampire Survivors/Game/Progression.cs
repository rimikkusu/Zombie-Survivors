using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public enum UpgradeType
    {
        HealthBoost,
        SpeedBoost,
        Durability,
        DamageBoost,
        CriticalTraining,
        BulletSpeed,
        RapidFire
    }

    public class UpgradeSystem
    {
        // Each stack adds the same bonus again, so we only need a count per upgrade.
        private readonly int[] upgradeStacks = new int[7];
        public int Version { get; private set; }
        public static IReadOnlyList<UpgradeType> Types { get; } = Enum.GetValues<UpgradeType>();

        private static readonly (string Name, string Effect, int Amount, string Unit)[] definitions =
        [
            ("Health Boost", "MAX HP", 10, " HP"),
            ("Speed Boost", "MOVEMENT SPEED", 3, "%"),
            ("Durability", "DAMAGE RESISTANCE", 3, "%"),
            ("Damage Boost", "DAMAGE", 5, "%"),
            ("Critical Training", "CRITICAL CHANCE", 1, "%"),
            ("Bullet Speed", "PROJECTILE SPEED", 5, "%"),
            ("Rapid Fire", "FIRE RATE", 4, "%")
        ];

        public int GetStackCount(UpgradeType type) => upgradeStacks[(int)type];

        public List<UpgradeType> GetRandomUpgrades(int count = 3)
        {
            // Remove each chosen item from the pool so one menu has no duplicates.
            List<UpgradeType> pool = new(Types);
            List<UpgradeType> result = new();
            while (result.Count < count && pool.Count > 0)
            {
                int index = Random.Shared.Next(pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return result;
        }

        public void ApplyUpgrade(UpgradeType type, Player player)
        {
            // Apply the new stack immediately to the player's stats.
            upgradeStacks[(int)type]++;
            Version++;
            int amount = definitions[(int)type].Amount;
            float percent = amount / 100f;

            switch (type)
            {
                case UpgradeType.HealthBoost:
                    player.Stats.BonusMaxHealth += amount;
                    player.Heal(amount);
                    break;

                case UpgradeType.SpeedBoost:
                    player.Stats.MoveSpeedBonusPercent += percent;
                    break;

                case UpgradeType.Durability:
                    player.Stats.DamageResistancePercent += percent;
                    break;

                case UpgradeType.DamageBoost:
                    player.Stats.DamageBonusPercent += percent;
                    break;

                case UpgradeType.CriticalTraining:
                    player.Stats.CritChanceBonus += percent;
                    break;

                case UpgradeType.BulletSpeed:
                    player.Stats.BulletSpeedBonusPercent += percent;
                    break;

                case UpgradeType.RapidFire:
                    player.Stats.FireRateBonusPercent += percent;
                    break;
            }
        }

        public static (string Title, string Description) GetUpgradeInfo(UpgradeType type)
        {
            var definition = definitions[(int)type];
            string unit = type == UpgradeType.HealthBoost ? "" : "%";
            return (definition.Name.ToUpperInvariant(), $"+{definition.Amount}{unit} {definition.Effect}");
        }

        public string GetStackText(UpgradeType type)
        {
            var definition = definitions[(int)type];
            int stacks = GetStackCount(type);
            return $"{definition.Name} x{stacks} (+{stacks * definition.Amount}{definition.Unit})";
        }

        public void Reset()
        {
            Array.Clear(upgradeStacks);
            Version++;
        }
    }
}
