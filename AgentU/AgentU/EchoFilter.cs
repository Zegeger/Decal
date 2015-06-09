using System;
using System.Collections.Generic;
using System.Text;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Plugins.AgentU
{
    class EchoFilter
    {
        internal const int GAME_EVENT = 0xF7B0;
        internal const int GAME_ACTION = 0xF7B1;
        internal const int LOGIN_INFO = 0x13;
        internal const int LEVEL = 0x19;
        internal const int UPDATE_XP = 0x2CF;
        internal const int UPDATE_TOTALXP = 0x1;
        internal const int UPDATE_UNASSXP = 0x2;
        internal const int KILL_DEATH_MESSAGE = 0x1AD;
        internal const int UPDATE_OPTIONS = 0x1A1;
        internal const int IDENTIFY = 0xC9;
        internal const int UPDATE_HEALTH = 0x1C0;
        internal const int VISUAL_EFFECT = 0xF755;

        public EchoFilter(CoreManager core)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            core.EchoFilter.ServerDispatch += new EventHandler<NetworkMessageEventArgs>(EchoFilter_ServerDispatch);
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void EchoFilter_ServerDispatch(object sender, NetworkMessageEventArgs e)
        {
            //TraceLogger.Write("Enter", TraceLevel.Noise);
            //TraceLogger.Write("Message Type: " + e.Message.Type, TraceLevel.Noise);
            switch (e.Message.Type)
            {
                case GAME_EVENT:
                    int gameEvent = e.Message.Value<int>("event");
                    //TraceLogger.Write("Game Event Type: " + gameEvent, TraceLevel.Noise);
                    switch (gameEvent)
                    {
                        case KILL_DEATH_MESSAGE:

                            break;
                    }
                    break;
            }
            //TraceLogger.Write("Exit", TraceLevel.Noise);
        }
    }
}
