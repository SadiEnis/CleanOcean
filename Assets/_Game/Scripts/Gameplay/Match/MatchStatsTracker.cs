using System.Collections.Generic;
using _Game.Scripts.Gameplay.Match;
using _Game.Scripts.Gameplay.Player;
using Unity.Netcode;
using UnityEngine;

public class MatchStatsTracker : MonoSingleton<MatchStatsTracker>
{
    [SerializeField] private int scoreToCurrencyDivisor = 5;

    private readonly Dictionary<ulong, PlayerMatchStats> _statsByClientId = new();

    public IReadOnlyDictionary<ulong, PlayerMatchStats> StatsByClientId => _statsByClientId;

    private void Start()
    {
        if (!IsServer())
        {
            Debug.Log("MatchStatsTracker exists, but this instance is not server. It will not track stats.");
            return;
        }

        GameEventsManager.Instance.OnScoreIncrease += OnScoreIncrease;
        GameEventsManager.Instance.OnRecycledItem += OnRecycledItem;
        GameEventsManager.Instance.OnPlayerRevived += OnPlayerRevived;
        GameEventsManager.Instance.OnPlayerDeath += OnPlayerDeath;

        Debug.Log("MatchStatsTracker subscribed to game events.");
    }

    private void OnDestroy()
    {
        if (GameEventsManager.Instance == null)
            return;

        GameEventsManager.Instance.OnScoreIncrease -= OnScoreIncrease;
        GameEventsManager.Instance.OnRecycledItem -= OnRecycledItem;
        GameEventsManager.Instance.OnPlayerRevived -= OnPlayerRevived;
        GameEventsManager.Instance.OnPlayerDeath -= OnPlayerDeath;

        Debug.Log("MatchStatsTracker unsubscribed from game events.");
    }

    private bool IsServer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    }

    private PlayerMatchStats GetOrCreate(ulong clientId)
    {
        if (_statsByClientId.TryGetValue(clientId, out var stats))
            return stats;

        stats = new PlayerMatchStats
        {
            ClientId = clientId,
            DbUserId = 0,
            FinalScore = 0,
            TrashRecycledCount = 0,
            RevivesDone = 0,
            TimesFainted = 0,
            EarnedCurrency = 0
        };

        _statsByClientId.Add(clientId, stats);

        Debug.Log($"MatchStatsTracker created stats for ClientId={clientId}");

        return stats;
    }

    private void OnScoreIncrease(object sender, GameEventsManager.OnScoreEventsArgs e)
    {
        if (!IsServer())
            return;

        var stats = GetOrCreate(e.playerId);

        stats.FinalScore += e.score;
        stats.EarnedCurrency += CalculateCurrencyFromScore(e.score);

        Debug.Log(
            $"MatchStatsTracker Score → ClientId={e.playerId}, " +
            $"Score+={e.score}, TotalScore={stats.FinalScore}, Currency={stats.EarnedCurrency}"
        );
    }

    private void OnRecycledItem(object sender, GameEventsManager.OnRecycledEnterArgs e)
    {
        if (!IsServer())
            return;

        var recyclableObject = e.recyclableObject;

        if (recyclableObject == null)
        {
            Debug.LogWarning("MatchStatsTracker Recycle event received but recyclableObject is null.");
            return;
        }

        var holdingPlayer = recyclableObject.GetHoldingPlayer;

        if (holdingPlayer.Value.TryGet(out NetworkObject player))
        {
            ulong clientId = player.OwnerClientId;

            var stats = GetOrCreate(clientId);
            stats.TrashRecycledCount += 1;

            Debug.Log(
                $"MatchStatsTracker Recycle → ClientId={clientId}, " +
                $"TrashRecycled={stats.TrashRecycledCount}"
            );
        }
        else
        {
            Debug.LogWarning("MatchStatsTracker Recycle event received but holding player could not be resolved.");
        }
    }

    private void OnPlayerRevived(object sender, GameEventsManager.OnPlayerRevivedEventsArgs e)
    {
        if (!IsServer())
            return;

        var stats = GetOrCreate(e.reviverPlayerId);
        stats.RevivesDone += 1;

        Debug.Log(
            $"MatchStatsTracker Revive → ReviverClientId={e.reviverPlayerId}, " +
            $"Revives={stats.RevivesDone}"
        );
    }

    private void OnPlayerDeath(object sender, GameEventsManager.OnPlayerDeathEventsArgs e)
    {
        if (!IsServer())
            return;

        var stats = GetOrCreate(e.playerId);
        stats.TimesFainted += 1;

        Debug.Log(
            $"MatchStatsTracker Faint → ClientId={e.playerId}, " +
            $"Faints={stats.TimesFainted}"
        );
    }

    private int CalculateCurrencyFromScore(int score)
    {
        if (scoreToCurrencyDivisor <= 0)
            return 0;

        return Mathf.Max(0, score / scoreToCurrencyDivisor);
    }

    public void BindDbUserIds()
    {
        if (!IsServer())
            return;

        var playerIdentities = FindObjectsByType<PlayerNetworkIdentity>(FindObjectsSortMode.None);

        foreach (var identity in playerIdentities)
        {
            ulong clientId = identity.OwnerClientId;

            var stats = GetOrCreate(clientId);
            stats.DbUserId = identity.dbUserId.Value;

            Debug.Log(
                $"MatchStatsTracker BindDbUserId → ClientId={clientId}, " +
                $"DbUserId={stats.DbUserId}"
            );
        }
    }

    public int GetTotalTrashRecycled()
    {
        int total = 0;

        foreach (var stats in _statsByClientId.Values)
            total += stats.TrashRecycledCount;

        return total;
    }

    public void ResetStats()
    {
        if (!IsServer())
            return;

        _statsByClientId.Clear();

        Debug.Log("MatchStatsTracker stats reset.");
    }
}