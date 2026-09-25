using Vampire_Survivors.Entities;
using Vampire_Survivors.Rendering;
using Vampire_Survivors.Systems;
using Vampire_Survivors.UI;

namespace Vampire_Survivors
{
    public partial class Form1 : Form
    {
        private enum GameState
        {
            MainMenu,
            Playing,
            GameOver,
            Settings
        }

        private readonly Player player = new();
        private readonly List<Bullet> bullets = new();
        private readonly List<Enemy> enemies = new();
        private readonly List<DamageNumber> damageNumbers = new();

        private readonly InputManager input = new();
        private readonly CombatSystem combat = new();
        private readonly EnemyManager enemyManager = new();
        private readonly GameRenderer renderer;
        private readonly LogoAnimator logoAnimator;
        private readonly MenuAnimator menuAnimator = new();

        private readonly System.Windows.Forms.Timer gameTimer;

        private readonly int healthFillMaxWidth;
        private readonly int xpFillMaxWidth;
        private int lastHealth;

        private GameState state = GameState.MainMenu;

        private DisplayMode currentDisplayMode = DisplayMode.WindowedMaximized;
        private Rectangle savedWindowedBounds;
        private bool isApplyingDisplayMode;

        private float menuSettleOffset;
        private float settingsSettleOffset;

        private bool isCleanedUp;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            KeyPreview = true;

            EnableDoubleBuffering(pnlMainMenu);
            EnableDoubleBuffering(pnlMenuContent);
            EnableDoubleBuffering(pnlSettings);
            EnableDoubleBuffering(pnlSettingsContent);
            EnableDoubleBuffering(picLogo);

            // Establish sensible initial windowed bounds centered on work area
            Rectangle workArea = Screen.FromControl(this).WorkingArea;
            int initialW = Math.Min(1280, (int)(workArea.Width * 0.85f));
            int initialH = Math.Min(720, (int)(workArea.Height * 0.85f));
            int initialX = workArea.X + (workArea.Width - initialW) / 2;
            int initialY = workArea.Y + (workArea.Height - initialH) / 2;
            savedWindowedBounds = new Rectangle(initialX, initialY, initialW, initialH);

            healthFillMaxWidth = pnlHealthBackground.ClientSize.Width;
            xpFillMaxWidth = pnlXpBackground.ClientSize.Width;
            lastHealth = player.Health;

            pnlHealthBackground.BringToFront();
            lblHealth.BringToFront();
            lblLevel.BringToFront();
            pnlXpBackground.BringToFront();
            lblXp.BringToFront();

            UpdateHealthUI();
            UpdateExperienceUI();

            player.X = (ClientSize.Width - Player.Width) / 2f;
            player.Y = (ClientSize.Height - Player.Height) / 2f;

            renderer = new GameRenderer();

            logoAnimator = new LogoAnimator(picLogo, Properties.Resources.Logo);

            // Configure ComboBox appearance and owner-draw
            cmbDisplayMode.DrawMode = DrawMode.OwnerDrawFixed;
            cmbDisplayMode.ItemHeight = 24;
            cmbDisplayMode.DrawItem += CmbDisplayMode_DrawItem;
            SyncDisplayModeComboBox();

            // Register menu buttons in shared animator
            menuAnimator.RegisterButton(btnPlay);
            menuAnimator.RegisterButton(btnSettings);
            menuAnimator.RegisterButton(btnQuit);
            menuAnimator.RegisterButton(btnApplySettings);
            menuAnimator.RegisterButton(btnSettingsBack);

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            MouseMove += Form1_MouseMove;
            MouseDown += Form1_MouseDown;
            Resize += Form1_Resize;
            SizeChanged += Form1_SizeChanged;

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameTimer_Tick;

            ShowMainMenu();

            FormClosed += (_, _) => Cleanup();
            Disposed += (_, _) => Cleanup();
        }

