/*
gnu general public license v3.0
сделано ivi3
*/

uusing System;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpmsg);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern short RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool ShutdownBlockReasonCreate(IntPtr hWnd, string pwszReason);

    [DllImport("user32.dll")]
    private static extern bool ShutdownBlockReasonDestroy(IntPtr hWnd);

    private const int WM_QUERYENDSESSION = 0x0011;

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public System.Drawing.Point pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_QUERYENDSESSION)
        {
            return IntPtr.Zero; // Block shutdown
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    static void Main()
    {
        IntPtr hInstance = GetModuleHandle(null);
        WNDCLASS wc = new WNDCLASS();
        wc.lpszClassName = "ShutdownBlocker";
        wc.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(new WndProcDelegate(WndProc));
        wc.hInstance = hInstance;

        short classAtom = RegisterClass(ref wc);
        if (classAtom == 0)
        {
            return;
        }

        IntPtr hWnd = CreateWindowEx(0, "ShutdownBlocker", "ShutdownBlocker", 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
        if (hWnd == IntPtr.Zero)
        {
            return;
        }

        // современного API для блокировки завершения работы
        ShutdownBlockReasonCreate(hWnd, "Remote SSH access required");

        MSG msg;
        while (GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }

        ShutdownBlockReasonDestroy(hWnd);
        DestroyWindow(hWnd);
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
