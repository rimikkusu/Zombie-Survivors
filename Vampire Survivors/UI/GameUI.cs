using Vampire_Survivors.Entities;
using Vampire_Survivors.Systems;

namespace Vampire_Survivors.UI
{
    public sealed class HudController
    {
        private readonly GameSession session;
        private readonly Control[] hudControls;
        private readonly Label healthLabel;
        private readonly Panel healthFill;
        private readonly Label levelLabel;
        private readonly Label xpLabel;
        private readonly Panel xpFill;
        private readonly Label waveLabel;
        private readonly ListBox upgradesList;
        private readonly Panel activeAbilityPanel;
        private readonly Label activeAbilityLabel;
        private readonly Panel debugPanel;
        private readonly Button pauseButton;

        private (int Upgrades, int Abilities)? lastUpgrades;
        private (int Version, string Instant)? lastAbilityFeedback;
        private int instantFeedbackRemainingMs;
        private string instantFeedbackText = string.Empty;

        public HudController(GameSession session, Label healthLabel, Panel healthBackground,
            Panel healthFill, Label levelLabel, Label xpLabel, Panel xpBackground,
            Panel xpFill, Label waveLabel, Label upgradesTitle, ListBox upgradesList,
            Button pauseButton, Panel activeAbilityPanel, Label activeAbilityLabel, Panel debugPanel)
        {
            this.session = session;
            this.healthLabel = healthLabel;
            this.healthFill = healthFill;
            this.levelLabel = levelLabel;
            this.xpLabel = xpLabel;
            this.xpFill = xpFill;
            this.waveLabel = waveLabel;
            this.upgradesList = upgradesList;
            this.activeAbilityPanel = activeAbilityPanel;
            this.activeAbilityLabel = activeAbilityLabel;
            this.debugPanel = debugPanel;
            this.pauseButton = pauseButton;
            hudControls = [healthLabel, healthBackground, levelLabel, xpLabel, xpBackground,
                waveLabel, upgradesTitle, upgradesList, pauseButton];
        }

        public bool UpdateHealth()
        {
            Player player = session.Player;
            UpdateBar(healthFill, player.Health, player.MaxHealth);
            healthLabel.Text = $"HP {player.Health} / {player.MaxHealth}";
            return player.IsDead;
        }

        public bool RefreshFrame(int elapsedMs)
        {
            UpdateActiveAbilityFeedback(elapsedMs);
            UpdateUpgrades();
            UpdateExperience();
            UpdateWave();
            return UpdateHealth();
        }

        public void UpdateExperience()
        {
            Player player = session.Player;
            UpdateBar(xpFill, player.Experience, player.ExperienceToNextLevel);
            levelLabel.Text = $"LEVEL {player.Level}";
            xpLabel.Text = $"{player.Experience} / {player.ExperienceToNextLevel} XP";
        }

        private static void UpdateBar(Panel fill, int value, int maximum)
        {
            // Resize only the fill panel; the labels stay above the bar.
            float fraction = maximum > 0 ? value / (float)maximum : 0f;
            fraction = Math.Clamp(fraction, 0f, 1f);
            fill.Width = (int)(fill.Parent!.ClientSize.Width * fraction);
        }

        public void UpdateWave()
        {
            WaveManager waves = session.Waves;
            int seconds = (waves.IntermissionRemainingMs + 999) / 1000;
            waveLabel.Text = waves.IsWaveActive
                ? $"WAVE {waves.CurrentWave}{Environment.NewLine}ENEMIES: {waves.GetEnemiesRemaining(session.Enemies.Count)}"
                : $"WAVE {waves.CurrentWave} CLEARED{Environment.NewLine}NEXT WAVE: {seconds}s";
        }

        public void UpdateUpgrades()
        {
            var version = (session.Upgrades.Version, session.Abilities.HudVersion);
            if (lastUpgrades == version)
                return;
            lastUpgrades = version;
            upgradesList.BeginUpdate();
            upgradesList.Items.Clear();
            foreach (UpgradeType type in UpgradeSystem.Types)
            {
                if (session.Upgrades.GetStackCount(type) > 0)
                    upgradesList.Items.Add(session.Upgrades.GetStackText(type));
            }

            IReadOnlyList<MajorAbilityType> acquired = session.Abilities.AcquiredAbilities;
            if (acquired.Count > 0)
            {
                if (upgradesList.Items.Count > 0)
                    upgradesList.Items.Add(string.Empty);
                upgradesList.Items.Add("ABILITIES");
                for (int i = 0; i < acquired.Count; i++)
                {
                    MajorAbilityType ability = acquired[i];
                    string title = AbilitySystem.GetInfo(ability).Title;
                    string status = session.Abilities.GetHudStatus(ability);
                    upgradesList.Items.Add($"[{i + 1}] {title} - {status}");
                }
            }

            upgradesList.EndUpdate();
        }

        public void ShowInstantAbilityFeedback(string text)
        {
            instantFeedbackText = text;
            instantFeedbackRemainingMs = 850;
            RefreshActiveAbilityFeedback();
        }

        public void UpdateActiveAbilityFeedback(int elapsedMs)
        {
            if (instantFeedbackRemainingMs > 0)
            {
                instantFeedbackRemainingMs = Math.Max(0, instantFeedbackRemainingMs - elapsedMs);
                if (instantFeedbackRemainingMs == 0)
                    instantFeedbackText = string.Empty;
            }
            RefreshActiveAbilityFeedback();
        }

