using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RadminSavePassword
{
    [Serializable]
    public class SystemConfig
    {
        public bool IsAutoEnter { get; set; }

        public string RadminOpenPath { get; set; }

        public Dictionary<string, ServerInfo> ServerList { get; set; }

        public SystemConfig()
        {
            ServerList = new Dictionary<string, ServerInfo>();
        }

        public static SystemConfig Load(string filePath)
        {
            using (Stream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] byteData = new byte[fStream.Length];
                fStream.Read(byteData, 0, byteData.Length);
                fStream.Position = 0;

                BinaryFormatter binaryFormat = new BinaryFormatter();
                SystemConfig config = (SystemConfig)binaryFormat.Deserialize(fStream);

                return config;
            }
        }

        public static void Save(SystemConfig config, string filePath)
        {
            using (Stream fStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter binaryFormat = new BinaryFormatter();
                binaryFormat.Serialize(fStream, config);

                fStream.Flush();
            }
        }
    }
}