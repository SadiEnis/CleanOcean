using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : BaseUIComponent
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        playButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            LobbyEventsManager.Instance.TriggerOnPressPlayButton();
        });
        settingsButton.onClick.AddListener(() => { });
        exitButton.onClick.AddListener(() => { Application.Quit(); });

        LobbyEventsManager.Instance.OnReturnToMainMenu += (object sender, EventArgs e) => Show();
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost) Hide(); // Bağlantı var mı? 
    }
}