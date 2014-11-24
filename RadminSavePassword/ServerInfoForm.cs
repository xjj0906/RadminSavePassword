using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RadminSavePassword
{
    public partial class ServerInfoForm : Form
    {
        public ServerInfo _sourceServerInfo;

        public ServerInfo ServerInfo { get; private set; }

        public ServerInfoForm()
            : this(null)
        {

        }

        public ServerInfoForm(ServerInfo serverInfo)
        {
            InitializeComponent();

            ServerInfo = new ServerInfo();
            _sourceServerInfo = serverInfo;

            if (_sourceServerInfo != null)
            {
                UpdateUIControlData(_sourceServerInfo);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btnOk.Click += btnOk_Click;
            btnCancel.Click += btnCancel_Click;
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            ServerInfo = null;
            UpdateUIControlData(ServerInfo);
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            if ((Global.SystemConfig.ServerList.ContainsKey(txtName.Text) && Global.SystemConfig.ServerList[txtName.Text] != _sourceServerInfo))
            {
                MessageBox.Show("该服务器名称已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveUIDataToObj(ServerInfo);
            DialogResult = DialogResult.OK;
        }

        protected virtual void UpdateUIControlData(ServerInfo serverInfo)
        {
            if (serverInfo != null)
            {
                txtName.Text = serverInfo.Name;
                txtUsername.Text = serverInfo.UserName;
                txtPassword.Text = serverInfo.Password;
            }
            else
            {
                txtName.Text = string.Empty;
                txtUsername.Text = string.Empty;
                txtPassword.Text = string.Empty;
            }
        }

        protected virtual void SaveUIDataToObj(ServerInfo serverInfo)
        {
            if (serverInfo == null) return;

            serverInfo.Name = txtName.Text;
            serverInfo.UserName = txtUsername.Text;
            serverInfo.Password = txtPassword.Text;
        }
    }
}