        public void RefreshActiveAbilityFeedback()
        {
            var feedback = (session.Abilities.HudVersion, instantFeedbackText);
            if (lastAbilityFeedback != feedback)
            {
                lastAbilityFeedback = feedback;
                List<string> lines = new();
                foreach (MajorAbilityType ability in session.Abilities.AcquiredAbilities)
                {
                    int remainingMs = session.Abilities.GetRemainingDurationMs(ability);
                    if (remainingMs <= 0)
                        continue;
                    string title = AbilitySystem.GetInfo(ability).Title;
                    int seconds = Math.Max(1, (int)Math.Ceiling(remainingMs / 1000d));
                    lines.Add($"{title} ACTIVE - {seconds}s");
                }
                if (instantFeedbackRemainingMs > 0 && !string.IsNullOrEmpty(instantFeedbackText))
                    lines.Add(instantFeedbackText);
                activeAbilityLabel.Text = string.Join(Environment.NewLine, lines);
                activeAbilityPanel.Height = activeAbilityLabel.PreferredHeight + 8;
                Layout(waveLabel.Parent!.ClientSize);
            }
            activeAbilityPanel.Visible = activeAbilityLabel.Text.Length > 0 && waveLabel.Visible;
        }

        public void ResetActiveAbilityFeedback()
        {
            lastAbilityFeedback = null;
            instantFeedbackRemainingMs = 0;
            instantFeedbackText = string.Empty;
            activeAbilityLabel.Text = string.Empty;
            activeAbilityPanel.Visible = false;
        }

        public void SetVisible(bool visible)
        {
            foreach (Control control in hudControls)
                control.Visible = visible;
            if (!visible)
            {
                activeAbilityPanel.Visible = false;
                debugPanel.Visible = false;
            }
            else if (debugPanel.Visible)
                debugPanel.BringToFront();
        }

        public void Layout(Size size)
        {
            const int gap = 8;
            int margin = healthLabel.Left;
            waveLabel.Left = Math.Max(0, (size.Width - waveLabel.Width) / 2);
            waveLabel.Top = healthLabel.Top;
            if (waveLabel.Left < pauseButton.Right + gap)
                waveLabel.Top = Math.Max(pauseButton.Bottom, healthFill.Parent!.Bottom) + gap;
            if (waveLabel.Right > upgradesList.Left - gap)
                waveLabel.Top = Math.Max(waveLabel.Top, upgradesList.Bottom + gap);

            debugPanel.Top = Math.Max(0, Math.Min(upgradesList.Bottom + gap,
                size.Height - debugPanel.Height - margin));
            activeAbilityPanel.Top = waveLabel.Bottom + gap;
            int left = (size.Width - activeAbilityPanel.Width) / 2;
            // Keep ability text beside the sidebar and Debug when the window is narrow.
            if (activeAbilityPanel.Top < upgradesList.Bottom)
                left = Math.Min(left, upgradesList.Left - activeAbilityPanel.Width - gap);
            if (debugPanel.Visible)
                left = Math.Min(left, debugPanel.Left - activeAbilityPanel.Width - gap);
            activeAbilityPanel.Left = Math.Max(margin, left);
        }
    }

    public sealed class LevelUpController
    {
        private readonly GameSession session;
        private readonly HudController hud;
        private readonly ScreenController screens;
        private readonly Label title;
        private readonly Button[] buttons;
        private List<UpgradeType> upgradeChoices = new();
        private List<MajorAbilityType> abilityChoices = new();
        private bool isMajorAbility;

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
            if (session.PendingLevels.Count == 0 || session.Player.IsDead)
            {
                screens.CloseLevelUp(session.Player.IsDead);
                return;
            }
            int level = session.PendingLevels.Peek();
            // Every tenth level offers an unowned ability, then falls back to upgrades.
            abilityChoices = level % 10 == 0 ? session.Abilities.GetRandomChoices() : new();
            isMajorAbility = abilityChoices.Count > 0;
            upgradeChoices = isMajorAbility ? new() : session.Upgrades.GetRandomUpgrades();
            title.Text = isMajorAbility
                ? $"LEVEL {level} — CHOOSE AN ABILITY"
                : $"LEVEL {level} — CHOOSE AN UPGRADE";
            for (int index = 0; index < buttons.Length; index++)
            {
                Button button = buttons[index];
                int choiceCount = isMajorAbility ? abilityChoices.Count : upgradeChoices.Count;
                button.Visible = index < choiceCount;
                if (index >= choiceCount)
                    continue;
                if (isMajorAbility)
                {
                    var definition = AbilitySystem.GetInfo(abilityChoices[index]);
                    button.Text = $"{definition.Title}{Environment.NewLine}{definition.Description}";
                }
                else
                {
                    UpgradeType choice = upgradeChoices[index];
                    (string name, string description) = UpgradeSystem.GetUpgradeInfo(choice);
                    button.Text = $"{name}{Environment.NewLine}{description}   Owned: {session.Upgrades.GetStackCount(choice)}";
                }
            }
            screens.ShowLevelUp();
        }

        public bool Choose(int index, bool infiniteXp)
        {
            int count = isMajorAbility ? abilityChoices.Count : upgradeChoices.Count;
            if (screens.State != GameState.LevelUp || session.PendingLevels.Count == 0 || index < 0 || index >= count)
                return false;
            if (isMajorAbility)
                session.Abilities.Acquire(abilityChoices[index]);
            else
                session.Upgrades.ApplyUpgrade(upgradeChoices[index], session.Player);
            session.PendingLevels.Dequeue();
            if (infiniteXp)
                session.GrantNextLevel();
            hud.UpdateHealth();
            hud.UpdateExperience();
            hud.UpdateUpgrades();
            ShowNext();
            return true;
        }
    }
}
