using System;
using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Leaderboard;
using _Game.Scripts.Services.Leaderboard;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Leaderboard
{
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("Leaderboard")]
        [SerializeField] private Transform _container;
        [SerializeField] private GameObject _scoreItemPrefab;
        [SerializeField] private int _limit = 10;

        [Header("Optional")]
        [SerializeField] private Button _refreshButton;

        private LeaderboardApiClient _leaderboardApiClient;
        private bool _isLoading;

        private void Awake()
        {
            _leaderboardApiClient = new LeaderboardApiClient();

            if (_refreshButton != null)
                _refreshButton.onClick.AddListener(RefreshLeaderboard);
        }

        private void OnEnable()
        {
            RefreshLeaderboard();
        }

        private void OnDestroy()
        {
            if (_refreshButton != null)
                _refreshButton.onClick.RemoveListener(RefreshLeaderboard);
        }

        public void RefreshLeaderboard()
        {
            _ = RefreshLeaderboardAsync();
        }

        private async Task RefreshLeaderboardAsync()
        {
            if (_isLoading)
                return;

            _isLoading = true;

            try
            {
                ClearItems();

                LeaderboardResponse response =
                    await _leaderboardApiClient.GetLeaderboardAsync(_limit);

                if (response == null)
                {
                    Debug.LogWarning("[LeaderboardUI] Response is null.");
                    return;
                }

                if (!response.success)
                {
                    Debug.LogWarning($"[LeaderboardUI] Request failed: {response.message}");
                    return;
                }

                if (response.players == null || response.players.Length == 0)
                {
                    Debug.Log("[LeaderboardUI] No leaderboard players found.");
                    return;
                }

                foreach (LeaderboardEntryResponse player in response.players)
                {
                    GameObject itemObject = Instantiate(_scoreItemPrefab, _container);

                    PlayerScoreItem item = itemObject.GetComponent<PlayerScoreItem>();

                    if (item == null)
                    {
                        Debug.LogError("[LeaderboardUI] Score item prefab does not have PlayerScoreItem component.");
                        continue;
                    }

                    string playerName = $"{player.rank}. {player.displayName}";
                    int score = player.totalScore;

                    item.SetScoreData(playerName, score);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[LeaderboardUI] Failed to refresh leaderboard: {exception}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ClearItems()
        {
            foreach (Transform child in _container)
            {
                Destroy(child.gameObject);
            }
        }
    }
}