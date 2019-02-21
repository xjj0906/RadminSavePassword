using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace RadminSavePassword
{
    [Serializable]
    public class SystemConfig
    {
        // Don't use Application.ExecutablePath
        // see https://stackoverflow.com/questions/12945805/odd-c-sharp-path-issue
        private static readonly string ExecutablePath = Assembly.GetEntryAssembly().Location;

        private static string Key = "RadminSavePassword_" + Application.StartupPath.GetHashCode();

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

        #region 自动启动
        /// <summary>
        /// 设置开机启动项
        /// </summary>
        /// <param name="started">是否启动</param>
        public static void SetAutoStart(bool started)
        {
            Microsoft.Win32.RegistryKey HKCU = Microsoft.Win32.Registry.CurrentUser;
            Microsoft.Win32.RegistryKey runKey = HKCU.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            if (started == true)
            {
                try
                {
                    runKey.SetValue(Key, $"{ExecutablePath} --min-mode");
                }
                catch { }
                finally
                {
                    HKCU.Close();
                }
            }
            else
            {
                try
                {
                    runKey.DeleteValue(Key);
                    HKCU.Close();
                }
                catch { }
                finally
                {
                    HKCU.Close();
                }
            }
        }

        /// <summary>
        /// 检查开机启动项是否有效
        /// </summary>
        /// <returns></returns>
        public static bool CheckIsAutoStart()
        {
            Microsoft.Win32.RegistryKey HKCU = Microsoft.Win32.Registry.CurrentUser;
            Microsoft.Win32.RegistryKey runKey = HKCU.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            try
            {
                string[] runList = runKey.GetValueNames();
                foreach (string item in runList)
                {
                    if (item.Equals(Key, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            finally
            {
                HKCU.Close();
            }
        }
        #endregion
    }
}