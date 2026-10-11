using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyService : MonoSingleton<LobbyService>, Interfaces.IUnityLobbyService
{
    public Lobby hostLobby;
    public Lobby joinedLobby;

    private List<Lobby> lobbiesList;
    private Coroutine heartbeatCoroutine;
    private float refreshTimer;
    private float lobbyPollTimer;
    private bool _isRefreshingLobby;
    private bool _isHandlingMigration;
    private string _connectedRelayCode;

    private const string KEY_RELAY_CODE = "RelayJoinCode";

    private void Update()
    {
        LobbyRefresh();
    }

    private IEnumerator HeartbeaLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            if (hostLobby != null)
                Unity.Services.Lobbies.LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            else
            {
                yield break;
            }

            yield return delay;
        }
    }
    
    // public async void LobbyHeartbeat()
    // {
    //     if (hostLobby != null)
    //     {
    //         heartbeatTimer -= Time.deltaTime;
    //         if (heartbeatTimer <= 0)
    //         {
    //             heartbeatTimer = 15f;
    //             await Unity.Services.Lobbies.LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
    //         }
    //     }
    // }

    public async void LobbyRefresh()
    {
        if (joinedLobby == null)
            return;

        if (_isRefreshingLobby)
            return;

        refreshTimer -= Time.deltaTime;

        if (refreshTimer > 0)
            return;

        refreshTimer = 2f;
        _isRefreshingLobby = true;

        try
        {
            Lobby previousLobby = joinedLobby;

            string previousHostId = previousLobby.HostId;
            string previousRelayCode = GetRelayCode(previousLobby);

            Lobby updatedLobby =
                await Unity.Services.Lobbies.LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

            string updatedHostId = updatedLobby.HostId;
            string updatedRelayCode = GetRelayCode(updatedLobby);

            joinedLobby = updatedLobby;

            await HandleLobbyMigrationChangesAsync(
                previousHostId,
                updatedHostId,
                previousRelayCode,
                updatedRelayCode,
                updatedLobby
            );

            LobbyEventsManager.Instance.TriggerOnUpdatedLobby(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[LobbyService] LobbyRefresh LobbyServiceException: {e}");

            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                Debug.LogWarning("[LobbyService] Current lobby was deleted or no longer exists.");

                StopHeartbeat();

                joinedLobby = null;
                hostLobby = null;
                _connectedRelayCode = string.Empty;

                await ShutdownNetworkIfNeededAsync();

                LobbyEventsManager.Instance.TriggerOnDeletedLobby();

                if (LobbyUI.Instance != null)
                    LobbyUI.Instance.ShowLobbiesListState();

                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LobbyService] LobbyRefresh unexpected error: {e}");
        }
        finally
        {
            _isRefreshingLobby = false;
        }
    }
    private async Task LobbyPollingLoop(float pollingRate)
    {
        // Bu tarz bir yöntem de varmış. Denemek için yazdım. Refresh çalışınca kullanılmadı.
        while (joinedLobby != null)
        {
            try
            {
                await Task.Delay((int)(pollingRate * 1000));
            }
            catch (TaskCanceledException)
            {
                break;
            }

            if (joinedLobby == null) break;

            try
            {
                Lobby updatedLobby = await Unity.Services.Lobbies.LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                if (updatedLobby.Version > joinedLobby.Version)
                {
                    joinedLobby = updatedLobby;
                    LobbyEventsManager.Instance.TriggerOnUpdatedLobby(joinedLobby);
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Lobi servisi hatası: {e}");
                joinedLobby = null; // Lobi yok, döngüyü de durdur.
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lobi sorgulamada beklenmedik hata: {ex}");
            }
        }
    }


    public async void Authenticate(string playerName)
    {
        try
        {
            string safeProfileName = ToSafeUnityProfileName(playerName);

            Debug.Log($"Unity Auth Profile Name → Raw='{playerName}', Safe='{safeProfileName}'");

            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                InitializationOptions initializationOptions = new InitializationOptions();
                initializationOptions.SetProfile(safeProfileName);

                await UnityServices.InitializeAsync(initializationOptions);
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                Debug.Log("Unity Authentication already signed in. Reusing existing session.");
            }

            Debug.Log(
                $"Signed in! PlayerId={AuthenticationService.Instance.PlayerId}, " +
                $"DisplayName={playerName}"
            );

            LobbyUI.Instance.authUI.Hide();
            LobbyUI.Instance.mainHubUI.Show();

            ListLobbies();
        }
        catch (Exception exception)
        {
            Debug.LogError($"LobbyService Authenticate failed: {exception.Message}");
        }
    }

