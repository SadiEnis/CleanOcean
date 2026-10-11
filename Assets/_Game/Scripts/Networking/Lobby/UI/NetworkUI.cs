using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;

    private void Awake()
    {
        _hostButton.onClick.AddListener(() =>
        {
            try
            {
                NetworkManager.Singleton.StartHost();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to start host: {e.Message}");
            }
        });
        _clientButton.onClick.AddListener(() =>
        {
            try
            {
                NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to start client: {e.Message}");
            }
        });
    }
}
