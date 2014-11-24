using System;

namespace RadminSavePassword.Hook
{
    public class ServerInfoEventArgs : EventArgs
    {
        public ServerInfo ServerInfo { get; private set; }

        public ServerInfoEventArgs(ServerInfo serverInfo)
        {
            ServerInfo = serverInfo;
        }
    }
}