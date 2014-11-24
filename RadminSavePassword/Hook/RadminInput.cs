using System;
using System.Text;
using System.Windows.Forms;

namespace RadminSavePassword.Hook
{
    public delegate void CatchServerInfoEventHandler(object sender, ServerInfoEventArgs args);

    public class RadminInput : NativeWindow
    {
        protected const string RadminProgramFlag = "Radmin 安全性: ";

        public bool IsStarted { get; protected set; }

        protected CBTHook CBTHook { get; private set; }
        protected MouseLLHook MouseLLHook { get; private set; }
        protected KeyboardLLHook KeyboardLLHook { get; private set; }

        protected IntPtr _parentHandle;

        protected ControlHandle _activeControlHandle = null;

        public event CatchServerInfoEventHandler CatchServerInfo;

        public RadminInput(IntPtr parentHandle)
        {
            _parentHandle = parentHandle;

            CBTHook = new CBTHook(_parentHandle);
            MouseLLHook = new MouseLLHook(_parentHandle);
            KeyboardLLHook = new KeyboardLLHook(_parentHandle);

            CBTHook.Activate += CBTHook_Activate;
            MouseLLHook.MouseUp += MouseLLHook_MouseUp;
            KeyboardLLHook.KeyDown += KeyboardLLHook_KeyDown;
            CBTHook.DestroyWindow += CBTHook_DestroyWindow;
        }

        void KeyboardLLHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (_activeControlHandle != null && e.KeyCode == Keys.Enter)
            {
                StringBuilder sb = new StringBuilder(256);
                WindowsApi.GetWindowText(_activeControlHandle.ParentHandle, sb, sb.Capacity);
                string name = sb.ToString().Replace(RadminProgramFlag, "");
                ServerInfo serverInfo = PickUpServerInfo(_activeControlHandle, name);
                if (CatchServerInfo != null && serverInfo != null)
                    CatchServerInfo(this, new ServerInfoEventArgs(serverInfo));
            }
        }

        void MouseLLHook_MouseUp(object sender, MouseEventArgs e)
        {
            if (_activeControlHandle != null && e.Button == MouseButtons.Left)
            {
                WindowsApi.Point point = new WindowsApi.Point { X = (uint)e.X, Y = (uint)e.Y };
                IntPtr handle = WindowsApi.WindowFromPoint(point);

                if (_activeControlHandle.OkButtonHandle == handle)
                {
                    StringBuilder sb = new StringBuilder(256);
                    WindowsApi.GetWindowText(_activeControlHandle.ParentHandle, sb, sb.Capacity);
                    string name = sb.ToString().Replace(RadminProgramFlag, "");
                    ServerInfo serverInfo = PickUpServerInfo(_activeControlHandle, name);
                    if (CatchServerInfo != null && serverInfo != null)
                        CatchServerInfo(this, new ServerInfoEventArgs(serverInfo));
                }
            }
        }

        void CBTHook_DestroyWindow(IntPtr handle)
        {
            StringBuilder sb = new StringBuilder(256);
            WindowsApi.GetWindowText(handle, sb, sb.Capacity);
            string s = sb.ToString();
            if (s.StartsWith(RadminProgramFlag))
            {
                _activeControlHandle = null;
            }
        }

