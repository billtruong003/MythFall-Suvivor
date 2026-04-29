using UnityEngine;
using BillGameCore;
using Mythfall.Enemy;

namespace Mythfall.Gameplay
{
    /// <summary>
    /// Bare-bones wave-based enemy spawner. Picks a random spawn point and spawns
    /// <see cref="enemiesPerWave"/> enemies every <see cref="waveInterval"/> seconds.
    /// Sprint 2/4 will replace this with a difficulty-curve driven spawner.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Spawn points")]
        [SerializeField] Transform[] spawnPoints;

        [Header("Wave config")]
        [SerializeField] string enemyPoolKey = "Enemy_Swarmer";
        [SerializeField, Min(1)] int enemiesPerWave = 5;
        [SerializeField, Min(0.1f)] float waveInterval = 5f;
        [SerializeField] bool spawnOnStart = true;

        TimerHandle waveTimer;

        void Start()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning($"[{name}] No spawn points assigned — disabling.");
                enabled = false;
                return;
            }

            if (Bill.Pool == null)
            {
                Debug.LogWarning($"[{name}] Bill.Pool not ready — waiting for GameReady.");
                Bill.Events.SubscribeOnce<GameReadyEvent>(_ => StartSpawning());
                return;
            }

            StartSpawning();
        }

        void StartSpawning()
        {
            if (spawnOnStart) SpawnWave();
            waveTimer = Bill.Timer.Repeat(waveInterval, SpawnWave);
        }

        void OnDestroy()
        {
            if (waveTimer == null || !Bill.IsReady) return;
            Bill.Timer.Cancel(waveTimer);
        }

        void SpawnWave()
        {
            for (int i = 0; i < enemiesPerWave; i++)
            {
                var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var go = Bill.Pool.Spawn(enemyPoolKey, point.position, Quaternion.identity);
                if (go == null) continue;

                var enemy = go.GetComponent<EnemyBase>();
                if (enemy != null) enemy.OnSpawn();
            }
        }
    }
}
