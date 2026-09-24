using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public class EnemyManager
    {
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
    }
}
