using System;

namespace _Game.Scripts.NetworkModels.Auth
{
    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;

        public LoginRequest(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
}