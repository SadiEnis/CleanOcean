using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Scripts.Gameplay.Rescue
{
    public class RescueSpawnManager : NetworkBehaviour
    {
        [Header("Spawn Settings")] [SerializeField]
        private NetworkObject _rescueTargetPrefab;
        [SerializeField] private float _respawnDelaySeconds = 10f;

        [SerializeField] private Transform _spawnPointContainer;
        [SerializeField] private int _initialSpawnCount = 1;

        private readonly List<Transform> _spawnPoints = new();
        private int _lastSpawnPointIndex = -1;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                enabled = false;
                return;
            }

            CacheSpawnPoints();

            GameEventsManager.Instance.OnGameStarted += HandleGameStarted;
            GameEventsManager.Instance.OnGameEnded += HandleGameEnded;
            GameEventsManager.Instance.OnRescueCompleted += HandleRescueCompleted;

            if (GameManager.Instance != null && GameManager.Instance.IsGameActive.Value)
            {
                SpawnInitialTargets();
            }
        }

        public override void OnDestroy()
        {
            if (GameEventsManager.Instance != null)
            {
                GameEventsManager.Instance.OnGameStarted -= HandleGameStarted;
                GameEventsManager.Instance.OnGameEnded -= HandleGameEnded;
                GameEventsManager.Instance.OnRescueCompleted -= HandleRescueCompleted;
            }
            base.OnDestroy();
        }

        private void CacheSpawnPoints()
        {
            _spawnPoints.Clear();

            if (_spawnPointContainer == null)
            {
                Debug.LogWarning("RescueSpawnManager: SpawnPointContainer is not assigned.");
                return;
            }

            foreach (Transform child in _spawnPointContainer)
            {
                _spawnPoints.Add(child);
            }

            Debug.Log($"RescueSpawnManager: Cached spawn points. Count={_spawnPoints.Count}");
        }

        private void HandleGameStarted(object sender, EventArgs e)
        {
            if (!IsServer)
                return;

            SpawnInitialTargets();
        }

        private void HandleGameEnded(object sender, EventArgs e)
        {
            if (!IsServer)
                return;

            CleanupRescueTargets();
        }
        
        private void HandleRescueCompleted(object sender, GameEventsManager.OnRescueCompletedEventsArgs e)
        {
            if (!IsServer)
                return;

            if (e.rescueTarget == null)
                return;

            StartCoroutine(RespawnAfterDelay(e.rescueTarget));
        }
        
        private System.Collections.IEnumerator RespawnAfterDelay(RescueTarget completedTarget)
        {
            Debug.Log($"RescueSpawnManager: Rescue completed. Respawn in {_respawnDelaySeconds} seconds.");

            NetworkObject completedNetworkObject = completedTarget.GetComponent<NetworkObject>();

            if (completedNetworkObject != null && completedNetworkObject.IsSpawned)
            {
                completedNetworkObject.Despawn(true);
            }
            else if (completedTarget != null)
            {
                Destroy(completedTarget.gameObject);
            }

            yield return new WaitForSeconds(_respawnDelaySeconds);

            if (GameManager.Instance != null && !GameManager.Instance.IsGameActive.Value)
            {
                Debug.Log("RescueSpawnManager: Game is not active. Respawn skipped.");
                yield break;
            }

            if (_spawnPoints.Count == 0)
            {
                Debug.LogWarning("RescueSpawnManager: No spawn points found. Respawn skipped.");
                yield break;
            }

            Transform spawnPoint = GetRandomSpawnPoint();

            if (spawnPoint == null)
            {
                Debug.LogWarning("RescueSpawnManager: Spawn point is null. Respawn skipped.");
                yield break;
            }

            SpawnRescueTarget(spawnPoint.position);
        }

        private void SpawnInitialTargets()
        {
            if (_rescueTargetPrefab == null)
            {
                Debug.LogError("RescueSpawnManager: RescueTargetPrefab is not assigned.");
                return;
            }

            if (_spawnPoints.Count == 0)
            {
                Debug.LogWarning("RescueSpawnManager: No spawn points found.");
                return;
            }

            CleanupRescueTargets();

            int spawnCount = Mathf.Min(_initialSpawnCount, _spawnPoints.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                Transform spawnPoint = GetRandomSpawnPoint();

                if (spawnPoint == null)
                {
                    Debug.LogWarning("RescueSpawnManager: Spawn point is null. Spawn skipped.");
                    return;
                }

                SpawnRescueTarget(spawnPoint.position);
            }
        }

        private Transform GetRandomSpawnPoint()
        {
            if (_spawnPoints.Count == 0)
                return null;

            if (_spawnPoints.Count == 1)
            {
                _lastSpawnPointIndex = 0;
                return _spawnPoints[0];
            }

            int index;

            do
            {
                index = Random.Range(0, _spawnPoints.Count);
            }
            while (index == _lastSpawnPointIndex);

            _lastSpawnPointIndex = index;

            return _spawnPoints[index];
        }

        private void SpawnRescueTarget(Vector3 position)
        {
            NetworkObject rescueTarget = Instantiate(
                _rescueTargetPrefab,
                position,
                Quaternion.identity
            );

            rescueTarget.Spawn();
        }

        private void CleanupRescueTargets()
        {
            RescueTarget[] targets = FindObjectsByType<RescueTarget>(FindObjectsSortMode.None);

            foreach (RescueTarget target in targets)
            {
                NetworkObject networkObject = target.GetComponent<NetworkObject>();

                if (networkObject != null && networkObject.IsSpawned)
                {
                    networkObject.Despawn(true);
                }
                else if (target != null)
                {
                    Destroy(target.gameObject);
                }
            }
            Debug.Log("RescueSpawnManager: Cleanup completed.");
        }
    }
}