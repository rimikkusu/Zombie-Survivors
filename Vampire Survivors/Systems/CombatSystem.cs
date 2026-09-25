using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public class CombatSystem
    {
        public const int ContactDamage = 20;

        // Immediate XP reward granted when a normal enemy dies.
        public const int MinXpReward = 10;
        public const int MaxXpReward = 30;

        public const double CriticalChance = 0.05;

        // Visual-only delay before the floating "+XX XP" text appears.
        // XP itself is always awarded immediately on kill.
        public const int XpTextDelayMs = 250;

        // Vertical gap between the red kill number and the XP text.
        public const float XpTextVerticalOffsetPx = 30f;

        public void Shoot(Player player, List<Bullet> bullets, float mouseX, float mouseY)
        {
            if (player.IsDead)
                return;

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

            // 5% critical hit chance
            isCritical = Random.Shared.NextDouble() < CriticalChance;

            if (isCritical)
            {
                // Critical hit is randomly either 1.5x or 2x
                float criticalMultiplier =
                    Random.Shared.Next(2) == 0 ? 1.5f : 2f;

                damage = (int)MathF.Round(damage * criticalMultiplier);
            }

            return damage;
        }

        public int UpdateBullets(
            Player player,
            List<Bullet> bullets,
            List<Enemy> enemies,
            List<DamageNumber> damageNumbers,
            Size clientSize)
        {
            int totalXpAwarded = 0;

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

                        bool isLethal = enemy.Health <= 0;

                        // Exactly one damage number per hit.
                        // Lethal coloring takes priority over critical coloring.
                        CombatTextType textType = isLethal
                            ? (critical ? CombatTextType.CriticalKillDamage : CombatTextType.KillDamage)
                            : (critical ? CombatTextType.CriticalDamage : CombatTextType.NormalDamage);

                        float enemyCenterX = enemy.X + Enemy.Size / 2f;

                        damageNumbers.Add(new DamageNumber
                        {
                            X = enemyCenterX,
                            Y = enemy.Y,
                            Damage = damage,
                            Type = textType
                        });

                        // Kill enemy: award immediate random XP exactly once,
                        // then remove the enemy.
                        if (isLethal)
                        {
                            // Preserve death position before removing the enemy.
                            float deathCenterX = enemyCenterX;
                            float deathY = enemy.Y;

                            int xpReward = Random.Shared.Next(MinXpReward, MaxXpReward + 1);

                            // Awarded immediately; only the floating text is delayed.
                            player.AddExperience(xpReward);
                            totalXpAwarded += xpReward;

                            // XP text appears slightly below the kill number
                            // after a short visual delay.
                            damageNumbers.Add(new DamageNumber
                            {
                                X = deathCenterX,
                                Y = deathY + XpTextVerticalOffsetPx,
                                Damage = xpReward,
                                Type = CombatTextType.Experience,
                                DelayRemainingMs = XpTextDelayMs
                            });

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

            return totalXpAwarded;
        }

        public void UpdateDamageNumbers(List<DamageNumber> damageNumbers, int elapsedMs)
        {
            for (int i = damageNumbers.Count - 1; i >= 0; i--)
            {
                DamageNumber number = damageNumbers[i];

                // Delayed text (XP notification) counts down first;
                // it neither floats nor fades until the delay expires.
                if (number.DelayRemainingMs > 0)
                {
                    number.DelayRemainingMs -= elapsedMs;
                    continue;
                }

                // Float upward
                number.Y -= 1.5f;

                number.Life--;

                if (number.Life <= 0)
                {
                    damageNumbers.RemoveAt(i);
                }
            }
        }

        public void UpdateEnemyContactDamage(
            Player player,
            IReadOnlyList<Enemy> enemies,
            int elapsedMs)
        {
            player.UpdateDamageTimers(elapsedMs);

            if (player.IsDead)
                return;

            if (player.IsInvulnerable)
                return;

            RectangleF playerHitbox = player.GetHitbox();

            foreach (Enemy enemy in enemies)
            {
                if (playerHitbox.IntersectsWith(enemy.GetHitbox()))
                {
                    // Only one damage instance per frame;
                    // TakeDamage activates invulnerability immediately.
                    player.TakeDamage(ContactDamage);
                    break;
                }
            }
        }
    }
}
