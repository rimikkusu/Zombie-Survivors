using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Vampire_Survivors.Entities;
using Vampire_Survivors.Systems;

namespace Vampire_Survivors.Rendering
{
    public sealed class GameRenderer : IDisposable
    {
        private static readonly bool DrawHitboxes = false;

        private readonly Bitmap playerSprite;
        private readonly Bitmap fastZombieSprite;
        private readonly Bitmap heavyZombieSprite;
        private readonly Bitmap vampireSprite;
        private readonly Bitmap bulletSprite;
        private readonly Bitmap grassSprite;
        private readonly Font normalDamageFont;
        private readonly Font criticalDamageFont;
        private readonly Font experienceFont;

        private static readonly ColorMatrix hitFlashColorMatrix = new(
            new float[][]
            {
                new float[] { 1f, 0f, 0f, 0f, 0f },
                new float[] { 0f, 0.25f, 0f, 0f, 0f },
                new float[] { 0f, 0f, 0.25f, 0f, 0f },
                new float[] { 0f, 0f, 0f, 1f, 0f },
                new float[] { 0.45f, 0f, 0f, 0f, 1f }
            });

        private readonly ImageAttributes hitFlashAttributes = new();
        private bool isDisposed;

        public GameRenderer()
        {
            playerSprite = Properties.Resources.Player;
            fastZombieSprite = Properties.Resources.Zombie_1;
            heavyZombieSprite = Properties.Resources.Zombie_2;
            vampireSprite = Properties.Resources.Vampire_1;
            bulletSprite = Properties.Resources.Bullet;
            grassSprite = Properties.Resources.Grass;

            normalDamageFont = new Font("Arial", 14, FontStyle.Regular);
            criticalDamageFont = new Font("Arial", 18, FontStyle.Bold);
            experienceFont = new Font("Arial", 12, FontStyle.Regular);
            hitFlashAttributes.SetColorMatrix(hitFlashColorMatrix);
        }

        public void Draw(
            Graphics g,
            Size viewportSize,
            Camera camera,
            Player player,
            List<Enemy> enemies,
            List<Bullet> bullets,
            List<DamageNumber> damageNumbers,
            bool forcefieldActive = false,
            List<Bandage>? bandages = null)
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.FromArgb(12, 16, 14));

            GraphicsState worldState = g.Save();
            g.TranslateTransform(-camera.X, -camera.Y);
            g.SetClip(
                new RectangleF(0, 0, GameWorld.WorldWidth, GameWorld.WorldHeight),
                CombineMode.Intersect);

            DrawGrass(g, viewportSize, camera);
            if (bandages is not null)
                DrawBandages(g, bandages, camera.GetVisibleWorldBounds(viewportSize));
            DrawEnemies(g, enemies, camera.GetVisibleWorldBounds(viewportSize));
            DrawBullets(g, bullets, camera.GetVisibleWorldBounds(viewportSize));
            DrawPlayer(g, player);
            if (forcefieldActive)
                DrawForcefield(g, player);
            DrawDamageNumbers(g, damageNumbers, camera.GetVisibleWorldBounds(viewportSize));
            DrawWorldBoundary(g);

            if (DrawHitboxes)
                DrawHitboxDebug(g, player, enemies, bullets);

