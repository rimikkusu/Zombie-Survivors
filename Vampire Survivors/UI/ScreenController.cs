namespace Vampire_Survivors.UI
{
    public enum GameState { MainMenu, Playing, LevelUp, Paused, GameOver, Settings }

    public sealed class ScreenController
    {
        private static readonly Color ButtonNormalBack = Color.FromArgb(28, 28, 32);
        private static readonly Color ButtonHoverBack = Color.FromArgb(46, 46, 54);
        private readonly Form form;
        private readonly HudController hud;
        private readonly Panel mainMenu;
        private readonly Panel menuContent;
        private readonly Panel settings;
        private readonly Panel settingsContent;
        private readonly Panel gameOver;
        private readonly Panel levelUp;
        private readonly Panel pause;
        private readonly PictureBox logo;
        private readonly Label waveLabel;
        private readonly Panel activeAbility;
        private readonly System.Windows.Forms.Timer menuAnimationTimer;
        private readonly Rectangle logoBaseBounds;
        private float logoScale = 1f;
        private float logoScaleDirection = 1f;
        private GameState settingsReturnState = GameState.MainMenu;

        public GameState State { get; private set; } = GameState.MainMenu;

        public ScreenController(Form form, HudController hud, Panel mainMenu, Panel menuContent,
            Panel settings, Panel settingsContent, Panel gameOver, Panel levelUp, Panel pause,
            PictureBox logo, Label waveLabel, Panel activeAbility,
            System.Windows.Forms.Timer menuAnimationTimer, IEnumerable<Button> hoverButtons)
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
            this.logo = logo;
            this.waveLabel = waveLabel;
            this.activeAbility = activeAbility;
            this.menuAnimationTimer = menuAnimationTimer;
            logoBaseBounds = logo.Bounds;

            foreach (Control control in new Control[] { mainMenu, menuContent, settings, settingsContent, logo })
                EnableDoubleBuffering(control);
            foreach (Button button in hoverButtons)
            {
                button.MouseEnter += Button_MouseEnter;
                button.MouseLeave += Button_MouseLeave;
            }
        }

        private static void EnableDoubleBuffering(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(control, true, null);
        }

        private static void Button_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is Button button)
                button.BackColor = ButtonHoverBack;
        }

        private static void Button_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is Button button)
                button.BackColor = ButtonNormalBack;
        }

        public void ShowMainMenu()
        {
            State = GameState.MainMenu;
            hud.SetVisible(false);
            gameOver.Visible = false;
            settings.Visible = false;
            pause.Visible = false;
            levelUp.Visible = false;
            mainMenu.Visible = true;
            mainMenu.BringToFront();
            RecenterLayout();
            menuAnimationTimer.Start();
        }

        public void StartGame()
        {
            hud.SetVisible(true);
            mainMenu.Visible = false;
            settings.Visible = false;
            StopMenuAnimation();
        }

        public void SetPlaying()
        {
            levelUp.Visible = false;
            pause.Visible = false;
            gameOver.Visible = false;
            State = GameState.Playing;
        }

        public bool Pause()
        {
            if (State != GameState.Playing)
                return false;
            State = GameState.Paused;
            pause.Visible = true;
            pause.BringToFront();
            RecenterLayout();
            return true;
        }

        public bool Resume()
        {
            if (State != GameState.Paused)
                return false;
            pause.Visible = false;
            State = GameState.Playing;
            return true;
        }

        public void ShowSettingsFromMenu()
        {
            settingsReturnState = GameState.MainMenu;
            State = GameState.Settings;
            hud.SetVisible(false);
            StopMenuAnimation();
            mainMenu.Visible = false;
            settings.Visible = true;
            settings.BringToFront();
            RecenterLayout();
        }

        public bool ShowSettingsFromPause()
        {
            if (State != GameState.Paused)
                return false;
            settingsReturnState = GameState.Paused;
            State = GameState.Settings;
            pause.Visible = false;
            settings.Visible = true;
            settings.BringToFront();
            RecenterLayout();
            return true;
        }

        public void ReturnFromPauseSettings()
        {
            settings.Visible = false;
            State = GameState.Paused;
            pause.Visible = true;
            pause.BringToFront();
            RecenterLayout();
        }

        public bool SettingsReturnToPause => settingsReturnState == GameState.Paused;

        public void ShowLevelUp()
        {
            State = GameState.LevelUp;
            levelUp.Visible = true;
            levelUp.BringToFront();
            RecenterLayout();
        }

        public void CloseLevelUp(bool dead)
        {
            levelUp.Visible = false;
            State = dead ? GameState.GameOver : GameState.Playing;
        }

        public void ShowGameOver()
        {
            State = GameState.GameOver;
            pause.Visible = false;
            levelUp.Visible = false;
            gameOver.Visible = true;
            gameOver.BringToFront();
            RecenterLayout();
        }

        public void AnimateLogo()
        {
            if (State != GameState.MainMenu)
                return;
            logoScale += 0.0015f * logoScaleDirection;
            if (logoScale >= 1.04f)
            {
                logoScale = 1.04f;
                logoScaleDirection = -1f;
            }
            else if (logoScale <= 0.96f)
            {
                logoScale = 0.96f;
                logoScaleDirection = 1f;
            }
            int width = (int)Math.Round(logoBaseBounds.Width * logoScale);
            int height = (int)Math.Round(logoBaseBounds.Height * logoScale);
            int x = logoBaseBounds.X - (width - logoBaseBounds.Width) / 2;
            int y = logoBaseBounds.Y - (height - logoBaseBounds.Height) / 2;
            logo.SetBounds(x, y, width, height);
        }

        private void StopMenuAnimation()
        {
            menuAnimationTimer.Stop();
            logo.Bounds = logoBaseBounds;
            logoScale = 1f;
            logoScaleDirection = 1f;
        }

        public void RecenterLayout()
        {
            Center(menuContent, mainMenu.ClientSize);
            Center(settingsContent, settings.ClientSize);
            Center(gameOver, form.ClientSize);
            Center(levelUp, form.ClientSize);
            Center(pause, form.ClientSize);
            waveLabel.Left = Math.Max(0, (form.ClientSize.Width - waveLabel.Width) / 2);
            activeAbility.Left = Math.Max(0, (form.ClientSize.Width - activeAbility.Width) / 2);
            activeAbility.Top = waveLabel.Bottom + 4;
        }

        private static void Center(Control control, Size available)
        {
            control.Location = new Point(Math.Max(0, (available.Width - control.Width) / 2),
                Math.Max(0, (available.Height - control.Height) / 2));
        }
    }
}