        private static void EnableDoubleBuffering(Control? control)
        {
            if (control is null)
                return;

            typeof(Control).GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            )?.SetValue(control, true, null);
        }

        public void ApplyDisplayMode(DisplayMode mode)
        {
            isApplyingDisplayMode = true;
            try
            {
                // Preserve valid normal windowed bounds before leaving Windowed mode
                if (currentDisplayMode == DisplayMode.Windowed && WindowState == FormWindowState.Normal)
                {
                    savedWindowedBounds = Bounds;
                }

                switch (mode)
                {
                    case DisplayMode.Windowed:
                        if (WindowState == FormWindowState.Maximized)
                        {
                            WindowState = FormWindowState.Normal;
                        }

                        FormBorderStyle = FormBorderStyle.Sizable;
                        MaximizeBox = true;

                        Rectangle workArea = Screen.FromControl(this).WorkingArea;
                        int w = Math.Clamp(savedWindowedBounds.Width, 640, workArea.Width);
                        int h = Math.Clamp(savedWindowedBounds.Height, 480, workArea.Height);
                        int x = Math.Clamp(savedWindowedBounds.X, workArea.Left, Math.Max(workArea.Left, workArea.Right - w));
                        int y = Math.Clamp(savedWindowedBounds.Y, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - h));
                        savedWindowedBounds = new Rectangle(x, y, w, h);

                        Bounds = savedWindowedBounds;
                        WindowState = FormWindowState.Normal;
                        break;

                    case DisplayMode.WindowedMaximized:
                        if (FormBorderStyle != FormBorderStyle.Sizable)
                        {
                            FormBorderStyle = FormBorderStyle.Sizable;
                        }
                        MaximizeBox = true;

                        if (WindowState != FormWindowState.Maximized)
                        {
                            WindowState = FormWindowState.Maximized;
                        }
                        break;

                    case DisplayMode.Fullscreen:
                        // WinForms requires normalizing WindowState before removing borders for reliable fullscreen
                        if (WindowState == FormWindowState.Maximized)
                        {
                            WindowState = FormWindowState.Normal;
                        }

                        FormBorderStyle = FormBorderStyle.None;
                        WindowState = FormWindowState.Normal;

                        Screen screen = Screen.FromControl(this);
                        Bounds = screen.Bounds;
                        break;
                }

                currentDisplayMode = mode;
                SyncDisplayModeComboBox();
                RecenterLayout();
            }
            finally
            {
                isApplyingDisplayMode = false;
            }
        }

        private void SyncDisplayModeComboBox()
        {
            string target = currentDisplayMode switch
            {
                DisplayMode.Windowed => "Windowed",
                DisplayMode.WindowedMaximized => "Windowed Maximized",
                DisplayMode.Fullscreen => "Fullscreen",
                _ => "Windowed Maximized"
            };

            if (cmbDisplayMode.SelectedItem as string != target)
            {
                cmbDisplayMode.SelectedItem = target;
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            RecenterLayout();
        }

        private void Form1_SizeChanged(object? sender, EventArgs e)
        {
            if (!isApplyingDisplayMode)
            {
                if (WindowState == FormWindowState.Maximized && currentDisplayMode != DisplayMode.Fullscreen)
                {
                    currentDisplayMode = DisplayMode.WindowedMaximized;
                    SyncDisplayModeComboBox();
                }
                else if (WindowState == FormWindowState.Normal && currentDisplayMode == DisplayMode.WindowedMaximized)
                {
                    currentDisplayMode = DisplayMode.Windowed;
                    savedWindowedBounds = Bounds;
                    SyncDisplayModeComboBox();
                }
                else if (WindowState == FormWindowState.Normal && currentDisplayMode == DisplayMode.Windowed)
                {
                    savedWindowedBounds = Bounds;
                }
            }

            RecenterLayout();
        }

        private void RecenterLayout()
        {
            if (pnlMenuContent != null && pnlMainMenu != null)
            {
                int x = Math.Max(0, (pnlMainMenu.ClientSize.Width - pnlMenuContent.Width) / 2);
                int y = Math.Max(0, (pnlMainMenu.ClientSize.Height - pnlMenuContent.Height) / 2 + (int)Math.Round(menuSettleOffset));
                pnlMenuContent.Location = new Point(x, y);
            }

            if (pnlSettingsContent != null && pnlSettings != null)
            {
                int x = Math.Max(0, (pnlSettings.ClientSize.Width - pnlSettingsContent.Width) / 2);
                int y = Math.Max(0, (pnlSettings.ClientSize.Height - pnlSettingsContent.Height) / 2 + (int)Math.Round(settingsSettleOffset));
                pnlSettingsContent.Location = new Point(x, y);
            }

            if (pnlGameOver != null)
            {
                int x = Math.Max(0, (ClientSize.Width - pnlGameOver.Width) / 2);
                int y = Math.Max(0, (ClientSize.Height - pnlGameOver.Height) / 2);
                pnlGameOver.Location = new Point(x, y);
            }
        }

        private void CmbDisplayMode_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = isSelected ? Color.FromArgb(50, 50, 60) : Color.FromArgb(28, 28, 32);
            Color fg = Color.FromArgb(235, 235, 240);

            using (var brush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            string text = cmbDisplayMode.Items[e.Index]?.ToString() ?? string.Empty;
            TextRenderer.DrawText(
                e.Graphics,
                text,
                cmbDisplayMode.Font,
                e.Bounds,
                fg,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter
            );
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            input.KeyDown(e.KeyCode);
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            input.KeyUp(e.KeyCode);
        }

        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            input.SetMouse(e.X, e.Y);
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            input.SetMouse(e.X, e.Y);

            if (e.Button == MouseButtons.Left && state == GameState.Playing && !player.IsDead)
            {
                combat.Shoot(player, bullets, input.MouseX, input.MouseY);
            }
        }

        private void UpdatePlayer()
        {
            PointF playerCenter = player.GetCenter();

            enemyManager.Update(enemies, playerCenter);

            player.UpdateAngle(input.MouseX, input.MouseY);

            PointF direction = input.GetMovementDirection();
            player.Move(direction.X, direction.Y, ClientSize);
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (state != GameState.Playing)
                return;

            if (!player.IsDead)
            {
                UpdatePlayer();

                int xpGained = combat.UpdateBullets(player, bullets, enemies, damageNumbers, ClientSize);

                combat.UpdateDamageNumbers(damageNumbers, 16);

                enemyManager.UpdateHitFlashTimers(enemies);

                combat.UpdateEnemyContactDamage(player, enemies, 16);

                if (player.Health != lastHealth)
                {
                    lastHealth = player.Health;
                    UpdateHealthUI();
                }

                if (xpGained > 0)
                {
                    UpdateExperienceUI();
                }

                if (!player.IsDead)
                {
                    enemyManager.UpdateSpawning(enemies, ClientSize);
                }
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            renderer.Draw(
                e.Graphics,
                ClientSize,
                player,
                enemies,
                bullets,
                damageNumbers
            );
        }

        private void UpdateHealthUI()
        {
            float healthFraction = player.MaxHealth > 0
                ? player.Health / (float)player.MaxHealth
                : 0f;

            healthFraction = Math.Clamp(healthFraction, 0f, 1f);

            pnlHealthFill.Width = Math.Clamp(
                (int)(healthFillMaxWidth * healthFraction),
                0,
                healthFillMaxWidth
            );

            lblHealth.Text = $"HP {player.Health} / {player.MaxHealth}";

            if (player.IsDead)
            {
                state = GameState.GameOver;
                pnlGameOver.Visible = true;
                pnlGameOver.BringToFront();
                RecenterLayout();
            }
        }

        private void MenuAnimationTimer_Tick(object? sender, EventArgs e)
        {
            double dt = menuAnimationTimer.Interval / 1000.0;

            if (state == GameState.MainMenu)
            {
                logoAnimator.Update(dt);
                menuAnimator.Update(dt);

                if (Math.Abs(menuSettleOffset) > 0.05f)
                {
                    menuSettleOffset *= 0.82f;
                    if (Math.Abs(menuSettleOffset) < 0.1f)
                        menuSettleOffset = 0f;
                    RecenterLayout();
                }
            }
            else if (state == GameState.Settings)
            {
                menuAnimator.Update(dt);

                if (Math.Abs(settingsSettleOffset) > 0.05f)
                {
                    settingsSettleOffset *= 0.82f;
                    if (Math.Abs(settingsSettleOffset) < 0.1f)
                        settingsSettleOffset = 0f;
                    RecenterLayout();
                }
            }
        }

        private void BtnPlay_Click(object? sender, EventArgs e)
        {
            StartNewGame();
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            state = GameState.Settings;

            SetHudVisible(false);

            pnlMainMenu.Visible = false;
            pnlSettings.Visible = true;
            pnlSettings.BringToFront();

            settingsSettleOffset = 18f;
            RecenterLayout();
        }

        private void BtnSettingsBack_Click(object? sender, EventArgs e)
        {
            ShowMainMenu();
        }

        private void BtnApplySettings_Click(object? sender, EventArgs e)
        {
            if (cmbDisplayMode.SelectedItem is string selectedText)
            {
                DisplayMode mode = selectedText switch
                {
                    "Windowed" => DisplayMode.Windowed,
                    "Windowed Maximized" => DisplayMode.WindowedMaximized,
                    "Fullscreen" => DisplayMode.Fullscreen,
                    _ => currentDisplayMode
                };

                ApplyDisplayMode(mode);
            }
        }

        private void BtnQuit_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ShowMainMenu()
        {
            state = GameState.MainMenu;

            SetHudVisible(false);

            pnlGameOver.Visible = false;
            pnlSettings.Visible = false;

            pnlMainMenu.Visible = true;
            pnlMainMenu.BringToFront();

            menuSettleOffset = 18f;
            RecenterLayout();

            menuAnimationTimer.Start();
        }

        private void StartNewGame()
        {
            SetHudVisible(true);

            pnlMainMenu.Visible = false;
            pnlSettings.Visible = false;

            menuAnimationTimer.Stop();
            logoAnimator.Restore();
            menuAnimator.Reset();

            RestartGame();

            gameTimer.Start();
        }

        private void SetHudVisible(bool visible)
        {
            lblHealth.Visible = visible;
            pnlHealthBackground.Visible = visible;
            lblLevel.Visible = visible;
            lblXp.Visible = visible;
            pnlXpBackground.Visible = visible;
        }

        private void BtnRespawn_Click(object? sender, EventArgs e)
        {
            RestartGame();
        }

        private void RestartGame()
        {
            player.Reset(
                (ClientSize.Width - Player.Width) / 2f,
                (ClientSize.Height - Player.Height) / 2f
            );

            input.Reset();
            enemyManager.Reset();

            enemies.Clear();
            bullets.Clear();
            damageNumbers.Clear();

            lastHealth = player.Health;

            pnlGameOver.Visible = false;

            UpdateHealthUI();
            UpdateExperienceUI();

            state = GameState.Playing;

            Invalidate();
        }

        private void UpdateExperienceUI()
        {
            float xpFraction = player.ExperienceToNextLevel > 0
                ? player.Experience / (float)player.ExperienceToNextLevel
                : 0f;

            xpFraction = Math.Clamp(xpFraction, 0f, 1f);

            pnlXpFill.Width = (int)(xpFillMaxWidth * xpFraction);

            lblLevel.Text = $"LEVEL {player.Level}";
            lblXp.Text = $"{player.Experience} / {player.ExperienceToNextLevel} XP";
        }

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

            menuAnimationTimer.Stop();
            menuAnimationTimer.Tick -= MenuAnimationTimer_Tick;
            menuAnimationTimer.Dispose();

            logoAnimator.Dispose();

            renderer.Dispose();
        }
    }
}
