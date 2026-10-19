using Vampire_Survivors.Systems;

namespace Vampire_Survivors.UI
{
    public sealed class GameInputController
    {
        private readonly HashSet<Keys> keysDown = new();
        private readonly GameSession session;
        private readonly ScreenController screens;
        private readonly HudController hud;
        private readonly LevelUpController levels;
        private readonly DebugController debug;
        private readonly Action pause;
        private readonly Action resume;

        public GameInputController(GameSession session, ScreenController screens,
            HudController hud, LevelUpController levels, DebugController debug,
            Action pause, Action resume)
        {
            this.session = session;
            this.screens = screens;
            this.hud = hud;
            this.levels = levels;
            this.debug = debug;
            this.pause = pause;
            this.resume = resume;
        }

        public void KeyDown(KeyEventArgs e)
        {
            // Holding a key should not repeatedly toggle Pause or the debug panel.
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
