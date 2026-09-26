namespace Vampire_Survivors.Systems
{
    public sealed class WaveManager
    {
        public const int IntermissionDurationMs = 2000;

        private int spawnDelayRemainingMs = GetNextSpawnDelay();
        private int intermissionRemainingMs;

        public int CurrentWave { get; private set; } = 1;
        public int EnemiesSpawned { get; private set; }
        public int EnemiesRequired => 3 + (CurrentWave - 1) * 2;
        public int IntermissionRemainingMs => intermissionRemainingMs;
        public bool IsWaveActive { get; private set; } = true;

        public int GetEnemiesRemaining(int livingEnemies)
        {
            return Math.Max(0, EnemiesRequired - EnemiesSpawned + livingEnemies);
        }

        /// <returns>True if visible wave state changed.</returns>
        public bool Update(int elapsedMs, int livingEnemies, Func<int, int, int, bool> spawnEnemy)
        {
            if (!IsWaveActive)
            {
                intermissionRemainingMs = Math.Max(0, intermissionRemainingMs - elapsedMs);
                if (intermissionRemainingMs > 0)
                    return false;

                CurrentWave++;
                EnemiesSpawned = 0;
                spawnDelayRemainingMs = GetNextSpawnDelay();
                IsWaveActive = true;
                return true;
            }

            if (EnemiesSpawned < EnemiesRequired)
            {
                spawnDelayRemainingMs -= elapsedMs;
                if (spawnDelayRemainingMs <= 0 && spawnEnemy(CurrentWave, EnemiesSpawned, EnemiesRequired))
                {
                    EnemiesSpawned++;
                    spawnDelayRemainingMs = GetNextSpawnDelay();
                    return true;
                }

                return false;
            }

            if (livingEnemies == 0)
            {
                IsWaveActive = false;
                intermissionRemainingMs = IntermissionDurationMs;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            CurrentWave = 1;
            EnemiesSpawned = 0;
            spawnDelayRemainingMs = GetNextSpawnDelay();
            intermissionRemainingMs = 0;
            IsWaveActive = true;
        }

        public void CompleteCurrentWave()
        {
            if (!IsWaveActive)
                return;

            EnemiesSpawned = EnemiesRequired;
        }

        public void AdvanceToNextWave()
        {
            CurrentWave++;
            EnemiesSpawned = 0;
            spawnDelayRemainingMs = GetNextSpawnDelay();
            intermissionRemainingMs = 0;
            IsWaveActive = true;
        }

        private static int GetNextSpawnDelay()
        {
            return Random.Shared.Next(500, 901);
        }
    }
}
