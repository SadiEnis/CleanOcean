using System;
using _Game.Scripts.NetworkModels.Matches;
using _Game.Scripts.Services.Matches;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Match
{
    public class MatchResultSubmitter : MonoBehaviour
    {
        private MatchApiClient _matchApiClient;
        private MatchResultBuilder _matchResultBuilder;
    
        private void Awake()
        {
            _matchApiClient = new MatchApiClient();
            _matchResultBuilder = new MatchResultBuilder();
        }
    
        private void Start()
        {
            if (!IsServer())
            {
                Debug.Log("MatchResultSubmitter exists, but this instance is not server. It will not submit match result.");
                return;
            }
    
            GameEventsManager.Instance.OnGameEnded += OnGameEnded;
    
            Debug.Log("MatchResultSubmitter subscribed to OnGameEnded.");
        }
    
        private void OnDestroy()
        {
            if (GameEventsManager.Instance == null)
                return;
    
            GameEventsManager.Instance.OnGameEnded -= OnGameEnded;
        }
    
        private bool IsServer()
        {
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        }
    
        private async void OnGameEnded(object sender, EventArgs e)
        {
            if (!IsServer())
                return;
    
            try
            {
                CompleteMatchRequest request = _matchResultBuilder.Build();
    
                Debug.Log(
                    $"Submitting match result. " +
                    $"Players={request.players.Count}, " +
                    $"TotalTrashRecycled={request.totalTrashRecycled}, " +
                    $"Duration={request.durationSeconds}"
                );
    
                if (request.players.Count == 0)
                {
                    Debug.LogError("Match result submit failed: no valid DB users found in match result.");
                    return;
                }
    
                CompleteMatchResponse response = await _matchApiClient.CompleteMatchAsync(request);
    
                Debug.Log(
                    $"Match submit response: success={response.success}, " +
                    $"message={response.message}, " +
                    $"matchId={response.matchId}, " +
                    $"matchCode={response.matchCode}"
                );
            }
            catch (Exception exception)
            {
                Debug.LogError($"Match result submit failed: {exception.Message}");
            }
        }
    }
}