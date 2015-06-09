using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Zegeger.Decal.Plugins.ZChatSystem.Diagnostics;


namespace Zegeger.Decal.Plugins.ZChatSystem
{
    internal partial class PluginCore
    {
        private static int MessageColor = 13;

        public PluginCore()
        {

        }

        private void initChatEvents()
        {
            TraceLogger.Write("Enter");

            Core.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(Core_ChatBoxMessage);
            Core.CommandLineText += new EventHandler<ChatParserInterceptEventArgs>(Core_CommandLineText);

            TraceLogger.Write("Exit");
        }

        private void destroyChatEvents()
        {
            Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(Core_ChatBoxMessage);
            Core.CommandLineText -= new EventHandler<ChatParserInterceptEventArgs>(Core_CommandLineText);
        }

        private void Core_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e)
        {
            try
            {
                ZChatSystem.internalChatBoxMessage(e);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        private void Core_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter: " + e.Text);
                if (e.Text.StartsWith("/zc") | e.Text.StartsWith("@zc"))
                {
                    string command = e.Text.Substring(3).Trim();
                    TraceLogger.Write("Matches prefix, command: " + command);
                    if (command.StartsWith("help"))
                    {
                        TraceLogger.Write("Command matches help");
                        WriteToChat("ZChatSystem 1.0.0.0");
                        WriteToChat("By Zegeger of Harvestgain");
                    }
                }
                TraceLogger.Write("Exit: " + e.Text);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void wtcw(string message)
        {
            WriteToChat(message);
        }

        internal static void WriteToChat(string message)
        {
            //TraceLogger.Write("Enter - message: " + message);
            try
            {
                PluginCore.MyHost.Actions.AddChatText("<ZChat> " + message, MessageColor);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            //TraceLogger.Write("Exit - message: " + message);
        }

        internal static void WriteToChatDebug(string message)
        {
            try
            {
                if(PluginCore.MyHost != null && PluginCore.MyHost.Actions != null)
                    PluginCore.MyHost.Actions.AddChatText("<ZChat> " + message, MessageColor);
            }
            catch (Exception ex)
            {
            }
        }

        internal static void WriteToChatRaw(string message, int color, int target)
        {
            TraceLogger.Write("Enter - message: " + message + " color: " + color + " target: " + target);
            try
            {
                PluginCore.MyHost.Actions.AddChatText(message, color, target);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit - message: " + message + " color: " + color + " target: " + target);
        }
    }
}