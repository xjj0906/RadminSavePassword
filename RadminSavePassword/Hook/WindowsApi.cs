using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RadminSavePassword.Hook
{
    public static class WindowsApi
    {
        public struct Rect
        {
            public uint Left;
            public uint Top;
            public uint Right;
            public uint Bottom;
        }

        public struct Point
        {
            public uint X;
            public uint Y;
        }

        public const int WM_KEYDOWN = 0X100;
        public const int WM_KEYUP = 0X101;
        public const int WM_SYSCHAR = 0X106;
        public const int WM_SYSKEYUP = 0X105;
        public const int WM_SYSKEYDOWN = 0X104;
        public const int WM_CHAR = 0X102;

        public const int BM_GETCHECK = 0x00F0;
        public const int WM_GETTEXT = 0x000D;
        public const int WM_SETTEXT = 0x000C;

        public const int BST_UNCHECKED = 0x0000;
        public const int BST_CHECKED = 0x0001;

        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;
        public const int WH_MOUSE = 7;

        public const int GWL_STYLE = -16;

        public const long ES_PASSWORD = 0x0020L;

        [DllImport("user32.dll")]
        public static extern long SendMessage(IntPtr childHandle, int msg, int lParam, string wParam);

        [DllImport("user32.dll")]
        public static extern long SendMessage(IntPtr childHandle, int msg, int lParam, StringBuilder wParam);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, int wParam, uint lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr EnumChildWindows(IntPtr hWndParent, CallBack lpfn, IntPtr lParam);

        [DllImport("user32")]
        public static extern bool GetWindowRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        public static extern bool GetWindowText(IntPtr hWnd, StringBuilder lpString, int cch);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookCallBack lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, int wParam, int lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern long GetWindowLong(IntPtr hWnd, int nlndex);

        [DllImport("user32.dll")]
        public static extern long SetWindowLong(IntPtr hWnd, int nlndex, long dwNewLong);

        [DllImport("user32.dll")]
        public static extern long SetWindowWord(IntPtr hWnd, int nlndex, long wNewWord);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public delegate bool CallBack(IntPtr hwnd, int lParam);

        public delegate int HookCallBack(int code, int wparam, int lparam);
    }
}