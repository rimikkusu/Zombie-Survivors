using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public class CombatSystem
    {
        public void Shoot(Player player, List<Bullet> bullets, float mouseX, float mouseY)
        {
            PointF muzzle = player.GetGunMuzzlePosition();
            float muzzleX = muzzle.X;
            float muzzleY = muzzle.Y;

            // Shoot from the muzzle toward the mouse
            float dx = mouseX - muzzleX;
            float dy = mouseY - muzzleY;

            float length = MathF.Sqrt(dx * dx + dy * dy);

            if (length <= 0.001f)
                return;

            dx /= length;
            dy /= length;

            Bullet bullet = new Bullet
            {
                X = muzzleX - Bullet.Size / 2f,
                Y = muzzleY - Bullet.Size / 2f,

                VelocityX = dx * Bullet.Speed,
                VelocityY = dy * Bullet.Speed,

                Angle = MathF.Atan2(dy, dx) * 180f / MathF.PI
            };

            bullets.Add(bullet);
        }

        public int CalculateBulletDamage(out bool isCritical)
        {
            // Normal hit = 50 to 60 damage
            int damage = Random.Shared.Next(50, 61);

            // 20% critical hit chance
            isCritical = Random.Shared.NextDouble() < 0.20;

            if (isCritical)
            {
                // Critical hit is randomly either 1.5x or 2x
                float criticalMultiplier =
                    Random.Shared.Next(2) == 0 ? 1.5f : 2f;

                damage = (int)MathF.Round(damage * criticalMultiplier);
            }

            return damage;
        }

        public void UpdateBullets(
            List<Bullet> bullets,
            List<Enemy> enemies,
            List<DamageNumber> damageNumbers,
            Size clientSize)
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];

                bullet.X += bullet.VelocityX;
                bullet.Y += bullet.VelocityY;

                bool bulletHitSomething = false;

                RectangleF bulletBounds = new RectangleF(
                    bullet.X,
                    bullet.Y,
                    Bullet.Size,
                    Bullet.Size
                );

                // Check against every enemy
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    Enemy enemy = enemies[j];

                    RectangleF enemyBounds = new RectangleF(
                        enemy.X,
                        enemy.Y,
                        Enemy.Size,
                        Enemy.Size
                    );

                    if (bulletBounds.IntersectsWith(enemyBounds))
                    {
                        int damage = CalculateBulletDamage(out bool critical);

                        enemy.Health -= damage;

                        // Flash enemy red
                        enemy.HitFlashTimer = 7;

                        // Create floating damage number
                        damageNumbers.Add(new DamageNumber
                        {
                            X = enemy.X + Enemy.Size / 2f,
                            Y = enemy.Y,
                            Damage = damage,
                            IsCritical = critical
                        });

                        // Kill enemy
                        if (enemy.Health <= 0)
                        {
                            enemies.RemoveAt(j);
                        }

                        bullets.RemoveAt(i);
                        bulletHitSomething = true;

                        break;
                    }
                }

                if (bulletHitSomething)
                    continue;

                // Remove bullets when they leave the screen
                if (bullet.X < -Bullet.Size ||
                    bullet.X > clientSize.Width ||
                    bullet.Y < -Bullet.Size ||
                    bullet.Y > clientSize.Height)
                {
                    bullets.RemoveAt(i);
                }
            }
        }

        public void UpdateDamageNumbers(List<DamageNumber> damageNumbers)
        {
            for (int i = damageNumbers.Count - 1; i >= 0; i--)
            {
                DamageNumber number = damageNumbers[i];

                // Float upward
                number.Y -= 1.5f;

                number.Life--;

                if (number.Life <= 0)
                {
                    damageNumbers.RemoveAt(i);
                }
            }
        }
    }
}
