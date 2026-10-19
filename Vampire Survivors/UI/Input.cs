using Vampire_Survivors.Systems;

namespace Vampire_Survivors.UI
{
    public sealed class GameInputController
    {
        private readonly HashSet<Keys> keysDown = new();
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
            // Holding a key should not repeatedly toggle Pause or activate an ability.
            bool isNewPress = keysDown.Add(e.KeyCode);
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
                TryGetAbilitySlot(e.KeyCode, out int slot))
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

        public void KeyUp(KeyEventArgs e) => keysDown.Remove(e.KeyCode);

        public void Reset() => keysDown.Clear();

        public void ResetMovement()
        {
            keysDown.Remove(Keys.W);
            keysDown.Remove(Keys.A);
            keysDown.Remove(Keys.S);
            keysDown.Remove(Keys.D);
        }

        public PointF GetMovementDirection()
        {
            float x = (keysDown.Contains(Keys.D) ? 1 : 0) - (keysDown.Contains(Keys.A) ? 1 : 0);
            float y = (keysDown.Contains(Keys.S) ? 1 : 0) - (keysDown.Contains(Keys.W) ? 1 : 0);
            // Normalize diagonals so they move at the same speed as straight lines.
            if (x != 0 && y != 0)
                return new PointF(x * 0.7071f, y * 0.7071f);
            return new PointF(x, y);
        }

        private static bool TryGetAbilitySlot(Keys key, out int slot)
        {
            slot = -1;
            if (key >= Keys.D1 && key <= Keys.D9)
                slot = (int)key - (int)Keys.D1;
            else if (key >= Keys.NumPad1 && key <= Keys.NumPad9)
                slot = (int)key - (int)Keys.NumPad1;
            return slot >= 0;
        }

        private void ActivateAbilityAtSlot(int slot)
        {
            Point cursor = form.PointToClient(Cursor.Position);

            MajorAbilityType? ability = session.ActivateAbility(slot, cursor, noAbilityCooldowns.Checked);
            if (ability is null)
                return;

            hud.UpdateExperience();
            if (session.PendingLevels.Count > 0 && screens.State == GameState.Playing)
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

            session.Shoot(e.Location);
            hud.UpdateExperience();
            if (session.PendingLevels.Count > 0)
                levels.ShowNext();
            debug.UpdateInfo();
        }
    }
}
