using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RadminSavePassword.Hook
{
    public class MouseLLHook : Hook
    {
        [DllImport("GlobalCbtHook.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitializeMouseLLHook(int threadID, IntPtr DestWindow);
        [DllImport("GlobalCbtHook.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UninitializeMouseLLHook();

        // Values retreived with RegisterWindowMessage
        private int _MsgID_MouseLL;
        private int _MsgID_MouseLL_HookReplaced;

        public event HookReplacedEventHandler HookReplaced;
        public event BasicHookEventHandler MouseLLEvent;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseMove;
        public event MouseEventHandler MouseUp;

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_RBUTTONDBLCLK = 0x0206;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MBUTTONDBLCLK = 0x0209;
        private const int WM_MOUSEWHEEL = 0x020A;

        struct MSLLHOOKSTRUCT
        {
            public System.Drawing.Point pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        };

        public MouseLLHook(IntPtr Handle)
            : base(Handle)
        {
        }

        protected override void OnStart()
        {
            // Retreive the message IDs that we'll look for in WndProc
            _MsgID_MouseLL = RegisterWindowMessage("WILSON_HOOK_MOUSELL");
            _MsgID_MouseLL_HookReplaced = RegisterWindowMessage("WILSON_HOOK_MOUSELL_REPLACED");

            // Start the hook
            InitializeMouseLLHook(0, _Handle);
        }

        protected override void OnStop()
        {
            UninitializeMouseLLHook();
        }

        public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == _MsgID_MouseLL)
            {
                if (MouseLLEvent != null)
                    MouseLLEvent(m.WParam, m.LParam);

                MSLLHOOKSTRUCT M = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(m.LParam, typeof(MSLLHOOKSTRUCT));

                if (m.WParam.ToInt32() == WM_MOUSEMOVE)
                {
                    if (MouseMove != null)
                        MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, M.pt.X, M.pt.Y, 0));
                }
                else if (m.WParam.ToInt32() == WM_LBUTTONDOWN)
                {
                    if (MouseDown != null)
                        MouseDown(this, new MouseEventArgs(MouseButtons.Left, 0, M.pt.X, M.pt.Y, 0));
                }
                else if (m.WParam.ToInt32() == WM_RBUTTONDOWN)
                {
                    if (MouseDown != null)
                        MouseDown(this, new MouseEventArgs(MouseButtons.Right, 0, M.pt.X, M.pt.Y, 0));
                }
                else if (m.WParam.ToInt32() == WM_LBUTTONUP)
                {
                    if (MouseUp != null)
                        MouseUp(this, new MouseEventArgs(MouseButtons.Left, 0, M.pt.X, M.pt.Y, 0));
                }
                else if (m.WParam.ToInt32() == WM_RBUTTONUP)
                {
                    if (MouseUp != null)
                        MouseUp(this, new MouseEventArgs(MouseButtons.Right, 0, M.pt.X, M.pt.Y, 0));
                }
            }
            else if (m.Msg == _MsgID_MouseLL_HookReplaced)
            {
                if (HookReplaced != null)
                    HookReplaced();
            }
        }
    }
}