public async void CreateLobby(string playerName, string lobbyName, string maxPlayers, bool isPrivate)
{
    Debug.Log("[LobbyService] CreateLobby started.");

    int intMaxPlayers = int.Parse(maxPlayers);

    try
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("[LobbyService] NetworkManager was still listening before CreateLobby. Shutting it down first.");
            await ShutdownNetworkIfNeededAsync();
        }
        
        string _lobbyName = string.IsNullOrEmpty(lobbyName) ? "New Lobby" : lobbyName;
        string _playername = string.IsNullOrEmpty(playerName) ? "NewPlayer" : playerName;

        if (intMaxPlayers > 4) intMaxPlayers = 4;
        int _maxPlayers = (intMaxPlayers > 0) ? intMaxPlayers : 4;

        Debug.Log("[LobbyService] Creating relay...");

        string relayJoinCode = await RelayManager.Instance.CreateRelay(_maxPlayers);
        _connectedRelayCode = relayJoinCode;

        if (string.IsNullOrWhiteSpace(relayJoinCode))
        {
            Debug.LogError("[LobbyService] Relay code is null or empty. Lobby creation cancelled.");
            return;
        }

        Debug.Log($"[LobbyService] Relay created. Code: {relayJoinCode}");
        Debug.Log("[LobbyService] Creating Unity Lobby...");

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = GetPlayer(_playername),
            Data = new Dictionary<string, DataObject>
            {
                {
                    KEY_RELAY_CODE,
                    new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode)
                },
            }
        };

        Lobby lobby = await Unity.Services.Lobbies.LobbyService.Instance.CreateLobbyAsync(
            _lobbyName,
            _maxPlayers,
            options
        );

        Debug.Log($"[LobbyService] Unity Lobby created. Name: {lobby.Name}, Id: {lobby.Id}");

        hostLobby = lobby;
        joinedLobby = lobby;

        if (heartbeatCoroutine != null)
            StopCoroutine(heartbeatCoroutine);

        heartbeatCoroutine = StartCoroutine(HeartbeaLobbyCoroutine(hostLobby.Id, 15f));
    
        Debug.Log("[LobbyService] Starting host...");

        bool hostStarted = NetworkManager.Singleton.StartHost();

        Debug.Log(
            $"[LobbyService] StartHost result: {hostStarted} | " +
            $"IsListening: {NetworkManager.Singleton.IsListening} | " +
            $"IsHost: {NetworkManager.Singleton.IsHost} | " +
            $"IsServer: {NetworkManager.Singleton.IsServer} | " +
            $"IsClient: {NetworkManager.Singleton.IsClient}"
        );

        if (!hostStarted)
        {
            Debug.LogError("[LobbyService] Host could not be started. Cleaning up created lobby.");

            await Unity.Services.Lobbies.LobbyService.Instance.DeleteLobbyAsync(lobby.Id);

            joinedLobby = null;
            hostLobby = null;

            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }

            return;
        }

        Debug.Log("[LobbyService] Triggering OnJoinedLobby...");
        LobbyEventsManager.Instance.TriggerOnJoinedLobby(lobby);
        Debug.Log("[LobbyService] OnJoinedLobby completed.");

        Debug.Log("[LobbyService] Triggering OnCreatedLobby...");
        LobbyEventsManager.Instance.TriggerOnCreatedLobby();
        Debug.Log("[LobbyService] OnCreatedLobby completed.");

        Debug.Log("[LobbyService] Forcing LobbyInfo UI state...");
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.ShowLobbyInfoState();
        }
        Debug.Log("[LobbyService] Force LobbyInfo UI state completed.");

        Debug.Log("Lobby created: " + lobby.Name + " with ID: " + lobby.Id + " with code: " + lobby.LobbyCode);

        foreach (var player in lobby.Players)
            Debug.Log("Player Name: " + player.Data["PlayerName"].Value);
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError($"[LobbyService] LobbyServiceException while creating lobby: {e}");
    }
    catch (Exception e)
    {
        Debug.LogError($"[LobbyService] Unexpected exception while creating lobby: {e}");
    }
}

