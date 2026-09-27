using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    internal static class EnemyBehavior
    {
        public static void Update(Enemy enemy, PointF playerCenter, int elapsedMs)
        {
            PointF center = enemy.GetCenter();
            float dx = playerCenter.X - center.X;
            float dy = playerCenter.Y - center.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);
            float towardX = distance > 0.001f ? dx / distance : 1f;
            float towardY = distance > 0.001f ? dy / distance : 0f;

            enemy.SpecialCooldownRemainingMs = Math.Max(0, enemy.SpecialCooldownRemainingMs - elapsedMs);
            enemy.StaggerRemainingMs = Math.Max(0, enemy.StaggerRemainingMs - elapsedMs);
            enemy.Angle = MathF.Atan2(towardY, towardX) * 180f / MathF.PI;

            switch (enemy.Type)
            {
                case EnemyType.ZombieFast:
                case EnemyType.ZombieHeavy:
                    UpdateZombie(enemy, distance, towardX, towardY, playerCenter, elapsedMs);
                    break;
                case EnemyType.Vampire:
                    UpdateVampire(enemy, distance, towardX, towardY, playerCenter, elapsedMs);
                    break;
            }
        }

        private static void UpdateZombie(
            Enemy enemy, float distance, float towardX, float towardY, PointF playerCenter, int elapsedMs)
        {
            bool heavy = enemy.Type == EnemyType.ZombieHeavy;
            // Both zombies warn the player before bursting forward. The heavy one is slower to start.
            switch (enemy.BehaviorState)
            {
                case EnemyBehaviorState.Windup:
                    if ((enemy.BehaviorRemainingMs -= elapsedMs) <= 0)
                        StartBurst(enemy, playerCenter, heavy ? 256 : 160);
                    return;
                case EnemyBehaviorState.Burst:
                    MoveEnemy(enemy, enemy.BurstDirectionX, enemy.BurstDirectionY, enemy.Stats.Speed * (heavy ? 3.1f : 2.1f));
                    if ((enemy.BehaviorRemainingMs -= elapsedMs) <= 0)
                    {
                        enemy.BehaviorState = EnemyBehaviorState.Recover;
                        enemy.BehaviorRemainingMs = heavy ? 520 : 240;
                        enemy.SpecialCooldownRemainingMs = Random.Shared.Next(heavy ? 4000 : 2000, heavy ? 6001 : 4001);
                        enemy.ProximityDelayRemainingMs = Random.Shared.Next(heavy ? 300 : 150, heavy ? 1000 : 650);
                    }
                    return;
                case EnemyBehaviorState.Recover:
                    if (!heavy)
                        MoveEnemy(enemy, towardX, towardY, enemy.Stats.Speed * 0.4f);
                    if ((enemy.BehaviorRemainingMs -= elapsedMs) <= 0)
                        enemy.BehaviorState = EnemyBehaviorState.Chase;
                    return;
            }

            if (ReadyForSpecial(enemy, distance, heavy ? 330f : 220f, elapsedMs))
            {
                enemy.BehaviorState = EnemyBehaviorState.Windup;
                enemy.BehaviorRemainingMs = Random.Shared.Next(heavy ? 440 : 220, heavy ? 581 : 301);
                return;
            }

            float sideStep = 0f;
            if (!heavy)
            {
                enemy.WanderPhase += elapsedMs * 0.0022f;
                sideStep = MathF.Sin(enemy.WanderPhase) * 0.2f;
            }
            MoveEnemy(enemy, towardX - towardY * sideStep, towardY + towardX * sideStep,
                enemy.Stats.Speed);
        }

        private static void UpdateVampire(
            Enemy enemy, float distance, float towardX, float towardY, PointF playerCenter, int elapsedMs)
        {
            switch (enemy.BehaviorState)
            {
                case EnemyBehaviorState.Strafe:
                    // Circle the player while moving inward or outward to keep some space.
                    float radial = 0f;
                    if (distance < 185f)
                        radial = -0.5f;
                    else if (distance > 310f)
                        radial = 0.35f;
                    float strafeX = -towardY * enemy.StrafeDirection + towardX * radial;
                    float strafeY = towardX * enemy.StrafeDirection + towardY * radial;
                    MoveEnemy(enemy, strafeX, strafeY, enemy.Stats.Speed * 1.15f);
                    if ((enemy.BehaviorRemainingMs -= elapsedMs) <= 0)
                    {
                        enemy.BehaviorState = EnemyBehaviorState.Windup;
                        enemy.BehaviorRemainingMs = 250;
                    }
                    return;
                case EnemyBehaviorState.Windup:
                    if ((enemy.BehaviorRemainingMs -= elapsedMs) <= 0)
                        StartBurst(enemy, playerCenter, 192);
                    return;
                case EnemyBehaviorState.Burst:
                    MoveEnemy(enemy, enemy.BurstDirectionX, enemy.BurstDirectionY, enemy.Stats.Speed * 2.45f);
                    if ((enemy.BehaviorRemainingMs -= elapsedMs) <= 0)
                    {
                        enemy.BehaviorState = EnemyBehaviorState.Retreat;
                        enemy.BehaviorRemainingMs = 560;
                        enemy.SpecialCooldownRemainingMs = Random.Shared.Next(1700, 2801);
                        enemy.ProximityDelayRemainingMs = Random.Shared.Next(150, 500);
                    }
                    return;
                case EnemyBehaviorState.Retreat:
                    // After the dash, the vampire backs away before chasing again.
                    MoveEnemy(enemy,
                        -towardX - towardY * enemy.StrafeDirection * 0.3f,
                        -towardY + towardX * enemy.StrafeDirection * 0.3f,
                        enemy.Stats.Speed * 0.9f);
                    if ((enemy.BehaviorRemainingMs -= elapsedMs) <= 0)
                        enemy.BehaviorState = EnemyBehaviorState.Chase;
                    return;
            }

            if (ReadyForSpecial(enemy, distance, 320f, elapsedMs))
            {
                enemy.StrafeDirection = Random.Shared.Next(2) == 0 ? -1 : 1;
                enemy.BehaviorState = EnemyBehaviorState.Strafe;
                enemy.BehaviorRemainingMs = Random.Shared.Next(650, 1101);
                return;
            }

            MoveEnemy(enemy, towardX - towardY * 0.12f * enemy.StrafeDirection,
                towardY + towardX * 0.12f * enemy.StrafeDirection, enemy.Stats.Speed);
        }

        private static bool ReadyForSpecial(Enemy enemy, float distance, float range, int elapsedMs)
        {
            if (distance > range)
                return false;

            enemy.ProximityDelayRemainingMs = Math.Max(0, enemy.ProximityDelayRemainingMs - elapsedMs);
            return enemy.SpecialCooldownRemainingMs == 0 && enemy.ProximityDelayRemainingMs == 0;
        }

        private static void StartBurst(Enemy enemy, PointF playerCenter, int durationMs)
        {
            // Lock the dash direction when the windup ends, so it can be dodged.
            PointF center = enemy.GetCenter();
            float dx = playerCenter.X - center.X;
            float dy = playerCenter.Y - center.Y;
            float length = MathF.Sqrt(dx * dx + dy * dy);
            enemy.BurstDirectionX = length > 0.001f ? dx / length : 1f;
            enemy.BurstDirectionY = length > 0.001f ? dy / length : 0f;
            enemy.BehaviorState = EnemyBehaviorState.Burst;
            enemy.BehaviorRemainingMs = durationMs;
            enemy.Angle = MathF.Atan2(enemy.BurstDirectionY, enemy.BurstDirectionX) * 180f / MathF.PI;
        }

        private static void MoveEnemy(Enemy enemy, float directionX, float directionY, float speed)
        {
            float length = MathF.Sqrt(directionX * directionX + directionY * directionY);
            if (length <= 0.001f)
                return;

            float staggerFactor = enemy.StaggerRemainingMs > 0
                ? enemy.Stats.StaggerSpeedFactor
                : 1f;
            enemy.X += directionX / length * speed * staggerFactor;
            enemy.Y += directionY / length * speed * staggerFactor;
        }

    }
}
