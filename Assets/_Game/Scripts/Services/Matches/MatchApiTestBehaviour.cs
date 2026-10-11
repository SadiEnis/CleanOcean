using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Matches;
using _Game.Scripts.Session;
using UnityEngine;

namespace _Game.Scripts.Services.Matches
{
    public class MatchApiTestBehaviour : MonoBehaviour
    {
        [Header("Test Match Data")]
        [SerializeField] private int durationSeconds = 180;
        [SerializeField] private int totalTrashSpawned = 25;
        [SerializeField] private int totalTrashRecycled = 10;

        [Header("Test Player Result")]
        [SerializeField] private int finalScore = 120;
        [SerializeField] private int trashRecycledCount = 10;
        [SerializeField] private int revivesDone = 1;
        [SerializeField] private int timesFainted = 0;
        [SerializeField] private int earnedCurrency = 25;

        [Header("Run")]
        [SerializeField] private bool sendOnStart = true;

        private async void Start()
        {
            if (!sendOnStart)
                return;

            await Task.Delay(1000);

            if (!CurrentUser.IsLoggedIn)
            {
                Debug.LogError("Match API test failed: CurrentUser is not logged in. Run AuthTest first.");
                return;
            }

            var matchApiClient = new MatchApiClient();

            try
            {
                string endedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
                string startedAt = DateTime.UtcNow.AddSeconds(-durationSeconds).ToString("yyyy-MM-ddTHH:mm:ss");

                string matchCode = $"MATCH_UNITY_TEST_{DateTime.UtcNow:yyyyMMdd_HHmmss}";

                var players = new List<MatchPlayerResultRequest>
                {
                    new MatchPlayerResultRequest(
                        CurrentUser.UserId,
                        finalScore,
                        trashRecycledCount,
                        revivesDone,
                        timesFainted,
                        earnedCurrency
                    )
                };

                var request = new CompleteMatchRequest(
                    matchCode,
                    startedAt,
                    endedAt,
                    durationSeconds,
                    totalTrashSpawned,
                    totalTrashRecycled,
                    players
                );

                CompleteMatchResponse response = await matchApiClient.CompleteMatchAsync(request);

                Debug.Log(
                    $"Complete Match Response: success={response.success}, " +
                    $"message={response.message}, matchId={response.matchId}, matchCode={response.matchCode}"
                );
            }
            catch (Exception exception)
            {
                Debug.LogError($"Match API test failed: {exception.Message}");
            }
        }
    }
}