public async void JoinLobbyById(string lobbyId, string playerName)
{
    try
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("[LobbyService] NetworkManager was still listening before JoinLobbyById. Shutting it down first.");
            await ShutdownNetworkIfNeededAsync();
        }

        JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
        {
            Player = GetPlayer(playerName)
        };

        Debug.Log($"[LobbyService] Joining lobby by id: {lobbyId}");

        Lobby lobby = await Unity.Services.Lobbies.LobbyService.Instance.JoinLobbyByIdAsync(
            lobbyId,
            options
        );

        joinedLobby = lobby;

        string relayJoinCode = await GetRelayCodeWithRefreshAsync(lobby);

        if (string.IsNullOrWhiteSpace(relayJoinCode))
        {
            Debug.LogError("[LobbyService] Joined lobby but RelayJoinCode is missing. Cleaning up failed join.");
            await CleanupFailedJoinAsync(lobby);
            return;
        }

        Debug.Log($"[LobbyService] Joining relay with code: {relayJoinCode}");

        bool isConnectedRelay = await RelayManager.Instance.JoinRelay(relayJoinCode);

        if (!isConnectedRelay)
        {
            Debug.LogError("[LobbyService] Joined lobby but couldn't connect relay. Cleaning up failed join.");
            await CleanupFailedJoinAsync(lobby);
            return;
        }

        _connectedRelayCode = relayJoinCode;

        bool clientStarted = NetworkManager.Singleton.StartClient();

        Debug.Log(
            $"[LobbyService] StartClient result: {clientStarted} | " +
            $"IsListening: {NetworkManager.Singleton.IsListening} | " +
            $"IsHost: {NetworkManager.Singleton.IsHost} | " +
            $"IsServer: {NetworkManager.Singleton.IsServer} | " +
            $"IsClient: {NetworkManager.Singleton.IsClient}"
        );

        if (!clientStarted)
        {
            Debug.LogError("[LobbyService] StartClient failed after joining relay. Cleaning up failed join.");
            await CleanupFailedJoinAsync(lobby);
            return;
        }

        Debug.Log($"[LobbyService] Joined lobby and relay successfully. Lobby: {lobby.Name}");

        LobbyEventsManager.Instance.TriggerOnJoinedLobby(joinedLobby);

        if (LobbyUI.Instance != null)
            LobbyUI.Instance.ShowLobbyInfoState();

        foreach (var player in joinedLobby.Players)
        {
            if (player.Data != null && player.Data.ContainsKey("PlayerName"))
                Debug.Log("Player Name: " + player.Data["PlayerName"].Value);
        }
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError($"[LobbyService] LobbyServiceException while joining lobby by id: {e}");
    }
    catch (Exception e)
    {
        Debug.LogError($"[LobbyService] Unexpected exception while joining lobby by id: {e}");
    }
}

    public async void JoinLobbyByCode(string lobbyCode, string playerName)
{
    try
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("[LobbyService] NetworkManager was still listening before JoinLobbyByCode. Shutting it down first.");
            await ShutdownNetworkIfNeededAsync();
        }

        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = GetPlayer(playerName)
        };

        Debug.Log($"[LobbyService] Joining lobby by code: {lobbyCode}");

        Lobby lobby = await Unity.Services.Lobbies.LobbyService.Instance.JoinLobbyByCodeAsync(
            lobbyCode,
            options
        );

        joinedLobby = lobby;

        string relayJoinCode = await GetRelayCodeWithRefreshAsync(lobby);

        if (string.IsNullOrWhiteSpace(relayJoinCode))
        {
            Debug.LogError("[LobbyService] Joined lobby but RelayJoinCode is missing. Cleaning up failed join.");
            await CleanupFailedJoinAsync(lobby);
            return;
        }

        Debug.Log($"[LobbyService] Joining relay with code: {relayJoinCode}");

        bool isConnectedRelay = await RelayManager.Instance.JoinRelay(relayJoinCode);

        if (!isConnectedRelay)
        {
            Debug.LogError("[LobbyService] Joined lobby but couldn't connect relay. Cleaning up failed join.");
            await CleanupFailedJoinAsync(lobby);
            return;
        }

        _connectedRelayCode = relayJoinCode;

        bool clientStarted = NetworkManager.Singleton.StartClient();

        Debug.Log(
            $"[LobbyService] StartClient result: {clientStarted} | " +
            $"IsListening: {NetworkManager.Singleton.IsListening} | " +
            $"IsHost: {NetworkManager.Singleton.IsHost} | " +
            $"IsServer: {NetworkManager.Singleton.IsServer} | " +
            $"IsClient: {NetworkManager.Singleton.IsClient}"
        );

        if (!clientStarted)
        {
            Debug.LogError("[LobbyService] StartClient failed after joining relay. Cleaning up failed join.");
            await CleanupFailedJoinAsync(lobby);
            return;
        }

        Debug.Log($"[LobbyService] Joined lobby and relay successfully. Lobby: {lobby.Name}");

        LobbyEventsManager.Instance.TriggerOnJoinedLobby(joinedLobby);

        if (LobbyUI.Instance != null)
            LobbyUI.Instance.ShowLobbyInfoState();

        foreach (var player in joinedLobby.Players)
        {
            if (player.Data != null && player.Data.ContainsKey("PlayerName"))
                Debug.Log("Player Name: " + player.Data["PlayerName"].Value);
        }
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError($"[LobbyService] LobbyServiceException while joining lobby by code: {e}");
    }
    catch (Exception e)
    {
        Debug.LogError($"[LobbyService] Unexpected exception while joining lobby by code: {e}");
    }
}

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 10,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                    // --> Sıralama oluşturulmaya göre yapılacak ve azalan sırada olacak.
                }
            };

            QueryResponse lobbiesListResult = await Unity.Services.Lobbies.LobbyService.Instance.QueryLobbiesAsync(options);
            lobbiesList = lobbiesListResult.Results;

            Debug.Log(lobbiesList.Count + " available lobbies found.");
            foreach (var lobby in lobbiesList)
                Debug.Log(lobby.Name + " with Code: " + lobby.LobbyCode);

            LobbyEventsManager.Instance.TriggerOnLobbyListChanged(lobbiesList);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
    
    public async void LeaveLobby()
    {
        await LeaveLobbyAsync();
    }
    
   public async Task LeaveLobbyAsync(bool triggerUiEvents = true)
{
    if (joinedLobby == null)
    {
        await ShutdownNetworkIfNeededAsync();
        _connectedRelayCode = string.Empty;
        return;
    }

    try
    {
        string playerId = AuthenticationService.Instance.PlayerId;

        if (joinedLobby.Players.Count == 1)
        {
            Debug.Log("[LobbyService] Leaving solo lobby. Deleting lobby.");

            await Unity.Services.Lobbies.LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        else
        {
            if (IsLobbyHost())
            {
                Player newHostPlayer = SelectNewHostPlayer();

                if (newHostPlayer != null)
                {
                    Debug.Log($"[LobbyService] Migrating lobby host to player: {newHostPlayer.Id}");

                    UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
                    {
                        HostId = newHostPlayer.Id
                    };

                    joinedLobby = await Unity.Services.Lobbies.LobbyService.Instance.UpdateLobbyAsync(
                        joinedLobby.Id,
                        updateLobbyOptions
                    );

                    Debug.Log("[LobbyService] Lobby host migration request completed.");
                }
                else
                {
                    Debug.LogWarning("[LobbyService] No new host candidate found.");
                }
            }

            Debug.Log("[LobbyService] Removing local player from lobby.");

            await Unity.Services.Lobbies.LobbyService.Instance.RemovePlayerAsync(
                joinedLobby.Id,
                playerId
            );
        }

        StopHeartbeat();

        joinedLobby = null;
        hostLobby = null;
        _connectedRelayCode = string.Empty;

        await ShutdownNetworkIfNeededAsync();

        if (triggerUiEvents)
        {
            LobbyEventsManager.Instance.TriggerOnLeftLobby();

            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.ShowLobbiesListState();
            }

            ListLobbies();
        }
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError($"[LobbyService] LeaveLobbyAsync LobbyServiceException: {e}");

        StopHeartbeat();

        joinedLobby = null;
        hostLobby = null;
        _connectedRelayCode = string.Empty;

        await ShutdownNetworkIfNeededAsync();
        
        if (triggerUiEvents && LobbyUI.Instance != null)
        {
            LobbyUI.Instance.ShowLobbiesListState();
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"[LobbyService] LeaveLobbyAsync unexpected exception: {e}");

        StopHeartbeat();

        joinedLobby = null;
        hostLobby = null;
        _connectedRelayCode = string.Empty;

        await ShutdownNetworkIfNeededAsync();
        
        if (triggerUiEvents && LobbyUI.Instance != null)
        {
            LobbyUI.Instance.ShowLobbiesListState();
        }
    }
}

