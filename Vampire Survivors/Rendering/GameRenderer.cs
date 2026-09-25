using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Rendering
{
    public sealed class GameRenderer : IDisposable
    {
        private readonly Bitmap playerSprite;
        private readonly Bitmap enemySprite;
        private readonly Bitmap bulletSprite;
        private readonly Bitmap grassSprite;

        private readonly Font normalDamageFont;
        private readonly Font criticalDamageFont;
        private readonly Font experienceFont;

        private static readonly System.Drawing.Imaging.ColorMatrix hitFlashColorMatrix =
            new System.Drawing.Imaging.ColorMatrix(
                new float[][]
                {
                    new float[] { 1f, 0f, 0f, 0f, 0f },
                    new float[] { 0f, 0.25f, 0f, 0f, 0f },
                    new float[] { 0f, 0f, 0.25f, 0f, 0f },
                    new float[] { 0f, 0f, 0f, 1f, 0f },
                    new float[] { 0.45f, 0f, 0f, 0f, 1f }
                });

        private readonly System.Drawing.Imaging.ImageAttributes hitFlashAttributes;

        private bool isDisposed;

        public GameRenderer()
        {
            playerSprite = Properties.Resources.Player;
            enemySprite = Properties.Resources.Zombie_1;
            bulletSprite = Properties.Resources.Bullet;
            grassSprite = Properties.Resources.Grass;

            normalDamageFont = new Font("Arial", 14, FontStyle.Regular);
            criticalDamageFont = new Font("Arial", 18, FontStyle.Bold);
            experienceFont = new Font("Arial", 12, FontStyle.Regular);

            hitFlashAttributes = new System.Drawing.Imaging.ImageAttributes();
            hitFlashAttributes.SetColorMatrix(hitFlashColorMatrix);
        }

        public void Draw(
            Graphics g,
            Size clientSize,
            Player player,
            List<Enemy> enemies,
            List<Bullet> bullets,
            List<DamageNumber> damageNumbers)
        {
            // Pixel-art rendering, configured once.
            g.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            g.PixelOffsetMode =
                System.Drawing.Drawing2D.PixelOffsetMode.Half;

            g.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.None;

            DrawGrass(g, clientSize);
            DrawEnemies(g, enemies);
            DrawBullets(g, bullets);
            DrawPlayer(g, player);
            DrawDamageNumbers(g, damageNumbers);
        }

        private void DrawGrass(Graphics g, Size clientSize)
        {
            int grassSize = 128;

            for (int y = 0; y < clientSize.Height; y += grassSize)
            {
                for (int x = 0; x < clientSize.Width; x += grassSize)
                {
                    g.DrawImage(
                        grassSprite,
                        new Rectangle(x, y, grassSize, grassSize),
                        0,
                        0,
                        grassSprite.Width,
                        grassSprite.Height,
                        GraphicsUnit.Pixel
                    );
                }
            }
        }

        private void DrawEnemies(Graphics g, List<Enemy> enemies)
        {
            foreach (Enemy enemy in enemies)
            {
                float enemyCenterX = enemy.X + Enemy.Size / 2f;
                float enemyCenterY = enemy.Y + Enemy.Size / 2f;

                var enemyState = g.Save();

                g.TranslateTransform(
                    enemyCenterX,
                    enemyCenterY
                );

                g.RotateTransform(enemy.Angle + 90f);

                if (enemy.HitFlashTimer > 0)
                {
                    g.DrawImage(
                        enemySprite,
                        new Rectangle(
                            -Enemy.Size / 2,
                            -Enemy.Size / 2,
                            Enemy.Size,
                            Enemy.Size
                        ),
                        0,
                        0,
                        enemySprite.Width,
                        enemySprite.Height,
                        GraphicsUnit.Pixel,
                        hitFlashAttributes
                    );
                }
                else
                {
                    g.DrawImage(
                        enemySprite,
                        -Enemy.Size / 2f,
                        -Enemy.Size / 2f,
                        Enemy.Size,
                        Enemy.Size
                    );
                }

                g.Restore(enemyState);
            }
        }

        private void DrawBullets(Graphics g, List<Bullet> bullets)
        {
            foreach (Bullet bullet in bullets)
            {
                float centerX = bullet.X + Bullet.Size / 2f;
                float centerY = bullet.Y + Bullet.Size / 2f;

                var bulletState = g.Save();

                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(bullet.Angle + 90f);

                g.DrawImage(
                    bulletSprite,
                    -Bullet.Size / 2f,
                    -Bullet.Size / 2f,
                    Bullet.Size,
                    Bullet.Size
                );

                g.Restore(bulletState);
            }
        }

        private void DrawPlayer(Graphics g, Player player)
        {
            PointF playerCenter = player.GetCenter();

            // Our PNG is originally 64x64.
            float scaleX = Player.Width / 64f;
            float scaleY = Player.Height / 64f;

            float pivotX = Player.PivotX * scaleX;
            float pivotY = Player.PivotY * scaleY;

            var state = g.Save();

            g.TranslateTransform(playerCenter.X, playerCenter.Y);
            g.RotateTransform(player.Angle);

            if (player.HitFlashRemainingMs > 0)
            {
                g.DrawImage(
                    playerSprite,
                    new Rectangle(
                        (int)-pivotX,
                        (int)-pivotY,
                        Player.Width,
                        Player.Height
                    ),
                    0,
                    0,
                    playerSprite.Width,
                    playerSprite.Height,
                    GraphicsUnit.Pixel,
                    hitFlashAttributes
                );
            }
            else
            {
                g.DrawImage(
                    playerSprite,
                    -pivotX,
                    -pivotY,
                    Player.Width,
                    Player.Height
                );
            }

            g.Restore(state);
        }

        private void DrawDamageNumbers(Graphics g, List<DamageNumber> damageNumbers)
        {
            // Floating text is drawn in normal screen space
            // so it always stays upright (never inherits sprite rotation).
            foreach (DamageNumber number in damageNumbers)
            {
                // Delayed text (XP notification) is not rendered yet.
                if (number.DelayRemainingMs > 0)
                    continue;

                int alpha = Math.Clamp(
                    number.Life * 255 / 45,
                    0,
                    255
                );

                Color textColor;
                Font textFont;
                string text;

                switch (number.Type)
                {
                    case CombatTextType.CriticalDamage:
                        textColor = Color.FromArgb(alpha, 255, 215, 0);
                        textFont = criticalDamageFont;
                        text = $"{number.Damage}!";
                        break;

                    case CombatTextType.KillDamage:
                        textColor = Color.FromArgb(alpha, 255, 60, 60);
                        textFont = criticalDamageFont;
                        text = number.Damage.ToString();
                        break;

                    case CombatTextType.CriticalKillDamage:
                        textColor = Color.FromArgb(alpha, 255, 60, 60);
                        textFont = criticalDamageFont;
                        text = $"{number.Damage}!";
                        break;

                    case CombatTextType.Experience:
                        textColor = Color.FromArgb(alpha, 150, 255, 255);
                        textFont = experienceFont;
                        text = $"+{number.Damage} XP";
                        break;

                    default:
                        textColor = Color.FromArgb(alpha, 255, 255, 255);
                        textFont = normalDamageFont;
                        text = number.Damage.ToString();
                        break;
                }

                using Brush damageBrush =
                    new SolidBrush(textColor);

                SizeF textSize =
                    g.MeasureString(text, textFont);

                g.DrawString(
                    text,
                    textFont,
                    damageBrush,
                    number.X - textSize.Width / 2f,
                    number.Y
                );
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            normalDamageFont.Dispose();
            criticalDamageFont.Dispose();
            experienceFont.Dispose();
            hitFlashAttributes.Dispose();
        }
    }
}
