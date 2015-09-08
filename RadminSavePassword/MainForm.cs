using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using RadminSavePassword.Hook;

namespace RadminSavePassword
{
    public partial class MainForm : Form
    {
        private readonly RadminInput _radminInput;
        protected readonly string _rootPath;
        protected const string ConfigName = "config.dat";
        private FormWindowState _lastWindowState;

        public MainForm()
        {
            InitializeComponent();
            _radminInput = new RadminInput(this.Handle);
            _rootPath = Application.StartupPath;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = Global.ProgramText;

            LoadConfig();

            btnStop.Enabled = false;
            停止EToolStripMenuItem.Enabled = false;

            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;

            btnAdd.Click += btnAdd_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;

            btnOpen.Click += btnOpen_Click;

            cbAutoEnter.CheckedChanged += cbAutoEnter_CheckedChanged;

            listView.ItemActivate += listView_ItemActivate;

            notifyIcon.MouseClick += notifyIcon_MouseClick;

            启动BToolStripMenuItem.Click += (sender, ex) => btnStart.PerformClick();
            停止EToolStripMenuItem.Click += (sender, ex) => btnStop.PerformClick();
            退出XToolStripMenuItem.Click += (sender, ex) => Application.Exit();

            _radminInput.CatchServerInfo += _radminInput_CatchServerInfo;
        }

        void _radminInput_CatchServerInfo(object sender, ServerInfoEventArgs args)
        {
            ListViewItem existItem = null;
            foreach (ListViewItem item in listView.Items)
            {
                ServerInfo si = (ServerInfo)item.Tag;
                if (si.Name == args.ServerInfo.Name)
                {
                    existItem = item;
                    break;
                }
            }
            if (existItem != null)
            {
                UpdateListViewItem(args.ServerInfo, existItem);
            }
            else
            {
                ListViewItem item = CreateListViewItem(args.ServerInfo);
                listView.Items.Add(item);
            }

            Global.SystemConfig.ServerList[args.ServerInfo.Name] = args.ServerInfo;

            SaveConfig();
        }

        void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            this.Visible = true;
            Thread.Sleep(20);//睡眠20毫秒，让显示动画平滑
            WindowState = _lastWindowState;
            this.Activate();
        }

        void listView_ItemActivate(object sender, EventArgs e)
        {
            btnEdit.PerformClick();
        }

        void cbAutoEnter_CheckedChanged(object sender, EventArgs e)
        {
            Global.SystemConfig.IsAutoEnter = cbAutoEnter.Checked;
            SaveConfig();
        }

