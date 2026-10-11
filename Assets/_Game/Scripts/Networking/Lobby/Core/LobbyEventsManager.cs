using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyEventsManager : MonoSingleton<LobbyEventsManager>
{
    public event EventHandler OnCreatedLobby;
    public event EventHandler OnLeftLobby;
    public event EventHandler OnDeletedLobby;
    public event EventHandler OnPressPlayButton;
    public event EventHandler OnReturnToMainMenu;
    public event EventHandler OnReturnToCreateProfile;
    public event EventHandler OnReturnToLobbiesList;

    public event EventHandler<LobbyEventsArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventsArgs> OnUpdatedLobby;

    public class LobbyEventsArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventsArgs> OnLobbyListChanged;

    public class OnLobbyListChangedEventsArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public event EventHandler<OnNameChangedArgs> OnNameChanged;

    public class OnNameChangedArgs : EventArgs
    {
        public string playerName;
    }

    public void TriggerOnCreatedLobby()
    {
        InvokeSafe(OnCreatedLobby, nameof(OnCreatedLobby));
    }
    public void TriggerOnLeftLobby()
    {
        InvokeSafe(OnLeftLobby, nameof(OnLeftLobby));
    }

    public void TriggerOnDeletedLobby()
    {
        InvokeSafe(OnDeletedLobby, nameof(OnDeletedLobby));
    }

    public void TriggerOnReturnToMainMenu()
    {
        InvokeSafe(OnReturnToMainMenu, nameof(OnReturnToMainMenu));
    }

    public void TriggerOnReturnToLobbiesList()
    {
        InvokeSafe(OnReturnToLobbiesList, nameof(OnReturnToLobbiesList));
    }
    public void TriggerOnPressPlayButton() => OnPressPlayButton?.Invoke(this, EventArgs.Empty);
    public void TriggerOnReturnCreateProfile()  => OnReturnToCreateProfile?.Invoke(this, EventArgs.Empty);

    public void TriggerOnLobbyListChanged(List<Lobby> lobbyList)
    {
        if (OnLobbyListChanged == null)
            return;

        var args = new OnLobbyListChangedEventsArgs
        {
            lobbyList = lobbyList
        };

        foreach (EventHandler<OnLobbyListChangedEventsArgs> handler in OnLobbyListChanged.GetInvocationList())
        {
            try
            {
                handler.Invoke(this, args);
            }
            catch (MissingReferenceException exception)
            {
                Debug.LogError(
                    $"[LobbyEventsManager] MissingReferenceException in OnLobbyListChanged subscriber: " +
                    $"{handler.Target?.GetType().Name}.{handler.Method.Name} | {exception.Message}"
                );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[LobbyEventsManager] Exception in OnLobbyListChanged subscriber: " +
                    $"{handler.Target?.GetType().Name}.{handler.Method.Name} | {exception}"
                );
            }
        }
    }

    public void TriggerOnUpdatedLobby(Lobby lobby)
    {
        OnUpdatedLobby?.Invoke(this, new LobbyEventsArgs()
        {
            lobby = lobby
        });
    }

    public void TriggerOnNameChanged(OnNameChangedArgs args)
    {
        OnNameChanged?.Invoke(this, args);
    }

    public void TriggerOnJoinedLobby(Lobby lobby)
    {
        InvokeSafe(
            OnJoinedLobby,
            new LobbyEventsArgs
            {
                lobby = lobby
            },
            nameof(OnJoinedLobby)
        );
    }
    
    // Helper
    private static string GetHandlerName(Delegate handler)
    {
        string targetName = handler.Target == null
            ? "static"
            : handler.Target.GetType().Name;

        return $"{targetName}.{handler.Method.Name}";
    }

    private void InvokeSafe(EventHandler eventHandler, string eventName)
    {
        if (eventHandler == null)
            return;

        foreach (EventHandler handler in eventHandler.GetInvocationList())
        {
            try
            {
                handler.Invoke(this, EventArgs.Empty);
            }
            catch (MissingReferenceException exception)
            {
                Debug.LogError(
                    $"[LobbyEventsManager] MissingReferenceException in {eventName} subscriber: " +
                    $"{GetHandlerName(handler)} | {exception.Message}"
                );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[LobbyEventsManager] Exception in {eventName} subscriber: " +
                    $"{GetHandlerName(handler)} | {exception}"
                );
            }
        }
    }

    private void InvokeSafe<TArgs>(
        EventHandler<TArgs> eventHandler,
        TArgs args,
        string eventName)
        where TArgs : EventArgs
    {
        if (eventHandler == null)
            return;

        foreach (EventHandler<TArgs> handler in eventHandler.GetInvocationList())
        {
            try
            {
                handler.Invoke(this, args);
            }
            catch (MissingReferenceException exception)
            {
                Debug.LogError(
                    $"[LobbyEventsManager] MissingReferenceException in {eventName} subscriber: " +
                    $"{GetHandlerName(handler)} | {exception.Message}"
                );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[LobbyEventsManager] Exception in {eventName} subscriber: " +
                    $"{GetHandlerName(handler)} | {exception}"
                );
            }
        }
    }
}