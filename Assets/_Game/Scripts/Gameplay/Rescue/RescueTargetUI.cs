using TMPro;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Rescue
{
    public class RescueTargetUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RescueTarget _rescueTarget;
    [SerializeField] private TextMeshProUGUI _playerCountText;
    [SerializeField] private RectTransform _fillRect;
    [SerializeField] private GameObject _root;

    [Header("Settings")]
    [SerializeField] private float _fillMaxWidth = 200f;

    private void Awake()
    {
        if (_root == null)
            _root = gameObject;

        if (_rescueTarget == null)
            _rescueTarget = GetComponentInParent<RescueTarget>();
    }

    private void OnEnable()
    {
        if (_rescueTarget == null)
            return;

        _rescueTarget.Progress.OnValueChanged += OnProgressChanged;
        _rescueTarget.PlayerCountInZone.OnValueChanged += OnPlayerCountChanged;
        _rescueTarget.IsCompleted.OnValueChanged += OnCompletedChanged;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (_rescueTarget == null)
            return;

        _rescueTarget.Progress.OnValueChanged -= OnProgressChanged;
        _rescueTarget.PlayerCountInZone.OnValueChanged -= OnPlayerCountChanged;
        _rescueTarget.IsCompleted.OnValueChanged -= OnCompletedChanged;
    }

    private void RefreshAll()
    {
        RefreshProgress(_rescueTarget.Progress.Value);
        RefreshPlayerCount(_rescueTarget.PlayerCountInZone.Value);
        RefreshCompleted(_rescueTarget.IsCompleted.Value);
    }

    private void OnProgressChanged(float previousValue, float newValue)
    {
        RefreshProgress(newValue);
    }

    private void OnPlayerCountChanged(int previousValue, int newValue)
    {
        RefreshPlayerCount(newValue);
    }

    private void OnCompletedChanged(bool previousValue, bool newValue)
    {
        RefreshCompleted(newValue);
    }

    private void RefreshProgress(float progress)
    {
        if (_fillRect == null || _rescueTarget == null)
            return;

        float normalized = Mathf.Clamp01(progress / _rescueTarget.RequiredDuration);
        _fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _fillMaxWidth * normalized);
    }

    private void RefreshPlayerCount(int count)
    {
        if (_playerCountText == null || _rescueTarget == null)
            return;

        _playerCountText.text = $"{count}/{_rescueTarget.RequiredPlayerCount}";
    }

    private void RefreshCompleted(bool completed)
    {
        if (_root != null)
            _root.SetActive(!completed);
    }
}
}