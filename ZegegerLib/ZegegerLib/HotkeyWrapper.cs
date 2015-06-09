using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Zegeger.Decal.Plugins.ZChatSystem;
using Zegeger.Diagnostics;
using VirindiHotkeySystem;

namespace Zegeger.Decal.Hotkey
{
    public delegate bool HotkeyCallback(string name);

    public static class ZHotkeyWrapper
    {
        static bool isStarted = false;
        static HotkeySystem DHS = null;
        static VHotkeySystem VHS = null;
        static string PluginName;
        static string ShortPluginName;

        static Dictionary<string, HotkeyCallback> callbacks;

        public static void Initialize(string pluginName, string shortPluginName, CoreManager Core)
        {
            TraceLogger.Write("Enter name: " + pluginName + ", short name: " + shortPluginName, TraceLevel.Verbose);
            PluginName = pluginName;
            ShortPluginName = shortPluginName;
            callbacks = new Dictionary<string, HotkeyCallback>();
            if (Core.HotkeySystem != null)
            {
                TraceLogger.Write("DHS Present", TraceLevel.Verbose);
                DHS = Core.HotkeySystem;
                DHS.Hotkey += DHS_Hotkey;
            }
            if (IsVHSPresent())
            {
                TraceLogger.Write("VHS Present", TraceLevel.Verbose);
                VHS = VHotkeySystem.InstanceReal;
            }
            isStarted = true;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (!isStarted) return;
            isStarted = false;
            callbacks.Clear();
            if (DHS != null)
            {
                DHS.Hotkey -= DHS_Hotkey;
            }
            DHS = null;
            VHS = null;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        static void DHS_Hotkey(object sender, HotkeyEventArgs e)
        {
            TraceLogger.Write("Enter DHS Press for " + e.Title, TraceLevel.Verbose);
            if (!isStarted) return;
            string prefix = ShortPluginName + ": ";
            string title = e.Title;
            if (title.Length <= prefix.Length) return;
            if (!title.StartsWith(prefix)) return;
            title = title.Substring(prefix.Length);

            if (callbacks.ContainsKey(title))
            {
                e.Eat = callbacks[title](title);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static void AddHotkey(string name, string description, int virtualKey, bool altState, bool ctrlState, bool shiftState, HotkeyCallback callback)
        {
            TraceLogger.Write("Enter name: " + name, TraceLevel.Verbose);
            if (!isStarted)
                throw new InvalidOperationException("Hotkey Wrapper has not been started!");
            if (!callbacks.ContainsKey(name))
            {
                TraceLogger.Write("Adding " + name, TraceLevel.Verbose);
                callbacks.Add(name, callback);
                if (DHS != null)
                {
                    if (!DHS.Exists(ShortPluginName + ": " + name))
                    {
                        TraceLogger.Write("Adding DHS Hotkey", TraceLevel.Verbose);
                        DHS.AddHotkey(PluginName, ShortPluginName + ": " + name, description, virtualKey, altState, ctrlState, shiftState);
                    }
                }
                if (VHS != null)
                {
                    if (VHS.GetHotkeyByName(name) == null)
                    {
                        TraceLogger.Write("Adding VHS Hotkey", TraceLevel.Verbose);
                        VHotkeyInfo info = new VHotkeyInfo(PluginName, false, name, description, virtualKey, altState, ctrlState, shiftState);
                        info.Fired2 += info_Fired2;
                        VHS.AddHotkey(info);
                    }
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static void RemoveHotkey(string name)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (!isStarted)
                throw new InvalidOperationException("Hotkey Wrapper has not been started!");
            if (callbacks.ContainsKey(name))
            {
                TraceLogger.Write("Removing " + name, TraceLevel.Verbose);
                callbacks.Remove(name);
                if (DHS != null)
                {
                    if (DHS.Exists(name))
                    {
                        TraceLogger.Write("Removing DHS Hotkey", TraceLevel.Verbose);
                        DHS.DeleteHotkey(PluginName, name);
                    }
                }
                if (VHS != null)
                {
                    VHotkeyInfo info = VHS.GetHotkeyByName(name);
                    if (info != null)
                    {
                        TraceLogger.Write("Removing VHS Hotkey", TraceLevel.Verbose);
                        info.Fired2 -= info_Fired2;
                        VHS.RemoveHotkey(info);
                    }
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        static void info_Fired2(object sender, VHotkeyInfo.cEatableFiredEventArgs e)
        {
            TraceLogger.Write("Enter VHS Press", TraceLevel.Verbose);
            if (!isStarted) return;
            VHotkeyInfo info = (VHotkeyInfo)sender;
            if (callbacks.ContainsKey(info.HotkeyName))
            {
                e.Eat = callbacks[info.HotkeyName](info.HotkeyName);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        static bool IsVHSPresent()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            try
            {
                //See if VCS assembly is loaded
                System.Reflection.Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
                bool l = false;
                foreach (System.Reflection.Assembly a in asms)
                {
                    AssemblyName nmm = a.GetName();
                    if ((nmm.Name == "VirindiHotkeySystem") && (nmm.Version >= new System.Version("1.0.0.0")))
                    {
                        l = true;
                        break;
                    }
                }

                if (l)
                    if (VirindiHotkeySystem.VHotkeySystem.Running)
                    {
                        TraceLogger.Write("Exit true", TraceLevel.Verbose);
                        return true;
                    }
            }
            catch
            {

            }
            TraceLogger.Write("Exit false", TraceLevel.Verbose);
            return false;
        }
    }
}
