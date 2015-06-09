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

        static bool[,] TargetList;
        int TargetListCount = 0;

        void initEchoFilter()
        {
            TraceLogger.Write("Enter");
            TargetList = new bool[5,35];
            resetTargetList();
            Core.EchoFilter.ServerDispatch += new EventHandler<NetworkMessageEventArgs>(EchoFilter_ServerDispatch);
            Core.EchoFilter.ClientDispatch += new EventHandler<NetworkMessageEventArgs>(EchoFilter_ClientDispatch);
            TraceLogger.Write("Exit");
        }

        void destroyEchoFilter()
        {
            TraceLogger.Write("Enter");
            TargetList = null;
            Core.EchoFilter.ServerDispatch -= new EventHandler<NetworkMessageEventArgs>(EchoFilter_ServerDispatch);
            Core.EchoFilter.ClientDispatch -= new EventHandler<NetworkMessageEventArgs>(EchoFilter_ClientDispatch);
            TraceLogger.Write("Exit");
        }

        private void resetTargetList()
        {
            try
            {
                TraceLogger.Write("Enter");
                for(int x = 0; x < TargetList.GetLength(0); x++)
                {
                    for(int y = 0; y < TargetList.GetLength(1); y++)
                    {
                        TargetList[x,y] = false;
                    }
                }
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal void setTargetList(int a, int b, bool v)
        {
            try
            {
                TraceLogger.Write("Enter - win: " + a + ", type: " + b + ", val: " + v);
                TargetList[a,b] = v;
                TargetListCount++;
                TraceLogger.Write("Exit - win: " + a + ", type: " + b + ", val: " + v);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static bool getTargetList(int a, int b)
        {
            try
            {
                TraceLogger.Write("Enter - win: " + a + ", type: " + b);
                bool rtn = TargetList[a, b];
                TraceLogger.Write("Enter - win: " + a + ", type: " + b + ", val: " + rtn);
                return rtn;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
                return false;
            }
        }

        void EchoFilter_ServerDispatch(object sender, NetworkMessageEventArgs e)
        {
            try
            {
                //TraceLogger.Write("Enter");
                switch(e.Message.Type)
                {
                    case GAME_EVENT:
                        //TraceLogger.Write("Game Event");
                        switch(e.Message.Value<int>("event"))
                        {
                            case LOGIN_INFO:
                                TraceLogger.Write("Login Info");
                                HandleOptions(e.Message);
                            break;
                        }
                    break;
                }
                //TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        void EchoFilter_ClientDispatch(object sender, NetworkMessageEventArgs e)
        {
            try
            {
                //TraceLogger.Write("Enter");
                switch(e.Message.Type)
                {
                    case GAME_ACTION:
                        //TraceLogger.Write("Game Action");
                        switch(e.Message.Value<int>("action"))
                        {
                            case UPDATE_OPTIONS:
                                TraceLogger.Write("Update Options");
                                HandleOptions(e.Message);
                            break;
                        }
                    break;
                }
                //TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        void HandleOptions(Message pMsg)
        {
            TraceLogger.Write("Enter");
            MessageStruct Options;
            MessageStruct OptionsProp;
            MessageStruct OptionsPropItem;
            MessageStruct Windows;
            MessageStruct WindowsItem;
            MessageStruct Property;
            int i, j, k, n, o, p;
            long mask;

           resetTargetList();
           Options = pMsg.Struct("options");
           if((Options.Value<int>("flags") & 0x200) != 0 )
           {
               n = (int)Options.Value<byte>("optionPropertyCount") - 1;
               TraceLogger.Write("n: " + n);
               if (n >= 0)
               {
                   OptionsProp = Options.Struct("optionProperties");
                   for(i = 0; i <= n; i++)
                   {
                       OptionsPropItem = OptionsProp.Struct(i);
                       if(OptionsPropItem.Value<int>("type") == 0x1000008C)
                       {
                           o = OptionsPropItem.Value<int>("windowCount") - 1;
                           TraceLogger.Write("o: " + o);
                           if (o >= 0)
                           {
                               Windows = OptionsPropItem.Struct("windows");
                               for (j = 0; j <= o; j++)
                               {
                                   TraceLogger.Write("Window: " + j);
                                   //wtcw("Window " + j);
                                   if (j != 1 && j != 2 && j != 3 && j != 4 && j != 7)
                                       continue;
                                   WindowsItem = Windows.Struct(j);
                                   if(WindowsItem.Value<int>("type") == 0x1000008B)
                                   {
                                       p = (int)WindowsItem.Value<byte>("propertyCount") - 1;
                                       TraceLogger.Write("p: " + p);
                                       if(p >= 0)
                                       {
                                           Property = WindowsItem.Struct("properties");
                                           for(k = 0; k <= p; k++)
                                           {
                                               if(Property.Struct(k).Value<int>("key") == 0x1000007F)
                                               {
                                                   int w = j;
                                                   if (w == 7)
                                                       w = 0;
                                                   mask = Property.Struct(k).Value<long>("value");
                                                   TraceLogger.Write("mask: " + mask);
                                                   //wtcw("Mask: " + mask);
                                                   if((mask & 0x3912021) != 0)
                                                   {
                                                       //wtcw("Gameplay (main chat window only)");
                                                       TraceLogger.Write("Gameplay (main chat window only)");
                                                       setTargetList(w, 0, true);
                                                       setTargetList(w, 5, true);
                                                       setTargetList(w, 6, true);
                                                       setTargetList(w, 13, true);
                                                       setTargetList(w, 16, true);
                                                       setTargetList(w, 20, true);
                                                       setTargetList(w, 23, true);
                                                       setTargetList(w, 24, true);
                                                       setTargetList(w, 25, true);
                                                       setTargetList(w, 31, true);
                                                   }
                                                   if((mask & 0x1004) != 0)
                                                   {
                                                       //wtcw ("Area Chat");
                                                       TraceLogger.Write("Area Chat");
                                                       setTargetList(w, 2, true);
                                                       setTargetList(w, 12, true);
                                                   }
                                                   if((mask & 0x18) != 0)
                                                   {
                                                       //wtcw ("Tells");
                                                       TraceLogger.Write("Tells");
                                                       setTargetList(w, 3, true);
                                                       setTargetList(w, 4, true);
                                                   }
                                                   if((mask & 0x600040) != 0)
                                                   {
                                                       //wtcw ("Combat");
                                                       TraceLogger.Write("Combat");
                                                       setTargetList(w, 21, true);
                                                       setTargetList(w, 22, true);
                                                       setTargetList(w, 6, true);
                                                   }
                                                   if((mask & 0x20080) != 0)
                                                   {
                                                       //wtcw ("Magic");
                                                       TraceLogger.Write("Magic");
                                                       setTargetList(w, 17, true);
                                                       setTargetList(w, 7, true);
                                                   }
                                                   if((mask & 0x40C00) != 0)
                                                   {
                                                       //wtcw ("Allegiance");
                                                       TraceLogger.Write("Allegiance");
                                                       setTargetList(w, 10, true);
                                                       setTargetList(w, 11, true);
                                                       setTargetList(w, 18, true);
                                                   }
                                                   if((mask & 0x80000) != 0)
                                                   {
                                                       //wtcw ("Fellowship");
                                                       TraceLogger.Write("Fellowship");
                                                       setTargetList(w, 19, true);
                                                   }
                                                   if((mask & 0x4000000) != 0)
                                                   {
                                                       //wtcw ("Errors");
                                                       TraceLogger.Write("Errors");
                                                       setTargetList(w, 26, true);
                                                   }
                                                   if((mask & 0x8000000) != 0)
                                                   {
                                                       //wtcw ("General Channel");
                                                       TraceLogger.Write("General Channel");
                                                       setTargetList(w, 27, true);
                                                   }
                                                   if((mask & 0x10000000) != 0)
                                                   {
                                                       //wtcw ("Trade Channel");
                                                       TraceLogger.Write("Trade Channel");
                                                       setTargetList(w, 28, true);
                                                   }
                                                   if((mask & 0x20000000) != 0)
                                                   {
                                                       //wtcw ("LFG Channel");
                                                       TraceLogger.Write("LFG Channel");
                                                       setTargetList(w, 29, true);
                                                   }
                                                   if((mask & 0x40000000) != 0)
                                                   {
                                                       //wtcw ("Roleplay Channel");
                                                       TraceLogger.Write("Roleplay Channel");
                                                       setTargetList(w, 30, true);
                                                   }
                                                   if((mask & 0x100000000) != 0)
                                                   {
                                                       //wtcw ("Society Channel");
                                                       TraceLogger.Write("Society Channel");
                                                       setTargetList(w, 32, true);
                                                   }
                                               }
                                           }
                                       }
                                   }
                               }
                           }
                       }
                   }
               }
           }

           if(TargetListCount == 0)
           {
               TraceLogger.Write("TargetListCount is 0, using defaults");
               setTargetList(0, 0, true);
               setTargetList(0, 5, true);
               setTargetList(0, 6, true);
               setTargetList(0, 13, true);
               setTargetList(0, 16, true);
               setTargetList(0, 20, true);
               setTargetList(0, 23, true);
               setTargetList(0, 24, true);
               setTargetList(0, 25, true);
               setTargetList(0, 2, true);
               setTargetList(0, 12, true);
               setTargetList(0, 3, true);
               setTargetList(0, 4, true);
               setTargetList(0, 21, true);
               setTargetList(0, 22, true);
               setTargetList(0, 7, true);
               setTargetList(0, 17, true);
               setTargetList(0, 10, true);
               setTargetList(0, 11, true);
               setTargetList(0, 18, true);
               setTargetList(0, 19, true);
               setTargetList(0, 27, true);
               setTargetList(0, 28, true);
               setTargetList(0, 29, true);
               setTargetList(0, 30, true);
               setTargetList(1, 2, true);
               setTargetList(1, 12, true);
               setTargetList(1, 3, true);
               setTargetList(1, 4, true);
               setTargetList(2, 10, true);
               setTargetList(2, 11, true);
               setTargetList(2, 18, true);
               setTargetList(3, 19, true);
               setTargetList(4, 27, true);
               setTargetList(4, 28, true);
               setTargetList(4, 29, true);
               setTargetList(4, 30, true);
           }
           TraceLogger.Write("Exit");
        }
    }
}