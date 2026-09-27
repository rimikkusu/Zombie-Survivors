using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Vampire_Survivors.Entities;
using Vampire_Survivors.Systems;

namespace Vampire_Survivors.Rendering
{
    public sealed class GameRenderer : IDisposable
    {
        private static readonly bool DrawHitboxes = false;

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
        private readonly ImageAttributes telegraphAttributes = CreateTint(1.1f, 0.65f, 0.65f, 0.23f, 0.04f, 0.04f);
        private readonly Pen border = new(Color.FromArgb(220, 38, 44, 42), 8f);
        private readonly Pen bandageOutline = new(Color.FromArgb(255, 35, 35, 35), 2f);
        private readonly Pen warningPen = new(Color.FromArgb(225, 255, 85, 65), 2f);
        private readonly Pen forcefieldPen = new(Color.FromArgb(190, 90, 210, 255), 3f);
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

        public void Draw(Graphics graphics, Size viewport, GameSession session)
        {
            Draw(graphics, viewport, session.Camera, session.Player, session.Enemies,
                session.Bullets, session.DamageNumbers,
                session.Abilities.IsActive(MajorAbilityType.Forcefield),
                session.Bandages, session.DeathParticles);
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
            List<Bandage>? bandages = null,
            List<DeathParticle>? deathParticles = null)
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.FromArgb(12, 16, 14));

            GraphicsState worldState = g.Save();
            // Everything below uses world positions; the camera shifts it onto the screen.
            g.TranslateTransform(-camera.X, -camera.Y);
            g.SetClip(
                new RectangleF(0, 0, GameWorld.WorldWidth, GameWorld.WorldHeight),
                CombineMode.Intersect);

            RectangleF visibleBounds = camera.GetVisibleWorldBounds(viewportSize);
            // Draw the ground first, then pickups, enemies, bullets, effects, and borders.
            DrawGrass(g, viewportSize, camera);
            if (bandages is not null)
                DrawBandages(g, bandages, visibleBounds);
            DrawEnemies(g, enemies, visibleBounds);
            DrawBullets(g, bullets, visibleBounds);
            DrawPlayer(g, player);
            if (forcefieldActive)
                DrawForcefield(g, player);
            if (deathParticles is not null)
                DrawDeathParticles(g, deathParticles, visibleBounds);
            DrawDamageNumbers(g, damageNumbers, visibleBounds);
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
            g.DrawRectangle(border, 4, 4, GameWorld.WorldWidth - 8, GameWorld.WorldHeight - 8);
        }

        private void DrawBandages(Graphics g, List<Bandage> bandages, RectangleF visibleBounds)
        {
            RectangleF cullBounds = RectangleF.Inflate(visibleBounds, Bandage.Size, Bandage.Size);

            foreach (Bandage bandage in bandages)
            {
                RectangleF bounds = new(bandage.X, bandage.Y, Bandage.Size, Bandage.Size);
                if (!cullBounds.IntersectsWith(bounds))
                    continue;

                g.FillRectangle(bandageWhite, bounds);
                g.DrawRectangle(bandageOutline, Rectangle.Round(bounds));
                g.FillRectangle(bandageRed, bandage.X + 11, bandage.Y + 4, 6, 20);
                g.FillRectangle(bandageRed, bandage.X + 4, bandage.Y + 11, 20, 6);
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
                // Rotating around the center keeps the sprite in place while it turns.
                g.TranslateTransform(center.X, center.Y);
                g.RotateTransform(enemy.Angle + 90f);

                RectangleF spriteBounds = new(-size / 2f, -size / 2f, size, size);
                if (enemy.HitFlashTimer > 0)
                {
                    ImageAttributes attributes = enemy.CriticalHitFlash
                        ? criticalFlashAttributes
                        : hitFlashAttributes;
                    g.DrawImage(sprite, Rectangle.Round(spriteBounds), 0, 0,
                        sprite.Width, sprite.Height, GraphicsUnit.Pixel, attributes);
                }
                else if (enemy.BehaviorState == EnemyBehaviorState.Windup)
                {
                    g.DrawImage(sprite, Rectangle.Round(spriteBounds), 0, 0,
                        sprite.Width, sprite.Height, GraphicsUnit.Pixel, telegraphAttributes);
                }
                else
                {
                    g.DrawImage(sprite, spriteBounds.X, spriteBounds.Y, spriteBounds.Width, spriteBounds.Height);
                }

                if (enemy.BehaviorState == EnemyBehaviorState.Windup)
                {
                    float warningWidth = enemy.Stats.HitboxWidth + 14f;
                    float warningHeight = enemy.Stats.HitboxHeight + 14f;
                    g.DrawEllipse(warningPen,
                        -warningWidth / 2f, -warningHeight / 2f,
                        warningWidth, warningHeight);
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
            float shotStrength = player.ShotFeedbackRemainingMs / (float)Player.ShotFeedbackDurationMs;
            g.TranslateTransform(-3f * shotStrength, 0f);

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

        private void DrawDeathParticles(Graphics g, List<DeathParticle> particles, RectangleF visibleBounds)
        {
            foreach (DeathParticle particle in particles)
            {
                if (!visibleBounds.Contains(particle.X, particle.Y))
                    continue;

                int alpha = Math.Clamp(particle.RemainingMs * 230 / DeathParticle.LifetimeMs, 0, 230);
                Color color = particle.IsDark
                    ? Color.FromArgb(alpha, 55, 16, 20)
                    : Color.FromArgb(alpha, 190, 42, 48);
                effectBrush.Color = color;
                g.FillRectangle(effectBrush, particle.X, particle.Y, particle.Size, particle.Size);
            }
        }

        private void DrawForcefield(Graphics g, Player player)
        {
            PointF center = player.GetCenter();
            float diameter = AbilitySystem.ForcefieldRadius * 2f;
            g.DrawEllipse(
                forcefieldPen,
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
            criticalFlashAttributes.Dispose();
            telegraphAttributes.Dispose();
            border.Dispose();
            bandageOutline.Dispose();
            warningPen.Dispose();
            forcefieldPen.Dispose();
            bandageWhite.Dispose();
            bandageRed.Dispose();
            muzzleGlow.Dispose();
            effectBrush.Dispose();
        }
    }
}
