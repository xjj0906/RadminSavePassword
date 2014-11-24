using System.Collections.Generic;

namespace RadminSavePassword
{
    public static class Global
    {
        public static SystemConfig SystemConfig { get; set; }
        public static Dictionary<string, ServerInfo> UnSaveServerInfos { get; set; }

        static Global()
        {
            SystemConfig = new SystemConfig();
            UnSaveServerInfos = new Dictionary<string, ServerInfo>();
        }
    }
}