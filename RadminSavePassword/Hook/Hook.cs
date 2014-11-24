using System;
using System.Runtime.InteropServices;

namespace RadminSavePassword.Hook
{
    public abstract class Hook
    {
        // API call needed to retreive the value of the messages to intercept from the unmanaged DLL
        [DllImport("user32.dll")]
        protected static extern int RegisterWindowMessage(string lpString);

        public delegate void HookReplacedEventHandler();
        public delegate void WindowEventHandler(IntPtr Handle);
        public delegate void SysCommandEventHandler(int SysCommand, int lParam);
        public delegate void ActivateShellWindowEventHandler();
        public delegate void TaskmanEventHandler();
        public delegate void BasicHookEventHandler(IntPtr Handle1, IntPtr Handle2);
        public delegate void WndProcEventHandler(IntPtr Handle, IntPtr Message, IntPtr wParam, IntPtr lParam);

        protected bool _IsActive = false;
        protected IntPtr _Handle;

        public Hook(IntPtr Handle)
        {
            _Handle = Handle;
        }

        public void Start()
        {
            if (!_IsActive)
            {
                _IsActive = true;
                OnStart();
            }
        }

        public void Stop()
        {
            if (_IsActive)
            {
                OnStop();
                _IsActive = false;
            }
        }

        ~Hook()
        {
            Stop();
        }

        public bool IsActive
        {
            get { return _IsActive; }
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        public abstract void ProcessWindowMessage(ref System.Windows.Forms.Message m);
    }
}