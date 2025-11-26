using System.Runtime.InteropServices;

namespace ConsoleDeck;

static class Program
{
    private static readonly HashSet<int> pressedKeys = [];

    // Win32 API for global keyboard hook
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    

    #region Win32 API Imports
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    #endregion

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Set global keyboard hook
        _hookID = SetHook(_proc);

        ApplicationConfiguration.Initialize();

        // Read configuration
        ProcessingUnit.ReadConfiguration();
        
        Application.Run(new MainForm());

        // Unhook on exit
        UnhookWindowsHookEx(_hookID);

    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule ?? throw new Exception("Failed to get current module.");
        return SetWindowsHookEx(13, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int WM_KEYDOWN = 0x0100;
            int WM_KEYUP = 0x0101;
            if (wParam == WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                pressedKeys.Add(vkCode);
            }
            else if (wParam == WM_KEYUP)
            {
                var mainForm = Application.OpenForms.Count > 0 ? Application.OpenForms[0] as MainForm : null;
                if (mainForm != null && mainForm.Visible)
                {
                    string keys = string.Join("+", pressedKeys.OrderBy(k => k).Select(k => ((Keys)k).ToString()));
                    mainForm.ShowKeyEvent(keys);
                }
                else
                {
                    ProcessingUnit.ProcessKeyEvent(pressedKeys);
                }
                pressedKeys.Clear();
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
}