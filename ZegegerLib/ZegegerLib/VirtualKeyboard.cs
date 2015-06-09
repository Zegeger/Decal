using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Zegeger.Diagnostics;

namespace Zegeger.Input
{
    [Flags]
    public enum KeyModifiers
    {
        Shift = 256,
        Ctrl = 512,
        Alt = 1024,
        Windows = 2048
    }

    public static class VirtualKeyboard
    {
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        const uint SHIFT = 16;
        const uint CTRL = 17;
        const uint ALT = 18;
        const uint WIN = 91;

        static List<KeyModifiers> KeyMods = new List<KeyModifiers>();

        static VirtualKeyboard()
        {
            KeyMods.Add(KeyModifiers.Shift);
            KeyMods.Add(KeyModifiers.Ctrl);
            KeyMods.Add(KeyModifiers.Alt);
            KeyMods.Add(KeyModifiers.Windows);
        }

        public static void SendKey(uint key)
        {
            TraceLogger.Write("Enter key = " + key, TraceLevel.Verbose);
            Dictionary<KeyModifiers, bool> KeyModPresent = new Dictionary<KeyModifiers, bool>();
            foreach (KeyModifiers keymask in KeyMods)
            {
                if ((key & (uint)keymask) == (uint)keymask)
                {
                    KeyModPresent.Add(keymask, true);
                }
                else
                {
                    KeyModPresent.Add(keymask, false);
                }
            }

            if (KeyModPresent[KeyModifiers.Shift])
            {
                TraceLogger.Write("Shift key present", TraceLevel.Verbose);
                byte shiftScan = (byte)MapVirtualKey(SHIFT, 0);
                keybd_event((byte)SHIFT, shiftScan, 0, 0);
            }

            if (KeyModPresent[KeyModifiers.Ctrl])
            {
                TraceLogger.Write("Ctrl key present", TraceLevel.Verbose);
                byte ctrlScan = (byte)MapVirtualKey(CTRL, 0);
                keybd_event((byte)CTRL, ctrlScan, 0, 0);
            }

            if (KeyModPresent[KeyModifiers.Alt])
            {
                TraceLogger.Write("Alt key present", TraceLevel.Verbose);
                byte altScan = (byte)MapVirtualKey(ALT, 0);
                keybd_event((byte)ALT, altScan, 0, 0);
            }

            if (KeyModPresent[KeyModifiers.Windows])
            {
                TraceLogger.Write("Windows key present", TraceLevel.Verbose);
                byte winScan = (byte)MapVirtualKey(WIN, 0);
                keybd_event((byte)WIN, winScan, 1, 0);
            }

            byte bkey = (byte)(key % 256);
            byte scan = (byte)MapVirtualKey(key % 256, 0);
            keybd_event(bkey, scan, 0, 0);
            keybd_event(bkey, scan, 2, 0);

            if (KeyModPresent[KeyModifiers.Shift])
            {
                byte shiftScan = (byte)MapVirtualKey(SHIFT, 0);
                keybd_event((byte)SHIFT, shiftScan, 2, 0);
            }

            if (KeyModPresent[KeyModifiers.Ctrl])
            {
                byte ctrlScan = (byte)MapVirtualKey(CTRL, 0);
                keybd_event((byte)CTRL, ctrlScan, 2, 0);
            }

            if (KeyModPresent[KeyModifiers.Alt])
            {
                byte altScan = (byte)MapVirtualKey(ALT, 0);
                keybd_event((byte)ALT, altScan, 2, 0);
            }

            if (KeyModPresent[KeyModifiers.Windows])
            {
                byte winScan = (byte)MapVirtualKey(WIN, 0);
                keybd_event((byte)WIN, winScan, 3, 0);
            }

            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }
}
