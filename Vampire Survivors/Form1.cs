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
            LevelUp,
            Paused,
            GameOver,
            Settings
        }

        private readonly Player player = new();
        private readonly List<Bullet> bullets = new();
        private readonly List<Enemy> enemies = new();
        private readonly List<Bandage> bandages = new();
        private readonly List<DamageNumber> damageNumbers = new();

        private readonly InputManager input = new();
        private readonly CombatSystem combat = new();
        private readonly EnemyManager enemyManager = new();
        private readonly UpgradeSystem upgradeSystem = new();
        private readonly AbilitySystem abilitySystem = new();
        private readonly WaveManager waveManager = new();
        private readonly Camera camera = new();
        private readonly Size worldSize = new(GameWorld.WorldWidth, GameWorld.WorldHeight);
        private readonly GameRenderer renderer;
        private readonly Queue<int> pendingLevelUps = new();
        private List<UpgradeType> currentUpgradeChoices = new();
        private List<MajorAbilityType> currentMajorAbilityChoices = new();
        private bool selectingMajorAbility;

        private Rectangle logoBaseBounds;
        private float logoScale = 1f;
        private float logoScaleDirection = 1f;

        private static readonly Color ButtonNormalBack = Color.FromArgb(28, 28, 32);
        private static readonly Color ButtonHoverBack = Color.FromArgb(46, 46, 54);

        private readonly System.Windows.Forms.Timer gameTimer;

        private readonly int healthFillMaxWidth;
        private readonly int xpFillMaxWidth;
        private int lastHealth;

        private GameState state = GameState.MainMenu;
        private GameState settingsReturnState = GameState.MainMenu;
        private string lastWaveHudText = string.Empty;
        private int instantAbilityFeedbackRemainingMs;
        private string instantAbilityFeedbackText = string.Empty;

        private DisplayMode currentDisplayMode = DisplayMode.WindowedMaximized;
        private Rectangle savedWindowedBounds;
        private bool isApplyingDisplayMode;

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
            lblWave.BringToFront();
            pnlActiveAbility.BringToFront();
            lblUpgradesTitle.BringToFront();
            lstUpgrades.BringToFront();
            btnPause.BringToFront();
            pnlDebug.BringToFront();

            UpdateHealthUI();
            UpdateExperienceUI();

            player.X = (GameWorld.WorldWidth - Player.Width) / 2f;
            player.Y = (GameWorld.WorldHeight - Player.Height) / 2f;
            camera.Reset(player.GetCenter(), ClientSize);

            renderer = new GameRenderer();

            logoBaseBounds = picLogo.Bounds;

            // Configure ComboBox appearance and owner-draw
            cmbDisplayMode.DrawMode = DrawMode.OwnerDrawFixed;
            cmbDisplayMode.ItemHeight = 24;
            cmbDisplayMode.DrawItem += CmbDisplayMode_DrawItem;
            SyncDisplayModeComboBox();

            SetupButtonHover(btnPlay);
            SetupButtonHover(btnSettings);
            SetupButtonHover(btnQuit);
            SetupButtonHover(btnApplySettings);
            SetupButtonHover(btnSettingsBack);
            SetupButtonHover(btnRespawn);
            SetupButtonHover(btnPause);
            SetupButtonHover(btnResume);
            SetupButtonHover(btnPauseSettings);
            SetupButtonHover(btnPauseMainMenu);

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

        private static void SetupButtonHover(Button button)
        {
            button.MouseEnter += (_, _) => button.BackColor = ButtonHoverBack;
            button.MouseLeave += (_, _) => button.BackColor = ButtonNormalBack;
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
            camera.Follow(player.GetCenter(), ClientSize);
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
                int y = Math.Max(0, (pnlMainMenu.ClientSize.Height - pnlMenuContent.Height) / 2);
                pnlMenuContent.Location = new Point(x, y);
            }

            if (pnlSettingsContent != null && pnlSettings != null)
            {
                int x = Math.Max(0, (pnlSettings.ClientSize.Width - pnlSettingsContent.Width) / 2);
                int y = Math.Max(0, (pnlSettings.ClientSize.Height - pnlSettingsContent.Height) / 2);
                pnlSettingsContent.Location = new Point(x, y);
            }

            if (pnlGameOver != null)
            {
                int x = Math.Max(0, (ClientSize.Width - pnlGameOver.Width) / 2);
                int y = Math.Max(0, (ClientSize.Height - pnlGameOver.Height) / 2);
                pnlGameOver.Location = new Point(x, y);
            }

            if (pnlLevelUp != null)
            {
                int x = Math.Max(0, (ClientSize.Width - pnlLevelUp.Width) / 2);
                int y = Math.Max(0, (ClientSize.Height - pnlLevelUp.Height) / 2);
                pnlLevelUp.Location = new Point(x, y);
            }

            if (pnlPause != null)
            {
                int x = Math.Max(0, (ClientSize.Width - pnlPause.Width) / 2);
                int y = Math.Max(0, (ClientSize.Height - pnlPause.Height) / 2);
                pnlPause.Location = new Point(x, y);
            }

            if (lblWave != null)
                lblWave.Left = Math.Max(0, (ClientSize.Width - lblWave.Width) / 2);

            if (pnlActiveAbility != null && lblWave != null)
            {
                pnlActiveAbility.Left = Math.Max(0, (ClientSize.Width - pnlActiveAbility.Width) / 2);
                pnlActiveAbility.Top = lblWave.Bottom + 4;
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
            bool isNewPress = input.KeyDown(e.KeyCode);

            if (e.KeyCode == Keys.F3)
            {
                if (isNewPress && state is not (GameState.MainMenu or GameState.Settings))
                {
                    pnlDebug.Visible = !pnlDebug.Visible;
                    if (pnlDebug.Visible)
                    {
                        UpdateDebugInfo();
                        pnlDebug.BringToFront();
                    }
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                if (isNewPress && state == GameState.Playing)
                    PauseGame();
                else if (isNewPress && state == GameState.Paused)
                    ResumeGame();
                else if (isNewPress && state == GameState.Settings && settingsReturnState == GameState.Paused)
                    ReturnFromPauseSettings();

                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (state == GameState.Playing && !player.IsDead && isNewPress &&
                InputManager.TryGetAbilitySlot(e.KeyCode, out int slot))
            {
                ActivateAbilityAtSlot(slot);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            input.KeyUp(e.KeyCode);
        }

        private void ActivateAbilityAtSlot(int zeroBasedSlot)
        {
            MajorAbilityType? selectedAbility = abilitySystem.GetAbilityAtSlot(zeroBasedSlot);
            if (selectedAbility is not MajorAbilityType ability ||
                !abilitySystem.TryActivate(ability, player, chkNoAbilityCooldowns.Checked))
            {
                return;
            }

            switch (ability)
            {
                case MajorAbilityType.Teleportation:
                    TeleportTowardCursor();
                    break;
                case MajorAbilityType.Forcefield:
                    enemyManager.PushEnemiesOutsideRadius(
                        enemies,
                        player.GetCenter(),
                        AbilitySystem.ForcefieldRadius,
                        worldSize);
                    break;
                case MajorAbilityType.Shockwave:
                    ApplyShockwave();
                    break;
            }

            if (abilitySystem.GetRemainingDurationMs(ability) > 0)
                RefreshActiveAbilityFeedback();
            else
                ShowInstantAbilityFeedback($"{UpgradeSystem.GetMajorAbilityInfo(ability).Title}!");

            UpdateUpgradeHud();
            Invalidate();
        }

        private void TeleportTowardCursor()
        {
            Point mouseScreen = PointToClient(Cursor.Position);
            PointF mouseWorld = camera.ScreenToWorld(new PointF(mouseScreen.X, mouseScreen.Y));
            PointF currentCenter = player.GetCenter();
            float dx = mouseWorld.X - currentCenter.X;
            float dy = mouseWorld.Y - currentCenter.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);

            if (distance > 0.001f && distance > 450f)
            {
                dx = dx / distance * 450f;
                dy = dy / distance * 450f;
                mouseWorld = new PointF(currentCenter.X + dx, currentCenter.Y + dy);
            }

            float desiredX = Math.Clamp(mouseWorld.X - Player.Width / 2f, 0f, worldSize.Width - Player.Width);
            float desiredY = Math.Clamp(mouseWorld.Y - Player.Height / 2f, 0f, worldSize.Height - Player.Height);
            PointF destination = FindSafeTeleportLocation(desiredX, desiredY, currentCenter);
            player.X = destination.X;
            player.Y = destination.Y;
            player.UpdateAngle(mouseWorld.X, mouseWorld.Y);
            camera.Follow(player.GetCenter(), ClientSize);
        }

        private PointF FindSafeTeleportLocation(float desiredX, float desiredY, PointF originCenter)
        {
            if (IsTeleportLocationSafe(desiredX, desiredY))
                return new PointF(desiredX, desiredY);

            for (int ring = 1; ring <= 3; ring++)
            {
                float radius = ring * 40f;
                for (int direction = 0; direction < 8; direction++)
                {
                    float angle = direction * MathF.PI / 4f;
                    float x = Math.Clamp(desiredX + MathF.Cos(angle) * radius, 0f, worldSize.Width - Player.Width);
                    float y = Math.Clamp(desiredY + MathF.Sin(angle) * radius, 0f, worldSize.Height - Player.Height);
                    PointF center = new(x + Player.Width / 2f, y + Player.Height / 2f);

                    if (EnemyManager.Distance(originCenter, center) <= 450f && IsTeleportLocationSafe(x, y))
                        return new PointF(x, y);
                }
            }

            return new PointF(desiredX, desiredY);
        }

        private bool IsTeleportLocationSafe(float x, float y)
        {
            float centerX = x + Player.Width / 2f;
            float centerY = y + Player.Height / 2f;
            RectangleF playerHitbox = new(
                centerX - Player.HitboxWidth / 2f,
                centerY + 5f - Player.HitboxHeight / 2f,
                Player.HitboxWidth,
                Player.HitboxHeight);
            return enemies.All(enemy => !playerHitbox.IntersectsWith(enemy.GetHitbox()));
        }

        private void ApplyShockwave()
        {
            PointF center = player.GetCenter();
            int previousLevel = player.Level;
            int xpAwarded = 0;

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                if (EnemyManager.Distance(center, enemy.GetCenter()) > 300f)
                    continue;

                enemyManager.PushEnemyAway(enemy, center, 180f, worldSize);
                xpAwarded += combat.ApplyEnemyDamage(player, enemy, 20, false, damageNumbers, bandages);
                if (enemy.Health <= 0)
                    enemies.RemoveAt(i);
            }

            if (xpAwarded > 0)
            {
                QueuePlayerLevelUps(previousLevel);

                UpdateExperienceUI();
                if (pendingLevelUps.Count > 0)
                    ShowNextLevelUp();
            }

            UpdateWaveUI();
        }

        private void ShowInstantAbilityFeedback(string text)
        {
            instantAbilityFeedbackText = text;
            instantAbilityFeedbackRemainingMs = 850;
            RefreshActiveAbilityFeedback();
        }

        private void UpdateActiveAbilityFeedback(int elapsedMs)
        {
            if (instantAbilityFeedbackRemainingMs > 0)
            {
                instantAbilityFeedbackRemainingMs = Math.Max(0, instantAbilityFeedbackRemainingMs - elapsedMs);
                if (instantAbilityFeedbackRemainingMs == 0)
                    instantAbilityFeedbackText = string.Empty;
            }

            RefreshActiveAbilityFeedback();
        }

        private void RefreshActiveAbilityFeedback()
        {
            List<string> lines = new();
            foreach (MajorAbilityType ability in abilitySystem.AcquiredAbilities)
            {
                int remainingMs = abilitySystem.GetRemainingDurationMs(ability);
                if (remainingMs <= 0)
                    continue;

                string title = UpgradeSystem.GetMajorAbilityInfo(ability).Title;
                int secondsRemaining = Math.Max(1, (int)Math.Ceiling(remainingMs / 1000d));
                lines.Add($"{title} ACTIVE - {secondsRemaining}s");
            }

            if (instantAbilityFeedbackRemainingMs > 0 && !string.IsNullOrEmpty(instantAbilityFeedbackText))
                lines.Add(instantAbilityFeedbackText);

            lblActiveAbility.Text = string.Join(Environment.NewLine, lines);
            pnlActiveAbility.Visible = lines.Count > 0 && lblWave.Visible;
            if (pnlActiveAbility.Visible)
                pnlActiveAbility.BringToFront();
        }

        private void ResetActiveAbilityFeedback()
        {
            instantAbilityFeedbackRemainingMs = 0;
            instantAbilityFeedbackText = string.Empty;
            lblActiveAbility.Text = string.Empty;
            pnlActiveAbility.Visible = false;
        }

        private void CollectBandages()
        {
            if (player.IsDead || player.Health >= player.MaxHealth)
                return;

            RectangleF playerHitbox = player.GetHitbox();
            for (int i = bandages.Count - 1; i >= 0; i--)
            {
                if (!playerHitbox.IntersectsWith(bandages[i].GetHitbox()))
                    continue;

                int healed = player.Heal(20);
                if (healed <= 0)
                    continue;

                PointF playerCenter = player.GetCenter();
                damageNumbers.Add(new DamageNumber
                {
                    X = playerCenter.X,
                    Y = player.Y,
                    Damage = healed,
                    Type = CombatTextType.Healing
                });
                bandages.RemoveAt(i);
                lastHealth = player.Health;
                UpdateHealthUI();
            }
        }

        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            input.SetMouse(e.X, e.Y);
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (pnlDebug.Visible && pnlDebug.Bounds.Contains(e.Location))
                return;

            input.SetMouse(e.X, e.Y);

            if (e.Button == MouseButtons.Left && state == GameState.Playing && !player.IsDead)
            {
                PointF mouseWorld = camera.ScreenToWorld(new PointF(e.X, e.Y));
                player.UpdateAngle(mouseWorld.X, mouseWorld.Y);
                int previousLevel = player.Level;
                int previousExperience = player.Experience;

                if (abilitySystem.IsActive(MajorAbilityType.Multishot))
                {
                    if (combat.Shoot(
                            player,
                            bullets,
                            mouseWorld.X,
                            mouseWorld.Y,
                            -2f,
                            enemies: enemies,
                            damageNumbers: damageNumbers,
                            bandages: bandages))
                    {
                        combat.Shoot(
                            player,
                            bullets,
                            mouseWorld.X,
                            mouseWorld.Y,
                            2f,
                            bypassCooldown: true,
                            enemies: enemies,
                            damageNumbers: damageNumbers,
                            bandages: bandages);
                    }
                }
                else
                {
                    combat.Shoot(
                        player,
                        bullets,
                        mouseWorld.X,
                        mouseWorld.Y,
                        enemies: enemies,
                        damageNumbers: damageNumbers,
                        bandages: bandages);
                }

                HandleImmediateExperienceChange(previousLevel, previousExperience);
            }
        }

        private void UpdatePlayer()
        {
            PointF direction = input.GetMovementDirection();
            player.Move(direction.X, direction.Y, worldSize);
            camera.Follow(player.GetCenter(), ClientSize);

            Point mouseScreen = PointToClient(Cursor.Position);
            input.SetMouse(mouseScreen.X, mouseScreen.Y);
            PointF mouseWorld = camera.ScreenToWorld(new PointF(input.MouseX, input.MouseY));
            player.UpdateAngle(mouseWorld.X, mouseWorld.Y);

            float forcefieldRadius = abilitySystem.IsActive(MajorAbilityType.Forcefield)
                ? AbilitySystem.ForcefieldRadius
                : 0f;
            enemyManager.Update(enemies, player.GetCenter(), worldSize, forcefieldRadius, player.GetHitbox());
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (state != GameState.Playing)
                return;

            if (!player.IsDead)
            {
                abilitySystem.Update(16, player, chkNoAbilityCooldowns.Checked);
                UpdateActiveAbilityFeedback(16);
                UpdateUpgradeHud();
                UpdatePlayer();
                CollectBandages();

                int previousLevel = player.Level;
                int xpGained = combat.UpdateBullets(player, bullets, enemies, damageNumbers, worldSize, bandages);

                if (xpGained > 0)
                {
                    QueuePlayerLevelUps(previousLevel);

                    UpdateExperienceUI();

                    if (pendingLevelUps.Count > 0)
                        ShowNextLevelUp();
                }

                if (state == GameState.Playing)
                {
                    combat.UpdateDamageNumbers(damageNumbers, 16);
                    enemyManager.UpdateHitFlashTimers(enemies);

                    bool waveChanged = waveManager.Update(
                        16,
                        enemies.Count,
                        (wave, spawnIndex, _) => enemyManager.TrySpawnEnemy(
                            enemies,
                            worldSize,
                            camera.GetVisibleWorldBounds(ClientSize),
                            player.GetCenter(),
                            player.Level,
                            wave,
                            spawnIndex));

                    if (waveChanged)
                        UpdateWaveUI();

                    combat.UpdateEnemyContactDamage(player, enemies, 16, chkInfiniteHealth.Checked);
                }

                if (player.Health != lastHealth)
                {
                    lastHealth = player.Health;
                    UpdateHealthUI();
                }

                UpdateWaveUI();
            }

            UpdateDebugInfo();

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            renderer.Draw(
                e.Graphics,
                ClientSize,
                camera,
                player,
                enemies,
                bullets,
                damageNumbers,
                abilitySystem.IsActive(MajorAbilityType.Forcefield),
                bandages
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
                pnlPause.Visible = false;
                pnlLevelUp.Visible = false;
                pnlGameOver.Visible = true;
                pnlGameOver.BringToFront();
                RecenterLayout();
            }
        }

        private void MenuAnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (state != GameState.MainMenu)
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

            int newWidth = (int)Math.Round(logoBaseBounds.Width * logoScale);
            int newHeight = (int)Math.Round(logoBaseBounds.Height * logoScale);
            int newX = logoBaseBounds.X - (newWidth - logoBaseBounds.Width) / 2;
            int newY = logoBaseBounds.Y - (newHeight - logoBaseBounds.Height) / 2;

            picLogo.SetBounds(newX, newY, newWidth, newHeight);
        }

        private void BtnPlay_Click(object? sender, EventArgs e)
        {
            StartNewGame();
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            settingsReturnState = GameState.MainMenu;
            state = GameState.Settings;

            SetHudVisible(false);

            menuAnimationTimer.Stop();
            picLogo.Bounds = logoBaseBounds;
            logoScale = 1f;
            logoScaleDirection = 1f;

            pnlMainMenu.Visible = false;
            pnlSettings.Visible = true;
            pnlSettings.BringToFront();

            RecenterLayout();
        }

        private void BtnSettingsBack_Click(object? sender, EventArgs e)
        {
            if (settingsReturnState == GameState.Paused)
                ReturnFromPauseSettings();
            else
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

        private void BtnPause_Click(object? sender, EventArgs e)
        {
            PauseGame();
        }

        private void PauseGame()
        {
            if (state != GameState.Playing)
                return;

            state = GameState.Paused;
            input.ResetMovement();
            pnlPause.Visible = true;
            pnlPause.BringToFront();
            RecenterLayout();
        }

        private void BtnResume_Click(object? sender, EventArgs e)
        {
            ResumeGame();
        }

        private void ResumeGame()
        {
            if (state != GameState.Paused)
                return;

            input.ResetMovement();
            pnlPause.Visible = false;
            state = GameState.Playing;
            if (pendingLevelUps.Count > 0)
                ShowNextLevelUp();
        }

        private void BtnPauseSettings_Click(object? sender, EventArgs e)
        {
            if (state != GameState.Paused)
                return;

            settingsReturnState = GameState.Paused;
            state = GameState.Settings;
            pnlPause.Visible = false;
            pnlSettings.Visible = true;
            pnlSettings.BringToFront();
            RecenterLayout();
        }

        private void ReturnFromPauseSettings()
        {
            pnlSettings.Visible = false;
            state = GameState.Paused;
            pnlPause.Visible = true;
            pnlPause.BringToFront();
            RecenterLayout();
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
            state = GameState.MainMenu;

            SetHudVisible(false);

            pnlGameOver.Visible = false;
            pnlSettings.Visible = false;
            pnlPause.Visible = false;
            pnlLevelUp.Visible = false;

            pnlMainMenu.Visible = true;
            pnlMainMenu.BringToFront();

            RecenterLayout();

            menuAnimationTimer.Start();
        }

        private void StartNewGame()
        {
            SetHudVisible(true);

            pnlMainMenu.Visible = false;
            pnlSettings.Visible = false;

            menuAnimationTimer.Stop();
            picLogo.Bounds = logoBaseBounds;
            logoScale = 1f;
            logoScaleDirection = 1f;

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
            lblWave.Visible = visible;
            lblUpgradesTitle.Visible = visible;
            lstUpgrades.Visible = visible;
            btnPause.Visible = visible;
            if (!visible)
            {
                pnlActiveAbility.Visible = false;
                pnlDebug.Visible = false;
            }
        }

        private void ChkInfiniteHealth_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkInfiniteHealth.Checked && !player.IsDead)
            {
                player.Heal(player.MaxHealth);
                lastHealth = player.Health;
                UpdateHealthUI();
            }

            UpdateDebugInfo();
        }

        private void BtnDebugAddXp_Click(object? sender, EventArgs e)
        {
            AddExperienceAndQueueLevelUps(100);
        }

        private void BtnDebugLevelUp_Click(object? sender, EventArgs e)
        {
            int remainingExperience = Math.Max(0, player.ExperienceToNextLevel - player.Experience);
            AddExperienceAndQueueLevelUps(remainingExperience);
        }

        private void BtnDebugNextWave_Click(object? sender, EventArgs e)
        {
            enemies.Clear();
            bullets.Clear();
            waveManager.AdvanceToNextWave();
            UpdateWaveUI();
            UpdateDebugInfo();
            Invalidate();
        }

        private void BtnDebugKillEnemies_Click(object? sender, EventArgs e)
        {
            enemies.Clear();
            bullets.Clear();
            waveManager.CompleteCurrentWave();
            UpdateWaveUI();
            UpdateDebugInfo();
            Invalidate();
        }

        private void BtnDebugHeal_Click(object? sender, EventArgs e)
        {
            int healed = player.Heal(player.MaxHealth);
            if (healed > 0)
            {
                lastHealth = player.Health;
                UpdateHealthUI();
            }

            UpdateDebugInfo();
        }

        private void AddExperienceAndQueueLevelUps(int amount)
        {
            if (amount <= 0 || player.IsDead)
                return;

            int previousLevel = player.Level;
            int previousExperience = player.Experience;
            player.AddExperience(amount);
            QueuePlayerLevelUps(previousLevel);

            if (player.Level != previousLevel || player.Experience != previousExperience)
                UpdateExperienceUI();

            if (pendingLevelUps.Count > 0 && state == GameState.Playing)
                ShowNextLevelUp();

            UpdateDebugInfo();
            Invalidate();
        }

        private void QueuePlayerLevelUps(int previousLevel)
        {
            for (int level = previousLevel + 1; level <= player.Level; level++)
                pendingLevelUps.Enqueue(level);
        }

        private void HandleImmediateExperienceChange(int previousLevel, int previousExperience)
        {
            if (player.Level != previousLevel || player.Experience != previousExperience)
            {
                QueuePlayerLevelUps(previousLevel);
                UpdateExperienceUI();

                if (pendingLevelUps.Count > 0 && state == GameState.Playing)
                    ShowNextLevelUp();
            }

            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            if (!pnlDebug.Visible)
                return;

            string info =
                $"LEVEL: {player.Level}{Environment.NewLine}" +
                $"XP: {player.Experience} / {player.ExperienceToNextLevel}{Environment.NewLine}" +
                $"WAVE: {waveManager.CurrentWave}{Environment.NewLine}" +
                $"HP: {player.Health} / {player.MaxHealth}{Environment.NewLine}" +
                $"ENEMIES: {enemies.Count}";

            if (!string.Equals(lblDebugInfo.Text, info, StringComparison.Ordinal))
                lblDebugInfo.Text = info;
        }

        private void BtnRespawn_Click(object? sender, EventArgs e)
        {
            RestartGame();
        }

        private void RestartGame()
        {
            pendingLevelUps.Clear();
            currentUpgradeChoices.Clear();
            currentMajorAbilityChoices.Clear();
            selectingMajorAbility = false;
            upgradeSystem.Reset();
            waveManager.Reset();
            pnlLevelUp.Visible = false;
            pnlPause.Visible = false;

            player.Reset(
                (GameWorld.WorldWidth - Player.Width) / 2f,
                (GameWorld.WorldHeight - Player.Height) / 2f
            );

            abilitySystem.Reset(player);
            ResetActiveAbilityFeedback();

            camera.Reset(player.GetCenter(), ClientSize);

            input.Reset();

            enemies.Clear();
            bandages.Clear();
            bullets.Clear();
            damageNumbers.Clear();

            lastHealth = player.Health;

            pnlGameOver.Visible = false;

            UpdateHealthUI();
            UpdateExperienceUI();
            UpdateUpgradeHud();
            UpdateWaveUI();

            state = GameState.Playing;
            UpdateDebugInfo();

            Invalidate();
        }

        private void ShowNextLevelUp()
        {
            if (pendingLevelUps.Count == 0 || player.IsDead)
            {
                pnlLevelUp.Visible = false;
                state = player.IsDead ? GameState.GameOver : GameState.Playing;
                return;
            }

            int level = pendingLevelUps.Peek();
            selectingMajorAbility = UpgradeSystem.IsMajorAbilityLevel(level);

            if (selectingMajorAbility)
            {
                currentMajorAbilityChoices = abilitySystem.GetRandomChoices();
                selectingMajorAbility = currentMajorAbilityChoices.Count > 0;
            }

            if (selectingMajorAbility)
            {
                currentUpgradeChoices.Clear();
                lblLevelUpTitle.Text = $"LEVEL {level} — CHOOSE AN ABILITY";

                SetUpgradeButtonText(btnUpgrade1, currentMajorAbilityChoices, 0);
                SetUpgradeButtonText(btnUpgrade2, currentMajorAbilityChoices, 1);
                SetUpgradeButtonText(btnUpgrade3, currentMajorAbilityChoices, 2);
            }
            else
            {
                currentUpgradeChoices = upgradeSystem.GetRandomUpgrades();
                currentMajorAbilityChoices.Clear();
                lblLevelUpTitle.Text = $"LEVEL {level} — CHOOSE AN UPGRADE";

                SetUpgradeButtonText(btnUpgrade1, currentUpgradeChoices, 0);
                SetUpgradeButtonText(btnUpgrade2, currentUpgradeChoices, 1);
                SetUpgradeButtonText(btnUpgrade3, currentUpgradeChoices, 2);
            }

            state = GameState.LevelUp;
            pnlLevelUp.Visible = true;
            pnlLevelUp.BringToFront();
            RecenterLayout();
        }

        private void SetUpgradeButtonText(Button button, IReadOnlyList<UpgradeType> choices, int index)
        {
            if (index >= choices.Count)
            {
                button.Visible = false;
                return;
            }

            UpgradeType choice = choices[index];
            (string title, string description) = UpgradeSystem.GetUpgradeInfo(choice);
            button.Text = $"{title}{Environment.NewLine}{description}   Owned: {upgradeSystem.GetStackCount(choice)}";
            button.Visible = true;
        }

        private void SetUpgradeButtonText(Button button, IReadOnlyList<MajorAbilityType> choices, int index)
        {
            if (index >= choices.Count)
            {
                button.Visible = false;
                return;
            }

            (string title, string description) = UpgradeSystem.GetMajorAbilityInfo(choices[index]);
            button.Text = $"{title}{Environment.NewLine}{description}";
            button.Visible = true;
        }

        private void ChooseLevelUpOption(int index)
        {
            if (state != GameState.LevelUp || pendingLevelUps.Count == 0)
                return;

            if (selectingMajorAbility)
            {
                if (index >= currentMajorAbilityChoices.Count)
                    return;

                abilitySystem.Acquire(currentMajorAbilityChoices[index]);
                UpdateUpgradeHud();
            }
            else
            {
                if (index >= currentUpgradeChoices.Count)
                    return;

                upgradeSystem.ApplyUpgrade(currentUpgradeChoices[index], player);
                UpdateUpgradeHud();
                UpdateHealthUI();
            }

            pendingLevelUps.Dequeue();
            if (chkInfiniteXp.Checked && !player.IsDead)
            {
                int remainingExperience = Math.Max(0, player.ExperienceToNextLevel - player.Experience);
                AddExperienceAndQueueLevelUps(remainingExperience);
            }

            UpdateExperienceUI();
            ShowNextLevelUp();
        }

        private void BtnUpgrade1_Click(object? sender, EventArgs e) => ChooseLevelUpOption(0);
        private void BtnUpgrade2_Click(object? sender, EventArgs e) => ChooseLevelUpOption(1);
        private void BtnUpgrade3_Click(object? sender, EventArgs e) => ChooseLevelUpOption(2);

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

        private void UpdateWaveUI()
        {
            string text = waveManager.IsWaveActive
                ? $"WAVE {waveManager.CurrentWave}{Environment.NewLine}ENEMIES: {waveManager.GetEnemiesRemaining(enemies.Count)}"
                : $"WAVE {waveManager.CurrentWave} CLEARED{Environment.NewLine}NEXT WAVE: {Math.Max(1, (int)Math.Ceiling(waveManager.IntermissionRemainingMs / 1000f))}s";

            if (lastWaveHudText == text)
                return;

            lastWaveHudText = text;
            lblWave.Text = text;
        }

        private void UpdateUpgradeHud()
        {
            List<string> entries = new();

            foreach (UpgradeType type in Enum.GetValues<UpgradeType>())
            {
                int stacks = upgradeSystem.GetStackCount(type);
                if (stacks <= 0)
                    continue;

                string entry = type switch
                {
                    UpgradeType.HealthBoost => $"Health Boost x{stacks} (+{stacks * 10} HP)",
                    UpgradeType.SpeedBoost => $"Speed Boost x{stacks} (+{stacks * 3}%)",
                    UpgradeType.Durability => $"Durability x{stacks} (+{stacks * 3}%)",
                    UpgradeType.DamageBoost => $"Damage Boost x{stacks} (+{stacks * 5}%)",
                    UpgradeType.CriticalTraining => $"Critical Training x{stacks} (+{stacks}%)",
                    UpgradeType.BulletSpeed => $"Bullet Speed x{stacks} (+{stacks * 5}%)",
                    UpgradeType.RapidFire => $"Rapid Fire x{stacks} (+{stacks * 4}%)",
                    _ => string.Empty
                };

                entries.Add(entry);
            }

            if (abilitySystem.AcquiredAbilities.Count > 0)
            {
                if (entries.Count > 0)
                    entries.Add(string.Empty);

                entries.Add("ABILITIES");
                for (int i = 0; i < abilitySystem.AcquiredAbilities.Count; i++)
                {
                    MajorAbilityType ability = abilitySystem.AcquiredAbilities[i];
                    string title = UpgradeSystem.GetMajorAbilityInfo(ability).Title;
                    string status = abilitySystem.GetHudStatus(ability);
                    entries.Add($"[{i + 1}] {title} - {status}");
                }
            }

            if (lstUpgrades.Items.Cast<object>().Select(item => item?.ToString() ?? string.Empty).SequenceEqual(entries))
                return;

            lstUpgrades.BeginUpdate();
            lstUpgrades.Items.Clear();
            lstUpgrades.Items.AddRange(entries.Cast<object>().ToArray());
            lstUpgrades.EndUpdate();
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

            renderer.Dispose();
        }
    }
}
