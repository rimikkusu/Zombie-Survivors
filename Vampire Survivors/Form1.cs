using Vampire_Survivors.Rendering;
using Vampire_Survivors.Systems;
using Vampire_Survivors.UI;

namespace Vampire_Survivors
{
    public partial class Form1 : Form
    {
        private readonly GameSession session;
        private readonly GameRenderer renderer;
        private readonly HudController hud;
        private readonly LevelUpController levels;
        private readonly DebugController debug;
        private readonly GameInputController gameInput;
        private readonly System.Windows.Forms.Timer gameTimer;
        private readonly ScreenController screens;
        private readonly DisplayManager display;
        private bool isCleanedUp;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;

            Control[] foregroundControls = [pnlHealthBackground, lblHealth, lblLevel, pnlXpBackground,
                lblXp, lblWave, lblUpgradesTitle, lstUpgrades, btnPause, pnlDebug];
            foreach (Control control in foregroundControls)
                control.BringToFront();

            session = new GameSession(ClientSize);
            renderer = new GameRenderer();
            hud = new HudController(session, lblHealth, pnlHealthBackground, pnlHealthFill,
                lblLevel, lblXp, pnlXpBackground, pnlXpFill, lblWave, lblUpgradesTitle,
                lstUpgrades, btnPause, pnlDebug);
            screens = new ScreenController(this, hud, pnlMainMenu, pnlMenuContent, pnlSettings,
                pnlSettingsContent, pnlGameOver, pnlLevelUp, pnlPause);
            display = new DisplayManager(this, cmbDisplayMode);
            levels = new LevelUpController(session, hud, screens, lblLevelUpTitle,
                btnUpgrade1, btnUpgrade2, btnUpgrade3);
            debug = new DebugController(session, hud, levels, screens, pnlDebug, lblDebugInfo,
                chkInfiniteHealth, Invalidate);
            gameInput = new GameInputController(session, screens, hud, levels, debug,
                PauseGame, ResumeGame);
            hud.UpdateHealth();
            hud.UpdateExperience();

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            MouseDown += Form1_MouseDown;
            Resize += Form1_Resize;
            SizeChanged += Form1_SizeChanged;
            Deactivate += (_, _) => gameInput.Reset();

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameTimer_Tick;

            ShowMainMenu();

            Disposed += (_, _) => Cleanup();
        }

        public void ApplyDisplayMode(DisplayMode mode) => display.Apply(mode);

        private void Form1_Resize(object? sender, EventArgs e)
        {
            screens.RecenterLayout();
            session.Resize(ClientSize);
        }

        private void Form1_SizeChanged(object? sender, EventArgs e) => display.HandleSizeChanged();

        private void Form1_KeyDown(object? sender, KeyEventArgs e) => gameInput.KeyDown(e);
        private void Form1_KeyUp(object? sender, KeyEventArgs e) => gameInput.KeyUp(e);
        private void Form1_MouseDown(object? sender, MouseEventArgs e) => gameInput.MouseDown(e);

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (screens.State != GameState.Playing)
                return;

            if (!session.Player.IsDead)
            {
                session.Update(PointToClient(Cursor.Position),
                    gameInput.GetMovementDirection(), chkInfiniteHealth.Checked);
                bool died = hud.RefreshFrame();
                if (session.PendingLevels.Count > 0)
                    levels.ShowNext();
                if (died)
                    screens.ShowGameOver();
            }

            debug.UpdateInfo();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            renderer.Draw(e.Graphics, ClientSize, session);
        }

        private void BtnPlay_Click(object? sender, EventArgs e) => StartNewGame();
        private void BtnSettings_Click(object? sender, EventArgs e) => screens.ShowSettingsFromMenu();
        private void BtnSettingsBack_Click(object? sender, EventArgs e)
        {
            if (screens.SettingsReturnToPause)
                screens.ReturnFromPauseSettings();
            else
                ShowMainMenu();
        }
        private void BtnApplySettings_Click(object? sender, EventArgs e) => display.ApplySelectedMode();
        private void BtnQuit_Click(object? sender, EventArgs e) => Application.Exit();
        private void BtnPause_Click(object? sender, EventArgs e) => PauseGame();
        private void BtnResume_Click(object? sender, EventArgs e) => ResumeGame();
        private void BtnPauseSettings_Click(object? sender, EventArgs e) => screens.ShowSettingsFromPause();

        private void PauseGame()
        {
            if (screens.Pause())
                gameInput.ResetMovement();
        }

        private void ResumeGame()
        {
            if (!screens.Resume())
                return;
            gameInput.ResetMovement();
            if (session.PendingLevels.Count > 0)
                levels.ShowNext();
        }

        private void BtnPauseMainMenu_Click(object? sender, EventArgs e)
        {
            gameTimer.Stop();
            RestartGame();
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            gameTimer.Stop();
            screens.ShowMainMenu();
        }

        private void StartNewGame()
        {
            RestartGame();
            gameTimer.Start();
        }

        private void ChkInfiniteHealth_CheckedChanged(object? sender, EventArgs e) => debug.SyncInfiniteHealth();
        private void BtnDebugAddXp_Click(object? sender, EventArgs e) => debug.AddExperience(100);
        private void BtnDebugLevelUp_Click(object? sender, EventArgs e) => debug.GrantNextLevel();
        private void BtnDebugNextWave_Click(object? sender, EventArgs e) => debug.ClearWave(true);
        private void BtnDebugKillEnemies_Click(object? sender, EventArgs e) => debug.ClearWave(false);
        private void BtnDebugHeal_Click(object? sender, EventArgs e) => debug.Heal();

        private void BtnRespawn_Click(object? sender, EventArgs e) => RestartGame();

        private void RestartGame()
        {
            // The form may finish maximizing after the session was created.
            session.Resize(ClientSize);
            session.Reset();
            gameInput.Reset();
            screens.SetPlaying();
            hud.UpdateHealth();
            hud.UpdateExperience();
            hud.UpdateUpgrades();
            hud.UpdateWave();
            debug.UpdateInfo();
            Invalidate();
        }

        private void ChooseLevelUpOption(int index)
        {
            if (levels.Choose(index, chkInfiniteXp.Checked))
                debug.UpdateInfo();
        }

        private void BtnUpgrade1_Click(object? sender, EventArgs e) => ChooseLevelUpOption(0);
        private void BtnUpgrade2_Click(object? sender, EventArgs e) => ChooseLevelUpOption(1);
        private void BtnUpgrade3_Click(object? sender, EventArgs e) => ChooseLevelUpOption(2);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Cleanup();
            base.OnFormClosed(e);
        }

        private void Cleanup()
        {
            if (isCleanedUp)
                return;

            isCleanedUp = true;

            gameTimer.Stop();
            gameTimer.Tick -= GameTimer_Tick;
            gameTimer.Dispose();

            renderer.Dispose();
        }
    }
}
