using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public sealed class AbilitySystem
    {
        public const int MaximumSlots = 9;
        public const float ForcefieldRadius = 180f;

        private readonly List<MajorAbilityType> acquiredAbilities = new();
        private readonly Dictionary<MajorAbilityType, AbilityRuntime> runtimeByAbility = new();

        public IReadOnlyList<MajorAbilityType> AcquiredAbilities => acquiredAbilities;

        public List<MajorAbilityType> GetRandomChoices(int count = 3)
        {
            List<MajorAbilityType> pool = Enum.GetValues<MajorAbilityType>()
                .Where(type => !runtimeByAbility.ContainsKey(type))
                .ToList();
            List<MajorAbilityType> result = new();

            while (result.Count < count && pool.Count > 0)
            {
                int index = Random.Shared.Next(pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        public bool Acquire(MajorAbilityType ability)
        {
            if (acquiredAbilities.Count >= MaximumSlots || runtimeByAbility.ContainsKey(ability))
                return false;

            acquiredAbilities.Add(ability);
            runtimeByAbility.Add(ability, new AbilityRuntime());
            return true;
        }

        public MajorAbilityType? GetAbilityAtSlot(int zeroBasedSlot)
        {
            return zeroBasedSlot >= 0 && zeroBasedSlot < acquiredAbilities.Count
                ? acquiredAbilities[zeroBasedSlot]
                : null;
        }

        public bool IsActive(MajorAbilityType ability)
        {
            return runtimeByAbility.TryGetValue(ability, out AbilityRuntime? runtime) &&
                   runtime.ActiveDurationRemainingMs > 0;
        }

        public int GetRemainingDurationMs(MajorAbilityType ability)
        {
            return runtimeByAbility.TryGetValue(ability, out AbilityRuntime? runtime)
                ? runtime.ActiveDurationRemainingMs
                : 0;
        }

        public bool TryActivate(MajorAbilityType ability, Player player, bool ignoreCooldowns = false)
        {
            if (!runtimeByAbility.TryGetValue(ability, out AbilityRuntime? runtime) ||
                runtime.ActiveDurationRemainingMs > 0 ||
                (!ignoreCooldowns && runtime.CooldownRemainingMs > 0))
            {
                return false;
            }

            (int cooldown, int duration) = GetTiming(ability);
            runtime.CooldownRemainingMs = duration == 0 && !ignoreCooldowns ? cooldown : 0;
            runtime.CooldownAfterDurationMs = duration > 0 ? cooldown : 0;
            runtime.ActiveDurationRemainingMs = duration;
            SyncTemporaryStats(player);
            return true;
        }

        public void Update(int elapsedMs, Player player, bool ignoreCooldowns = false)
        {
            foreach (AbilityRuntime runtime in runtimeByAbility.Values)
            {
                if (runtime.ActiveDurationRemainingMs > 0)
                {
                    runtime.ActiveDurationRemainingMs = Math.Max(0, runtime.ActiveDurationRemainingMs - elapsedMs);
                    if (runtime.ActiveDurationRemainingMs == 0)
                    {
                        runtime.CooldownRemainingMs = ignoreCooldowns ? 0 : runtime.CooldownAfterDurationMs;
                        runtime.CooldownAfterDurationMs = 0;
                    }
                }
                else if (ignoreCooldowns)
                {
                    runtime.CooldownRemainingMs = 0;
                }
                else
                {
                    runtime.CooldownRemainingMs = Math.Max(0, runtime.CooldownRemainingMs - elapsedMs);
                }
            }

            SyncTemporaryStats(player);
        }

        public string GetHudStatus(MajorAbilityType ability)
        {
            if (!runtimeByAbility.TryGetValue(ability, out AbilityRuntime? runtime))
                return "READY";

            if (runtime.ActiveDurationRemainingMs > 0)
                return "ACTIVE";

            if (runtime.CooldownRemainingMs > 0)
                return $"{Math.Max(1, (int)Math.Ceiling(runtime.CooldownRemainingMs / 1000d))}s";

            return "READY";
        }

        public void Reset(Player player)
        {
            acquiredAbilities.Clear();
            runtimeByAbility.Clear();
            player.Stats.TemporaryDamageBonusPercent = 0f;
            player.Stats.TemporaryMoveSpeedBonusPercent = 0f;
            player.Stats.TemporaryDamageResistancePercent = 0f;
        }

        private void SyncTemporaryStats(Player player)
        {
            player.Stats.TemporaryDamageBonusPercent =
                (IsActive(MajorAbilityType.Bloodlust) ? 0.50f : 0f) +
                (IsActive(MajorAbilityType.LastStand) ? 0.50f : 0f);
            player.Stats.TemporaryMoveSpeedBonusPercent =
                IsActive(MajorAbilityType.Bloodlust) ? 0.20f : 0f;
            player.Stats.TemporaryDamageResistancePercent =
                IsActive(MajorAbilityType.LastStand) ? 0.50f : 0f;
        }

        private static (int CooldownMs, int DurationMs) GetTiming(MajorAbilityType ability)
        {
            return ability switch
            {
                MajorAbilityType.Teleportation => (75_000, 0),
                MajorAbilityType.Forcefield => (90_000, 6_000),
                MajorAbilityType.Shockwave => (45_000, 0),
                MajorAbilityType.Bloodlust => (60_000, 8_000),
                MajorAbilityType.Multishot => (60_000, 10_000),
                MajorAbilityType.LastStand => (120_000, 6_000),
                _ => (0, 0)
            };
        }

        private sealed class AbilityRuntime
        {
            public int CooldownRemainingMs { get; set; }
            public int ActiveDurationRemainingMs { get; set; }
            public int CooldownAfterDurationMs { get; set; }
        }
    }
}