        void btnOpen_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Global.SystemConfig.RadminOpenPath))
            {
                RegistryKey regLM = Registry.LocalMachine;
                RegistryKey regUninstall = regLM.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (regUninstall == null) return;

                string[] reguninstallSubKeyNames = regUninstall.GetSubKeyNames();
                foreach (var name in reguninstallSubKeyNames)
                {
                    RegistryKey key = regUninstall.OpenSubKey(name);
                    if (key == null) continue;

                    string displayName = (string)key.GetValue("DisplayName");
                    if (string.IsNullOrEmpty(displayName)) continue;

                    if (displayName.StartsWith("Radmin Viewer"))
                    {
                        string installLocation = (string)key.GetValue("InstallLocation");
                        if (string.IsNullOrEmpty(installLocation)) continue;

                        Global.SystemConfig.RadminOpenPath = Path.Combine(installLocation, "Radmin.exe");
                        SaveConfig();
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(Global.SystemConfig.RadminOpenPath) || !File.Exists(Global.SystemConfig.RadminOpenPath))
            {
                MessageBox.Show("找不到Radmin的安装目录，请手工指定", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                OpenFileDialog opDialog = new OpenFileDialog();
                opDialog.CheckFileExists = true;
                opDialog.Filter = "程序文件(*.exe)|*.exe|所有文件(*.*)|*.*";
                opDialog.FileName = "Radmin.exe";
                if (opDialog.ShowDialog() == DialogResult.OK)
                {
                    Global.SystemConfig.RadminOpenPath = opDialog.FileName;
                    SaveConfig();
                }
                else
                    return;
            }
            Process.Start(Global.SystemConfig.RadminOpenPath);
        }

        void btnStop_Click(object sender, EventArgs e)
        {
            _radminInput.Stop();

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            启动BToolStripMenuItem.Enabled = true;
            停止EToolStripMenuItem.Enabled = false;
        }

        void btnStart_Click(object sender, EventArgs e)
        {
            _radminInput.Start();

            btnStart.Enabled = false;
            btnStop.Enabled = true;
            启动BToolStripMenuItem.Enabled = false;
            停止EToolStripMenuItem.Enabled = true;
        }

        void btnDelete_Click(object sender, EventArgs e)
        {
            if (listView.FocusedItem == null) return;

            if (MessageBox.Show("确认删除选中的记录？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) !=
                DialogResult.OK) return;

            ListViewItem focusedItem = listView.FocusedItem;
            ServerInfo serverInfo = (ServerInfo)focusedItem.Tag;
            listView.Items.Remove(focusedItem);

            Global.SystemConfig.ServerList.Remove(serverInfo.Name);

            SaveConfig();
        }

        void btnEdit_Click(object sender, EventArgs e)
        {
            if (listView.FocusedItem == null) return;

            ListViewItem focusedItem = listView.FocusedItem;
            ServerInfo serverInfo = (ServerInfo)focusedItem.Tag;

            ServerInfoForm form = new ServerInfoForm(serverInfo);
            if (form.ShowDialog() != DialogResult.OK) return;

            UpdateListViewItem(form.ServerInfo, focusedItem);

            Global.SystemConfig.ServerList[form.ServerInfo.Name] = form.ServerInfo;

            SaveConfig();
        }

        void btnAdd_Click(object sender, EventArgs e)
        {
            ServerInfoForm form = new ServerInfoForm();
            if (form.ShowDialog() != DialogResult.OK) return;

            ListViewItem item = CreateListViewItem(form.ServerInfo);
            listView.Items.Add(item);

            Global.SystemConfig.ServerList[form.ServerInfo.Name] = form.ServerInfo;

            SaveConfig();
        }

        private ListViewItem CreateListViewItem(ServerInfo serverInfo)
        {
            ListViewItem item = new ListViewItem();
            for (int i = 0; i < listView.Columns.Count - 1; i++)
                item.SubItems.Add(string.Empty);

            UpdateListViewItem(serverInfo, item);
            return item;
        }

        private void UpdateListViewItem(ServerInfo serverInfo, ListViewItem item)
        {
            item.ImageKey = serverInfo.LoginType == LoginType.Radmin ? "device-laptop-R.png" : "device-laptop-W.png";
            item.Text = serverInfo.Name;
            item.SubItems[1].Text = serverInfo.UserName;
            item.SubItems[2].Text = serverInfo.Password.Length == 0 ? "(无)" : "*****";
            item.SubItems[3].Text = serverInfo.Domain;
            item.Tag = serverInfo;
        }

        protected void LoadConfig()
        {
            if (File.Exists(Path.Combine(_rootPath, ConfigName)))
                Global.SystemConfig = SystemConfig.Load(Path.Combine(_rootPath, ConfigName));

            listView.Items.Clear();
            foreach (var pair in Global.SystemConfig.ServerList)
            {
                listView.Items.Add(CreateListViewItem(pair.Value));
            }

            cbAutoEnter.Checked = Global.SystemConfig.IsAutoEnter;
        }

        protected void SaveConfig()
        {
            SystemConfig.Save(Global.SystemConfig, Path.Combine(_rootPath, ConfigName));
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (WindowState == FormWindowState.Maximized || WindowState == FormWindowState.Normal)
            {
                _lastWindowState = WindowState;
            }

            if (WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
            }
        }

        ~MainForm()
        {
            _radminInput.Stop();
        }
    }
}
