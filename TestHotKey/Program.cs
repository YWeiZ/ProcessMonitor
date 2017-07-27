using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


namespace TestHotKey 
{
    class MyApplicationContext
    {
        static void Main(string[] args)
        {
        }
        


        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        void PressScrollLock()
        {
            byte SCROLLKEY = 145;  // magic num
            uint KEYEVENTF_KEYUP = 0x0002; // magic num
            keybd_event(SCROLLKEY, 0, 0, 0);
            Thread.Sleep(10);
            keybd_event(SCROLLKEY, 0, KEYEVENTF_KEYUP, 0);
        }


    }
}
