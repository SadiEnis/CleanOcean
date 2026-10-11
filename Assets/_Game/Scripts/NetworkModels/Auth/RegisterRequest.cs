using System;

namespace _Game.Scripts.NetworkModels.Auth
{
    [Serializable]
    public class RegisterRequest
    {
        public string username;
        public string password;
        public string displayName;

        public RegisterRequest(string username, string password, string displayName)
        {
            this.username = username;
            this.password = password;
            this.displayName = displayName;
        }
    }
}