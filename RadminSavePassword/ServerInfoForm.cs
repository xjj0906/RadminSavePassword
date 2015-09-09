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
        public LoginType _lastLoginType;
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

            rbRadmin.Tag = LoginType.Radmin;
            rbWindows.Tag = LoginType.Windows;
            _lastLoginType = LoginType.Windows;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btnOk.Click += btnOk_Click;
            btnCancel.Click += btnCancel_Click;
            rbRadmin.CheckedChanged += rb_CheckedChanged;
            rbWindows.CheckedChanged += rb_CheckedChanged;

            if (_sourceServerInfo != null)
                UpdateUIControlData(_sourceServerInfo);
        }

        void rb_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (!rb.Checked) return;

            LoginType loginType = (LoginType)rb.Tag;
            UpdateUIControlLayout(loginType);
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

        protected virtual void UpdateUIControlLayout(LoginType loginType)
        {
            if (_lastLoginType == loginType) return;

            int size = 26;
            int change = loginType == LoginType.Windows ? 1 : -1;
            bool visible = loginType == LoginType.Windows;

            lbDomain.Visible = txtDomain.Visible = visible;
            lbDomain.Location = new Point(lbDomain.Location.X, lbDomain.Location.Y + (size * change));
            txtDomain.Location = new Point(txtDomain.Location.X, txtDomain.Location.Y + (size * change));
            btnOk.Location = new Point(btnOk.Location.X, btnOk.Location.Y + (size * change));
            btnCancel.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y + (size * change));
            Size = new Size(Size.Width, Size.Height + (size * change));
            _lastLoginType = loginType;
        }

        protected virtual void UpdateUIControlData(ServerInfo serverInfo)
        {
            if (serverInfo != null)
            {
                switch (serverInfo.LoginType)
                {
                    case LoginType.Radmin:
                        rbRadmin.Checked = true;
                        break;
                    case LoginType.Windows:
                        rbWindows.Checked = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                txtName.Text = serverInfo.Name;
                txtUsername.Text = serverInfo.UserName;
                txtPassword.Text = serverInfo.Password;
                txtDomain.Text = serverInfo.Domain;
            }
            else
            {
                rbRadmin.Checked = true;
                txtName.Text = string.Empty;
                txtUsername.Text = string.Empty;
                txtPassword.Text = string.Empty;
                txtDomain.Text = string.Empty;
            }
        }

        protected virtual void SaveUIDataToObj(ServerInfo serverInfo)
        {
            if (serverInfo == null) return;

            serverInfo.Name = txtName.Text;
            serverInfo.UserName = txtUsername.Text;
            serverInfo.Password = txtPassword.Text;
            if (rbRadmin.Checked)
                serverInfo.LoginType = (LoginType)rbRadmin.Tag;
            else if (rbWindows.Checked)
                serverInfo.LoginType = (LoginType)rbWindows.Tag;
            else
                throw new NotImplementedException();

            if (serverInfo.LoginType == LoginType.Windows)
                serverInfo.Domain = txtDomain.Text;
            else
                serverInfo.Domain = string.Empty;
        }
    }
}
