using Vampire_Survivors.Entities;
using Vampire_Survivors.Rendering;
using Vampire_Survivors.Systems;

namespace Vampire_Survivors
{
    public partial class Form1 : Form
    {
        private readonly Player player = new();
        private readonly List<Bullet> bullets = new();
        private readonly List<Enemy> enemies = new();
        private readonly List<DamageNumber> damageNumbers = new();
        private readonly List<ExperienceGem> experienceGems = new();

        private readonly InputManager input = new();
        private readonly CombatSystem combat = new();
        private readonly EnemyManager enemyManager = new();
        private readonly GameRenderer renderer;

        private readonly System.Windows.Forms.Timer gameTimer;

        private readonly int healthFillMaxWidth;
        private readonly int xpFillMaxWidth;
        private int lastHealth;

        private bool isCleanedUp;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            KeyPreview = true;

            healthFillMaxWidth = pnlHealthFill.Width;
            xpFillMaxWidth = pnlXpFill.Width;
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

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            MouseMove += Form1_MouseMove;
            MouseDown += Form1_MouseDown;

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            FormClosed += (_, _) => Cleanup();
            Disposed += (_, _) => Cleanup();
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

            if (e.Button == MouseButtons.Left && !player.IsDead)
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
            if (!player.IsDead)
            {
                UpdatePlayer();

                combat.UpdateBullets(bullets, enemies, damageNumbers, experienceGems, ClientSize);

                combat.UpdateDamageNumbers(damageNumbers);

                enemyManager.UpdateHitFlashTimers(enemies);

                combat.UpdateEnemyContactDamage(player, enemies, 16);

                if (player.Health != lastHealth)
                {
                    lastHealth = player.Health;
                    UpdateHealthUI();
                }

                if (combat.UpdateExperienceGems(player, experienceGems))
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
                damageNumbers,
                experienceGems
            );
        }

        private void UpdateHealthUI()
        {
            float healthFraction = player.MaxHealth > 0
                ? player.Health / (float)player.MaxHealth
                : 0f;

            pnlHealthFill.Width = (int)(healthFillMaxWidth * healthFraction);

            lblHealth.Text = $"HP {player.Health} / {player.MaxHealth}";

            if (player.IsDead)
            {
                pnlGameOver.Visible = true;
                pnlGameOver.BringToFront();
            }
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
            experienceGems.Clear();

            lastHealth = player.Health;

            pnlGameOver.Visible = false;

            UpdateHealthUI();
            UpdateExperienceUI();

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

            renderer.Dispose();
        }
    }
}
