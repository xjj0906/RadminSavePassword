using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RadminSavePassword.Hook
{
    public class KeyboardLLHook : Hook
    {
        [DllImport("GlobalCbtHook.dll")]
        private static extern void InitializeKeyboardLLHook(int threadID, IntPtr DestWindow);
        [DllImport("GlobalCbtHook.dll")]
        private static extern void UninitializeKeyboardLLHook();

        // Values retreived with RegisterWindowMessage
        private int _MsgID_KeyboardLL;
        private int _MsgID_KeyboardLL_HookReplaced;

        public event HookReplacedEventHandler HookReplaced;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        public event KeyEventHandler KeyPress;

        public KeyboardLLHook(IntPtr Handle)
            : base(Handle)
        {
        }

        protected override void OnStart()
        {
            // Retreive the message IDs that we'll look for in WndProc
            _MsgID_KeyboardLL = RegisterWindowMessage("WILSON_HOOK_KEYBOARDLL");
            _MsgID_KeyboardLL_HookReplaced = RegisterWindowMessage("WILSON_HOOK_KEYBOARDLL_REPLACED");

            // Start the hook
            InitializeKeyboardLLHook(0, _Handle);
        }

        protected override void OnStop()
        {
            UninitializeKeyboardLLHook();
        }

        public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == _MsgID_KeyboardLL)
            {
                KBDLLHOOKSTRUCT kbhs = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(m.LParam, typeof(KBDLLHOOKSTRUCT));
                if (m.WParam == (IntPtr)WindowsApi.WM_KEYDOWN && KeyDown != null)
                    KeyDown(this, new KeyEventArgs((Keys)kbhs.vkCode));
                else
                {
                    if (m.WParam == (IntPtr)WindowsApi.WM_KEYUP && KeyUp != null)
                        KeyUp.Invoke(this, new KeyEventArgs((Keys)kbhs.vkCode));
                    if (m.WParam == (IntPtr)WindowsApi.WM_KEYUP && KeyPress != null)
                        KeyPress(this, new KeyEventArgs((Keys)kbhs.vkCode));
                }
            }
            else if (m.Msg == _MsgID_KeyboardLL_HookReplaced)
            {
                if (HookReplaced != null)
                    HookReplaced();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            /// <summary>
            /// 虚拟按键码(1--254)
            /// </summary>
            public int vkCode;
            /// <summary>
            /// 硬件按键扫描码
            /// </summary>
            public int scanCode;
            /// <summary>
            /// 键按下：128 抬起：0
            /// </summary>
            public int flags;
            /// <summary>
            /// 消息时间戳间
            /// </summary>
            public int time;
            /// <summary>
            /// 额外信息
            /// </summary>
            public int dwExtraInfo;
        }
    }
}