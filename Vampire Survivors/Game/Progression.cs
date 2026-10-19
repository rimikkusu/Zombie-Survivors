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

        public List<UpgradeType> GetRandomUpgrades(int count = 3) => TakeRandom(new List<UpgradeType>(Types), count);

        internal static List<T> TakeRandom<T>(List<T> pool, int count)
        {
            // Remove each chosen item from the pool so one menu has no duplicates.
            List<T> result = new();
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

    public enum MajorAbilityType
    {
        Teleportation,
        Forcefield,
        Shockwave,
        Bloodlust,
        Multishot,
        LastStand
    }

    public sealed class AbilitySystem
    {
        public const float ForcefieldRadius = 180f;
        private static readonly (string Title, string Effect, int CooldownMs, int DurationMs)[] definitions =
        [
            ("TELEPORTATION", "Teleport toward your cursor.", 75_000, 0),
            ("FORCEFIELD", "Block and push enemies for {0}s.", 90_000, 6_000),
            ("SHOCKWAVE", "Push nearby enemies and deal 20 damage.", 45_000, 0),
            ("BLOODLUST", "+50% damage and +20% speed for {0}s.", 60_000, 8_000),
            ("MULTISHOT", "Fire two angled bullets for {0}s.", 60_000, 10_000),
            ("LAST STAND", "+50% damage and resistance for {0}s.", 120_000, 6_000)
        ];
        private readonly List<MajorAbilityType> acquiredAbilities = new();
        // The enum value is the index in both timer arrays.
        private readonly int[] cooldowns = new int[definitions.Length];
        private readonly int[] durations = new int[definitions.Length];
        public int HudVersion { get; private set; }
        public IReadOnlyList<MajorAbilityType> AcquiredAbilities => acquiredAbilities;

        public static (string Title, string Description) GetInfo(MajorAbilityType ability)
        {
            var info = definitions[(int)ability];
            string effect = string.Format(info.Effect, info.DurationMs / 1000);
            return (info.Title, $"{effect} Cooldown: {info.CooldownMs / 1000}s");
        }

        public List<MajorAbilityType> GetRandomChoices(int count = 3)
        {
            List<MajorAbilityType> pool = new();
            foreach (MajorAbilityType ability in Enum.GetValues<MajorAbilityType>())
            {
                if (!acquiredAbilities.Contains(ability))
                    pool.Add(ability);
            }
            return UpgradeSystem.TakeRandom(pool, count);
        }

        public bool Acquire(MajorAbilityType ability)
        {
            if (acquiredAbilities.Contains(ability))
                return false;
            // Number-key slots use acquisition order, rather than enum order.
            acquiredAbilities.Add(ability);
            HudVersion++;
            return true;
        }

        public MajorAbilityType? GetAbilityAtSlot(int slot)
        {
            return slot >= 0 && slot < acquiredAbilities.Count ? acquiredAbilities[slot] : null;
        }

        public int GetRemainingDurationMs(MajorAbilityType ability) => durations[(int)ability];
        public bool IsActive(MajorAbilityType ability) => durations[(int)ability] > 0;

        public bool TryActivate(MajorAbilityType ability, Player player, bool ignoreCooldowns = false)
        {
            int index = (int)ability;
            if (player.IsDead || !acquiredAbilities.Contains(ability) || durations[index] > 0 ||
                (!ignoreCooldowns && cooldowns[index] > 0))
                return false;
            durations[index] = definitions[index].DurationMs;
            cooldowns[index] = durations[index] == 0 && !ignoreCooldowns ? definitions[index].CooldownMs : 0;
            HudVersion++;
            SyncTemporaryStats(player);
            return true;
        }

        public void Update(int elapsedMs, Player player, bool ignoreCooldowns = false)
        {
            foreach (MajorAbilityType ability in acquiredAbilities)
            {
                int index = (int)ability;
                int previous = GetDisplayState(index);
                if (durations[index] > 0)
                {
                    durations[index] = Math.Max(0, durations[index] - elapsedMs);
                    // Timed abilities begin cooling down when their effect finishes.
                    if (durations[index] == 0)
                        cooldowns[index] = ignoreCooldowns ? 0 : definitions[index].CooldownMs;
                }
                else
                    cooldowns[index] = ignoreCooldowns ? 0 : Math.Max(0, cooldowns[index] - elapsedMs);
                if (previous != GetDisplayState(index))
                    HudVersion++;
            }
            SyncTemporaryStats(player);
        }

        private int GetDisplayState(int index)
        {
            if (durations[index] > 0)
                return (durations[index] + 999) / 1000;
            return -(cooldowns[index] + 999) / 1000;
        }

        public string GetHudStatus(MajorAbilityType ability)
        {
            int index = (int)ability;
            if (durations[index] > 0)
                return "ACTIVE";
            if (cooldowns[index] > 0)
                return $"{(cooldowns[index] + 999) / 1000}s";
            return "READY";
        }

        private void SyncTemporaryStats(Player player)
        {
            float damage = 0f;
            if (IsActive(MajorAbilityType.Bloodlust))
                damage += 0.50f;
            if (IsActive(MajorAbilityType.LastStand))
                damage += 0.50f;
            player.Stats.TemporaryDamageBonusPercent = damage;
            player.Stats.TemporaryMoveSpeedBonusPercent = IsActive(MajorAbilityType.Bloodlust) ? 0.20f : 0f;
            player.Stats.TemporaryDamageResistancePercent = IsActive(MajorAbilityType.LastStand) ? 0.50f : 0f;
        }

        public void Reset(Player player)
        {
            acquiredAbilities.Clear();
            Array.Clear(cooldowns);
            Array.Clear(durations);
            HudVersion++;
            SyncTemporaryStats(player);
        }
    }
}