public async void DeleteLobby()
{
    try
    {
        if (joinedLobby == null)
        {
            await ShutdownNetworkIfNeededAsync();

            hostLobby = null;
            _connectedRelayCode = string.Empty;

            LobbyEventsManager.Instance.TriggerOnDeletedLobby();
            return;
        }

        Debug.Log("[LobbyService] Deleting lobby...");

        await Unity.Services.Lobbies.LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

        StopHeartbeat();

        joinedLobby = null;
        hostLobby = null;
        _connectedRelayCode = string.Empty;

        await ShutdownNetworkIfNeededAsync();

        LobbyEventsManager.Instance.TriggerOnDeletedLobby();

        Debug.Log("[LobbyService] Lobby deleted and local network state cleaned.");
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError($"[LobbyService] DeleteLobby LobbyServiceException: {e}");

        StopHeartbeat();

        joinedLobby = null;
        hostLobby = null;
        _connectedRelayCode = string.Empty;

        await ShutdownNetworkIfNeededAsync();

        LobbyEventsManager.Instance.TriggerOnDeletedLobby();
    }
    catch (Exception e)
    {
        Debug.LogError($"[LobbyService] DeleteLobby unexpected exception: {e}");

        StopHeartbeat();

        joinedLobby = null;
        hostLobby = null;
        _connectedRelayCode = string.Empty;

        await ShutdownNetworkIfNeededAsync();

        LobbyEventsManager.Instance.TriggerOnDeletedLobby();
    }
}