        protected void CBTHook_Activate(IntPtr handle)
        {
            StringBuilder sb = new StringBuilder(256);
            WindowsApi.GetWindowText(handle, sb, sb.Capacity);
            string s = sb.ToString();
            if (s.StartsWith(RadminProgramFlag))
            {
                string name = s.Replace(RadminProgramFlag, "");
                OnPaddingProcess(handle, name);
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Check to see if we've received any Windows messages telling us about our hooks
            if (KeyboardLLHook != null)
                KeyboardLLHook.ProcessWindowMessage(ref m);
            if (MouseLLHook != null)
                MouseLLHook.ProcessWindowMessage(ref m);
            if (CBTHook != null)
                CBTHook.ProcessWindowMessage(ref m);

            base.WndProc(ref m);
        }

        public void Start()
        {
            if (!IsStarted)
            {
                AssignHandle(_parentHandle);

                CBTHook.Start();
                MouseLLHook.Start();
                KeyboardLLHook.Start();

                IntPtr handle = IntPtr.Zero;
                string name = string.Empty;
                WindowsApi.EnumChildWindows(IntPtr.Zero, (hwnd, lParam) =>
                {
                    StringBuilder sb = new StringBuilder(256);
                    WindowsApi.GetWindowText(hwnd, sb, sb.Capacity);
                    string s = sb.ToString();
                    if (s.StartsWith(RadminProgramFlag))
                    {
                        handle = hwnd;
                        name = s.Replace(RadminProgramFlag, "");
                        return false;
                    }
                    return true;
                }, IntPtr.Zero);

                OnPaddingProcess(handle, name);

                IsStarted = true;
            }
        }

        public void Stop()
        {
            if (IsStarted)
            {
                CBTHook.Stop();
                MouseLLHook.Stop();
                KeyboardLLHook.Stop();
                ReleaseHandle();
                IsStarted = false;
            }
        }

        protected virtual void OnPaddingProcess(IntPtr handle, string name)
        {
            ControlHandle controlHandle = GetControlHandle(handle);
            _activeControlHandle = controlHandle;

            if (Global.SystemConfig.ServerList.ContainsKey(name))
            {
                ServerInfo serverInfo = Global.SystemConfig.ServerList[name];

                WindowsApi.SendMessage(controlHandle.UsernameHandle, WindowsApi.WM_SETTEXT, 1024, serverInfo.UserName);
                WindowsApi.SendMessage(controlHandle.PasswordHandle, WindowsApi.WM_SETTEXT, 1024, serverInfo.Password);

                if (Global.SystemConfig.IsAutoEnter)
                {
                    IntPtr btnPtr = WindowsApi.FindWindowEx(handle, IntPtr.Zero, "Button", "确定");
                    WindowsApi.PostMessage(btnPtr, WindowsApi.WM_KEYDOWN, 0X0D, 0);
                }
            }
        }

        protected virtual ServerInfo PickUpServerInfo(ControlHandle controlHandle, string serverName)
        {
            int defaultCheckValue = (int)WindowsApi.SendMessage(controlHandle.DefaultCheckHandle, WindowsApi.BM_GETCHECK, 1024, (string)null);
            if (defaultCheckValue == WindowsApi.BST_CHECKED) return null;

            ServerInfo serverInfo = new ServerInfo();
            serverInfo.Name = serverName;

            StringBuilder stringBuilder = new StringBuilder(1024);
            WindowsApi.SendMessage(controlHandle.UsernameHandle, WindowsApi.WM_GETTEXT, 1024, stringBuilder);
            serverInfo.UserName = stringBuilder.ToString();


            long dwStyle = WindowsApi.GetWindowLong(controlHandle.PasswordHandle, WindowsApi.GWL_STYLE);//获取密码框原来样式
            WindowsApi.SetWindowWord(controlHandle.PasswordHandle, WindowsApi.GWL_STYLE, 0);//取消所有样式
            WindowsApi.SetWindowLong(controlHandle.PasswordHandle, WindowsApi.GWL_STYLE, dwStyle & ~WindowsApi.ES_PASSWORD);//去掉密码属性

            stringBuilder.Remove(0, stringBuilder.Length);
            WindowsApi.SendMessage(controlHandle.PasswordHandle, WindowsApi.WM_GETTEXT, 1024, stringBuilder);//没有了ES_PASSWORD属性WM_GETTEXT可以成功

            WindowsApi.SetWindowLong(controlHandle.PasswordHandle, WindowsApi.GWL_STYLE, dwStyle);//还原密码框原来样式


            serverInfo.Password = stringBuilder.ToString();
            if (string.IsNullOrEmpty(serverInfo.Password)) return null;

            return serverInfo;
        }

        ~RadminInput()
        {
            Stop();
        }

        protected ControlHandle GetControlHandle(IntPtr handle)
        {
            ControlHandle controlHandle = new ControlHandle();
            controlHandle.ParentHandle = handle;

            WindowsApi.EnumChildWindows(handle, (hwnd, lParam) =>
            {
                WindowsApi.Rect rect = new WindowsApi.Rect();
                WindowsApi.GetWindowRect(hwnd, out rect);
                WindowsApi.Point point = new WindowsApi.Point();
                point.X = rect.Left;
                point.Y = rect.Top;
                WindowsApi.ScreenToClient(handle, ref point);
                if (point.X == 83 && point.Y == 55) //密码框
                    controlHandle.PasswordHandle = hwnd;
                if (point.X == 83 && point.Y == 20) //用户名
                    controlHandle.UsernameHandle = hwnd;
                if (point.X == 18 && point.Y == 88)//缺省值CheckBox
                    controlHandle.DefaultCheckHandle = hwnd;

                if (point.X == 83 && point.Y == 111)//确定按钮
                    controlHandle.OkButtonHandle = hwnd;
                if (point.X == 180 && point.Y == 111)//取消按钮
                    controlHandle.CancelButtonHandle = hwnd;

                return true;
            }, IntPtr.Zero);

            return controlHandle;
        }

        protected class ControlHandle
        {
            public IntPtr ParentHandle { get; set; }

            public IntPtr UsernameHandle { get; set; }
            public IntPtr PasswordHandle { get; set; }
            public IntPtr DefaultCheckHandle { get; set; }

            public IntPtr OkButtonHandle { get; set; }
            public IntPtr CancelButtonHandle { get; set; }
        }
    }
}