            g.Restore(worldState);
        }

        private void DrawGrass(Graphics g, Size viewportSize, Camera camera)
        {
            const int tileSize = 128;
            int startX = Math.Max(0, (int)MathF.Floor(camera.X / tileSize) * tileSize);
            int startY = Math.Max(0, (int)MathF.Floor(camera.Y / tileSize) * tileSize);
            int endX = Math.Min(GameWorld.WorldWidth, (int)MathF.Ceiling(camera.X + viewportSize.Width));
            int endY = Math.Min(GameWorld.WorldHeight, (int)MathF.Ceiling(camera.Y + viewportSize.Height));

            for (int y = startY; y < endY; y += tileSize)
            {
                for (int x = startX; x < endX; x += tileSize)
                {
                    g.DrawImage(
                        grassSprite,
                        new Rectangle(x, y, tileSize, tileSize),
                        0, 0, grassSprite.Width, grassSprite.Height,
                        GraphicsUnit.Pixel);
                }
            }
        }

        private void DrawWorldBoundary(Graphics g)
        {
            using var border = new Pen(Color.FromArgb(220, 38, 44, 42), 8f);
            g.DrawRectangle(border, 4, 4, GameWorld.WorldWidth - 8, GameWorld.WorldHeight - 8);
        }

        private static void DrawBandages(Graphics g, List<Bandage> bandages, RectangleF visibleBounds)
        {
            RectangleF cullBounds = RectangleF.Inflate(visibleBounds, Bandage.Size, Bandage.Size);
            using var whiteBrush = new SolidBrush(Color.FromArgb(245, 245, 238));
            using var redBrush = new SolidBrush(Color.FromArgb(205, 45, 50));
            using var outline = new Pen(Color.FromArgb(255, 35, 35, 35), 2f);

            foreach (Bandage bandage in bandages)
            {
                RectangleF bounds = new(bandage.X, bandage.Y, Bandage.Size, Bandage.Size);
                if (!cullBounds.IntersectsWith(bounds))
                    continue;

                g.FillRectangle(whiteBrush, bounds);
                g.DrawRectangle(outline, Rectangle.Round(bounds));
                g.FillRectangle(redBrush, bandage.X + 11, bandage.Y + 4, 6, 20);
                g.FillRectangle(redBrush, bandage.X + 4, bandage.Y + 11, 20, 6);
            }
        }

        private void DrawEnemies(Graphics g, List<Enemy> enemies, RectangleF visibleBounds)
        {
            foreach (Enemy enemy in enemies)
            {
                int size = enemy.RenderSize;
                RectangleF cullBounds = RectangleF.Inflate(visibleBounds, size, size);
                if (!cullBounds.IntersectsWith(new RectangleF(enemy.X, enemy.Y, size, size)))
                    continue;

                Bitmap sprite = enemy.Type switch
                {
                    EnemyType.ZombieHeavy => heavyZombieSprite,
                    EnemyType.Vampire => vampireSprite,
                    _ => fastZombieSprite
                };

                PointF center = enemy.GetCenter();
                GraphicsState state = g.Save();
                g.TranslateTransform(center.X, center.Y);
                g.RotateTransform(enemy.Angle + 90f);

                RectangleF spriteBounds = new(-size / 2f, -size / 2f, size, size);
                if (enemy.HitFlashTimer > 0)
                {
                    g.DrawImage(sprite, Rectangle.Round(spriteBounds), 0, 0,
                        sprite.Width, sprite.Height, GraphicsUnit.Pixel, hitFlashAttributes);
                }
                else
                {
                    g.DrawImage(sprite, spriteBounds.X, spriteBounds.Y, spriteBounds.Width, spriteBounds.Height);
                }

                g.Restore(state);
            }
        }

        private void DrawBullets(Graphics g, List<Bullet> bullets, RectangleF visibleBounds)
        {
            RectangleF cullBounds = RectangleF.Inflate(visibleBounds, Bullet.Size, Bullet.Size);
            foreach (Bullet bullet in bullets)
            {
                RectangleF spriteBounds = new(bullet.X, bullet.Y, Bullet.Size, Bullet.Size);
                if (!cullBounds.IntersectsWith(spriteBounds))
                    continue;

                float centerX = bullet.X + Bullet.Size / 2f;
                float centerY = bullet.Y + Bullet.Size / 2f;
                GraphicsState state = g.Save();
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(bullet.Angle + 90f);
                g.DrawImage(bulletSprite, -Bullet.Size / 2f, -Bullet.Size / 2f, Bullet.Size, Bullet.Size);
                g.Restore(state);
            }
        }

        private void DrawPlayer(Graphics g, Player player)
        {
            PointF center = player.GetCenter();
            float scaleX = Player.Width / 64f;
            float scaleY = Player.Height / 64f;
            float pivotX = Player.PivotX * scaleX;
            float pivotY = Player.PivotY * scaleY;

            GraphicsState state = g.Save();
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(player.Angle);

            if (player.HitFlashRemainingMs > 0)
            {
                g.DrawImage(playerSprite,
                    new Rectangle((int)-pivotX, (int)-pivotY, Player.Width, Player.Height),
                    0, 0, playerSprite.Width, playerSprite.Height,
                    GraphicsUnit.Pixel, hitFlashAttributes);
            }
            else
            {
                g.DrawImage(playerSprite, -pivotX, -pivotY, Player.Width, Player.Height);
            }

            g.Restore(state);
        }

        private static void DrawForcefield(Graphics g, Player player)
        {
            PointF center = player.GetCenter();
            float diameter = AbilitySystem.ForcefieldRadius * 2f;
            using var pen = new Pen(Color.FromArgb(190, 90, 210, 255), 3f);
            g.DrawEllipse(
                pen,
                center.X - AbilitySystem.ForcefieldRadius,
                center.Y - AbilitySystem.ForcefieldRadius,
                diameter,
                diameter);
        }

        private void DrawDamageNumbers(Graphics g, List<DamageNumber> damageNumbers, RectangleF visibleBounds)
        {
            foreach (DamageNumber number in damageNumbers)
            {
                if (number.DelayRemainingMs > 0 || !visibleBounds.Contains(number.X, number.Y))
                    continue;

                int alpha = Math.Clamp(number.Life * 255 / 45, 0, 255);
                Color color;
                Font font;
                string text;

                switch (number.Type)
                {
                    case CombatTextType.CriticalDamage:
                        color = Color.FromArgb(alpha, 255, 215, 0);
                        font = criticalDamageFont;
                        text = $"{number.Damage}!";
                        break;
                    case CombatTextType.KillDamage:
                        color = Color.FromArgb(alpha, 255, 60, 60);
                        font = criticalDamageFont;
                        text = number.Damage.ToString();
                        break;
                    case CombatTextType.CriticalKillDamage:
                        color = Color.FromArgb(alpha, 255, 60, 60);
                        font = criticalDamageFont;
                        text = $"{number.Damage}!";
                        break;
                    case CombatTextType.Experience:
                        color = Color.FromArgb(alpha, 150, 255, 255);
                        font = experienceFont;
                        text = $"+{number.Damage} XP";
                        break;
                    case CombatTextType.Healing:
                        color = Color.FromArgb(alpha, 95, 255, 125);
                        font = experienceFont;
                        text = $"+{number.Damage} HP";
                        break;
                    default:
                        color = Color.FromArgb(alpha, 255, 255, 255);
                        font = normalDamageFont;
                        text = number.Damage.ToString();
                        break;
                }

                using Brush brush = new SolidBrush(color);
                SizeF size = g.MeasureString(text, font);
                g.DrawString(text, font, brush, number.X - size.Width / 2f, number.Y);
            }
        }

        private static void DrawHitboxDebug(Graphics g, Player player, List<Enemy> enemies, List<Bullet> bullets)
        {
            using var playerPen = new Pen(Color.Lime, 2f);
            using var enemyPen = new Pen(Color.Red, 2f);
            using var bulletPen = new Pen(Color.Yellow, 2f);
            g.DrawRectangle(playerPen, Rectangle.Round(player.GetHitbox()));
            foreach (Enemy enemy in enemies)
                g.DrawRectangle(enemyPen, Rectangle.Round(enemy.GetHitbox()));
            foreach (Bullet bullet in bullets)
                g.DrawRectangle(bulletPen, Rectangle.Round(bullet.GetHitbox()));
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
