using TMPro;
using UnityEngine;

public class PlayerScoreItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    public void SetScoreData(string playerName, int score)
    {
        _playerNameText.text = playerName;
        _scoreText.text = score.ToString();
    }
}