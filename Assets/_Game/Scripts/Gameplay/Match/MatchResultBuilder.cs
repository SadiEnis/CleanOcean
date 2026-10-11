using System;
using System.Collections.Generic;
using _Game.Scripts.Gameplay.Player;
using _Game.Scripts.NetworkModels.Matches;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Match
{
    public class MatchResultBuilder
    {
        public CompleteMatchRequest Build()
        {
            var gameManager = GameManager.Instance;
            var statsTracker = MatchStatsTracker.Instance;

            statsTracker.BindDbUserIds();
            var players = new List<MatchPlayerResultRequest>();
            List<UsedItemRequest> usedItems = BuildUsedItems();
            List<PlayerActionLogRequest> actionLogs = BuildActionLogs();

            foreach (var stats in statsTracker.StatsByClientId.Values)
            {
                if (stats.DbUserId <= 0) continue;

                players.Add(new MatchPlayerResultRequest(
                    stats.DbUserId,
                    stats.FinalScore,
                    stats.TrashRecycledCount,
                    stats.RevivesDone,
                    stats.TimesFainted,
                    stats.EarnedCurrency
                ));
            }

            int totalSpawnedTrash = 0;
            if (SpawnManager.Instance != null)
                totalSpawnedTrash = SpawnManager.Instance.TotalSpawnedCount;

            string matchCode = $"MATCH_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{NetworkManager.Singleton.LocalClientId}";

            return new CompleteMatchRequest(
                matchCode,
                gameManager.MatchStartedAtUtc.ToString("yyyy-MM-ddTHH:mm:ss"),
                gameManager.MatchEndedAtUtc.ToString("yyyy-MM-ddTHH:mm:ss"),
                gameManager.GameDurationSeconds,
                totalTrashSpawned: totalSpawnedTrash,
                totalTrashRecycled: statsTracker.GetTotalTrashRecycled(),
                players,
                usedItems,
                actionLogs
            );
        }

        private List<UsedItemRequest> BuildUsedItems()
        {
            var usedItems = new List<UsedItemRequest>();

            var runtimeInventories = UnityEngine.Object.FindObjectsByType<PlayerRuntimeInventory>(
                FindObjectsSortMode.None
            );

            foreach (var runtimeInventory in runtimeInventories)
            {
                if (!runtimeInventory.TryGetComponent(out PlayerNetworkIdentity identity))
                {
                    Debug.LogWarning(
                        $"MatchResultBuilder: PlayerNetworkIdentity not found for OwnerClientId={runtimeInventory.OwnerClientId}"
                    );
                    continue;
                }

                ulong dbUserId = identity.dbUserId.Value;

                if (dbUserId <= 0)
                {
                    Debug.LogWarning(
                        $"MatchResultBuilder: DbUserId is invalid for OwnerClientId={runtimeInventory.OwnerClientId}"
                    );
                    continue;
                }

                if (runtimeInventory.UsedMedKitCount.Value > 0)
                {
                    usedItems.Add(new UsedItemRequest(
                        dbUserId,
                        "MED_KIT",
                        runtimeInventory.UsedMedKitCount.Value
                    ));

                    Debug.Log(
                        $"MatchResultBuilder UsedItem → UserId={dbUserId}, ItemCode=MED_KIT, Quantity={runtimeInventory.UsedMedKitCount.Value}"
                    );
                }

                if (runtimeInventory.UsedRescueFoodCount.Value > 0)
                {
                    usedItems.Add(new UsedItemRequest(
                        dbUserId,
                        "RESCUE_FOOD",
                        runtimeInventory.UsedRescueFoodCount.Value
                    ));

                    Debug.Log(
                        $"MatchResultBuilder UsedItem → UserId={dbUserId}, " +
                        $"ItemCode=RESCUE_FOOD, Quantity={runtimeInventory.UsedRescueFoodCount.Value}"
                    );
                }

                if (runtimeInventory.HasGoodSwimmer.Value)
                {
                    usedItems.Add(new UsedItemRequest(
                        dbUserId,
                        "GOOD_SWIMMER",
                        1
                    ));

                    Debug.Log(
                        $"MatchResultBuilder UsedItem → UserId={dbUserId}, ItemCode=GOOD_SWIMMER, Quantity=1"
                    );
                }

                if (runtimeInventory.HasEcoBadge.Value)
                {
                    usedItems.Add(new UsedItemRequest(
                        dbUserId,
                        "ECO_BADGE",
                        1
                    ));

                    Debug.Log(
                        $"MatchResultBuilder UsedItem → UserId={dbUserId}, ItemCode=ECO_BADGE, Quantity=1"
                    );
                }

                if (runtimeInventory.UsedOilCleanerCount.Value > 0)
                {
                    usedItems.Add(new UsedItemRequest(
                        dbUserId,
                        "OIL_CLEANER",
                        runtimeInventory.UsedOilCleanerCount.Value
                    ));

                    Debug.Log(
                        $"MatchResultBuilder UsedItem → UserId={dbUserId}, " +
                        $"ItemCode=OIL_CLEANER, Quantity={runtimeInventory.UsedOilCleanerCount.Value}"
                    );
                }
            }

            return usedItems;
        }
        
        private List<PlayerActionLogRequest> BuildActionLogs()
        {
            if (MatchActionLogTracker.Instance == null)
                return new List<PlayerActionLogRequest>();

            return MatchActionLogTracker.Instance.CreateSnapshot();
        }
    }
}