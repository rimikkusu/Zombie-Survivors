using Vampire_Survivors.Systems;

namespace Vampire_Survivors.UI
{
    public sealed class LevelUpController
    {
        private readonly GameSession session;
        private readonly HudController hud;
        private readonly ScreenController screens;
        private readonly Label title;
        private readonly Button[] buttons;

        public LevelUpController(GameSession session, HudController hud, ScreenController screens,
            Label title, Button first, Button second, Button third)
        {
            this.session = session;
            this.hud = hud;
            this.screens = screens;
            this.title = title;
            buttons = [first, second, third];
        }

        public void ShowNext()
        {
            LevelUpSelection selection = session.LevelUps;
            if (!selection.HasPending || session.Player.IsDead)
            {
                screens.CloseLevelUp(session.Player.IsDead);
                return;
            }

            selection.PrepareNext();
            int level = selection.CurrentLevel;
            title.Text = selection.IsMajorAbility
                ? $"LEVEL {level} — CHOOSE AN ABILITY"
                : $"LEVEL {level} — CHOOSE AN UPGRADE";
            for (int index = 0; index < buttons.Length; index++)
            {
                Button button = buttons[index];
                int choiceCount = selection.IsMajorAbility
                    ? selection.AbilityChoices.Count : selection.UpgradeChoices.Count;
                button.Visible = index < choiceCount;
                if (index >= choiceCount)
                    continue;
                if (selection.IsMajorAbility)
                {
                    AbilityDefinition definition = AbilitySystem.GetInfo(selection.AbilityChoices[index]);
                    button.Text = $"{definition.Title}{Environment.NewLine}{definition.Description}";
                }
                else
                {
                    UpgradeType choice = selection.UpgradeChoices[index];
                    (string name, string description) = UpgradeSystem.GetUpgradeInfo(choice);
                    button.Text = $"{name}{Environment.NewLine}{description}   Owned: {session.Upgrades.GetStackCount(choice)}";
                }
            }
            screens.ShowLevelUp();
        }

        public bool Choose(int index, bool infiniteXp)
        {
            if (screens.State != GameState.LevelUp || !session.LevelUps.HasPending)
                return false;
            bool wasMajorAbility = session.LevelUps.IsMajorAbility;
            if (!session.LevelUps.Select(index))
                return false;

            hud.UpdateUpgrades();
            if (!wasMajorAbility)
                hud.UpdateHealth();
            if (infiniteXp && !session.Player.IsDead)
                session.GrantNextLevel();
            hud.UpdateExperience();
            ShowNext();
            return true;
        }
    }
}
