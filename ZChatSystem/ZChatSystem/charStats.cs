using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Zegeger.Decal.Plugins.ZChatSystem.Diagnostics;


namespace Zegeger.Decal.Plugins.ZChatSystem
{
    internal partial class PluginCore
    {
        private void initCharStats()
        {
            TraceLogger.Write("Enter");
            Core.CharacterFilter.LoginComplete += new EventHandler(CharacterFilter_LoginComplete);
            TraceLogger.Write("Exit");
        }

        private void destroyCharStats()
        {
            TraceLogger.Write("Enter");
            Core.CharacterFilter.LoginComplete -= new EventHandler(CharacterFilter_LoginComplete);
            TraceLogger.Write("Exit");
        }

        void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter");
                wtcw("Plugin Loaded! Type /zc help for commands.");
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }
    }
}