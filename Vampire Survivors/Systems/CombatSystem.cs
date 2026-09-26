using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public class CombatSystem
    {
        public const double BandageDropChance = 0.20d;

        // Visual-only delay before the floating "+XX XP" text appears.
        // XP itself is always awarded immediately on kill.
        public const int XpTextDelayMs = 250;

        // Vertical gap between the red kill number and the XP text.
        public const float XpTextVerticalOffsetPx = 30f;

        public bool Shoot(
            Player player,
            List<Bullet> bullets,
            float mouseX,
            float mouseY,
            float angleOffsetDegrees = 0f,
            bool bypassCooldown = false,
            List<Enemy>? enemies = null,
            List<DamageNumber>? damageNumbers = null,
            List<Bandage>? bandages = null)
        {
            if (player.IsDead || (!bypassCooldown && player.ShootCooldownRemainingMs > 0))
                return false;

            PointF muzzle = player.GetGunMuzzlePosition();
            float muzzleX = muzzle.X;
            float muzzleY = muzzle.Y;

            // Shoot from the muzzle toward the mouse
            float dx = mouseX - muzzleX;
            float dy = mouseY - muzzleY;

            float length = MathF.Sqrt(dx * dx + dy * dy);

            if (length <= 0.001f)
                return false;

            dx /= length;
            dy /= length;

            if (MathF.Abs(angleOffsetDegrees) > 0.001f)
            {
                float offsetRadians = angleOffsetDegrees * MathF.PI / 180f;
                float cos = MathF.Cos(offsetRadians);
                float sin = MathF.Sin(offsetRadians);
                (dx, dy) = (dx * cos - dy * sin, dx * sin + dy * cos);
            }

            if (enemies is not null && damageNumbers is not null &&
                TryFindMuzzleObstruction(player, muzzle, dx, dy, enemies, out Enemy? obstructingEnemy))
            {
                _ = ApplyBulletHit(player, obstructingEnemy!, damageNumbers, bandages);
                if (obstructingEnemy!.Health <= 0)
                    enemies.Remove(obstructingEnemy);

                player.ShootCooldownRemainingMs = player.Stats.FireCooldownMs;
                return true;
            }

            float bulletSpeed = player.Stats.BulletSpeed;

            Bullet bullet = new Bullet
            {
                X = muzzleX - Bullet.Size / 2f,
                Y = muzzleY - Bullet.Size / 2f,

                VelocityX = dx * bulletSpeed,
                VelocityY = dy * bulletSpeed,

                Angle = MathF.Atan2(dy, dx) * 180f / MathF.PI
            };

            bullets.Add(bullet);
            player.ShootCooldownRemainingMs = player.Stats.FireCooldownMs;
            return true;
        }

        public int CalculateBulletDamage(Player player, out bool isCritical)
        {
            int rawBase = Random.Shared.Next(player.Stats.BaseDamageMin, player.Stats.BaseDamageMax + 1);
            int damage = player.Stats.CalculateDamage(rawBase);

            // Critical hit check based on player stats (base 2.5%, capped at 35%)
            isCritical = Random.Shared.NextDouble() < player.Stats.EffectiveCritChance;

            if (isCritical)
            {
                // Critical hit multiplier is 1.5x or 2.0x
                float criticalMultiplier =
                    Random.Shared.Next(2) == 0 ? 1.5f : 2f;

                damage = (int)MathF.Round(damage * criticalMultiplier);
            }

            return Math.Max(1, damage);
        }

        public int UpdateBullets(
            Player player,
            List<Bullet> bullets,
            List<Enemy> enemies,
            List<DamageNumber> damageNumbers,
            Size worldSize,
            List<Bandage>? bandages = null)
        {
            int totalXpAwarded = 0;

            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];
                PointF previousPosition = new(
                    bullet.X + Bullet.Size / 2f,
                    bullet.Y + Bullet.Size / 2f);

                bullet.X += bullet.VelocityX;
                bullet.Y += bullet.VelocityY;
                PointF currentPosition = new(
                    bullet.X + Bullet.Size / 2f,
                    bullet.Y + Bullet.Size / 2f);

                Enemy? hitEnemy = null;
                float nearestHit = float.MaxValue;
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    Enemy enemy = enemies[j];
                    RectangleF expandedHitbox = RectangleF.Inflate(
                        enemy.GetHitbox(),
                        Bullet.HitboxWidth / 2f,
                        Bullet.HitboxHeight / 2f);

                    if (TryGetSegmentIntersectionDistance(
                            previousPosition,
                            currentPosition,
                            expandedHitbox,
                            out float hitDistance) &&
                        hitDistance < nearestHit)
                    {
                        hitEnemy = enemy;
                        nearestHit = hitDistance;
                    }
                }

                if (hitEnemy is not null)
                {
                    totalXpAwarded += ApplyBulletHit(player, hitEnemy, damageNumbers, bandages);
                    if (hitEnemy.Health <= 0)
                        enemies.Remove(hitEnemy);

                    bullets.RemoveAt(i);
                    continue;
                }

                // Bullets use world coordinates and are removed at the finite map edge.
                if (bullet.X < -Bullet.Size ||
                    bullet.X > worldSize.Width ||
                    bullet.Y < -Bullet.Size ||
                    bullet.Y > worldSize.Height)
                {
                    bullets.RemoveAt(i);
                }
            }

            return totalXpAwarded;
        }

        private int ApplyBulletHit(
            Player player,
            Enemy enemy,
            List<DamageNumber> damageNumbers,
            List<Bandage>? bandages)
        {
            int damage = CalculateBulletDamage(player, out bool critical);
            return ApplyEnemyDamage(player, enemy, damage, critical, damageNumbers, bandages);
        }

        private static bool TryFindMuzzleObstruction(
            Player player,
            PointF muzzle,
            float shotDirectionX,
            float shotDirectionY,
            IReadOnlyList<Enemy> enemies,
            out Enemy? closestEnemy)
        {
            PointF origin = player.GetCenter();
            closestEnemy = null;
            float closestHit = float.MaxValue;

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                PointF enemyCenter = enemy.GetCenter();
                float directionProjection =
                    (enemyCenter.X - origin.X) * shotDirectionX +
                    (enemyCenter.Y - origin.Y) * shotDirectionY;

                // An enemy centered behind the player must not be hit just
                // because its hitbox reaches the beginning of this segment.
                if (directionProjection < 0f)
                    continue;

                if (TryGetSegmentIntersectionDistance(origin, muzzle, enemy.GetHitbox(), out float hitDistance) &&
                    hitDistance < closestHit)
                {
                    closestEnemy = enemy;
                    closestHit = hitDistance;
                }
            }

            return closestEnemy is not null;
        }

        private static bool TryGetSegmentIntersectionDistance(
            PointF start,
            PointF end,
            RectangleF rectangle,
            out float distanceAlongSegment)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float minimum = 0f;
            float maximum = 1f;

            if (!ClipSegment(-dx, start.X - rectangle.Left, ref minimum, ref maximum) ||
                !ClipSegment(dx, rectangle.Right - start.X, ref minimum, ref maximum) ||
                !ClipSegment(-dy, start.Y - rectangle.Top, ref minimum, ref maximum) ||
                !ClipSegment(dy, rectangle.Bottom - start.Y, ref minimum, ref maximum))
            {
                distanceAlongSegment = 0f;
                return false;
            }

            distanceAlongSegment = minimum * MathF.Sqrt(dx * dx + dy * dy);
            return true;
        }

        private static bool ClipSegment(float p, float q, ref float minimum, ref float maximum)
        {
            if (MathF.Abs(p) < 0.0001f)
                return q >= 0f;

            float intersection = q / p;
            if (p < 0f)
            {
                if (intersection > maximum)
                    return false;

                minimum = Math.Max(minimum, intersection);
            }
            else
            {
                if (intersection < minimum)
                    return false;

                maximum = Math.Min(maximum, intersection);
            }

            return true;
        }

        public int ApplyEnemyDamage(
            Player player,
            Enemy enemy,
            int damage,
            bool isCritical,
            List<DamageNumber> damageNumbers,
            List<Bandage>? bandages = null)
        {
            enemy.Health -= damage;
            enemy.HitFlashTimer = 7;

            bool isLethal = enemy.Health <= 0;
            CombatTextType textType = isLethal
                ? (isCritical ? CombatTextType.CriticalKillDamage : CombatTextType.KillDamage)
                : (isCritical ? CombatTextType.CriticalDamage : CombatTextType.NormalDamage);
            PointF center = enemy.GetCenter();

            damageNumbers.Add(new DamageNumber
            {
                X = center.X,
                Y = enemy.Y,
                Damage = damage,
                Type = textType
            });

            if (!isLethal)
                return 0;

            int xpReward = enemy.RollExperienceReward();
            player.AddExperience(xpReward);
            if (bandages is not null && Random.Shared.NextDouble() < BandageDropChance)
                bandages.Add(new Bandage(center));

            damageNumbers.Add(new DamageNumber
            {
                X = center.X,
                Y = enemy.Y + XpTextVerticalOffsetPx,
                Damage = xpReward,
                Type = CombatTextType.Experience,
                DelayRemainingMs = XpTextDelayMs
            });

            return xpReward;
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
            int elapsedMs,
            bool preventDamage = false)
        {
            player.UpdateDamageTimers(elapsedMs);

            if (player.IsDead || preventDamage)
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
                    player.TakeDamage(enemy.Stats.ContactDamage);
                    break;
                }
            }
        }
    }
}
