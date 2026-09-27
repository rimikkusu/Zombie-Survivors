using Vampire_Survivors.Systems;

namespace Vampire_Survivors.UI
{
    public sealed class GameInputController
    {
        private readonly Form form;
        private readonly GameSession session;
        private readonly ScreenController screens;
        private readonly HudController hud;
        private readonly LevelUpController levels;
        private readonly DebugController debug;
        private readonly CheckBox noAbilityCooldowns;
        private readonly Action pause;
        private readonly Action resume;

        public GameInputController(Form form, GameSession session, ScreenController screens,
            HudController hud, LevelUpController levels, DebugController debug,
            CheckBox noAbilityCooldowns, Action pause, Action resume)
        {
            this.form = form;
            this.session = session;
            this.screens = screens;
            this.hud = hud;
            this.levels = levels;
            this.debug = debug;
            this.noAbilityCooldowns = noAbilityCooldowns;
            this.pause = pause;
            this.resume = resume;
        }

        public void KeyDown(KeyEventArgs e)
        {
            bool isNewPress = session.Input.KeyDown(e.KeyCode);
            if (e.KeyCode == Keys.F3)
            {
                if (isNewPress)
                    debug.Toggle();
                Consume(e);
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                if (isNewPress && screens.State == GameState.Playing)
                    pause();
                else if (isNewPress && screens.State == GameState.Paused)
                    resume();
                else if (isNewPress && screens.State == GameState.Settings && screens.SettingsReturnToPause)
                    screens.ReturnFromPauseSettings();
                Consume(e);
                return;
            }

            if (screens.State == GameState.Playing && !session.Player.IsDead && isNewPress &&
                InputManager.TryGetAbilitySlot(e.KeyCode, out int slot))
            {
                ActivateAbilityAtSlot(slot);
                Consume(e);
            }
        }

        private static void Consume(KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        public void KeyUp(KeyEventArgs e) => session.Input.KeyUp(e.KeyCode);

        private void ActivateAbilityAtSlot(int slot)
        {
            Point cursor = form.PointToClient(Cursor.Position);
            int previousLevel = session.Player.Level;
            int previousExperience = session.Player.Experience;
            MajorAbilityType? ability = session.ActivateAbility(slot, cursor, noAbilityCooldowns.Checked);
            if (ability is null)
                return;

            if (session.Player.Level != previousLevel || session.Player.Experience != previousExperience)
                hud.UpdateExperience();
            if (session.LevelUps.HasPending && screens.State == GameState.Playing)
                levels.ShowNext();
            if (ability == MajorAbilityType.Shockwave)
                hud.UpdateWave();

            if (session.Abilities.GetRemainingDurationMs(ability.Value) > 0)
                hud.RefreshActiveAbilityFeedback();
            else
                hud.ShowInstantAbilityFeedback($"{AbilitySystem.GetInfo(ability.Value).Title}!");
            hud.UpdateUpgrades();
            form.Invalidate();
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (debug.Contains(e.Location) || e.Button != MouseButtons.Left ||
                screens.State != GameState.Playing || session.Player.IsDead)
                return;

            int previousLevel = session.Player.Level;
            int previousExperience = session.Player.Experience;
            session.Shoot(e.Location);
            if (session.Player.Level != previousLevel || session.Player.Experience != previousExperience)
            {
                hud.UpdateExperience();
                if (session.LevelUps.HasPending && screens.State == GameState.Playing)
                    levels.ShowNext();
            }
            debug.UpdateInfo();
        }
    }
}
