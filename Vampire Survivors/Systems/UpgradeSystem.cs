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

    public enum MajorAbilityType
    {
        Teleportation,
        Forcefield,
        Shockwave,
        Bloodlust,
        Multishot,
        LastStand
    }

    public class UpgradeSystem
    {
        private readonly Dictionary<UpgradeType, int> upgradeStacks = new();

        public static bool IsMajorAbilityLevel(int level)
        {
            return level > 0 && level % 10 == 0;
        }

        public int GetStackCount(UpgradeType type)
        {
            return upgradeStacks.TryGetValue(type, out int count) ? count : 0;
        }

        public IReadOnlyDictionary<UpgradeType, int> UpgradeStacks => upgradeStacks;

        public List<UpgradeType> GetRandomUpgrades(int count = 3)
        {
            var pool = Enum.GetValues<UpgradeType>().ToList();
            var result = new List<UpgradeType>();

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
            upgradeStacks[type] = GetStackCount(type) + 1;

            switch (type)
            {
                case UpgradeType.HealthBoost:
                    player.Stats.BonusMaxHealth += 10;
                    player.Heal(10);
                    break;

                case UpgradeType.SpeedBoost:
                    player.Stats.MoveSpeedBonusPercent += 0.03f;
                    break;

                case UpgradeType.Durability:
                    player.Stats.DamageResistancePercent += 0.03f;
                    break;

                case UpgradeType.DamageBoost:
                    player.Stats.DamageBonusPercent += 0.05f;
                    break;

                case UpgradeType.CriticalTraining:
                    player.Stats.CritChanceBonus += 0.01f;
                    break;

                case UpgradeType.BulletSpeed:
                    player.Stats.BulletSpeedBonusPercent += 0.05f;
                    break;

                case UpgradeType.RapidFire:
                    player.Stats.FireRateBonusPercent += 0.04f;
                    break;
            }
        }

        public static (string Title, string Description) GetUpgradeInfo(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.HealthBoost => ("HEALTH BOOST", "+10 MAX HP"),
                UpgradeType.SpeedBoost => ("SPEED BOOST", "+3% MOVEMENT SPEED"),
                UpgradeType.Durability => ("DURABILITY", "+3% DAMAGE RESISTANCE"),
                UpgradeType.DamageBoost => ("DAMAGE BOOST", "+5% DAMAGE"),
                UpgradeType.CriticalTraining => ("CRITICAL TRAINING", "+1% CRITICAL CHANCE"),
                UpgradeType.BulletSpeed => ("BULLET SPEED", "+5% PROJECTILE SPEED"),
                UpgradeType.RapidFire => ("RAPID FIRE", "+4% FIRE RATE"),
                _ => ("UPGRADE", "+BONUS")
            };
        }

        public static (string Title, string Description) GetMajorAbilityInfo(MajorAbilityType ability)
        {
            return ability switch
            {
                MajorAbilityType.Teleportation => ("TELEPORTATION", "Teleport toward your cursor. Cooldown: 75s"),
                MajorAbilityType.Forcefield => ("FORCEFIELD", "Block and push enemies for 6s. Cooldown: 90s"),
                MajorAbilityType.Shockwave => ("SHOCKWAVE", "Push nearby enemies and deal 20 damage. Cooldown: 45s"),
                MajorAbilityType.Bloodlust => ("BLOODLUST", "+50% damage and +20% speed for 8s. Cooldown: 60s"),
                MajorAbilityType.Multishot => ("MULTISHOT", "Fire two angled bullets for 10s. Cooldown: 60s"),
                MajorAbilityType.LastStand => ("LAST STAND", "+50% damage and resistance for 6s. Cooldown: 120s"),
                _ => ("MAJOR ABILITY", "")
            };
        }

        public void Reset()
        {
            upgradeStacks.Clear();
        }
    }
}
