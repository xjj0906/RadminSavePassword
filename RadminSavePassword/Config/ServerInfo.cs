using System;
using System.Runtime.Serialization;

namespace RadminSavePassword
{
    [Serializable]
    public class ServerInfo : ISerializable
    {
        private readonly EntryptDecrypt _entryptDecrypt = new EntryptDecrypt();

        public string Name { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public ServerInfo()
        {
        }

        protected ServerInfo(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name");
            UserName = info.GetString("UserName");

            string decryptPassword = _entryptDecrypt.Decrypt(info.GetString("Password"));
            Password = decryptPassword;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("UserName", UserName);

            string entryptPassword = _entryptDecrypt.Encrypt(Password);
            info.AddValue("Password", entryptPassword);
        }
    }
}