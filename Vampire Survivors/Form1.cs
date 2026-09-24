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

        private readonly InputManager input = new();
        private readonly CombatSystem combat = new();
        private readonly EnemyManager enemyManager = new();
        private readonly GameRenderer renderer;

        private readonly System.Windows.Forms.Timer gameTimer;

        private bool isCleanedUp;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            KeyPreview = true;

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

            enemies.Add(new Enemy
            {
                X = 100,
                Y = 100
            });
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

            if (e.Button == MouseButtons.Left)
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
            UpdatePlayer();

            combat.UpdateBullets(bullets, enemies, damageNumbers, ClientSize);

            combat.UpdateDamageNumbers(damageNumbers);

            enemyManager.UpdateHitFlashTimers(enemies);

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
