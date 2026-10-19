using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public sealed class CombatSystem
    {
        public const int XpTextDelayMs = 250;
        private readonly GameSession game;
        public CombatSystem(GameSession game) => this.game = game;

        public void Shoot(PointF cursor)
        {
            Player player = game.Player;
            if (player.IsDead || player.ShootCooldownRemainingMs > 0)
                return;
            player.UpdateAngle(cursor.X, cursor.Y);
            PointF center = player.GetCenter();
            PointF muzzle = player.GetGunMuzzlePosition();
            float dx = cursor.X - muzzle.X;
            float dy = cursor.Y - muzzle.Y;
            float length = MathF.Sqrt(dx * dx + dy * dy);
            // A cursor inside the gun's reach must not make a shot point backwards.
            if (EnemyManager.Distance(center, cursor) <= EnemyManager.Distance(center, muzzle))
            {
                dx = MathF.Cos(player.Angle * MathF.PI / 180f);
                dy = MathF.Sin(player.Angle * MathF.PI / 180f);
                length = 1f;
            }
            if (length < 0.001f)
                return;
            dx /= length;
            dy /= length;
            // Check the body-to-muzzle path so a touching enemy cannot be skipped.
            Enemy? closeEnemy = FindHit(center, muzzle, true);
            if (closeEnemy is not null)
                ApplyBulletHit(closeEnemy, dx, dy);
            else
                game.Bullets.Add(new Bullet
                {
                    X = muzzle.X - Bullet.Size / 2f, Y = muzzle.Y - Bullet.Size / 2f,
                    VelocityX = dx * player.Stats.BulletSpeed,
                    VelocityY = dy * player.Stats.BulletSpeed,
                    Angle = MathF.Atan2(dy, dx) * 180f / MathF.PI
                });
            player.ShootCooldownRemainingMs = player.Stats.FireCooldownMs;
            player.RegisterShot();
        }

        public void UpdateBullets()
        {
            // Move backwards through the list because hits remove bullets and enemies.
            for (int i = game.Bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = game.Bullets[i];
                PointF start = new(bullet.X + Bullet.Size / 2f, bullet.Y + Bullet.Size / 2f);
                bullet.X += bullet.VelocityX;
                bullet.Y += bullet.VelocityY;
                PointF end = new(bullet.X + Bullet.Size / 2f, bullet.Y + Bullet.Size / 2f);
                Enemy? enemy = FindHit(start, end);
                if (enemy is not null)
                    ApplyBulletHit(enemy, bullet.VelocityX, bullet.VelocityY);
                if (enemy is not null || end.X < 0 || end.Y < 0 ||
                    end.X > game.ArenaSize.Width || end.Y > game.ArenaSize.Height)
                    game.Bullets.RemoveAt(i);
            }
        }

        private Enemy? FindHit(PointF start, PointF end, bool closeRange = false)
        {
            Enemy? nearest = null;
            float nearestFraction = float.MaxValue;
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            foreach (Enemy enemy in game.Enemies)
            {
                PointF center = enemy.GetCenter();
                // Close-range hits only count on the side the player is facing.
                if (closeRange && (center.X - start.X) * (end.X - start.X) +
                    (center.Y - start.Y) * (end.Y - start.Y) < 0)
                    continue;
                RectangleF body = enemy.GetHitbox();
                if (!closeRange)
                    body.Inflate(Bullet.HitboxWidth / 2f, Bullet.HitboxHeight / 2f);
                float minimum = 0f;
                float maximum = 1f;
                // Sweep the full path, rather than testing only the bullet's new position.
                if (ClipSegment(-dx, start.X - body.Left, ref minimum, ref maximum) &&
                    ClipSegment(dx, body.Right - start.X, ref minimum, ref maximum) &&
                    ClipSegment(-dy, start.Y - body.Top, ref minimum, ref maximum) &&
                    ClipSegment(dy, body.Bottom - start.Y, ref minimum, ref maximum) &&
                    minimum < nearestFraction)
                {
                    nearest = enemy;
                    nearestFraction = minimum;
                }
            }
            return nearest;
        }

        private static bool ClipSegment(float direction, float edgeDistance,
            ref float minimum, ref float maximum)
        {
            // Keep the part of the bullet path that lies inside this side of the hitbox.
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

        public int CalculateBulletDamage(out bool critical)
        {
            PlayerStats stats = game.Player.Stats;
            int raw = Random.Shared.Next(stats.BaseDamageMin, stats.BaseDamageMax + 1);
            int damage = stats.CalculateDamage(raw);
            critical = Random.Shared.NextDouble() < stats.EffectiveCritChance;
            if (critical)
                damage = (int)MathF.Round(damage * (Random.Shared.Next(2) == 0 ? 1.5f : 2f));
            return Math.Max(1, damage);
        }

        private void ApplyBulletHit(Enemy enemy, float directionX, float directionY)
        {
            int damage = CalculateBulletDamage(out bool critical);
            ApplyEnemyDamage(enemy, damage, critical);
            if (enemy.Health <= 0)
            {
                game.Enemies.Remove(enemy);
                return;
            }
            enemy.StaggerRemainingMs = enemy.Stats.StaggerMs;
            float knockback = enemy.Stats.Knockback * (critical ? 1.5f : 1f);
            float length = MathF.Sqrt(directionX * directionX + directionY * directionY);
            if (length > 0.001f)
            {
                enemy.X += directionX / length * knockback;
                enemy.Y += directionY / length * knockback;
                enemy.ClampToArena(game.ArenaSize);
            }
        }

        private void ApplyEnemyDamage(Enemy enemy, int damage, bool critical)
        {
            if (enemy.Health <= 0)
                return;
            enemy.Health -= damage;
            enemy.HitFlashTimer = critical ? 9 : 7;
            enemy.CriticalHitFlash = critical;
            bool killed = enemy.Health <= 0;
            CombatTextType type = critical ? CombatTextType.CriticalDamage : CombatTextType.NormalDamage;
            if (killed)
                type = critical ? CombatTextType.CriticalKillDamage : CombatTextType.KillDamage;
            PointF center = enemy.GetCenter();
            game.DamageNumbers.Add(new DamageNumber
            {
                X = center.X, Y = enemy.Y, Damage = damage, Type = type
            });
            if (!killed)
                return;

            SpawnDeathBurst(center);
            int xp = enemy.RollExperienceReward();
            game.Player.AddExperience(xp); // Award XP now; delay only its floating notification.
            if (Random.Shared.NextDouble() < 0.10)
                game.Bandages.Add(new Bandage(center));
            game.DamageNumbers.Add(new DamageNumber
            {
                X = center.X, Y = enemy.Y + 30f, Damage = xp,
                Type = CombatTextType.Experience, DelayRemainingMs = XpTextDelayMs
            });
        }

        private void SpawnDeathBurst(PointF center)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = i * MathF.Tau / 6f + Random.Shared.NextSingle() * 0.35f;
                float speed = 1.8f + Random.Shared.NextSingle() * 2.3f;
                game.DeathParticles.Add(new DeathParticle
                {
                    X = center.X, Y = center.Y,
                    VelocityX = MathF.Cos(angle) * speed, VelocityY = MathF.Sin(angle) * speed,
                    Size = 4f + Random.Shared.NextSingle() * 3f, IsDark = i % 2 == 0
                });
            }
        }

        public void UpdateEffectsAndContact(int elapsedMs, bool preventDamage)
        {
            for (int i = game.DeathParticles.Count - 1; i >= 0; i--)
            {
                game.DeathParticles[i].Update(elapsedMs);
                if (game.DeathParticles[i].RemainingMs == 0)
                    game.DeathParticles.RemoveAt(i);
            }
            for (int i = game.DamageNumbers.Count - 1; i >= 0; i--)
            {
                DamageNumber number = game.DamageNumbers[i];
                if (number.DelayRemainingMs > 0)
                {
                    number.DelayRemainingMs -= elapsedMs;
                    continue;
                }
                number.Y -= 1.5f;
                if (--number.Life <= 0)
                    game.DamageNumbers.RemoveAt(i);
            }
            Player player = game.Player;
            player.UpdateDamageTimers(elapsedMs);
            if (player.IsDead || player.IsInvulnerable || preventDamage)
                return;
            RectangleF playerHitbox = player.GetHitbox();
            foreach (Enemy enemy in game.Enemies)
            {
                if (playerHitbox.IntersectsWith(enemy.GetHitbox()))
                {
                    player.TakeDamage(enemy.Stats.ContactDamage);
                    break; // One hit starts the player's short invulnerability timer.
                }
            }
        }
    }
}
