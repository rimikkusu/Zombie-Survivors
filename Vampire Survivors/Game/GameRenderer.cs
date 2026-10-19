using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Vampire_Survivors.Entities;
using Vampire_Survivors.Systems;

namespace Vampire_Survivors.Rendering
{
    public sealed class GameRenderer : IDisposable
    {
        // Resource bitmaps are shared; only the drawing objects below are owned here.
        private readonly Bitmap playerSprite = Properties.Resources.Player;
        private readonly Bitmap fastZombieSprite = Properties.Resources.Zombie_1;
        private readonly Bitmap heavyZombieSprite = Properties.Resources.Zombie_2;
        private readonly Bitmap vampireSprite = Properties.Resources.Vampire_1;
        private readonly Bitmap bulletSprite = Properties.Resources.Bullet;
        private readonly Bitmap grassSprite = Properties.Resources.Grass;
        private readonly Font normalDamageFont = new("Arial", 14, FontStyle.Regular);
        private readonly Font criticalDamageFont = new("Arial", 18, FontStyle.Bold);
        private readonly Font experienceFont = new("Arial", 12, FontStyle.Regular);
        private readonly ImageAttributes hitFlashAttributes = CreateTint(1f, 0.25f, 0.25f, 0.45f, 0f, 0f);
        private readonly ImageAttributes criticalFlashAttributes = CreateTint(1.2f, 0.3f, 0.25f, 0.65f, 0.05f, 0f);
        private readonly Pen bandageOutline = new(Color.FromArgb(255, 35, 35, 35), 2f);
        private readonly SolidBrush bandageWhite = new(Color.FromArgb(245, 245, 238));
        private readonly SolidBrush bandageRed = new(Color.FromArgb(205, 45, 50));
        private readonly SolidBrush muzzleGlow = new(Color.FromArgb(180, 255, 185, 40));
        private readonly SolidBrush effectBrush = new(Color.White);
        private bool isDisposed;

        private static ImageAttributes CreateTint(float red, float green, float blue,
            float redOffset, float greenOffset, float blueOffset)
        {
            ImageAttributes attributes = new();
            attributes.SetColorMatrix(new ColorMatrix
            {
                Matrix00 = red, Matrix11 = green, Matrix22 = blue,
                Matrix40 = redOffset, Matrix41 = greenOffset, Matrix42 = blueOffset
            });
            return attributes;
        }

        public void Draw(Graphics g, Size arenaSize, GameSession session)
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;
            // Screen and arena positions are identical. Grass covers the entire client area.
            for (int y = 0; y < arenaSize.Height; y += 128)
            {
                for (int x = 0; x < arenaSize.Width; x += 128)
                    g.DrawImage(grassSprite, new Rectangle(x, y, 128, 128));
            }
            DrawBandages(g, session.Bandages);
            DrawEnemies(g, session.Enemies);
            DrawBullets(g, session.Bullets);
            DrawPlayer(g, session.Player);
            DrawDeathParticles(g, session.DeathParticles);
            DrawDamageNumbers(g, session.DamageNumbers);
        }

        private void DrawBandages(Graphics g, List<Bandage> bandages)
        {
            foreach (Bandage bandage in bandages)
            {
                RectangleF bounds = new(bandage.X, bandage.Y, Bandage.Size, Bandage.Size);

                g.FillRectangle(bandageWhite, bounds);
                g.DrawRectangle(bandageOutline, Rectangle.Round(bounds));
                g.FillRectangle(bandageRed, bandage.X + 11, bandage.Y + 4, 6, 20);
                g.FillRectangle(bandageRed, bandage.X + 4, bandage.Y + 11, 20, 6);
            }
        }

        private void DrawEnemies(Graphics g, List<Enemy> enemies)
        {
            foreach (Enemy enemy in enemies)
            {
                int size = enemy.RenderSize;

                Bitmap sprite = enemy.Type switch
                {
                    EnemyType.ZombieHeavy => heavyZombieSprite,
                    EnemyType.Vampire => vampireSprite,
                    _ => fastZombieSprite
                };

                PointF center = enemy.GetCenter();
                GraphicsState state = g.Save();
                // Rotating around the center keeps the sprite in place while it turns.
                g.TranslateTransform(center.X, center.Y);
                g.RotateTransform(enemy.Angle + 90f);

                RectangleF spriteBounds = new(-size / 2f, -size / 2f, size, size);
                ImageAttributes? tint = null;
                if (enemy.HitFlashTimer > 0)
                    tint = enemy.CriticalHitFlash ? criticalFlashAttributes : hitFlashAttributes;
                DrawSprite(g, sprite, spriteBounds, tint);

                g.Restore(state);
            }
        }