private async Task CleanupFailedJoinAsync(Lobby lobby)
{
    try
    {
        if (lobby != null)
        {
            await Unity.Services.Lobbies.LobbyService.Instance.RemovePlayerAsync(
                lobby.Id,
                GetLocalPlayerId()
            );
        }
    }
    catch (Exception exception)
    {
        Debug.LogWarning($"[LobbyService] Failed join cleanup warning: {exception.Message}");
    }

    joinedLobby = null;
    hostLobby = null;
    _connectedRelayCode = string.Empty;

    await ShutdownNetworkIfNeededAsync();

    if (LobbyUI.Instance != null)
        LobbyUI.Instance.ShowLobbiesListState();

    ListLobbies();
}

    public bool IsLocalPlayerLobbyHost()
    {
        if (joinedLobby == null)
            return false;

        return joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    
    private async Task HandleLobbyMigrationChangesAsync(
    string previousHostId,
    string updatedHostId,
    string previousRelayCode,
    string updatedRelayCode,
    Lobby updatedLobby)
{
    if (_isHandlingMigration)
        return;

    if (updatedLobby == null)
        return;

    string localPlayerId = GetLocalPlayerId();

    bool hostChanged =
        !string.IsNullOrEmpty(previousHostId) &&
        !string.IsNullOrEmpty(updatedHostId) &&
        previousHostId != updatedHostId;

    bool relayChanged =
        !string.IsNullOrEmpty(previousRelayCode) &&
        !string.IsNullOrEmpty(updatedRelayCode) &&
        previousRelayCode != updatedRelayCode;

    if (!hostChanged && !relayChanged)
        return;

    try
    {
        _isHandlingMigration = true;

        Debug.Log(
            $"[LobbyService] Migration check | " +
            $"LocalPlayerId: {localPlayerId}, " +
            $"PreviousHostId: {previousHostId}, " +
            $"UpdatedHostId: {updatedHostId}, " +
            $"PreviousRelayCode: {previousRelayCode}, " +
            $"UpdatedRelayCode: {updatedRelayCode}, " +
            $"HostChanged: {hostChanged}, " +
            $"RelayChanged: {relayChanged}"
        );

        if (hostChanged)
        {
            Debug.Log($"[LobbyService] Host changed. Old: {previousHostId}, New: {updatedHostId}");

            if (updatedHostId == localPlayerId)
            {
                Debug.Log("[LobbyService] Local player is the new lobby host. Starting migrated host flow.");

                await BecomeMigratedHostAsync(updatedLobby);
                return;
            }

            Debug.Log("[LobbyService] Local player is not the new host. Waiting for migrated relay code.");
        }

        if (relayChanged)
        {
            if (updatedHostId == localPlayerId)
            {
                Debug.Log("[LobbyService] Relay changed, but local player is host. Skipping client reconnect.");
                return;
            }

            if (_connectedRelayCode == updatedRelayCode)
            {
                Debug.Log("[LobbyService] Relay changed event received, but client is already connected to this relay. Skipping.");
                return;
            }

            Debug.Log("[LobbyService] Relay code changed. Reconnecting client to migrated relay.");

            await ReconnectClientToMigratedRelayAsync(updatedRelayCode);
        }
    }
    catch (Exception exception)
    {
        Debug.LogError($"[LobbyService] HandleLobbyMigrationChangesAsync failed: {exception}");
    }
    finally
    {
        _isHandlingMigration = false;
    }
}
    
    private async Task BecomeMigratedHostAsync(Lobby lobby)
    {
        Debug.Log("[LobbyService] This client became the new lobby host.");

        await ShutdownNetworkIfNeededAsync();

        int maxPlayers = lobby.MaxPlayers;

        Debug.Log("[LobbyService] Creating new relay for migrated host.");

        string newRelayCode = await RelayManager.Instance.CreateRelay(maxPlayers);
        _connectedRelayCode = newRelayCode;

        Debug.Log($"[LobbyService] New migrated relay code: {newRelayCode}");

        bool hostStarted = NetworkManager.Singleton.StartHost();

        Debug.Log(
            $"[LobbyService] Migrated StartHost result: {hostStarted} | " +
            $"IsListening: {NetworkManager.Singleton.IsListening} | " +
            $"IsHost: {NetworkManager.Singleton.IsHost} | " +
            $"IsServer: {NetworkManager.Singleton.IsServer} | " +
            $"IsClient: {NetworkManager.Singleton.IsClient}"
        );

        if (!hostStarted)
        {
            Debug.LogError("[LobbyService] Migrated host could not start host.");
            return;
        }

        UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                {
                    KEY_RELAY_CODE,
                    new DataObject(DataObject.VisibilityOptions.Public, newRelayCode)
                }
            }
        };

        joinedLobby = await Unity.Services.Lobbies.LobbyService.Instance.UpdateLobbyAsync(
            lobby.Id,
            updateLobbyOptions
        );

        hostLobby = joinedLobby;

        StopHeartbeat();
        heartbeatCoroutine = StartCoroutine(HeartbeaLobbyCoroutine(hostLobby.Id, 15f));

        Debug.Log("[LobbyService] Migrated host updated lobby relay data.");

        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.ShowLobbyInfoState();
        }

        LobbyEventsManager.Instance.TriggerOnUpdatedLobby(joinedLobby);
    }
    
    private async Task ReconnectClientToMigratedRelayAsync(string newRelayCode)
    {
        if (string.IsNullOrEmpty(newRelayCode))
            return;

        if (_connectedRelayCode == newRelayCode)
            return;

        Debug.Log($"[LobbyService] Reconnecting client to migrated relay: {newRelayCode}");

        await ShutdownNetworkIfNeededAsync();

        bool joinedRelay = await RelayManager.Instance.JoinRelay(newRelayCode);

        if (!joinedRelay)
        {
            Debug.LogError("[LobbyService] Failed to join migrated relay.");
            return;
        }

        _connectedRelayCode = newRelayCode;

        bool clientStarted = NetworkManager.Singleton.StartClient();

        Debug.Log(
            $"[LobbyService] Migrated StartClient result: {clientStarted} | " +
            $"IsListening: {NetworkManager.Singleton.IsListening} | " +
            $"IsHost: {NetworkManager.Singleton.IsHost} | " +
            $"IsServer: {NetworkManager.Singleton.IsServer} | " +
            $"IsClient: {NetworkManager.Singleton.IsClient}"
        );

        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.ShowLobbyInfoState();
        }
    }

    private bool IsLobbyHost()
    {
        if (joinedLobby == null) return false;
        return joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
            foreach (Player player in joinedLobby.Players)
                if (player.Id == AuthenticationService.Instance.PlayerId)
                    return true;
        return false;
    }

    private static Player GetPlayer(string playerName)
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)
                } // PlayerName adında bir veri ekliyoruz. VisibilityOptions.Public ile bu veriye lobbydeki herkes erişebilir demiş oluyoruz.
            }
        };
    }
    
    private static string ToSafeUnityProfileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "player";

        var chars = new List<char>();

        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
            {
                // Unity profile için ASCII dışı karakterleri riske atmamak daha güvenli.
                if (c <= 127)
                    chars.Add(c);
            }
            else if (char.IsWhiteSpace(c))
            {
                chars.Add('_');
            }
        }

        string safeName = new string(chars.ToArray());

        if (string.IsNullOrWhiteSpace(safeName))
            safeName = "player";

        if (safeName.Length > 30)
            safeName = safeName.Substring(0, 30);

        return safeName;
    }
    
    private string GetLocalPlayerId()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    private string GetRelayCode(Lobby lobby)
    {
        if (lobby == null)
            return string.Empty;

        if (lobby.Data == null)
            return string.Empty;

        if (!lobby.Data.TryGetValue(KEY_RELAY_CODE, out DataObject relayCodeData))
            return string.Empty;

        return relayCodeData.Value;
    }

    private Player SelectNewHostPlayer()
    {
        if (joinedLobby == null || joinedLobby.Players == null)
            return null;

        string localPlayerId = GetLocalPlayerId();

        return joinedLobby.Players.FirstOrDefault(player => player.Id != localPlayerId);
    }

    private void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }

    private async Task ShutdownNetworkIfNeededAsync()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsListening)
            return;

        NetworkManager.Singleton.Shutdown();

        while (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            await Task.Yield();
        }

        Debug.Log("[LobbyService] NetworkManager shutdown completed.");
    }
    private async Task<string> GetRelayCodeWithRefreshAsync(Lobby lobby)
    {
        string relayCode = GetRelayCode(lobby);

        if (!string.IsNullOrWhiteSpace(relayCode))
            return relayCode;

        if (lobby == null)
            return string.Empty;

        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(250);

            try
            {
                Lobby refreshedLobby =
                    await Unity.Services.Lobbies.LobbyService.Instance.GetLobbyAsync(lobby.Id);

                joinedLobby = refreshedLobby;

                relayCode = GetRelayCode(refreshedLobby);

                if (!string.IsNullOrWhiteSpace(relayCode))
                    return relayCode;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[LobbyService] Relay code refresh attempt failed: {exception.Message}");
            }
        }

        return string.Empty;
    }
}