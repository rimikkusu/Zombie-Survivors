namespace Vampire_Survivors
{
    public partial class Form1 : Form
    {
        private const int PlayerSize = 40;
        private const int PlayerWidth = 128;
        private const int PlayerHeight = 128;

        private float playerX;
        private float playerY;

        private const float PlayerSpeed = 6f;

        private bool moveUp;
        private bool moveDown;
        private bool moveLeft;
        private bool moveRight;

        private float mouseX;
        private float mouseY;

        private float playerAngle;

        private const float PlayerPivotX = 32f;
        private const float PlayerPivotY = 20f;

        private readonly System.Windows.Forms.Timer gameTimer;



        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            KeyPreview = true;

            playerX = (ClientSize.Width - PlayerWidth) / 2f;
            playerY = (ClientSize.Height - PlayerHeight) / 2f;

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            MouseMove += Form1_MouseMove;
            MouseDown += Form1_MouseDown;

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }
        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            Console.WriteLine("Shoot!");
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                moveUp = true;

            if (e.KeyCode == Keys.S)
                moveDown = true;

            if (e.KeyCode == Keys.A)
                moveLeft = true;

            if (e.KeyCode == Keys.D)
                moveRight = true;
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                moveUp = false;

            if (e.KeyCode == Keys.S)
                moveDown = false;

            if (e.KeyCode == Keys.A)
                moveLeft = false;

            if (e.KeyCode == Keys.D)
                moveRight = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            e.Graphics.PixelOffsetMode =
                System.Drawing.Drawing2D.PixelOffsetMode.Half;

            float playerCenterX = playerX + PlayerWidth / 2f;
            float playerCenterY = playerY + PlayerHeight / 2f;

            // Our PNG is originally 64x64.
            float scaleX = PlayerWidth / 64f;
            float scaleY = PlayerHeight / 64f;

            float pivotX = PlayerPivotX * scaleX;
            float pivotY = PlayerPivotY * scaleY;

            var state = e.Graphics.Save();

            e.Graphics.TranslateTransform(playerCenterX, playerCenterY);
            e.Graphics.RotateTransform(playerAngle);

            e.Graphics.DrawImage(
                Properties.Resources.Player,
                -pivotX,
                -pivotY,
                PlayerWidth,
                PlayerHeight
            );

            e.Graphics.Restore(state);
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            float directionX = 0;
            float directionY = 0;

            if (moveUp)
                directionY -= 1;

            if (moveDown)
                directionY += 1;

            if (moveLeft)
                directionX -= 1;

            if (moveRight)
                directionX += 1;

            if (directionX != 0 && directionY != 0)
            {
                const float diagonal = 0.7071f;

                directionX *= diagonal;
                directionY *= diagonal;
            }

            float playerCenterX = playerX + PlayerWidth / 2f;
            float playerCenterY = playerY + PlayerHeight / 2f;

            float dx = mouseX - playerCenterX;
            float dy = mouseY - playerCenterY;

            playerAngle = (float)(Math.Atan2(dy, dx) * 180f / Math.PI);

            playerX += directionX * PlayerSpeed;
            playerY += directionY * PlayerSpeed;

            playerX = Math.Clamp(
                playerX,
                0,
                ClientSize.Width - PlayerWidth
            );

            playerY = Math.Clamp(
                playerY,
                0,
                ClientSize.Height - PlayerHeight
            );

            Invalidate();
        }
    }
}
