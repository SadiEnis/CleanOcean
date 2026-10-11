using System;
using Unity.Netcode;
using UnityEngine;
using _Game.Scripts.Gameplay.Player;

public class ScoreManager : NetworkSingleton<ScoreManager>
{
    private void Start()
    {
        GameEventsManager.Instance.OnRecycledItem += ScoreManager_OnRecycledItem;
        GameEventsManager.Instance.OnPlayerRevived += ScoreManager_OnPlayerRevived;
    }

    private void OnDestroy()
    {
        if (GameEventsManager.Instance == null)
            return;

        GameEventsManager.Instance.OnRecycledItem -= ScoreManager_OnRecycledItem;
        GameEventsManager.Instance.OnPlayerRevived -= ScoreManager_OnPlayerRevived;
    }

    private void ScoreManager_OnPlayerRevived(object sender, GameEventsManager.OnPlayerRevivedEventsArgs e)
    {
        if (!IsServer) return;

        GameEventsManager.Instance.TriggerOnScoreIncrease(e.reviverPlayerId, 50);
    }

    private void ScoreManager_OnRecycledItem(object sender, GameEventsManager.OnRecycledEnterArgs e)
    {
        if (!IsServer) return;

        Interfaces.IRecyclable recyclableObject = e.recyclableObject;
        if (recyclableObject == null) return;

        int baseScore = recyclableObject.GetScoreValue;

        var holdingPlayer = recyclableObject.GetHoldingPlayer;

        if (holdingPlayer.Value.TryGet(out NetworkObject player))
        {
            ulong playerId = player.OwnerClientId;
            string playerName = player.name;

            int finalScore = ApplyScoreBonuses(player, baseScore);

            GameEventsManager.Instance.TriggerOnScoreIncrease(playerId, finalScore);

            Debug.Log(
                $"Puan alan oyuncu: {playerName} - {playerId}, " +
                $"BaseScore={baseScore}, FinalScore={finalScore}"
            );
        }
        else
        {
            Debug.LogWarning("Objeye sahip bir oyuncuya erişilemedi.");
        }
    }

    private int ApplyScoreBonuses(NetworkObject player, int baseScore)
    {
        int finalScore = baseScore;

        if (player.TryGetComponent(out PlayerRuntimeInventory runtimeInventory))
        {
            if (runtimeInventory.HasEcoBadge.Value)
            {
                finalScore = Mathf.RoundToInt(baseScore * 1.5f);
            }
        }

        return finalScore;
    }
}
