using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MobileMice
{
    /**
     * Mouse and keyboard emulator
     * using System32 calls
     * */
    class MouseKbEmulator
    {
        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        public const UInt32 WM_KEYDOWN = 0x0100;
        public const UInt32 WM_KEYUP = 0x0101;

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetPhysicalCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public static void SetCursorPosition(int x, int y)
        {
            SetPhysicalCursorPos(x, y);
        }

        public static void SetCursorPosition(MousePoint point)
        {
            SetPhysicalCursorPos(point.X, point.Y);
        }

        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        public static void SetCursorPositionDelta(int x, int y)
        {

            MousePoint current = GetCursorPosition();
            // SetPhysicalCursorPos(current.X + x, current.Y + y);
            
            mouse_event((int)MouseEventFlags.Move, x, y, 0, 0);
        }

        public static void MouseEvent(MouseEventFlags value)
        {
            MousePoint position = GetCursorPosition();

            mouse_event((int)value, 0, 0, 0, 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public static void pressKeyUp(byte key)
        {
            //PostMessage(((IntPtr)0xffff), WM_KEYUP, key, 0);
            keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
        }

        public static void pressKeyDown(byte key)
        {
            //PostMessage(((IntPtr)0xffff), WM_KEYDOWN, key, 0);
            keybd_event(key, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }
    }
}
