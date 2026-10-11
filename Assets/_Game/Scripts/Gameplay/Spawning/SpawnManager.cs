using System;
using System.Collections.Generic;
using _Game.Scripts.Gameplay.Spawning;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : NetworkSingleton<SpawnManager>
{
    [SerializeField] private float _spawnInterval = 3f;
    [SerializeField] private float _checkRadius = 1.0f; // Spawn olunacak alanın yarıçapı
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private List<SpawnableObject> _spawnableObjects;
    [SerializeField] private Transform _spawnPointContainer;

    private float _timer;
    private bool _isActiveSpawning = false;
    private List<Transform> _spawnPoints = new List<Transform>();
    
    public int TotalSpawnedCount { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        GameEventsManager.Instance.OnGameStarted += StartSpawning;
        GameEventsManager.Instance.OnGameEnded += StopSpawning;
        
        // Event'e abone oluyoruz ama durum kontrolü de yapıyoruz ki zamanlama farkı oluşursa başlatılamamış spawn durumu oluşmasın.
        // Yüklenme anında event'in tetiklenmesini kaçırabiliriz ama durumu kontrol edip gerekli işlemleri yapabiliriz.
        // Sadece durum kontrolü yapsak bu sefer de her seferinde oyun restart edildiğinde kontrol ederiz.
        // Sonraki durumlarda eğer evnet kaçmıyorsa zaten olayı yakalamış oluruz. Asenkron sistemlerde bu yöntem en doğru kullanım olacaktır.
        // Adına Initialization Check with Event Subssciption (Olay Aboneliği ile Başlangış Kontrolü) denir. Hem geçmişi yakalar hem de geleceği güvence altına alır.
        
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive.Value)
        {
            StartSpawning(this, EventArgs.Empty);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.OnGameStarted -= StartSpawning;
            GameEventsManager.Instance.OnGameEnded -= StopSpawning;
        }
        base.OnNetworkDespawn();
    }

    private void Awake()
    {
        if (_spawnPointContainer != null)
        {
            foreach (Transform child in _spawnPointContainer)
            {
                _spawnPoints.Add(child);
            }
        }
        else
        {
            Debug.Log("SpawnManager: Container atanmamış.");
        }
    }

    private void Update()
    {
        if (!IsServer || !_isActiveSpawning) return;

        _timer += Time.deltaTime;
        if (_timer >= _spawnInterval)
        {
            _timer = 0;
            AttemptSpawn();
        }
    }

    private void StartSpawning(object sender, EventArgs e)
    {
        if (!IsServer) return;
        Debug.Log("SpawnManager: StartSpawning.");
        CleanUpScene();

        TotalSpawnedCount = 0;

        _isActiveSpawning = true;
        _timer = 0;
    }

    private void StopSpawning(object sender, EventArgs e)
    {
        if (!IsServer) return;
        Debug.Log("SpawnManager: StopSpawning.");
        _isActiveSpawning = false;
    }

    private void CleanUpScene()
    {
        Debug.Log("SpawnManager: CleanUpScene.");
        var allNetworkObj = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
        foreach (var netObj in allNetworkObj)
        {
            if (netObj.TryGetComponent(out Interfaces.ISpawnable spawnable))
            {
                Debug.Log("SpawnManager: CleanUpScene -- Çöp bulundu");
                if (netObj.IsSpawned)
                {
                    Debug.Log("SpawnManager: CleanUpScene -- Çöp isSpawned true");
                    spawnable.NetworkDespawn();
                }
            }
        }
    }

    private void AttemptSpawn()
    {
        Transform bestSpawnPoint = GetValidSpawnPoint();

        if (bestSpawnPoint != null)
        {
            NetworkObject prefabToSpawn = GetRandomPrefab();

            if (prefabToSpawn != null)
                SpawnObject(prefabToSpawn, bestSpawnPoint.position);
        }
    }

    private Transform GetValidSpawnPoint()
    {
        if (_spawnPoints == null || _spawnPoints.Count == 0) return null;
        
        int maxAttemps = 5; // Sonsuz döngü yaratmamak için
        for (int i = 0; i < maxAttemps; i++)
        {
            Transform randomPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];

            Collider2D hit = Physics2D.OverlapCircle(randomPoint.position, _checkRadius, _layerMask);
            if (hit == null) return randomPoint;
        }

        Debug.Log("Uygun alan bulunamadı.");
        return null;
    }

    private NetworkObject GetRandomPrefab()
    {
        if (_spawnableObjects.Count == 0) return null;

        int randomIndex = Random.Range(0, _spawnableObjects.Count);
        return _spawnableObjects[randomIndex].Prefab;
    }

    private void SpawnObject(NetworkObject prefab, Vector3 position)
    {
        GameObject newObj = Instantiate(prefab.gameObject, position, Quaternion.identity);
        newObj.GetComponent<NetworkObject>().Spawn();

        TotalSpawnedCount++;
        Debug.Log($"[SpawnManager] Object spawned. TotalSpawnedCount={TotalSpawnedCount}");
    }

    private void OnDrawGizmosSelected()
    {
        if (_spawnPoints == null) return;
        Gizmos.color = Color.red;
        foreach (var point in _spawnPoints)
        {
            if (point != null)
                // DrawWireSphere 3D olsa da görselleştirme için 2D projede de iş görür,
                // sadece ekranda bir küre çizer. Sorun yok, kalabilir.
                Gizmos.DrawWireSphere(point.position, _checkRadius);
        }
    }
}