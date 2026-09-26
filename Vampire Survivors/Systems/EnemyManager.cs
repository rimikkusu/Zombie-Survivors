using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public class EnemyManager
    {
        private const float MinimumPlayerSpawnDistance = 300f;
        private static readonly float[] ForcefieldPushAngles =
        {
            0f,
            MathF.PI / 4f,
            -MathF.PI / 4f,
            MathF.PI / 2f,
            -MathF.PI / 2f,
            3f * MathF.PI / 4f,
            -3f * MathF.PI / 4f,
            MathF.PI
        };

        public void Update(
            List<Enemy> enemies,
            PointF playerCenter,
            Size worldSize,
            float forcefieldRadius = 0f,
            RectangleF? playerHitbox = null)
        {
            foreach (Enemy enemy in enemies)
            {
                PointF center = enemy.GetCenter();
                float dx = playerCenter.X - center.X;
                float dy = playerCenter.Y - center.Y;
                float distance = MathF.Sqrt(dx * dx + dy * dy);

                enemy.Angle = MathF.Atan2(dy, dx) * 180f / MathF.PI;

                if (distance > 0.001f)
                {
                    enemy.X += dx / distance * enemy.Stats.Speed;
                    enemy.Y += dy / distance * enemy.Stats.Speed;
                }

                ClampToWorld(enemy, worldSize);
            }

            SeparateOverlappingEnemies(enemies, worldSize);

            if (playerHitbox is RectangleF hitbox)
                SeparateEnemiesFromPlayer(enemies, hitbox, worldSize);

            if (forcefieldRadius > 0f)
                PushEnemiesOutsideRadius(enemies, playerCenter, forcefieldRadius, worldSize);
        }

        public void UpdateHitFlashTimers(List<Enemy> enemies)
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy.HitFlashTimer > 0)
                    enemy.HitFlashTimer--;
            }
        }

        public bool TrySpawnEnemy(
            List<Enemy> enemies,
            Size worldSize,
            RectangleF visibleWorldBounds,
            PointF playerCenter,
            int playerLevel,
            int wave,
            int spawnIndex)
        {
            EnemyType type = ChooseEnemyType(wave, spawnIndex);
            EnemyStats stats = EnemyStats.GetStats(type);
            int size = stats.RenderSize;

            if (worldSize.Width <= size || worldSize.Height <= size)
                return false;

            for (int attempt = 0; attempt < 80; attempt++)
            {
                float x = Random.Shared.Next(0, worldSize.Width - size + 1);
                float y = Random.Shared.Next(0, worldSize.Height - size + 1);
                PointF candidateCenter = new(x + size / 2f, y + size / 2f);

                if (IsInsideExpandedView(candidateCenter, visibleWorldBounds, Random.Shared.Next(150, 301)))
                    continue;

                if (Distance(candidateCenter, playerCenter) < MinimumPlayerSpawnDistance)
                    continue;

                if (enemies.Any(existing => Distance(candidateCenter, existing.GetCenter()) < 100f))
                    continue;

                enemies.Add(new Enemy(type, playerLevel, wave)
                {
                    X = x,
                    Y = y
                });

                return true;
            }

            // If the view-padding test cannot find room, keep the spawn finite
            // and outside the immediate player danger zone.
            for (int attempt = 0; attempt < 200; attempt++)
            {
                float x = Random.Shared.Next(0, worldSize.Width - size + 1);
                float y = Random.Shared.Next(0, worldSize.Height - size + 1);
                PointF candidateCenter = new(x + size / 2f, y + size / 2f);

                if (Distance(candidateCenter, playerCenter) < MinimumPlayerSpawnDistance)
                    continue;

                enemies.Add(new Enemy(type, playerLevel, wave) { X = x, Y = y });
                return true;
            }

            return false;
        }

        public static EnemyType ChooseEnemyType(int wave, int spawnIndex)
        {
            if (wave <= 2)
                return EnemyType.ZombieFast;

            if (wave >= 10)
            {
                if (spawnIndex == 0)
                    return EnemyType.Vampire;

                float vampireChance = Math.Min(0.30f, 0.10f + (wave - 10) * 0.02f);
                if (Random.Shared.NextDouble() < vampireChance)
                    return EnemyType.Vampire;

                return Random.Shared.NextDouble() < 0.45
                    ? EnemyType.ZombieHeavy
                    : EnemyType.ZombieFast;
            }

            float heavyChance = wave switch
            {
                3 => 0.20f,
                4 => 0.30f,
                5 => 0.40f,
                _ => 0.45f
            };

            return Random.Shared.NextDouble() < heavyChance
                ? EnemyType.ZombieHeavy
                : EnemyType.ZombieFast;
        }

        public void PushEnemiesOutsideRadius(
            IReadOnlyList<Enemy> enemies,
            PointF center,
            float radius,
            Size worldSize)
        {
            foreach (Enemy enemy in enemies)
            {
                RectangleF hitbox = enemy.GetHitbox();
                PointF hitboxCenter = new(hitbox.Left + hitbox.Width / 2f, hitbox.Top + hitbox.Height / 2f);
                float dx = hitboxCenter.X - center.X;
                float dy = hitboxCenter.Y - center.Y;
                float distance = MathF.Sqrt(dx * dx + dy * dy);

                if (distance < 0.001f)
                {
                    dx = 1f;
                    dy = 0f;
                    distance = 1f;
                }

                float unitX = dx / distance;
                float unitY = dy / distance;
                float bodyReach = MathF.Abs(unitX) * hitbox.Width / 2f + MathF.Abs(unitY) * hitbox.Height / 2f;
                float minimumCenterDistance = radius + bodyReach;
                if (distance >= minimumCenterDistance)
                    continue;

                float baseAngle = MathF.Atan2(dy, dx);
                bool movedOutside = false;
                foreach (float angleOffset in ForcefieldPushAngles)
                {
                    if (TryPlaceOutsideForcefield(enemy, center, radius, baseAngle + angleOffset, worldSize))
                    {
                        movedOutside = true;
                        break;
                    }
                }

                if (!movedOutside)
                {
                    float push = minimumCenterDistance - distance;
                    enemy.X += unitX * push;
                    enemy.Y += unitY * push;
                    ClampToWorld(enemy, worldSize);
                }
            }
        }

        private static bool TryPlaceOutsideForcefield(
            Enemy enemy,
            PointF center,
            float radius,
            float angle,
            Size worldSize)
        {
            RectangleF hitbox = enemy.GetHitbox();
            float offsetX = hitbox.Left + hitbox.Width / 2f - enemy.X;
            float offsetY = hitbox.Top + hitbox.Height / 2f - enemy.Y;
            float unitX = MathF.Cos(angle);
            float unitY = MathF.Sin(angle);
            float bodyReach = MathF.Abs(unitX) * hitbox.Width / 2f + MathF.Abs(unitY) * hitbox.Height / 2f;
            float targetDistance = radius + bodyReach;
            float targetCenterX = center.X + unitX * targetDistance;
            float targetCenterY = center.Y + unitY * targetDistance;
            float candidateX = Math.Clamp(targetCenterX - offsetX, 0f, Math.Max(0, worldSize.Width - enemy.RenderSize));
            float candidateY = Math.Clamp(targetCenterY - offsetY, 0f, Math.Max(0, worldSize.Height - enemy.RenderSize));
            float actualCenterX = candidateX + offsetX;
            float actualCenterY = candidateY + offsetY;
            float dx = actualCenterX - center.X;
            float dy = actualCenterY - center.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);
            if (distance < 0.001f)
                return false;

            float actualUnitX = dx / distance;
            float actualUnitY = dy / distance;
            float actualBodyReach = MathF.Abs(actualUnitX) * hitbox.Width / 2f +
                                    MathF.Abs(actualUnitY) * hitbox.Height / 2f;
            if (distance + 0.01f < radius + actualBodyReach)
                return false;

            enemy.X = candidateX;
            enemy.Y = candidateY;
            return true;
        }

        public void PushEnemyAway(Enemy enemy, PointF center, float distance, Size worldSize)
        {
            PointF enemyCenter = enemy.GetCenter();
            float dx = enemyCenter.X - center.X;
            float dy = enemyCenter.Y - center.Y;
            float length = MathF.Sqrt(dx * dx + dy * dy);

            if (length < 0.001f)
            {
                dx = 1f;
                dy = 0f;
                length = 1f;
            }

            enemy.X += dx / length * distance;
            enemy.Y += dy / length * distance;
            ClampToWorld(enemy, worldSize);
        }

        private static bool IsInsideExpandedView(PointF point, RectangleF view, float padding)
        {
            return point.X >= view.Left - padding && point.X <= view.Right + padding &&
                   point.Y >= view.Top - padding && point.Y <= view.Bottom + padding;
        }

        public static float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        private static void SeparateOverlappingEnemies(List<Enemy> enemies, Size worldSize)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                for (int j = i + 1; j < enemies.Count; j++)
                {
                    Enemy first = enemies[i];
                    Enemy second = enemies[j];
                    PointF a = first.GetCenter();
                    PointF b = second.GetCenter();
                    float dx = b.X - a.X;
                    float dy = b.Y - a.Y;
                    float distance = MathF.Sqrt(dx * dx + dy * dy);
                    float separationDistance = Math.Max(
                        42f,
                        (first.Stats.HitboxWidth + second.Stats.HitboxWidth) / 2f + 8f);

                    if (distance >= separationDistance)
                        continue;

                    if (distance < 0.001f)
                    {
                        dx = 1f;
                        dy = 0f;
                        distance = 1f;
                    }

                    float push = (separationDistance - distance) / 2f;
                    float pushX = dx / distance * push;
                    float pushY = dy / distance * push;

                    first.X -= pushX;
                    first.Y -= pushY;
                    second.X += pushX;
                    second.Y += pushY;

                    ClampToWorld(first, worldSize);
                    ClampToWorld(second, worldSize);
                }
            }
        }

        private static void SeparateEnemiesFromPlayer(
            IReadOnlyList<Enemy> enemies,
            RectangleF playerHitbox,
            Size worldSize)
        {
            PointF playerHitboxCenter = new(
                playerHitbox.Left + playerHitbox.Width / 2f,
                playerHitbox.Top + playerHitbox.Height / 2f);

            foreach (Enemy enemy in enemies)
            {
                RectangleF enemyHitbox = enemy.GetHitbox();
                if (!playerHitbox.IntersectsWith(enemyHitbox))
                    continue;

                float overlapX = Math.Min(playerHitbox.Right, enemyHitbox.Right) -
                                 Math.Max(playerHitbox.Left, enemyHitbox.Left);
                float overlapY = Math.Min(playerHitbox.Bottom, enemyHitbox.Bottom) -
                                 Math.Max(playerHitbox.Top, enemyHitbox.Top);
                PointF enemyHitboxCenter = new(
                    enemyHitbox.Left + enemyHitbox.Width / 2f,
                    enemyHitbox.Top + enemyHitbox.Height / 2f);

                // Keep a one-pixel overlap so the existing contact-damage check
                // still sees an enemy that has reached the player.
                if (overlapX <= overlapY)
                {
                    float direction = enemyHitboxCenter.X < playerHitboxCenter.X ? -1f : 1f;
                    enemy.X += direction * Math.Max(0f, overlapX - 1f);
                }
                else
                {
                    float direction = enemyHitboxCenter.Y < playerHitboxCenter.Y ? -1f : 1f;
                    enemy.Y += direction * Math.Max(0f, overlapY - 1f);
                }

                ClampToWorld(enemy, worldSize);
            }
        }

        private static void ClampToWorld(Enemy enemy, Size worldSize)
        {
            enemy.X = Math.Clamp(enemy.X, 0, Math.Max(0, worldSize.Width - enemy.RenderSize));
            enemy.Y = Math.Clamp(enemy.Y, 0, Math.Max(0, worldSize.Height - enemy.RenderSize));
        }
    }
}
