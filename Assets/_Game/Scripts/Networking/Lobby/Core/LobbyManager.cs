using System;

public class LobbyManager : MonoSingleton<LobbyManager>
{
    [NonSerialized] public string playerName;

    private async void Start()
    {
        LobbyEventsManager.Instance.OnNameChanged += LobbyManager_OnNameChanged;
    }

    private void LobbyManager_OnNameChanged(object sender, LobbyEventsManager.OnNameChangedArgs e)
    {
        LobbyService.Instance.Authenticate(e.playerName);
        playerName = e.playerName;
    }
}