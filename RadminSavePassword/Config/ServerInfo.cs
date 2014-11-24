using System;

namespace RadminSavePassword
{
    [Serializable]
    public class ServerInfo
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}