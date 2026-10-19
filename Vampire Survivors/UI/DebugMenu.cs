using Vampire_Survivors.Systems;

namespace Vampire_Survivors.UI
{
    public sealed class DebugController
    {
        private readonly GameSession session;
        private readonly HudController hud;
        private readonly LevelUpController levels;
        private readonly ScreenController screens;
        private readonly Panel panel;
        private readonly Label information;
        private readonly CheckBox infiniteHealth;
        private readonly Action repaint;

        public DebugController(GameSession session, HudController hud, LevelUpController levels,
            ScreenController screens, Panel panel, Label information, CheckBox infiniteHealth,
            Action repaint)
        {
            this.session = session;
            this.hud = hud;
            this.levels = levels;
            this.screens = screens;
            this.panel = panel;
            this.information = information;
            this.infiniteHealth = infiniteHealth;
            this.repaint = repaint;
        }

        public void Toggle()
        {
            if (screens.State is GameState.MainMenu or GameState.Settings)
                return;
            panel.Visible = !panel.Visible;
            hud.Layout(panel.Parent!.ClientSize);
            if (panel.Visible)
            {
                UpdateInfo();
                panel.BringToFront();
            }
        }

        public bool Contains(Point location) => panel.Visible && panel.Bounds.Contains(location);

        public void SyncInfiniteHealth()
        {
            if (infiniteHealth.Checked && !session.Player.IsDead)
            {
                session.HealToFull();
                hud.UpdateHealth();
            }
            UpdateInfo();
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || session.Player.IsDead)
                return;
            session.AddExperience(amount);
            hud.UpdateExperience();
            if (session.PendingLevels.Count > 0 && screens.State == GameState.Playing)
                levels.ShowNext();
            UpdateInfo();
            repaint();
        }

        public void GrantNextLevel() => AddExperience(
            Math.Max(0, session.Player.ExperienceToNextLevel - session.Player.Experience));

        public void ClearWave(bool advance)
        {
            session.ClearDebugWave(advance);
            hud.UpdateWave();
            UpdateInfo();
            repaint();
        }

        public void Heal()
        {
            if (session.HealToFull() > 0)
                hud.UpdateHealth();
            UpdateInfo();
        }

        public void UpdateInfo()
        {
            if (!panel.Visible)
                return;
            string text =
                $"LEVEL: {session.Player.Level}{Environment.NewLine}" +
                $"XP: {session.Player.Experience} / {session.Player.ExperienceToNextLevel}{Environment.NewLine}" +
                $"WAVE: {session.Waves.CurrentWave}{Environment.NewLine}" +
                $"HP: {session.Player.Health} / {session.Player.MaxHealth}{Environment.NewLine}" +
                $"ENEMIES: {session.Enemies.Count}";
            information.Text = text;
        }
    }
}
