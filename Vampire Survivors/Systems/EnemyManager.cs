using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public class EnemyManager
    {
        public const int SpawnIntervalMs = 1500;
        public const int MaxAliveEnemies = 20;

        private int spawnElapsedMs;

        public void Reset()
        {
            spawnElapsedMs = 0;
        }

        public void Update(List<Enemy> enemies, PointF playerCenter)
        {
            foreach (Enemy enemy in enemies)
            {
                float enemyCenterX = enemy.X + Enemy.Size / 2f;
                float enemyCenterY = enemy.Y + Enemy.Size / 2f;

                float enemyDx = playerCenter.X - enemyCenterX;
                float enemyDy = playerCenter.Y - enemyCenterY;

                float enemyDistance = MathF.Sqrt(enemyDx * enemyDx + enemyDy * enemyDy);

                enemy.Angle = MathF.Atan2(enemyDy, enemyDx) * 180f / MathF.PI;

                if (enemyDistance > 0)
                {
                    enemyDx /= enemyDistance;
                    enemyDy /= enemyDistance;

                    enemy.X += enemyDx * Enemy.Speed;
                    enemy.Y += enemyDy * Enemy.Speed;
                }
            }
        }

        public void UpdateHitFlashTimers(List<Enemy> enemies)
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy.HitFlashTimer > 0)
                {
                    enemy.HitFlashTimer--;
                }
            }
        }

        public void UpdateSpawning(List<Enemy> enemies, Size clientSize)
        {
            // Game loop runs approximately every 16 ms.
            spawnElapsedMs += 16;

            if (spawnElapsedMs < SpawnIntervalMs)
                return;

            spawnElapsedMs = 0;
            TrySpawnEnemy(enemies, clientSize);
        }

        public void TrySpawnEnemy(List<Enemy> enemies, Size clientSize)
        {
            if (enemies.Count >= MaxAliveEnemies)
                return;

            if (clientSize.Width <= 0 || clientSize.Height <= 0)
                return;

            int side = Random.Shared.Next(4);

            float x;
            float y;

            if (side == 0)
            {
                // Top
                x = Random.Shared.Next(clientSize.Width);
                y = -Enemy.Size;
            }
            else if (side == 1)
            {
                // Bottom
                x = Random.Shared.Next(clientSize.Width);
                y = clientSize.Height + Enemy.Size;
            }
            else if (side == 2)
            {
                // Left
                x = -Enemy.Size;
                y = Random.Shared.Next(clientSize.Height);
            }
            else
            {
                // Right
                x = clientSize.Width + Enemy.Size;
                y = Random.Shared.Next(clientSize.Height);
            }

            enemies.Add(new Enemy
            {
                X = x,
                Y = y
            });
        }
    }
}
