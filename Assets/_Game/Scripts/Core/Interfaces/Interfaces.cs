using Unity.Netcode;
using UnityEngine;

public class Interfaces
{
    public interface ILobbyService
    {
    }

    public interface IUnityLobbyService : ILobbyService
    {
        void CreateLobby(string playerName, string lobbyName, string maxPlayers, bool isPrivate);
        void ListLobbies();
        void JoinLobbyByCode(string lobbyCode, string playerName);
        void JoinLobbyById(string lobbyCode, string playerName);
        void LeaveLobby();
        void DeleteLobby();
    }
    // public interface ISteamworksLobbyService : ILobbyService { }

    public interface IAttachable
    {
        NetworkVariable<NetworkObjectReference> GetHoldingPlayer { get; }
        void AttachServerRpc(ServerRpcParams rpcParams = default);
        void DetachServerRpc(ServerRpcParams rpcParams = default);
    }

    public interface IRecyclable : IAttachable
    {
        int GetScoreValue { get; }
        void RecycleServerRpc();
    }

    public interface ISpawnable
    {
        void NetworkDespawn();
    }
}