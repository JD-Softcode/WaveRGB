using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// This code was adapted from https://stackoverflow.com/questions/5852455/background-key-press-listener
//  which in turn was adapted from https://social.msdn.microsoft.com/Forums/vstudio/en-US/ed63b033-663a-4a20-80a5-a732d31e9486/keylogger-code-in-cnet?forum=csharpgeneral
//  also found on http://www.dylansweb.com/2014/10/low-level-global-keyboard-hook-sink-in-c-net/

namespace WaveRGB
{
    class KeyListener
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP   = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static int lastKeyWas = 0;

        private static WaveRGBActions parentProcess;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [STAThread]
        public static void StartKeyListener(WaveRGBActions theApp)
        {
            _hookID = SetHook(_proc);
            parentProcess = theApp;
        }

        public static void StopKeyListener() { 
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    var key = Marshal.ReadInt32(lParam);
                    if (key != lastKeyWas)      // holding a key down will report multiple key downs (really fast!). Don't want that.
                    {
                        parentProcess.KeyPressed(key);
                        lastKeyWas = key;
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    lastKeyWas = 0;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
