using System;
using System.Runtime.InteropServices;

namespace RadminSavePassword.Hook
{
    public class CBTHook : Hook
    {
        // Functions imported from our unmanaged DLL
        [DllImport("GlobalCbtHook.dll")]
        protected static extern bool InitializeCbtHook(int threadID, IntPtr DestWindow);
        [DllImport("GlobalCbtHook.dll")]
        protected static extern void UninitializeCbtHook();

        // Values retreived with RegisterWindowMessage
        private int _MsgID_CBT_HookReplaced;
        private int _MsgID_CBT_Activate;
        private int _MsgID_CBT_CreateWnd;
        private int _MsgID_CBT_DestroyWnd;
        private int _MsgID_CBT_MinMax;
        private int _MsgID_CBT_MoveSize;
        private int _MsgID_CBT_SetFocus;
        private int _MsgID_CBT_SysCommand;

        public delegate void HookReplacedEventHandler();
        public delegate void WindowEventHandler(IntPtr Handle);
        public delegate void SysCommandEventHandler(int SysCommand, int lParam);

        public event HookReplacedEventHandler HookReplaced;
        public event WindowEventHandler Activate;
        public event WindowEventHandler CreateWindow;
        public event WindowEventHandler DestroyWindow;
        public event WindowEventHandler MinMax;
        public event WindowEventHandler MoveSize;
        public event WindowEventHandler SetFocus;
        public event SysCommandEventHandler SysCommand;

        public CBTHook(IntPtr Handle)
            : base(Handle)
        {
        }

        protected override void OnStart()
        {
            // Retreive the message IDs that we'll look for in WndProc
            _MsgID_CBT_HookReplaced = RegisterWindowMessage("WILSON_HOOK_CBT_REPLACED");
            _MsgID_CBT_Activate = RegisterWindowMessage("WILSON_HOOK_HCBT_ACTIVATE");
            _MsgID_CBT_CreateWnd = RegisterWindowMessage("WILSON_HOOK_HCBT_CREATEWND");
            _MsgID_CBT_DestroyWnd = RegisterWindowMessage("WILSON_HOOK_HCBT_DESTROYWND");
            _MsgID_CBT_MinMax = RegisterWindowMessage("WILSON_HOOK_HCBT_MINMAX");
            _MsgID_CBT_MoveSize = RegisterWindowMessage("WILSON_HOOK_HCBT_MOVESIZE");
            _MsgID_CBT_SetFocus = RegisterWindowMessage("WILSON_HOOK_HCBT_SETFOCUS");
            _MsgID_CBT_SysCommand = RegisterWindowMessage("WILSON_HOOK_HCBT_SYSCOMMAND");

            // Start the hook
            InitializeCbtHook(0, _Handle);
        }

        protected override void OnStop()
        {
            UninitializeCbtHook();
        }

        public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == _MsgID_CBT_HookReplaced)
            {
                if (HookReplaced != null)
                    HookReplaced();
            }
            else if (m.Msg == _MsgID_CBT_Activate)
            {
                if (Activate != null)
                    Activate(m.WParam);
            }
            else if (m.Msg == _MsgID_CBT_CreateWnd)
            {
                if (CreateWindow != null)
                    CreateWindow(m.WParam);
            }
            else if (m.Msg == _MsgID_CBT_DestroyWnd)
            {
                if (DestroyWindow != null)
                    DestroyWindow(m.WParam);
            }
            else if (m.Msg == _MsgID_CBT_MinMax)
            {
                if (MinMax != null)
                    MinMax(m.WParam);
            }
            else if (m.Msg == _MsgID_CBT_MoveSize)
            {
                if (MoveSize != null)
                    MoveSize(m.WParam);
            }
            else if (m.Msg == _MsgID_CBT_SetFocus)
            {
                if (SetFocus != null)
                    SetFocus(m.WParam);
            }
            else if (m.Msg == _MsgID_CBT_SysCommand)
            {
                if (SysCommand != null)
                    SysCommand(m.WParam.ToInt32(), m.LParam.ToInt32());
            }
        }
    }
}