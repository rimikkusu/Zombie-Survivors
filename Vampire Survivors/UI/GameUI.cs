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
        private readonly Panel debugPanel;
        private readonly Button pauseButton;

        private int lastUpgradeVersion = -1;

        public HudController(GameSession session, Label healthLabel, Panel healthBackground,
            Panel healthFill, Label levelLabel, Label xpLabel, Panel xpBackground,
            Panel xpFill, Label waveLabel, Label upgradesTitle, ListBox upgradesList,
            Button pauseButton, Panel debugPanel)
        {
            this.session = session;
            this.healthLabel = healthLabel;
            this.healthFill = healthFill;
            this.levelLabel = levelLabel;
            this.xpLabel = xpLabel;
            this.xpFill = xpFill;
            this.waveLabel = waveLabel;
            this.upgradesList = upgradesList;
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

        public bool RefreshFrame()
        {
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
            if (lastUpgradeVersion == session.Upgrades.Version)
                return;
            lastUpgradeVersion = session.Upgrades.Version;
            upgradesList.BeginUpdate();
            upgradesList.Items.Clear();
            foreach (UpgradeType type in UpgradeSystem.Types)
            {
                if (session.Upgrades.GetStackCount(type) > 0)
                    upgradesList.Items.Add(session.Upgrades.GetStackText(type));
            }

            upgradesList.EndUpdate();
        }

        public void SetVisible(bool visible)
        {
            foreach (Control control in hudControls)
                control.Visible = visible;
            if (!visible)
                debugPanel.Visible = false;
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
            // Every level offers three different normal upgrades.
            upgradeChoices = session.Upgrades.GetRandomUpgrades(buttons.Length);
            title.Text = $"LEVEL {level} — CHOOSE AN UPGRADE";
            for (int index = 0; index < buttons.Length; index++)
            {
                Button button = buttons[index];
                button.Visible = true;
                UpgradeType choice = upgradeChoices[index];
                (string name, string description) = UpgradeSystem.GetUpgradeInfo(choice);
                button.Text = $"{name}{Environment.NewLine}{description}   Owned: {session.Upgrades.GetStackCount(choice)}";
            }
            screens.ShowLevelUp();
        }

        public bool Choose(int index, bool infiniteXp)
        {
            if (screens.State != GameState.LevelUp || session.PendingLevels.Count == 0 ||
                index < 0 || index >= upgradeChoices.Count)
                return false;
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
