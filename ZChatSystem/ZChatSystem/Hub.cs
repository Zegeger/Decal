using System;
using System.Collections.Generic;
using System.Text;

namespace ZChatSystem
{
    public class Hub
    {
        ZChatSystem.PluginCore zcpc;

        public static event dChatCompleteEvent ChatTextComplete;
        public static event dChatMessageEvent ChatBoxMessage;
        
        public Hub()
        {
            zcpc = new ZChatSystem.PluginCore();
        }

        public string RegisterPlugin(string PluginName)
        {
            return zcpc.internalRegisterPlugin(PluginName);
        }

        internal void RaiseChatMessageEvent(ZChatTextInterceptEventArgs e)
        {
            ChatBoxMessage(e);
        }
    }
}
