namespace Vampire_Survivors.UI
{
    public enum GameState { MainMenu, Playing, LevelUp, Paused, GameOver, Settings }

    public sealed class ScreenController
    {
        private readonly Form form;
        private readonly HudController hud;
        private readonly Panel mainMenu;
        private readonly Panel menuContent;
        private readonly Panel settings;
        private readonly Panel settingsContent;
        private readonly Panel gameOver;
        private readonly Panel levelUp;
        private readonly Panel pause;

        public GameState State { get; private set; } = GameState.MainMenu;
        public bool SettingsReturnToPause { get; private set; }

        public ScreenController(Form form, HudController hud, Panel mainMenu, Panel menuContent,
            Panel settings, Panel settingsContent, Panel gameOver, Panel levelUp, Panel pause)
        {
            this.form = form;
            this.hud = hud;
            this.mainMenu = mainMenu;
            this.menuContent = menuContent;
            this.settings = settings;
            this.settingsContent = settingsContent;
            this.gameOver = gameOver;
            this.levelUp = levelUp;
            this.pause = pause;
        }

        private void SetState(GameState state)
        {
            // Show one menu at a time, with the gameplay HUD behind it.
            State = state;
            mainMenu.Visible = state == GameState.MainMenu;
            settings.Visible = state == GameState.Settings;
            gameOver.Visible = state == GameState.GameOver;
            levelUp.Visible = state == GameState.LevelUp;
            pause.Visible = state == GameState.Paused;
            hud.SetVisible(state != GameState.MainMenu && state != GameState.Settings);
            Control? overlay = state switch
            {
                GameState.MainMenu => mainMenu,
                GameState.Settings => settings,
                GameState.GameOver => gameOver,
                GameState.LevelUp => levelUp,
                GameState.Paused => pause,
                _ => null
            };
            overlay?.BringToFront();
            RecenterLayout();
        }

        public void ShowMainMenu() => SetState(GameState.MainMenu);
        public void SetPlaying() => SetState(GameState.Playing);
        public void ShowLevelUp() => SetState(GameState.LevelUp);
        public void ShowGameOver() => SetState(GameState.GameOver);
        public void CloseLevelUp(bool dead) => SetState(dead ? GameState.GameOver : GameState.Playing);

        public bool Pause()
        {
            if (State != GameState.Playing)
                return false;
            SetState(GameState.Paused);
            return true;
        }

        public bool Resume()
        {
            if (State != GameState.Paused)
                return false;
            SetState(GameState.Playing);
            return true;
        }

        public void ShowSettingsFromMenu()
        {
            SettingsReturnToPause = false;
            SetState(GameState.Settings);
        }

        public void ShowSettingsFromPause()
        {
            if (State != GameState.Paused)
                return;
            SettingsReturnToPause = true;
            SetState(GameState.Settings);
        }

        public void ReturnFromPauseSettings() => SetState(GameState.Paused);

        public void RecenterLayout()
        {
            Center(menuContent, mainMenu.ClientSize);
            Center(settingsContent, settings.ClientSize);
            Center(gameOver, form.ClientSize);
            Center(levelUp, form.ClientSize);
            Center(pause, form.ClientSize);
            hud.Layout(form.ClientSize);
        }

        private static void Center(Control control, Size available)
        {
            control.Location = new Point(Math.Max(0, (available.Width - control.Width) / 2),
                Math.Max(0, (available.Height - control.Height) / 2));
        }
    }

    public enum DisplayMode { Windowed, WindowedMaximized, Fullscreen }

    public sealed class DisplayManager
    {
        private readonly Form form;
        private readonly ComboBox selector;
        private DisplayMode currentMode = DisplayMode.WindowedMaximized;
        private Rectangle windowedBounds;
        private bool applying;

        public DisplayManager(Form form, ComboBox selector)
        {
            this.form = form;
            this.selector = selector;
            Rectangle work = Screen.FromControl(form).WorkingArea;
            windowedBounds = new Rectangle(work.X + work.Width / 10, work.Y + work.Height / 10,
                work.Width * 4 / 5, work.Height * 4 / 5);
            selector.SelectedIndex = (int)currentMode;
        }

        public void ApplySelectedMode()
        {
            if (selector.SelectedIndex >= 0)
                Apply((DisplayMode)selector.SelectedIndex);
        }

        public void Apply(DisplayMode mode)
        {
            applying = true;
            try
            {
                if (currentMode == DisplayMode.Windowed && form.WindowState == FormWindowState.Normal)
                    windowedBounds = form.Bounds;
                Screen screen = Screen.FromControl(form);
                form.WindowState = FormWindowState.Normal;
                form.FormBorderStyle = mode == DisplayMode.Fullscreen
                    ? FormBorderStyle.None : FormBorderStyle.Sizable;
                switch (mode)
                {
                    case DisplayMode.Windowed:
                        Rectangle work = screen.WorkingArea;
                        int width = Math.Clamp(windowedBounds.Width, Math.Min(form.MinimumSize.Width, work.Width), work.Width);
                        int height = Math.Clamp(windowedBounds.Height, Math.Min(form.MinimumSize.Height, work.Height), work.Height);
                        int x = Math.Clamp(windowedBounds.X, work.Left, work.Right - width);
                        int y = Math.Clamp(windowedBounds.Y, work.Top, work.Bottom - height);
                        form.Bounds = new Rectangle(x, y, width, height);
                        break;
                    case DisplayMode.WindowedMaximized:
                        form.WindowState = FormWindowState.Maximized;
                        break;
                    case DisplayMode.Fullscreen:
                        form.Bounds = screen.Bounds;
                        break;
                }
                currentMode = mode;
                selector.SelectedIndex = (int)mode;
            }
            finally
            {
                applying = false;
            }
        }

        public void HandleSizeChanged()
        {
            if (applying || currentMode == DisplayMode.Fullscreen || form.WindowState == FormWindowState.Minimized)
                return;
            currentMode = form.WindowState == FormWindowState.Maximized
                ? DisplayMode.WindowedMaximized : DisplayMode.Windowed;
            if (currentMode == DisplayMode.Windowed)
                windowedBounds = form.Bounds;
            selector.SelectedIndex = (int)currentMode;
        }
    }
}
