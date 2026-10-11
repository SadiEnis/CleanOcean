using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoSingleton<RelayManager>
{
    [Header("Relay Connection")]
    [SerializeField] private bool _forceWebSocketsInEditor = true;

    public async Task<string> CreateRelay(int maxPlayers)
    {
        try
        {
            int maxConnections = Mathf.Max(1, maxPlayers - 1);

            Allocation allocation =
                await RelayService.Instance.CreateAllocationAsync(maxConnections);

            string joinCode =
                await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            string connectionType = GetRelayConnectionType();

            RelayServerData relayServerData =
                AllocationUtils.ToRelayServerData(allocation, connectionType);

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(relayServerData);

            Debug.Log($"[RelayManager] Relay created. Code: {joinCode}, ConnectionType: {connectionType}");

            return joinCode;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[RelayManager] CreateRelay failed: {exception}");
            return null;
        }
    }

    public async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation =
                await RelayService.Instance.JoinAllocationAsync(joinCode);

            string connectionType = GetRelayConnectionType();

            RelayServerData relayServerData =
                AllocationUtils.ToRelayServerData(joinAllocation, connectionType);

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(relayServerData);

            Debug.Log($"[RelayManager] Relay joined. Code: {joinCode}, ConnectionType: {connectionType}");

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[RelayManager] JoinRelay failed: {exception}");
            return false;
        }
    }

    private string GetRelayConnectionType()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return "wss";
#else
        return _forceWebSocketsInEditor ? "wss" : "udp";
#endif
    }
}