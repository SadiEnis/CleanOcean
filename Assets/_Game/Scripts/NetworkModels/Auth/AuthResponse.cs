using System;

namespace _Game.Scripts.NetworkModels.Auth
{
    [Serializable]
    public class AuthResponse
    {
        public bool success;
        public string message;

        public ulong userId;
        public string username;
        public string displayName;
    }
}