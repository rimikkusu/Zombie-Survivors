using Vampire_Survivors.Entities;

namespace Vampire_Survivors.Systems
{
    public class CombatSystem
    {
        // Visual-only delay before the floating "+XX XP" text appears.
        // XP itself is always awarded immediately on kill.
        public const int XpTextDelayMs = 250;

        // Vertical gap between the red kill number and the XP text.
        public const float XpTextVerticalOffsetPx = 30f;

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

        public int ApplyEnemyDamage(
            Player player,
            Enemy enemy,
            int damage,
            bool isCritical,
            List<DamageNumber> damageNumbers,
            List<Bandage>? bandages = null,
            List<DeathParticle>? deathParticles = null)
        {
            enemy.Health -= damage;
            enemy.HitFlashTimer = isCritical ? 9 : 7;
            enemy.CriticalHitFlash = isCritical;

            bool isLethal = enemy.Health <= 0;
            CombatTextType textType;
            if (isLethal && isCritical)
                textType = CombatTextType.CriticalKillDamage;
            else if (isLethal)
                textType = CombatTextType.KillDamage;
            else if (isCritical)
                textType = CombatTextType.CriticalDamage;
            else
                textType = CombatTextType.NormalDamage;
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

            if (deathParticles is not null)
                SpawnDeathBurst(center, deathParticles);

            int xpReward = enemy.RollExperienceReward();
            // XP is given now; its floating text appears a moment later.
            player.AddExperience(xpReward);
            if (bandages is not null && Random.Shared.NextDouble() < 0.10)
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

        private static void SpawnDeathBurst(PointF center, List<DeathParticle> particles)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = i * MathF.Tau / 6f + Random.Shared.NextSingle() * 0.35f;
                float speed = 1.8f + Random.Shared.NextSingle() * 2.3f;
                particles.Add(new DeathParticle
                {
                    X = center.X,
                    Y = center.Y,
                    VelocityX = MathF.Cos(angle) * speed,
                    VelocityY = MathF.Sin(angle) * speed,
                    Size = 4f + Random.Shared.NextSingle() * 3f,
                    IsDark = i % 2 == 0
                });
            }
        }

        public void UpdateDeathParticles(List<DeathParticle> particles, int elapsedMs)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update(elapsedMs);
                if (particles[i].RemainingMs == 0)
                    particles.RemoveAt(i);
            }
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

            if (player.IsDead || preventDamage || player.IsInvulnerable)
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
