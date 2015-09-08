using System;
using System.Text;
using System.Windows.Forms;

namespace RadminSavePassword.Hook
{
    public delegate void CatchServerInfoEventHandler(object sender, ServerInfoEventArgs args);

    public class RadminInput : NativeWindow
    {
        protected const string RadminProgramFlag = "Radmin 安全性: ";
        protected const string WindowsProgramFlag = "Windows 安全性：";

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
                ServerInfo serverInfo = PickUpServerInfo(_activeControlHandle);
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
                    ServerInfo serverInfo = PickUpServerInfo(_activeControlHandle);
                    if (CatchServerInfo != null && serverInfo != null)
                        CatchServerInfo(this, new ServerInfoEventArgs(serverInfo));
                }
            }
        }

        void CBTHook_DestroyWindow(IntPtr handle)
        {
            if (IsLoginWindows(handle))
            {
                _activeControlHandle = null;
            }
        }

        protected void CBTHook_Activate(IntPtr handle)
        {
            if (IsLoginWindows(handle))
            {
                string title = GetHandleText(handle);
                LoginType loginType = GetLoginType(title);
                string name = RemoveStringPreFlag(title);
                OnPaddingProcess(handle, name, loginType);
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
                LoginType loginType = LoginType.Radmin;
                WindowsApi.EnumChildWindows(IntPtr.Zero, (hwnd, lParam) =>
                {
                    if (IsLoginWindows(hwnd))
                    {
                        handle = hwnd;
                        string title = GetHandleText(handle);
                        loginType = GetLoginType(title);
                        name = RemoveStringPreFlag(title);
                        return false;
                    }
                    return true;
                }, IntPtr.Zero);

                if (handle != IntPtr.Zero)
                    OnPaddingProcess(handle, name, loginType);

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

        /// <summary>
        /// 填充主窗体信息
        /// </summary>
        /// <param name="handle">主窗体句柄</param>
        /// <param name="serverName">当前服务器名称</param>
        /// <param name="loginType">当前登陆类型</param>
        protected virtual void OnPaddingProcess(IntPtr handle, string serverName, LoginType loginType)
        {
            ControlHandle controlHandle = GetControlHandle(handle);
            _activeControlHandle = controlHandle;

            if (!Global.SystemConfig.ServerList.ContainsKey(serverName)) return;

            ServerInfo serverInfo = Global.SystemConfig.ServerList[serverName];
            if (serverInfo.LoginType != loginType) return;

            WindowsApi.SendMessage(controlHandle.UsernameHandle, WindowsApi.WM_SETTEXT, 1024, serverInfo.UserName);
            WindowsApi.SendMessage(controlHandle.PasswordHandle, WindowsApi.WM_SETTEXT, 1024, serverInfo.Password);

            if (controlHandle.LoginType == LoginType.Windows)
                WindowsApi.SendMessage(controlHandle.DomainHandle, WindowsApi.WM_SETTEXT, 1024, serverInfo.Domain);

            if (Global.SystemConfig.IsAutoEnter)
            {
                IntPtr btnPtr = WindowsApi.FindWindowEx(handle, IntPtr.Zero, "Button", "确定");
                WindowsApi.PostMessage(btnPtr, WindowsApi.WM_KEYDOWN, 0X0D, 0);
            }

        }

        /// <summary>
        /// 提取界面服务器信息
        /// </summary>
        /// <param name="controlHandle">窗体控件句柄数据</param>
        /// <returns></returns>
        protected virtual ServerInfo PickUpServerInfo(ControlHandle controlHandle)
        {
            int defaultCheckValue = (int)WindowsApi.SendMessage(controlHandle.DefaultCheckHandle, WindowsApi.BM_GETCHECK, 1024, (string)null);
            if (defaultCheckValue != WindowsApi.BST_CHECKED) return null;

            ServerInfo serverInfo = new ServerInfo();
            serverInfo.Name = controlHandle.Title;
            serverInfo.LoginType = controlHandle.LoginType;

            StringBuilder stringBuilder = new StringBuilder(1024);
            WindowsApi.SendMessage(controlHandle.UsernameHandle, WindowsApi.WM_GETTEXT, 1024, stringBuilder);
            serverInfo.UserName = stringBuilder.ToString();


            long dwStyle = WindowsApi.GetWindowLong(controlHandle.PasswordHandle, WindowsApi.GWL_STYLE); //获取密码框原来样式
            WindowsApi.SetWindowWord(controlHandle.PasswordHandle, WindowsApi.GWL_STYLE, 0); //取消所有样式
            WindowsApi.SetWindowLong(controlHandle.PasswordHandle, WindowsApi.GWL_STYLE,
                                     dwStyle & ~WindowsApi.ES_PASSWORD); //去掉密码属性

            stringBuilder.Remove(0, stringBuilder.Length);
            WindowsApi.SendMessage(controlHandle.PasswordHandle, WindowsApi.WM_GETTEXT, 1024, stringBuilder);
            //没有了ES_PASSWORD属性WM_GETTEXT可以成功

            WindowsApi.SetWindowLong(controlHandle.PasswordHandle, WindowsApi.GWL_STYLE, dwStyle); //还原密码框原来样式

            serverInfo.Password = stringBuilder.ToString();
            if (serverInfo.LoginType == LoginType.Windows)
            {
                stringBuilder.Remove(0, stringBuilder.Length);
                WindowsApi.SendMessage(controlHandle.DomainHandle, WindowsApi.WM_GETTEXT, 1024, stringBuilder);
                serverInfo.Domain = stringBuilder.ToString();
            }

            if (string.IsNullOrEmpty(serverInfo.Password))
                return null;

            return serverInfo;
        }

        ~RadminInput()
        {
            Stop();
        }

        /// <summary>
        /// 获取主窗体中子控件句柄
        /// </summary>
        /// <param name="handle">主窗体句柄</param>
        /// <returns></returns>
        protected ControlHandle GetControlHandle(IntPtr handle)
        {
            ControlHandle controlHandle = new ControlHandle();
            controlHandle.ParentHandle = handle;

            string title = GetHandleText(handle);

            controlHandle.LoginType = GetLoginType(title);
            controlHandle.Title = RemoveStringPreFlag(title);

            WindowsApi.EnumChildWindows(handle, (hwnd, lParam) =>
            {
                WindowsApi.Rect rect = new WindowsApi.Rect();
                WindowsApi.GetWindowRect(hwnd, out rect);
                WindowsApi.Point point = new WindowsApi.Point();
                point.X = rect.Left;
                point.Y = rect.Top;
                WindowsApi.ScreenToClient(handle, ref point);
                if (controlHandle.LoginType == LoginType.Radmin)
                {
                    if (point.X == 83 && point.Y == 20) //用户名
                        controlHandle.UsernameHandle = hwnd;
                    if (point.X == 83 && point.Y == 55) //密码框
                        controlHandle.PasswordHandle = hwnd;
                    if (point.X == 18 && point.Y == 88) //缺省值CheckBox
                        controlHandle.DefaultCheckHandle = hwnd;

                    if (point.X == 83 && point.Y == 111) //确定按钮
                        controlHandle.OkButtonHandle = hwnd;
                    if (point.X == 180 && point.Y == 111) //取消按钮
                        controlHandle.CancelButtonHandle = hwnd;
                }
                else if (controlHandle.LoginType == LoginType.Windows)
                {
                    if (point.X == 83 && point.Y == 20) //用户名
                        controlHandle.UsernameHandle = hwnd;
                    if (point.X == 83 && point.Y == 55) //密码框
                        controlHandle.PasswordHandle = hwnd;
                    if (point.X == 83 && point.Y == 89) //域名
                        controlHandle.DomainHandle = hwnd;
                    if (point.X == 18 && point.Y == 122) //缺省值CheckBox
                        controlHandle.DefaultCheckHandle = hwnd;

                    if (point.X == 83 && point.Y == 145) //确定按钮
                        controlHandle.OkButtonHandle = hwnd;
                    if (point.X == 180 && point.Y == 145) //取消按钮
                        controlHandle.CancelButtonHandle = hwnd;
                }
                else
                    throw new NotImplementedException("unknow ProgramFlag");

                return true;
            }, IntPtr.Zero);

            return controlHandle;
        }

        /// <summary>
        /// 获取窗体句柄的Text信息
        /// </summary>
        /// <param name="handle">窗体句柄</param>
        /// <returns></returns>
        protected string GetHandleText(IntPtr handle)
        {
            StringBuilder sb = new StringBuilder(256);
            WindowsApi.GetWindowText(handle, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// 根据窗体标题判断登陆类型
        /// </summary>
        /// <param name="title">窗体标题</param>
        /// <returns></returns>
        protected LoginType GetLoginType(string title)
        {
            LoginType loginType = LoginType.Radmin;
            if (title.StartsWith(RadminProgramFlag))
                loginType = LoginType.Radmin;
            else if (title.StartsWith(WindowsProgramFlag))
                loginType = LoginType.Windows;

            return loginType;
        }

        /// <summary>
        /// 移除字符串开头的用于标识Radmin程序窗体的前缀字符
        /// </summary>
        /// <param name="str">待处理的字符串</param>
        /// <returns></returns>
        protected string RemoveStringPreFlag(string str)
        {
            if (str.StartsWith(RadminProgramFlag))
                return str.Substring(RadminProgramFlag.Length);
            if (str.StartsWith(WindowsProgramFlag))
                return str.Substring(WindowsProgramFlag.Length);

            return str;
        }

        /// <summary>
        /// 校验当前句柄对应的窗体是否为登陆窗体
        /// </summary>
        /// <param name="handle">窗体句柄</param>
        /// <returns></returns>
        protected bool IsLoginWindows(IntPtr handle)
        {
            StringBuilder sb = new StringBuilder(256);
            WindowsApi.GetWindowText(handle, sb, sb.Capacity);
            string title = sb.ToString();

            return IsLoginWindows(title);
        }

        /// <summary>
        /// 校验当前句柄对应的窗体是否为登陆窗体
        /// </summary>
        /// <param name="title">窗体标题</param>
        /// <returns></returns>
        protected bool IsLoginWindows(string title)
        {
            return (title.StartsWith(RadminProgramFlag) || title.StartsWith(WindowsProgramFlag));
        }

        /// <summary>
        /// 窗体控件句柄数据
        /// </summary>
        protected class ControlHandle
        {
            /// <summary>
            /// 登陆类型
            /// </summary>
            public LoginType LoginType { get; set; }
            /// <summary>
            /// 窗体标题
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// 父窗体句柄
            /// </summary>
            public IntPtr ParentHandle { get; set; }
            /// <summary>
            /// 用户名TextBox句柄
            /// </summary>
            public IntPtr UsernameHandle { get; set; }
            /// <summary>
            /// 密码TextBox句柄
            /// </summary>
            public IntPtr PasswordHandle { get; set; }
            /// <summary>
            /// 域名TextBox句柄
            /// </summary>
            public IntPtr DomainHandle { get; set; }
            /// <summary>
            /// 缺省值CheckBox句柄
            /// </summary>
            public IntPtr DefaultCheckHandle { get; set; }
            /// <summary>
            /// 确认Button句柄
            /// </summary>
            public IntPtr OkButtonHandle { get; set; }
            /// <summary>
            /// 取消Button句柄
            /// </summary>
            public IntPtr CancelButtonHandle { get; set; }
        }
    }
}