        private void DrawBullets(Graphics g, List<Bullet> bullets)
        {
            foreach (Bullet bullet in bullets)
            {
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
            float shotStrength = player.ShotFeedbackRemainingMs / (float)Player.ShotFeedbackDurationMs;
            g.TranslateTransform(-3f * shotStrength, 0f);

            DrawSprite(g, playerSprite, new RectangleF(-pivotX, -pivotY, Player.Width, Player.Height),
                player.HitFlashRemainingMs > 0 ? hitFlashAttributes : null);

            if (player.ShotFeedbackRemainingMs > 0)
            {
                float muzzleX = (Player.GunMuzzleX - Player.PivotX) * scaleX;
                float muzzleY = (Player.GunMuzzleY - Player.PivotY) * scaleY;
                float radius = 6f + 3f * shotStrength;
                g.FillEllipse(muzzleGlow, muzzleX - radius, muzzleY - radius, radius * 2f, radius * 2f);
                g.FillEllipse(Brushes.LightYellow,
                    muzzleX - radius * 0.48f, muzzleY - radius * 0.48f,
                    radius * 0.96f, radius * 0.96f);
            }

            g.Restore(state);
        }

        private static void DrawSprite(Graphics g, Bitmap sprite, RectangleF bounds, ImageAttributes? tint)
        {
            // Player and enemy hit flashes use the same cached color tint.
            if (tint is null)
                g.DrawImage(sprite, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            else
                g.DrawImage(sprite, Rectangle.Round(bounds), 0, 0,
                    sprite.Width, sprite.Height, GraphicsUnit.Pixel, tint);
        }

        private void DrawDeathParticles(Graphics g, List<DeathParticle> particles)
        {
            foreach (DeathParticle particle in particles)
            {
                int alpha = Math.Clamp(particle.RemainingMs * 230 / DeathParticle.LifetimeMs, 0, 230);
                Color color = particle.IsDark
                    ? Color.FromArgb(alpha, 55, 16, 20)
                    : Color.FromArgb(alpha, 190, 42, 48);
                effectBrush.Color = color;
                g.FillRectangle(effectBrush, particle.X, particle.Y, particle.Size, particle.Size);
            }
        }

        private void DrawDamageNumbers(Graphics g, List<DamageNumber> damageNumbers)
        {
            foreach (DamageNumber number in damageNumbers)
            {
                if (number.DelayRemainingMs > 0)
                    continue;

                int alpha = Math.Clamp(number.Life * 255 / 45, 0, 255);
                Color color = Color.White;
                Font font = normalDamageFont;
                string text = number.Damage.ToString();
                switch (number.Type)
                {
                    case CombatTextType.CriticalDamage:
                        color = Color.Gold;
                        font = criticalDamageFont;
                        text = $"{number.Damage}!";
                        break;
                    case CombatTextType.KillDamage:
                    case CombatTextType.CriticalKillDamage:
                        color = Color.FromArgb(255, 60, 60);
                        font = criticalDamageFont;
                        if (number.Type == CombatTextType.CriticalKillDamage)
                            text = $"{number.Damage}!";
                        break;
                    case CombatTextType.Experience:
                        color = Color.FromArgb(150, 255, 255);
                        font = experienceFont;
                        text = $"+{number.Damage} XP";
                        break;
                    case CombatTextType.Healing:
                        color = Color.FromArgb(95, 255, 125);
                        font = experienceFont;
                        text = $"+{number.Damage} HP";
                        break;
                }
                effectBrush.Color = Color.FromArgb(alpha, color);
                SizeF size = g.MeasureString(text, font);
                g.DrawString(text, font, effectBrush, number.X - size.Width / 2f, number.Y);
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
            criticalFlashAttributes.Dispose();
            bandageOutline.Dispose();
            bandageWhite.Dispose();
            bandageRed.Dispose();
            muzzleGlow.Dispose();
            effectBrush.Dispose();
        }
    }
}
