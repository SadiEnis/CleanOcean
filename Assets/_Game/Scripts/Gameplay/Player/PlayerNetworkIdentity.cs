using _Game.Scripts.Session;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Game.Scripts.Gameplay.Player
{
    // To ensure that the database userID(CurrentUser.userId) is matched with the OwnerID in the game
    public class PlayerNetworkIdentity : NetworkBehaviour
    {
        public NetworkVariable<ulong> dbUserId = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<FixedString32Bytes> username = new(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            if (!CurrentUser.IsLoggedIn)
            {
                Debug.LogWarning("[PlayerNetworkIdentity] Current User is not logged in!");
                return;
            }
            SubmitIdentityServerRpc(CurrentUser.UserId, CurrentUser.Username);
        }

        [ServerRpc]
        private void SubmitIdentityServerRpc(ulong userId, string username)
        {
            dbUserId.Value = userId;
            this.username.Value = username;
            
            Debug.Log($"[PlayerNetworkIdentity] Player identity submitted. OwnerClientId={OwnerClientId}, DbUserId={dbUserId.Value}, Username={this.username.Value}");
        }
    }
}