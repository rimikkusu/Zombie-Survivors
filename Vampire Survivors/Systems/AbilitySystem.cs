using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public enum MajorAbilityType
    {
        Teleportation,
        Forcefield,
        Shockwave,
        Bloodlust,
        Multishot,
        LastStand
    }

    public sealed class AbilityDefinition
    {
        public string Title { get; }
        public string Effect { get; }
        public int CooldownMs { get; }
        public int DurationMs { get; }

        public AbilityDefinition(string title, string effect, int cooldownMs, int durationMs)
        {
            Title = title;
            Effect = effect;
            CooldownMs = cooldownMs;
            DurationMs = durationMs;
        }

        public string Description => $"{string.Format(Effect, DurationMs / 1000)} Cooldown: {CooldownMs / 1000}s";
    }

    public sealed class AbilitySystem
    {
        public const int MaximumSlots = 9;
        public const float ForcefieldRadius = 180f;

        private static readonly AbilityDefinition[] definitions =
        [
            new("TELEPORTATION", "Teleport toward your cursor.", 75_000, 0),
            new("FORCEFIELD", "Block and push enemies for {0}s.", 90_000, 6_000),
            new("SHOCKWAVE", "Push nearby enemies and deal 20 damage.", 45_000, 0),
            new("BLOODLUST", "+50% damage and +20% speed for {0}s.", 60_000, 8_000),
            new("MULTISHOT", "Fire two angled bullets for {0}s.", 60_000, 10_000),
            new("LAST STAND", "+50% damage and resistance for {0}s.", 120_000, 6_000)
        ];
        private static readonly MajorAbilityType[] types = Enum.GetValues<MajorAbilityType>();
        private readonly List<MajorAbilityType> acquiredAbilities = new();
        public int HudVersion { get; private set; }

        public static AbilityDefinition GetInfo(MajorAbilityType ability) => definitions[(int)ability];
        private readonly Dictionary<MajorAbilityType, AbilityRuntime> runtimeByAbility = new();

        public IReadOnlyList<MajorAbilityType> AcquiredAbilities => acquiredAbilities;

        public List<MajorAbilityType> GetRandomChoices(int count = 3)
        {
            List<MajorAbilityType> pool = new();
            foreach (MajorAbilityType ability in types)
            {
                if (!runtimeByAbility.ContainsKey(ability))
                    pool.Add(ability);
            }
            return UpgradeSystem.TakeRandom(pool, count);
        }

        public bool Acquire(MajorAbilityType ability)
        {
            if (acquiredAbilities.Count >= MaximumSlots || runtimeByAbility.ContainsKey(ability))
                return false;

            // Ability number keys follow the order in which abilities were picked.
            HudVersion++;
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

        public bool IsActive(MajorAbilityType ability) => GetRemainingDurationMs(ability) > 0;

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

            AbilityDefinition definition = GetInfo(ability);
            int cooldown = definition.CooldownMs;
            int duration = definition.DurationMs;
            // Instant abilities start cooling down now. Timed abilities start after they end.
            HudVersion++;
            runtime.CooldownRemainingMs = duration == 0 && !ignoreCooldowns ? cooldown : 0;
            runtime.ActiveDurationRemainingMs = duration;
            SyncTemporaryStats(player);
            return true;
        }

        public void Update(int elapsedMs, Player player, bool ignoreCooldowns = false)
        {
            // Timers only advance when GameSession calls this during play.
            foreach (var (ability, runtime) in runtimeByAbility)
            {
                int previousDisplay = GetDisplayState(runtime);
                if (runtime.ActiveDurationRemainingMs > 0)
                {
                    runtime.ActiveDurationRemainingMs = Math.Max(0, runtime.ActiveDurationRemainingMs - elapsedMs);
                    if (runtime.ActiveDurationRemainingMs == 0)
                    {
                        runtime.CooldownRemainingMs = ignoreCooldowns ? 0 : GetInfo(ability).CooldownMs;
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
                if (GetDisplayState(runtime) != previousDisplay)
                    HudVersion++;
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
            HudVersion++;
            acquiredAbilities.Clear();
            runtimeByAbility.Clear();
            player.Stats.TemporaryDamageBonusPercent = 0f;
            player.Stats.TemporaryMoveSpeedBonusPercent = 0f;
            player.Stats.TemporaryDamageResistancePercent = 0f;
        }

        private void SyncTemporaryStats(Player player)
        {
            // Bloodlust and Last Stand add temporary bonuses while active.
            float damageBonus = 0f;
            if (IsActive(MajorAbilityType.Bloodlust))
                damageBonus += 0.50f;
            if (IsActive(MajorAbilityType.LastStand))
                damageBonus += 0.50f;
            player.Stats.TemporaryDamageBonusPercent = damageBonus;

            player.Stats.TemporaryMoveSpeedBonusPercent = 0f;
            if (IsActive(MajorAbilityType.Bloodlust))
                player.Stats.TemporaryMoveSpeedBonusPercent = 0.20f;

            player.Stats.TemporaryDamageResistancePercent = 0f;
            if (IsActive(MajorAbilityType.LastStand))
                player.Stats.TemporaryDamageResistancePercent = 0.50f;
        }

        private static int GetDisplayState(AbilityRuntime runtime)
        {
            if (runtime.ActiveDurationRemainingMs > 0)
                return (runtime.ActiveDurationRemainingMs + 999) / 1000;
            return -(runtime.CooldownRemainingMs + 999) / 1000;
        }

        private sealed class AbilityRuntime
        {
            public int CooldownRemainingMs { get; set; }
            public int ActiveDurationRemainingMs { get; set; }
        }
    }
}
