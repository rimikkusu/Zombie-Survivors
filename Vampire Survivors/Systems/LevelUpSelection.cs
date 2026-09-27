using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public sealed class LevelUpSelection
    {
        private readonly Queue<int> pendingLevels = new();
        private readonly UpgradeSystem upgrades;
        private readonly AbilitySystem abilities;
        private readonly Player player;
        private List<UpgradeType> upgradeChoices = new();
        private List<MajorAbilityType> abilityChoices = new();

        public LevelUpSelection(UpgradeSystem upgrades, AbilitySystem abilities, Player player)
        {
            this.upgrades = upgrades;
            this.abilities = abilities;
            this.player = player;
        }

        public bool HasPending => pendingLevels.Count > 0;
        public int CurrentLevel => pendingLevels.Peek();
        public bool IsMajorAbility { get; private set; }
        public IReadOnlyList<UpgradeType> UpgradeChoices => upgradeChoices;
        public IReadOnlyList<MajorAbilityType> AbilityChoices => abilityChoices;

        public void QueueGainedLevels(int previousLevel)
        {
            // One XP reward can pass several levels, so queue every choice owed.
            for (int level = previousLevel + 1; level <= player.Level; level++)
                pendingLevels.Enqueue(level);
        }

        public void PrepareNext()
        {
            // Every tenth level offers a major ability if any are still available.
            IsMajorAbility = UpgradeSystem.IsMajorAbilityLevel(CurrentLevel);
            if (IsMajorAbility)
            {
                abilityChoices = abilities.GetRandomChoices();
                IsMajorAbility = abilityChoices.Count > 0;
            }

            if (IsMajorAbility)
                upgradeChoices.Clear();
            else
            {
                upgradeChoices = upgrades.GetRandomUpgrades();
                abilityChoices.Clear();
            }
        }

        public bool Select(int index)
        {
            if (!HasPending)
                return false;

            if (IsMajorAbility)
            {
                if (index >= abilityChoices.Count)
                    return false;
                abilities.Acquire(abilityChoices[index]);
            }
            else
            {
                if (index >= upgradeChoices.Count)
                    return false;
                upgrades.ApplyUpgrade(upgradeChoices[index], player);
            }

            pendingLevels.Dequeue();
            return true;
        }

        public void Reset()
        {
            pendingLevels.Clear();
            upgradeChoices.Clear();
            abilityChoices.Clear();
            IsMajorAbility = false;
        }
    }
}
