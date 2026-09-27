using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public sealed class ProjectileSystem
    {
        private readonly CombatSystem combat;

        public ProjectileSystem(CombatSystem combat) => this.combat = combat;

        public bool Shoot(
            Player player,
            List<Bullet> bullets,
            float mouseX,
            float mouseY,
            float angleOffsetDegrees = 0f,
            bool bypassCooldown = false,
            List<Enemy>? enemies = null,
            List<DamageNumber>? damageNumbers = null,
            List<Bandage>? bandages = null,
            List<DeathParticle>? deathParticles = null,
            Size? worldSize = null)
        {
            if (player.IsDead || (!bypassCooldown && player.ShootCooldownRemainingMs > 0))
                return false;

            PointF muzzle = player.GetGunMuzzlePosition();
            float dx = mouseX - muzzle.X;
            float dy = mouseY - muzzle.Y;

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

            // Check between the player's body and the gun so a close enemy can be hit.
            if (enemies is not null && damageNumbers is not null &&
                TryFindMuzzleObstruction(player, muzzle, dx, dy, enemies, out Enemy? obstructingEnemy))
            {
                _ = ApplyBulletHit(player, obstructingEnemy!, dx, dy,
                    worldSize ?? new Size(GameWorld.WorldWidth, GameWorld.WorldHeight),
                    damageNumbers, bandages, deathParticles);
                if (obstructingEnemy!.Health <= 0)
                    enemies.Remove(obstructingEnemy);
            }
            else
            {
                bullets.Add(new Bullet
                {
                    X = muzzle.X - Bullet.Size / 2f,
                    Y = muzzle.Y - Bullet.Size / 2f,
                    VelocityX = dx * player.Stats.BulletSpeed,
                    VelocityY = dy * player.Stats.BulletSpeed,
                    Angle = MathF.Atan2(dy, dx) * 180f / MathF.PI
                });
            }

            player.ShootCooldownRemainingMs = player.Stats.FireCooldownMs;
            player.RegisterShot();
            return true;
        }

        public int UpdateBullets(
            Player player,
            List<Bullet> bullets,
            List<Enemy> enemies,
            List<DamageNumber> damageNumbers,
            Size worldSize,
            List<Bandage>? bandages = null,
            List<DeathParticle>? deathParticles = null)
        {
            int totalXpAwarded = 0;

            // Go backwards because a bullet is removed when it hits or leaves the map.
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

                // The segment from the old position to the new one catches fast bullets.
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
                    totalXpAwarded += ApplyBulletHit(player, hitEnemy,
                        bullet.VelocityX, bullet.VelocityY, worldSize,
                        damageNumbers, bandages, deathParticles);
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
            float directionX,
            float directionY,
            Size worldSize,
            List<DamageNumber> damageNumbers,
            List<Bandage>? bandages,
            List<DeathParticle>? deathParticles)
        {
            int damage = combat.CalculateBulletDamage(player, out bool critical);
            int xpAwarded = combat.ApplyEnemyDamage(player, enemy, damage, critical,
                damageNumbers, bandages, deathParticles);

            if (enemy.Health > 0)
            {
                enemy.StaggerRemainingMs = enemy.Stats.StaggerMs;
                float knockback = enemy.Stats.Knockback;
                if (critical)
                    knockback *= 1.5f;

                float length = MathF.Sqrt(directionX * directionX + directionY * directionY);
                if (length > 0.001f)
                {
                    enemy.X += directionX / length * knockback;
                    enemy.Y += directionY / length * knockback;
                    enemy.ClampToWorld(worldSize);
                }
            }

            return xpAwarded;
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
            // Clip the bullet path to each side of the rectangular hitbox.
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

        private static bool ClipSegment(float direction, float edgeDistance,
            ref float minimum, ref float maximum)
        {
            if (MathF.Abs(direction) < 0.0001f)
                return edgeDistance >= 0f;

            float intersection = edgeDistance / direction;
            if (direction < 0f)
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

    }
}
