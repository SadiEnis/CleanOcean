using System.Collections;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.HUD
{
    public class ItemFeedbackUI : MonoBehaviour
    {
        public static ItemFeedbackUI Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private float _defaultDuration = 2f;
        [SerializeField] private float _hintCooldown = 1f;

        private Coroutine _hideCoroutine;
        private string _lastHintMessage;
        private float _lastHintTime;

        private void Awake()
        {
            Instance = this;

            if (_messageText == null)
                _messageText = GetComponentInChildren<TextMeshProUGUI>(true);

            if (_messageText == null)
                Debug.LogError("ItemFeedbackUI: MessageText bulunamadı!");
        }

        private void Start()
        {
            ClearMessage();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ShowMessage(string message)
        {
            ShowMessage(message, _defaultDuration);
        }

        public void ShowMessage(string message, float duration)
        {
            if (_messageText == null)
                _messageText = GetComponentInChildren<TextMeshProUGUI>(true);

            if (_messageText == null)
            {
                return;
            }

            _messageText.gameObject.SetActive(true);
            _messageText.text = message;

            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);

            _hideCoroutine = StartCoroutine(HideAfterDelay(duration));
        }
        
        public void ShowHint(string message)
        {
            if (_lastHintMessage == message && Time.time - _lastHintTime < _hintCooldown)
                return;

            _lastHintMessage = message;
            _lastHintTime = Time.time;

            ShowMessage(message, 1.25f);
        }

        private IEnumerator HideAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);
            ClearMessage();
        }

        private void ClearMessage()
        {
            if (_messageText != null)
                _messageText.text = string.Empty;
        }
    }
}