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
        private readonly int healthFillMaxWidth;
        private readonly Label levelLabel;
        private readonly Label xpLabel;
        private readonly Panel xpFill;
        private readonly int xpFillMaxWidth;
        private readonly Label waveLabel;
        private readonly ListBox upgradesList;
        private readonly Panel activeAbilityPanel;
        private readonly Label activeAbilityLabel;
        private readonly Panel debugPanel;

        private (int Wave, bool Active, int Remaining, int Seconds)? lastWave;
        private (int Upgrades, int Abilities)? lastUpgrades;
        private (int Version, string Instant)? lastAbilityFeedback;
        private int instantFeedbackRemainingMs;
        private string instantFeedbackText = string.Empty;

        public int LastHealth { get; private set; }

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
            healthFillMaxWidth = healthBackground.ClientSize.Width;
            xpFillMaxWidth = xpBackground.ClientSize.Width;
            hudControls = [healthLabel, healthBackground, levelLabel, xpLabel, xpBackground,
                waveLabel, upgradesTitle, upgradesList, pauseButton];
        }

        public bool UpdateHealth()
        {
            Player player = session.Player;
            LastHealth = player.Health;
            float fraction = player.MaxHealth > 0
                ? player.Health / (float)player.MaxHealth : 0f;
            fraction = Math.Clamp(fraction, 0f, 1f);
            healthFill.Width = Math.Clamp((int)(healthFillMaxWidth * fraction), 0, healthFillMaxWidth);
            healthLabel.Text = $"HP {player.Health} / {player.MaxHealth}";
            return player.IsDead;
        }

        public bool RefreshFrame(int elapsedMs, bool experienceChanged)
        {
            UpdateActiveAbilityFeedback(elapsedMs);
            UpdateUpgrades();
            if (experienceChanged)
                UpdateExperience();
            bool died = session.Player.Health != LastHealth && UpdateHealth();
            UpdateWave();
            return died;
        }

        public void UpdateExperience()
        {
            Player player = session.Player;
            float fraction = player.ExperienceToNextLevel > 0
                ? player.Experience / (float)player.ExperienceToNextLevel : 0f;
            fraction = Math.Clamp(fraction, 0f, 1f);
            xpFill.Width = (int)(xpFillMaxWidth * fraction);
            levelLabel.Text = $"LEVEL {player.Level}";
            xpLabel.Text = $"{player.Experience} / {player.ExperienceToNextLevel} XP";
        }

        public void UpdateWave()
        {
            WaveManager waves = session.Waves;
            var values = (Wave: waves.CurrentWave, Active: waves.IsWaveActive,
                Remaining: waves.GetEnemiesRemaining(session.Enemies.Count),
                Seconds: Math.Max(1, (int)Math.Ceiling(waves.IntermissionRemainingMs / 1000f)));
            if (lastWave == values)
                return;
            lastWave = values;
            waveLabel.Text = values.Active
                ? $"WAVE {values.Wave}{Environment.NewLine}ENEMIES: {values.Remaining}"
                : $"WAVE {values.Wave} CLEARED{Environment.NewLine}NEXT WAVE: {values.Seconds}s";
        }

        public void UpdateUpgrades()
        {
            var version = (session.Upgrades.Version, session.Abilities.HudVersion);
            if (lastUpgrades == version)
                return;
            lastUpgrades = version;
            List<string> entries = new();
            foreach (UpgradeType type in UpgradeSystem.Types)
            {
                if (session.Upgrades.GetStackCount(type) > 0)
                    entries.Add(session.Upgrades.GetStackText(type));
            }

            IReadOnlyList<MajorAbilityType> acquired = session.Abilities.AcquiredAbilities;
            if (acquired.Count > 0)
            {
                if (entries.Count > 0)
                    entries.Add(string.Empty);
                entries.Add("ABILITIES");
                for (int i = 0; i < acquired.Count; i++)
                {
                    MajorAbilityType ability = acquired[i];
                    string title = AbilitySystem.GetInfo(ability).Title;
                    string status = session.Abilities.GetHudStatus(ability);
                    entries.Add($"[{i + 1}] {title} - {status}");
                }
            }

            bool changed = entries.Count != upgradesList.Items.Count;
            for (int index = 0; !changed && index < entries.Count; index++)
                changed = entries[index] != upgradesList.Items[index]?.ToString();
            if (!changed)
                return;

            upgradesList.BeginUpdate();
            upgradesList.Items.Clear();
            foreach (string entry in entries)
                upgradesList.Items.Add(entry);
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
            }
            activeAbilityPanel.Visible = activeAbilityLabel.Text.Length > 0 && waveLabel.Visible;
            if (activeAbilityPanel.Visible)
                activeAbilityPanel.BringToFront();
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
        }
    }
}
