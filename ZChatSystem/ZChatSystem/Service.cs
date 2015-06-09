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
    public delegate void ChatBoxMessageEvent(ChatBoxMessageEventArgs e);
    public delegate void ChatTextCompleteEvent(ChatTextCompleteEventArgs e);

    public class ChatBoxMessageEventArgs : EventArgs
    {
        public int MessageID
        {
            get { return baseMessageID; }
        }
        public IDToken PluginID
        {
            set { basePluginID = value; basePluginIDSet = true; }
        }
        public string Text
        {
            get { return baseText; }
        }
        public WindowOutput Window
        {
            get { return baseWindow; }
        }
        public int Color
        {
            get { return baseColor; }
            set { baseColor = value; colorChanged = true; }
        }
        public void ForceDefaultColor()
        {
            colorChanged = true;
        }

        public int Type
        {
            get { return baseType; }
        }

        internal WindowOutput baseWindow;
        internal int baseMessageID;
        internal IDToken basePluginID;
        internal bool basePluginIDSet;

        internal int baseColor;
        internal bool colorChanged;
        //internal bool windowChanged;
        internal string baseText;
        internal int baseType;

        internal List<MsgChange> ModMessageList;

        public ChatBoxMessageEventArgs()
        {
            baseMessageID = -1;
            basePluginIDSet = false;
        }

        internal ChatBoxMessageEventArgs(int MsgID, string MsgText, int MsgColor, int MsgType, WindowOutput MsgWindow)
        {
            try
            {
                TraceLogger.Write("Enter ZChatTextInterceptEventArgs Constructor");

                baseMessageID = MsgID;
                PluginID = null;

                baseText = MsgText;
                ModMessageList = new List<MsgChange>();

                baseColor = MsgColor;
                colorChanged = false;

                baseType = MsgType;

                baseWindow = MsgWindow;
                //windowChanged = false;

                basePluginIDSet = false;

                TraceLogger.Write("Exit ZChatTextInterceptEventArgs Constructor");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public void ReplaceMessage(string Find, string Replace)
        {
            try
            {
                TraceLogger.Write("Enter - Find: " + Find + ", Replace: " + Replace);
                MsgChange t = new MsgChange();
                t.ChangeType = MsgModType.MESSAGE_REPLACE;
                t.Find = Find;
                t.NewText = Replace;
                ModMessageList.Add(t);
                TraceLogger.Write("Exit - Find: " + Find + ", Replace: " + Replace);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public void SubstituteMessage(string NewMessage)
        {
            try
            {
                TraceLogger.Write("Enter - NewMessage: " + NewMessage);
                MsgChange t = new MsgChange();
                t.ChangeType = MsgModType.MESSAGE_SUBSTITUTE;
                t.NewText = NewMessage;
                ModMessageList.Add(t);
                TraceLogger.Write("Exit - NewMessage: " + NewMessage);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public void AppendMessage(string AppendText)
        {
            try
            {
                TraceLogger.Write("Enter - AppendMessage: " + AppendText);
                MsgChange t = new MsgChange();
                t.ChangeType = MsgModType.MESSAGE_APPEND;
                t.NewText = AppendText;
                ModMessageList.Add(t);
                TraceLogger.Write("Exit - AppendMessage: " + AppendText);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public void PrependMessage(string PrependText)
        {
            try
            {
                TraceLogger.Write("Enter - PrependMessage: " + PrependText);
                MsgChange t = new MsgChange();
                t.ChangeType = MsgModType.MESSAGE_PREPEND;
                t.NewText = PrependText;
                ModMessageList.Add(t);
                TraceLogger.Write("Exit - PrependMessage: " + PrependText);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal bool isValid()
        {
            if (baseMessageID != -1 && basePluginID.isValid())
                return true;
            else
                return false;
        }
    }

    public class ChatTextCompleteEventArgs : EventArgs
    {
        public int MessageID { get; internal set; }
        public string Text { get; internal set; }
        public WindowOutputComplete Window { get; internal set; }
        public int Color { get; internal set; }
        public int Type { get; internal set; }

        public ChatTextCompleteEventArgs()
        {

        }
    }

    public class WindowOutputComplete : Object, ICloneable
    {
        public bool MainWindow { get; internal set; }
        public bool Window1 { get; internal set; }
        public bool Window2 { get; internal set; }
        public bool Window3 { get; internal set; }
        public bool Window4 { get; internal set; }

        public WindowOutputComplete()
        {
            MainWindow = false;
            Window1 = false;
            Window2 = false;
            Window3 = false;
            Window4 = false;
        }

        public bool IsShown
        {
            get
            {
                return MainWindow || Window1 || Window2 || Window3 || Window4;
            }
        }

        public object Clone()
        {
            WindowOutputComplete copy = new WindowOutputComplete();
            copy.MainWindow = this.MainWindow;
            copy.Window1 = this.Window1;
            copy.Window2 = this.Window2;
            copy.Window3 = this.Window3;
            copy.Window4 = this.Window4;
            return copy;
        }

        public override string ToString()
        {
            string rtn = "";
            if (MainWindow)
                rtn += "M";
            if (Window1)
                rtn += "1";
            if (Window2)
                rtn += "2";
            if (Window3)
                rtn += "3";
            if (Window4)
                rtn += "4";
            return rtn;
        }
    }

    public class WindowOutput : ICloneable
    {
        public bool MainWindow
        {
            get { return baseMainWindow; }
            set { baseMainWindow = value; windowsChanged = true; }
        }
        public bool Window1
        {
            get { return baseWindow1; }
            set { baseWindow1 = value; windowsChanged = true; }
        }
        public bool Window2
        {
            get { return baseWindow2; }
            set { baseWindow2 = value; windowsChanged = true; }
        }
        public bool Window3
        {
            get { return baseWindow3; }
            set { baseWindow3 = value; windowsChanged = true; }
        }
        public bool Window4
        {
            get { return baseWindow4; }
            set { baseWindow4 = value; windowsChanged = true; }
        }

        public bool IsShown
        {
            get
            {
                return baseMainWindow || baseWindow1 || baseWindow2 || baseWindow3 || baseWindow4;
            }
        }

        public WindowOutput()
        {
            baseMainWindow = false;
            baseWindow1 = false;
            baseWindow2 = false;
            baseWindow3 = false;
            baseWindow4 = false;
        }

        public WindowOutput(bool Main, bool W1, bool W2, bool W3, bool W4)
        {
            baseMainWindow = Main;
            baseWindow1 = W1;
            baseWindow2 = W2;
            baseWindow3 = W3;
            baseWindow4 = W4;
        }

        public void SetWindows(bool Main, bool W1, bool W2, bool W3, bool W4)
        {
            if(MainWindow != Main)
                MainWindow = Main;
            if(Window1 != W1)
                Window1 = W1;
            if (Window2 != W2)
                Window2 = W2;
            if (Window3 != W3)
                Window3 = W3;
            if (Window4 != W4)
                Window4 = W4;
        }
        public void ForceDefault()
        {
            windowsChanged = true;
        }

        public void BlockAll()
        {
            MainWindow = false;
            Window1 = false;
            Window2 = false;
            Window3 = false;
            Window4 = false;
        }

        internal bool windowsChanged = false;
        internal bool baseMainWindow;
        internal bool baseWindow1;
        internal bool baseWindow2;
        internal bool baseWindow3;
        internal bool baseWindow4;

        public override string ToString()
        {
            string rtn = "";
            if (baseMainWindow)
                rtn += "M";
            if (baseWindow1)
                rtn += "1";
            if (baseWindow2)
                rtn += "2";
            if (baseWindow3)
                rtn += "3";
            if (baseWindow4)
                rtn += "4";
            return rtn;
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        public WindowOutput Clone()
        {
            WindowOutput copy = new WindowOutput(this.MainWindow, this.Window1, this.Window2, this.Window3, this.Window4);
            copy.windowsChanged = this.windowsChanged;
            return copy;
        }

        #endregion
    }

    public class IDToken
    {
        internal Guid ID { get; set; }

        public IDToken()
        {
            ID = Guid.Empty;
        }

        internal IDToken(Guid zID)
        {
            ID = zID;
        }

        internal bool isValid()
        {
            if (!ID.Equals(Guid.Empty))
                return true;
            else
                return false;
        }
    }

    public class ChatType
    {
        public int Value { get; internal set; }
        internal IDToken Token { get;  set; }

        public ChatType()
        {
            Token = new IDToken();
            Value = 100;
        }

        internal bool isValid()
        {
            if (Token.isValid())
                return true;
            else
                return false;
        }
    }

    internal enum MsgModType
    {
        MESSAGE_REPLACE, MESSAGE_PREPEND, MESSAGE_APPEND, MESSAGE_SUBSTITUTE
    }

    internal class MessageData
    {
        public int iMessageID { get; set; }
        public WindowOutput DefaultWindows { get; set; }
        public MessageArgs MessageArgs;
        public Dictionary<Guid, bool> PluginResponse = new Dictionary<Guid, bool>();
        public Dictionary<Guid, int> newColor = new Dictionary<Guid, int>();
        public Dictionary<Guid, WindowOutput> newWindow = new Dictionary<Guid, WindowOutput>();
        public List<ModEntry> ModMessageList = new List<ModEntry>();
    }

    internal class MessageArgs
    {
        public string Text;
        public int Color;
        public int Type;
        public WindowOutput Target;
    }

    internal class ModEntry
    {
        public Guid Plugin { get; set; }
        public MsgModType ChangeType { get; set; }
        public string NewText { get; set; }
        public string Find { get; set; }
    }

    internal class OrderData
    {
        public Guid id { get; set; }
        public string plugin { get; set; }
        public bool enabled { get; set; }
    }

    internal class MsgChange
    {
        public MsgModType ChangeType { get; set; }
        public string NewText { get; set; }
        public string Find { get; set; }
    }

    internal class TypeData
    {
        internal int Color;
        internal WindowOutput Target;
        internal Guid ZChatID;

        internal TypeData(Guid inZChatID, int inColor, bool mainWindow, bool Window1, bool Window2, bool Window3, bool Window4)
        {
            Color = inColor;
            ZChatID = inZChatID;
            Target = new WindowOutput();
            Target.MainWindow = mainWindow;
            Target.Window1 = Window1;
            Target.Window2 = Window2;
            Target.Window3 = Window3;
            Target.Window4 = Window4;
        }
    }

    public static class ChatColors
    {
        public static int White { get { return 2; } }
        public static int Grey { get { return 12; } }
        public static int Yellow { get { return 3; } }
        public static int DimYellow { get { return 4; } }
        public static int Orange { get { return 18; } }
        public static int DarkRed { get { return 6; } }
        public static int LightRed { get { return 22; } }
        public static int Pink { get { return 8; } }
        public static int Purple { get { return 5; } }
        public static int DarkBlue { get { return 7; } }
        public static int LightBlue { get { return 14; } }
        public static int Turquoise { get { return 13; } }
        public static int Green { get { return 0; } }
    }

    public partial class ZChatSystem
    {
        public static event ChatTextCompleteEvent ChatTextComplete;
        public static event ChatBoxMessageEvent ChatBoxMessage;

        internal static int iCurMsgID;
        internal static List<OrderData> ColorOrder;
        internal static List<OrderData> WindowOrder;
        internal static List<OrderData> MessageOrder;

        internal static List<TypeData> PluginTypes;

        internal static Dictionary<Guid, string> Plugins;
        internal static Dictionary<string, Guid> PluginsName;

        public static bool Running { get; internal set; }
        
        private static int MessageColor;

        public ZChatSystem()
        {

        }

        internal static void initService()
        {
            TraceLogger.Write("Enter");

            iCurMsgID = 0;
            MessageColor = 13;

            ColorOrder = new List<OrderData>();
            WindowOrder = new List<OrderData>();
            MessageOrder = new List<OrderData>();
            PluginTypes = new List<TypeData>();

            Plugins = new Dictionary<Guid, string>();
            PluginsName = new Dictionary<string, Guid>();

            MainView.ListChange += new dListChange(MainView_ListChange);

            TraceLogger.Write("Exit");
        }

        internal static void destroyService()
        {
            TraceLogger.Write("Enter");

            MainView.ListChange -= new dListChange(MainView_ListChange);
            ColorOrder = null;
            WindowOrder = null;
            MessageOrder = null;
            PluginTypes = null;

            Plugins = null;
            PluginsName = null;

            TraceLogger.Write("Exit");
        }

        internal static WindowOutput DefaultTarget(int color)
        {
            TraceLogger.Write("Enter C:" + color);

            WindowOutput internalTarget;
            internalTarget = new WindowOutput();
            internalTarget.baseMainWindow = PluginCore.getTargetList(0, color);
            internalTarget.baseWindow1 = PluginCore.getTargetList(1, color);
            internalTarget.baseWindow2 = PluginCore.getTargetList(2, color);
            internalTarget.baseWindow3 = PluginCore.getTargetList(3, color);
            internalTarget.baseWindow4 = PluginCore.getTargetList(4, color);
            TraceLogger.Write("Exit - " + internalTarget.ToString());
            return internalTarget;
        }

        internal static WindowOutput IntegerTarget(int Target, int Color)
        {
            TraceLogger.Write("Enter T:" + Target + ", C:" + Color);
            WindowOutput internalTarget = new WindowOutput();
            switch (Target)
            {
                case 1:
                    internalTarget.baseMainWindow = true;
                    break;
                case 2:
                    internalTarget.baseWindow1 = true;
                    break;
                case 3:
                    internalTarget.baseWindow2 = true;
                    break;
                case 4:
                    internalTarget.baseWindow3 = true;
                    break;
                case 5:
                    internalTarget.baseWindow4 = true;
                    break;
                default:
                    internalTarget.baseMainWindow = PluginCore.getTargetList(0, Color);
                    internalTarget.baseWindow1 = PluginCore.getTargetList(1, Color);
                    internalTarget.baseWindow2 = PluginCore.getTargetList(2, Color);
                    internalTarget.baseWindow3 = PluginCore.getTargetList(3, Color);
                    internalTarget.baseWindow4 = PluginCore.getTargetList(4, Color);
                    break;
            }
            TraceLogger.Write("Exit - " + internalTarget.ToString());
            return internalTarget;
        }

        public static void WriteToChat(string Text, int Color)
        {
            try
            {
                TraceLogger.Write("Enter Txt:" + Text + ", C:" + Color);
                MessageArgs e = new MessageArgs();
                e.Text = Text;
                e.Color = Color;
                e.Type = 100;
                e.Target = DefaultTarget(Color);
                internalWTCMessage(e);
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }
        
        public static void WriteToChat(string Text, int Color, int Target)
        {
            try
            {
                TraceLogger.Write("Enter Txt:" + Text + ", C:" + Color + ", Tar:" + Target);
                MessageArgs e = new MessageArgs();
                e.Text = Text;
                e.Color = Color;
                e.Type = 100;
                e.Target = IntegerTarget(Target, Color);
                internalWTCMessage(e);
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, int Color, WindowOutput Target)
        {
            try
            {
                TraceLogger.Write("Enter Txt:" + Text + ", C:" + Color + ", Tar:" + Target.ToString());
                
                MessageArgs e = new MessageArgs();
                e.Text = Text;
                e.Color = Color;
                e.Type = 100;
                e.Target = Target;
                internalWTCMessage(e);
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, ChatType Type)
        {
            try
            {
                TraceLogger.Write("Enter Txt:" + Text + ", Typ:" + Type.Value + ", Z:" + Type.Token.ID.ToString());

                if (!Type.isValid())
                {
                    TraceLogger.Write("Invalid Type, Use ZChatSystem.Service.CreateChatType");
                    return;
                }

                MessageArgs e = new MessageArgs();
                e.Text = Text;
                e.Color = 0;
                if (Type.Value > 100 && Type.Value <= PluginTypes.Count + 100)
                {
                    e.Type = Type.Value;
                }
                else
                {
                    e.Type = 100;
                }
                WindowOutput internalTarget;
                if (e.Type > 100 && PluginTypes.Count >= e.Type - 100)
                {
                    if (Type.Token.ID.Equals(PluginTypes[e.Type - 101].ZChatID))
                    {
                        internalTarget = PluginTypes[e.Type - 101].Target;
                        e.Color = PluginTypes[e.Type - 101].Color;
                    }
                    else
                    {
                        internalTarget = DefaultTarget(e.Color);
                    }
                }
                else
                {
                    internalTarget = DefaultTarget(e.Color);
                }
                e.Target = internalTarget;
                internalWTCMessage(e);

                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, int Color, ChatType Type)
        {
            try
            {
                TraceLogger.Write("Enter Txt:" + Text + ", C:" + Color + ", Typ:" + Type.Value + ", Z:" + Type.Token.ID.ToString());

                if (!Type.isValid())
                {
                    TraceLogger.Write("Invalid Type, Use ZChatSystem.Service.CreateChatType");
                    return;
                }

                MessageArgs e = new MessageArgs();
                e.Text = Text;
                e.Color = Color;
                if (Type.Value > 100 && Type.Value <= PluginTypes.Count + 100)
                {
                    e.Type = Type.Value;
                }
                else
                {
                    e.Type = 100;
                }
                if (e.Type > 100 && PluginTypes.Count >= e.Type - 100)
                {
                    if (Type.Token.ID.Equals(PluginTypes[e.Type - 101].ZChatID))
                    {
                        e.Target = PluginTypes[e.Type - 101].Target;
                    }
                    else
                    {
                        e.Target = DefaultTarget(Color);
                    }
                }
                else
                {
                    e.Target = DefaultTarget(Color);
                }
                internalWTCMessage(e);
                TraceLogger.Write("Exit");
             }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, WindowOutput Target, ChatType Type)
        {
            try
            {
                TraceLogger.Write("Enter Txt:" + Text + ", Tar:" + Target.ToString() + ", Typ:" + Type.Value + ", Z:" + Type.Token.ID.ToString());

                if (!Type.isValid())
                {
                    TraceLogger.Write("Invalid Type, Use ZChatSystem.Service.CreateChatType");
                    return;
                }

                MessageArgs e = new MessageArgs();
                e.Text = Text;
                e.Color = 0;
                e.Type = 100;
                if (Type.Value > 100 && Type.Value <= PluginTypes.Count + 100)
                {
                    if (Type.Token.ID.Equals(PluginTypes[Type.Value - 101].ZChatID))
                    {
                        e.Type = Type.Value;
                        e.Color = PluginTypes[Type.Value - 101].Color;
                    }
                    else
                    {
                        
                    }
                }
                else
                {
                    
                }
                e.Target = Target;
                internalWTCMessage(e);
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, int Color, int Target, ChatType Type)
        {
            try
            {
                TraceLogger.Write("Enter Txt:" + Text + ", C:" + Color + ", Tar:" + Target + ", Typ:" + Type.Value + ", Z:" + Type.Token.ID.ToString());

                if (!Type.isValid())
                {
                    TraceLogger.Write("Invalid Type, Use ZChatSystem.Service.CreateChatType");
                    return;
                }

                MessageArgs e = new MessageArgs(); 
                e.Text = Text;
                e.Color = Color;
                if (Type.Value > 100 && Type.Value <= PluginTypes.Count + 100)
                {
                    if (Type.Token.ID.Equals(PluginTypes[Type.Value - 101].ZChatID))
                    {
                        e.Type = Type.Value;
                    }
                    else
                    {
                        e.Type = 100;
                    }
                }
                else
                {
                    e.Type = 100;
                }
                e.Target = IntegerTarget(Target, e.Type);
                internalWTCMessage(e);
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, int Color, WindowOutput Target, ChatType Type)
        {
            try
            {
                TraceLogger.Write("Enter Txt:" + Text + ", C:" + Color + ", Tar:" + Target.ToString() + ", Typ:" + Type.Value + ", Z:" + Type.Token.ID.ToString());

                if (!Type.isValid())
                {
                    TraceLogger.Write("Invalid Type, Use ZChatSystem.Service.CreateChatType");
                    return;
                }

                MessageArgs e = new MessageArgs();
                e.Text = Text;
                e.Color = Color;
                if (Type.Value > 100 && Type.Value <= PluginTypes.Count + 100)
                {
                    if(Type.Token.ID.Equals(PluginTypes[Type.Value - 101].ZChatID))
                    {
                        e.Type = Type.Value;
                    }
                    else
                    {
                        e.Type = 100;
                    }
                }
                else
                {
                    e.Type = 100;
                }
                e.Target = Target;
                internalWTCMessage(e);
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static IDToken RegisterPlugin(string PluginName)
        {
            try
            {
                TraceLogger.Write("Enter - Register Plugin: " + PluginName);
                if (PluginName.Equals(""))
                {
                    TraceLogger.Write("PluginName is null! Exiting and returning null.");
                    return null;
                }
                if (Plugins.ContainsValue(PluginName))
                {
                    TraceLogger.Write(PluginName + " has already been registered! Exiting and returning null.");
                    return null;
                }

                Guid newguid = Guid.NewGuid();

                if (PluginsName.ContainsKey(PluginName))
                {
                    TraceLogger.Write("Plugin settings exist.");
                    PluginsName[PluginName] = newguid;

                    int i;
                    i = MessageOrder.FindIndex(delegate(OrderData od)
                    {
                        return od.plugin == PluginName;
                    });
                    MessageOrder[i].id = newguid;

                    i = ColorOrder.FindIndex(delegate(OrderData od)
                    {
                        return od.plugin == PluginName;
                    });
                    ColorOrder[i].id = newguid;

                    i = WindowOrder.FindIndex(delegate(OrderData od)
                    {
                        return od.plugin == PluginName;
                    });
                    WindowOrder[i].id = newguid;
                }
                else
                {
                    TraceLogger.Write("Adding brand new plugin.");
                    PluginsName.Add(PluginName, newguid);

                    OrderData neworder = new OrderData();
                    neworder.id = newguid;
                    neworder.enabled = true;
                    neworder.plugin = PluginName;
                    ColorOrder.Add(neworder);

                    neworder = new OrderData();
                    neworder.id = newguid;
                    neworder.enabled = true;
                    neworder.plugin = PluginName;
                    WindowOrder.Add(neworder);

                    neworder = new OrderData();
                    neworder.id = newguid;
                    neworder.enabled = true;
                    neworder.plugin = PluginName;
                    MessageOrder.Add(neworder);

                    updateMessageView();
                    updateColorView();
                    updateWindowView();
                }

                Plugins.Add(newguid, PluginName);

                IDToken tmp = new IDToken(newguid);

                TraceLogger.Write("Exit - Register Plugin: " + PluginName + " GUID: " + newguid);
                return tmp;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
                return null;
            }
        }

        public static ChatType CreateChatType(IDToken ZChatID, int Color, bool MainWindow, bool Window1, bool Window2, bool Window3, bool Window4)
        {
            TraceLogger.Write("Enter Z:" + ZChatID.ID.ToString() + ", C:" + Color + ", M:" + MainWindow + ", W1:" + Window1 + ", W2:" + Window2 + ", W3:" + Window3 + ", W4:" + Window4);
            if (!ZChatID.isValid())
            {
                TraceLogger.Write("Invalid ZChatID! Exiting and returning null.");
                return null;
            }
            TypeData tmp = new TypeData(ZChatID.ID, Color, MainWindow, Window1, Window2, Window3, Window4);
            PluginTypes.Add(tmp);
            ChatType tmp2 = new ChatType();
            tmp2.Value = PluginTypes.Count + 100;
            tmp2.Token = ZChatID;
            TraceLogger.Write("Exit Type:" + (PluginTypes.Count + 100));
            return tmp2;
        }

        internal static void internalChatBoxMessage(ChatTextInterceptEventArgs e)
        {
            try
            {
                int perfid = startPerfMsg(e.Text);
                TraceLogger.Write("*START CHATBOX MESSAGE SERIES*");
                TraceLogger.Write("Enter C:" + e.Color + " T:" + e.Target + " M:" + e.Text);
                if (!PluginCore.initComplete)
                {
                    TraceLogger.Write("Exit, PluginInit Incomplete C:" + e.Color + " M:" + e.Text);
                    finishPerfMsg(perfid);
                    return;
                }
                if (Plugins.Count > 0 && ChatBoxMessage != null)
                {
                    iCurMsgID += 1;
                    TraceLogger.Write("NewMessageID: " + iCurMsgID);

                    WindowOutput win = new WindowOutput();

                    for (int w = 0; w < 5; w++)
                    {
                        if (PluginCore.getTargetList(w, e.Color))
                        {
                            TraceLogger.Write("MsgID: " + iCurMsgID + " - Default Window: " + w);
                            if (w == 0)
                            { win.baseMainWindow = true; }
                            else if (w == 1)
                            { win.baseWindow1 = true; }
                            else if (w == 2)
                            { win.baseWindow2 = true; }
                            else if (w == 3)
                            { win.baseWindow3 = true; }
                            else if (w == 4)
                            { win.baseWindow4 = true; }
                        }
                    }
                    TraceLogger.Write("MsgID: " + iCurMsgID + " - Default Window String: " + win);
                    MessageArgs tmp = new MessageArgs();
                    tmp.Color = e.Color;
                    tmp.Text = e.Text;
                    tmp.Type = e.Color;
                    tmp.Target = win;
                    e.Eat = internalHandleMessage(tmp, perfid);
                }
                else { TraceLogger.Write("No plugin's registered or no registered plugin's are hooking into the proper event"); }
                TraceLogger.Write("*END CHATBOX MESSAGE SERIES*");
                finishPerfMsg(perfid);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void internalWTCMessage(MessageArgs e)
        {
            try
            {
                int perfid = startPerfMsg(e.Text);
                TraceLogger.Write("*START WRITETOCHAT MESSAGE SERIES*");
                TraceLogger.Write("Enter C:" + e.Color + ", Typ:" + e.Type + " Tar:" + e.Target.ToString() + " Txt:" + e.Text);
                if (!PluginCore.initComplete)
                {
                    TraceLogger.Write("Exit, PluginInit Incomplete C:" + e.Color + " Txt:" + e.Text);
                    finishPerfMsg(perfid);
                    return;
                }

                if (Plugins.Count > 0 && ChatBoxMessage != null)
                {
                    iCurMsgID += 1;
                    TraceLogger.Write("NewMessageID: " + iCurMsgID);

                    internalHandleMessage(e, perfid);
                }
                else 
                { 
                    TraceLogger.Write("No plugin's registered or no registered plugin's are hooking into the proper event.  Writing to Chat with provided info.");
                    if (e.Target.baseMainWindow)
                    {
                        TraceLogger.Write("Outputting to Main");
                        PluginCore.WriteToChatRaw(e.Text, e.Color, 1);
                    }
                    if (e.Target.baseMainWindow)
                    {
                        TraceLogger.Write("Outputting to 1");
                        PluginCore.WriteToChatRaw(e.Text, e.Color, 2);
                    }
                    if (e.Target.baseMainWindow)
                    {
                        TraceLogger.Write("Outputting to 2");
                        PluginCore.WriteToChatRaw(e.Text, e.Color, 3);
                    }
                    if (e.Target.baseMainWindow)
                    {
                        TraceLogger.Write("Outputting to 3");
                        PluginCore.WriteToChatRaw(e.Text, e.Color, 4);
                    }
                    if (e.Target.baseMainWindow)
                    {
                        TraceLogger.Write("Outputting to 4");
                        PluginCore.WriteToChatRaw(e.Text, e.Color, 5);
                    }
                }
                TraceLogger.Write("*END WRITETOCHAT MESSAGE SERIES*");
                finishPerfMsg(perfid);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static bool internalHandleMessage(MessageArgs e, int perfid)
        {
            try
            {
                bool Eat = false;

                ChatBoxMessageEventArgs ea;
                //object[] args;
                MessageData newMessageData = new MessageData();
                newMessageData.iMessageID = iCurMsgID;
                newMessageData.MessageArgs = e;
                newMessageData.DefaultWindows = e.Target;
                foreach (Delegate d in ChatBoxMessage.GetInvocationList())
                {
                    checkPerfMsg(perfid, "Event Loop");
                    TraceLogger.Write("MsgID: " + iCurMsgID + " - Event Loop Start");
                    ea = new ChatBoxMessageEventArgs(iCurMsgID, e.Text, e.Color, e.Type, e.Target.Clone());
                    TraceLogger.Write("MsgID: " + iCurMsgID + " - Raise Event ChatBoxMessage");
                    d.DynamicInvoke(new object[] { ea });
                    if (!ea.basePluginIDSet || ea.basePluginID == null)
                    {
                        TraceLogger.Write("MsgID: " + iCurMsgID + " - Plugin did not provide their ZChatID, not processing changes");
                        continue;
                    }
                    TraceLogger.Write("MsgID: " + iCurMsgID + " - ZPluginID: " + ea.basePluginID.ID.ToString() + ", ColorChanged: " + ea.colorChanged.ToString() + ", WindowChanged: " + ea.Window.windowsChanged);
                    if (!ea.basePluginID.isValid())
                    {
                        TraceLogger.Write("MsgID: " + iCurMsgID + " - Invalid ZChatID! Use ZChatSystem.Service.RegisterPlugin to create a ZChatID Token.");
                        continue;
                    }
                    if (!Plugins.ContainsKey(ea.basePluginID.ID))
                    {
                        TraceLogger.Write("MsgID: " + iCurMsgID + " - ZChatID not present! Outdated or invalid Token provided.");
                        continue;
                    }

                    if (ea.colorChanged)
                    {
                        TraceLogger.Write("MsgID: " + iCurMsgID + " - Color Entered, Plugin: " + Plugins[ea.basePluginID.ID] + ", Color: " + ea.baseColor);
                        newMessageData.newColor[ea.basePluginID.ID] = ea.baseColor;
                    }
                    if (ea.Window.windowsChanged)
                    {
                        TraceLogger.Write("MsgID: " + iCurMsgID + " - Window Entered, Plugin: " + Plugins[ea.basePluginID.ID] + ", Window: " + ea.baseWindow);
                        newMessageData.newWindow[ea.basePluginID.ID] = ea.baseWindow;
                    }
                    TraceLogger.Write("MsgID: " + iCurMsgID + " - Text Modification Count: " + ea.ModMessageList.Count );
                    ModEntry me;
                    checkPerfMsg(perfid, "Pre MsgListConstruction");
                    foreach (MsgChange mc in ea.ModMessageList)
                    {
                        TraceLogger.Write("MsgID: " + iCurMsgID + " - Text Change, Plugin: " + Plugins[ea.basePluginID.ID] + ", ChangeType: " + mc.ChangeType + ", NewText: " + mc.NewText + ", Find: " + mc.Find);
                        me = new ModEntry();
                        me.Plugin = ea.basePluginID.ID;
                        me.ChangeType = mc.ChangeType;
                        me.NewText = mc.NewText;
                        me.Find = mc.Find;
                        newMessageData.ModMessageList.Add(me);
                    }
                    checkPerfMsg(perfid, "PostMsgListConstruction");

                    TraceLogger.Write("MsgID: " + iCurMsgID + " - Event Loop Complete, Plugin Handled: " + Plugins[ea.basePluginID.ID]);
                }
                checkPerfMsg(perfid, "Pre FinalizeMessage");
                Eat = FinalizeMessage(newMessageData);
                checkPerfMsg(perfid, "Post FinalizeMessage");
                TraceLogger.Write("MsgID: " + iCurMsgID + " - Eat: " + Eat);
                TraceLogger.Write("MsgID: " + iCurMsgID + " - Exit");
                return Eat;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
                return false;
            }
        }
                
        private static bool FinalizeMessage(MessageData msg)
        {
            try
            {
                int msgID = msg.iMessageID;
                TraceLogger.Write("MsgID: " + msgID + " - Enter");
                ChatTextCompleteEventArgs tmp = new ChatTextCompleteEventArgs();
                bool eat;
                tmp.Type = msg.MessageArgs.Type;
                if (tmp.Type >= 100)
                {
                    eat = true;
                }
                else
                {
                    eat = false;
                }
                tmp.MessageID = msgID;
                tmp.Color = msg.MessageArgs.Color;
                TraceLogger.Write("MsgID: " + msgID + " - Determining Color");
                if (msg.newColor.Count > 0)
                {
                    TraceLogger.Write("MsgID: " + msgID + " - Has newColor Entry");
                    foreach (OrderData x in ColorOrder)
                    {
                        if (x.id != Guid.Empty)
                        {
                            if (x.enabled)
                            {
                                if (msg.newColor.ContainsKey(x.id))
                                {
                                    tmp.Color = msg.newColor[x.id];
                                    eat = true;
                                    TraceLogger.Write("MsgID: " + msgID + " - New Color Selected, Plugin Name: " + x.plugin + ", newColor: " + tmp.Color);
                                    break;
                                }
                            }
                            else
                            {
                                TraceLogger.Write("MsgID: " + msgID + " - Skipping Disabled Plugin, Plugin Name: " + x.plugin);
                            }
                        }
                        else
                        {
                            TraceLogger.Write("MsgID: " + msgID + " - Skipping Non-existent Plugin, Plugin Name: " + x.plugin);
                        }
                    }
                }
                else
                {
                    TraceLogger.Write("MsgID: " + msgID + " - No newColor Entries. Using existing color: " + tmp.Color);
                }
                TraceLogger.Write("MsgID: " + msgID + " - Finished Determining Color");

                TraceLogger.Write("MsgID: " + msgID + " - Determining Window");
                tmp.Window = new WindowOutputComplete();
                tmp.Window.MainWindow = msg.DefaultWindows.baseMainWindow;
                tmp.Window.Window1 = msg.DefaultWindows.baseWindow1;
                tmp.Window.Window2 = msg.DefaultWindows.baseWindow2;
                tmp.Window.Window3 = msg.DefaultWindows.baseWindow3;
                tmp.Window.Window4 = msg.DefaultWindows.baseWindow4;
                if (msg.newWindow.Count > 0)
                {
                    TraceLogger.Write("MsgID: " + msgID + " - Has newWindow Entry");
                    foreach (OrderData x in WindowOrder)
                    {
                        if (!x.id.Equals(Guid.Empty))
                        {
                            if (x.enabled)
                            {
                                if (msg.newWindow.ContainsKey(x.id))
                                {
                                    tmp.Window.MainWindow = msg.newWindow[x.id].baseMainWindow;
                                    tmp.Window.Window1 = msg.newWindow[x.id].baseWindow1;
                                    tmp.Window.Window2 = msg.newWindow[x.id].baseWindow2;
                                    tmp.Window.Window3 = msg.newWindow[x.id].baseWindow3;
                                    tmp.Window.Window4 = msg.newWindow[x.id].baseWindow4;
                                    eat = true;
                                    TraceLogger.Write("MsgID: " + msgID + " - New Window Selected, Plugin Name: " + x.plugin + ", newWindow: " + tmp.Window);
                                    break;
                                }
                            }
                            else
                            {
                                TraceLogger.Write("MsgID: " + msgID + " - Skipping Disabled Plugin, Plugin Name: " + x.plugin);
                            }
                        }
                        else
                        {
                            TraceLogger.Write("MsgID: " + msgID + " - Skipping Non-existent Plugin, Plugin Name: " + x.plugin);
                        }
                    }
                }
                else
                {
                    TraceLogger.Write("MsgID: " + msgID + " - No newWindow Entries. Using default windows");
                }
                TraceLogger.Write("MsgID: " + msgID + " - Finished Determining Window");


                tmp.Text = msg.MessageArgs.Text;
                List<string> replaceList = new List<string>();
                List<string> replaceList2 = new List<string>();
                TraceLogger.Write("MsgID: " + msgID + " - Determining Message");
                if (msg.ModMessageList.Count > 0)
                {
                    TraceLogger.Write("MsgID: " + msgID + " - Has Message Changes");
                    bool globalend = false;
                    foreach (OrderData y in MessageOrder)
                    {
                        if (globalend)
                            break;
                        if (!y.id.Equals(Guid.Empty))
                        {
                            if (y.enabled)
                            {
                                foreach (ModEntry me in msg.ModMessageList)
                                {
                                    if (globalend)
                                        break;
                                    if (me.Plugin == y.id)
                                    {
                                        eat = true;
                                        switch (me.ChangeType)
                                        {
                                            case MsgModType.MESSAGE_SUBSTITUTE:
                                                tmp.Text = me.NewText;
                                                globalend = true;
                                                TraceLogger.Write("MsgID: " + msgID + " - Plugin Name: " + Plugins[y.id] + ", Substituting Message: " + tmp.Text);
                                                TraceLogger.Write("MsgID: " + msgID + " - Global Break");
                                                break;
                                            case MsgModType.MESSAGE_APPEND:
                                                tmp.Text = tmp.Text.Replace(Convert.ToChar(10).ToString(), "") + me.NewText + Convert.ToChar(10);
                                                TraceLogger.Write("MsgID: " + msgID + " - Plugin Name: " + Plugins[y.id] + ", Appending Message: " + me.NewText);
                                                break;
                                            case MsgModType.MESSAGE_PREPEND:
                                                tmp.Text = me.NewText + tmp.Text;
                                                TraceLogger.Write("MsgID: " + msgID + " - Plugin Name: " + Plugins[y.id] + ", Prepending Message: " + me.NewText);
                                                break;
                                            case MsgModType.MESSAGE_REPLACE:
                                                //StringBuilder b = new StringBuilder(tmp.Text);
                                                //tmp.Text = b.Replace(me.Find, me.NewText).ToString();
                                                if (replaceList.Exists(delegate(string comp)
                                                {
                                                    return comp.Contains(me.Find);
                                                }) || replaceList2.Exists(delegate(string comp)
                                                {
                                                    return comp.Contains(me.Find);
                                                }))
                                                {
                                                    TraceLogger.Write("MsgID: " + msgID + " - Plugin Name: " + Plugins[y.id] + ", Replace string already handled: " + me.Find);
                                                }
                                                else
                                                {
                                                    tmp.Text = tmp.Text.Replace(me.Find, me.NewText);
                                                    replaceList.Add(me.Find);
                                                    replaceList2.Add(me.NewText);
                                                    TraceLogger.Write("MsgID: " + msgID + " - Plugin Name: " + Plugins[y.id] + ", Replacing Message: " + me.Find + " with " + me.NewText);
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TraceLogger.Write("MsgID: " + msgID + " - Skipping Disabled Plugin, Plugin Name: " + y.plugin);
                            }
                        }
                        else
                        {
                            TraceLogger.Write("MsgID: " + msgID + " - Skipping Non-existent Plugin, Plugin Name: " + y.plugin);
                        }
                    }
                }
                else
                {
                    TraceLogger.Write("MsgID: " + msgID + " - No modMessage Entries. Using existing message: " + tmp.Text);
                }
                replaceList = null;
                replaceList2 = null;
                TraceLogger.Write("MsgID: " + msgID + " - Finished Determining Message");

                TraceLogger.Write("MsgID: " + msgID + " - Final Output - C:" + tmp.Color + " T:" + tmp.Window + " M:" + tmp.Text);

                if (eat)
                {
                    TraceLogger.Write("MsgID: " + msgID + " - Changes Enforced, Writing Output");
                    if (tmp.Window.MainWindow)
                    {
                        TraceLogger.Write("MsgID: " + msgID + " - Outputting to Main");
                        PluginCore.WriteToChatRaw(tmp.Text, tmp.Color, 1);
                    }
                    if (tmp.Window.Window1)
                    {
                        TraceLogger.Write("MsgID: " + msgID + " - Outputting to 1");
                        PluginCore.WriteToChatRaw(tmp.Text, tmp.Color, 2);
                    }
                    if (tmp.Window.Window2)
                    {
                        TraceLogger.Write("MsgID: " + msgID + " - Outputting to 2");
                        PluginCore.WriteToChatRaw(tmp.Text, tmp.Color, 3);
                    }
                    if (tmp.Window.Window3)
                    {
                        TraceLogger.Write("MsgID: " + msgID + " - Outputting to 3");
                        PluginCore.WriteToChatRaw(tmp.Text, tmp.Color, 4);
                    }
                    if (tmp.Window.Window4)
                    {
                        TraceLogger.Write("MsgID: " + msgID + " - Outputting to 4");
                        PluginCore.WriteToChatRaw(tmp.Text, tmp.Color, 5);
                    }
                }
                else
                {
                    TraceLogger.Write("MsgID: " + msgID + " - No changes, passing on");
                }
                TraceLogger.Write("MsgID: " + msgID + " - Raise Event ChatTextComplete");
                if(ChatTextComplete != null)
                    ChatTextComplete.Invoke(tmp);
                msg = null;
                TraceLogger.Write("MsgID: " + msgID + " - Exit");
                return eat;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
                return false;
            }
        }       

        static void MainView_ListChange(string list, string plugin, string dir)
        {
            TraceLogger.Write("Enter - list: " + list + ", plugin: " + plugin + ", dir: " + dir);
            try
            {
                int index;
                OrderData temp;
                switch (list)
                {
                    case "message":
                        TraceLogger.Write("Begin Processing Message List Change");
                        switch (dir)
                        {
                            case "up":
                                TraceLogger.Write("Begin move plugin " + plugin + " up");
                                index = MessageOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.plugin == plugin;
                                    });
                                TraceLogger.Write("Index: " + index);
                                if (index == 0)
                                    break;
                                temp = MessageOrder[index];
                                MessageOrder.Insert(index - 1, temp);
                                MessageOrder.RemoveAt(index + 1);
                                TraceLogger.Write("Finish move plugin " + plugin + " up");
                                break;
                            case "down":
                                TraceLogger.Write("Begin move plugin " + plugin + " down");
                                index = MessageOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.plugin == plugin;
                                    });
                                if (index == MessageOrder.Count - 1)
                                    break;
                                TraceLogger.Write("Index: " + index);
                                temp = MessageOrder[index];
                                MessageOrder.Insert(index + 2, temp);
                                MessageOrder.RemoveAt(index);
                                TraceLogger.Write("Finish move plugin " + plugin + " down");
                                break;
                            case "enable":
                                TraceLogger.Write("Begin enable plugin " + plugin);
                                index = MessageOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.plugin == plugin;
                                    });
                                TraceLogger.Write("Index: " + index);
                                MessageOrder[index].enabled = true;
                                TraceLogger.Write("Finish enable plugin " + plugin);
                                break;
                            case "disable":
                                TraceLogger.Write("Begin disable plugin " + plugin);
                                index = MessageOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.plugin == plugin;
                                    });
                                TraceLogger.Write("Index: " + index);
                                MessageOrder[index].enabled = false;
                                TraceLogger.Write("Finish disable plugin " + plugin);
                                break;
                        }
                        updateMessageView();
                        TraceLogger.Write("Finish Processing Message List Change");
                        break;
                    case "color":
                        TraceLogger.Write("Begin Processing Color List Change");
                        switch (dir)
                        {
                            case "up":
                                TraceLogger.Write("Begin move plugin " + plugin + " up");
                                index = ColorOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.id == PluginsName[plugin];
                                    });
                                TraceLogger.Write("Index: " + index);
                                if (index == 0)
                                    break;
                                temp = ColorOrder[index];
                                ColorOrder.Insert(index - 1, temp);
                                ColorOrder.RemoveAt(index + 1);
                                TraceLogger.Write("Finish move plugin " + plugin + " up");
                                break;
                            case "down":
                                TraceLogger.Write("Begin move plugin " + plugin + " down");
                                index = ColorOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.id == PluginsName[plugin];
                                    });
                                TraceLogger.Write("Index: " + index);
                                if (index == ColorOrder.Count - 1)
                                    break;
                                temp = ColorOrder[index];
                                ColorOrder.Insert(index + 2, temp);
                                ColorOrder.RemoveAt(index);
                                TraceLogger.Write("Finish move plugin " + plugin + " down");
                                break;
                            case "enable":
                                TraceLogger.Write("Begin enable plugin " + plugin);
                                index = ColorOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.id == PluginsName[plugin];
                                    });
                                TraceLogger.Write("Index: " + index);
                                ColorOrder[index].enabled = true;
                                TraceLogger.Write("Finish enable plugin " + plugin);
                                break;
                            case "disable":
                                TraceLogger.Write("Begin disable plugin " + plugin);
                                index = ColorOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.id == PluginsName[plugin];
                                    });
                                TraceLogger.Write("Index: " + index);
                                ColorOrder[index].enabled = false;
                                TraceLogger.Write("Finish disable plugin " + plugin);
                                break;
                        }
                        updateColorView();
                        TraceLogger.Write("Finish Processing Color List Change");
                        break;
                    case "window":
                        TraceLogger.Write("Begin Processing Window List Change");
                        switch (dir)
                        {
                            case "up":
                                TraceLogger.Write("Begin move plugin " + plugin + " up");
                                index = WindowOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.id == PluginsName[plugin];
                                    });
                                TraceLogger.Write("Index: " + index);
                                if (index == 0)
                                    break;
                                temp = WindowOrder[index];
                                WindowOrder.Insert(index - 1, temp);
                                WindowOrder.RemoveAt(index + 1);
                                TraceLogger.Write("Finish move plugin " + plugin + " up");
                                break;
                            case "down":
                                TraceLogger.Write("Begin move plugin " + plugin + " down");
                                index = WindowOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.id == PluginsName[plugin];
                                    });
                                TraceLogger.Write("Index: " + index);
                                if (index == WindowOrder.Count - 1)
                                    break;
                                temp = WindowOrder[index];
                                WindowOrder.Insert(index + 2, temp);
                                WindowOrder.RemoveAt(index);
                                TraceLogger.Write("Finish move plugin " + plugin + " down");
                                break;
                            case "enable":
                                TraceLogger.Write("Begin enable plugin " + plugin);
                                index = WindowOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.id == PluginsName[plugin];
                                    });
                                TraceLogger.Write("Index: " + index);
                                WindowOrder[index].enabled = true;
                                TraceLogger.Write("Finish enable plugin " + plugin);
                                break;
                            case "disable":
                                TraceLogger.Write("Begin disable plugin " + plugin);
                                index = WindowOrder.FindIndex(
                                    delegate(OrderData z)
                                    {
                                        return z.id == PluginsName[plugin];
                                    });
                                TraceLogger.Write("Index: " + index);
                                WindowOrder[index].enabled = false;
                                TraceLogger.Write("Finish disable plugin " + plugin);
                                break;
                        }
                        updateWindowView();
                        TraceLogger.Write("Finish Processing Window List Change");
                        break;
                }
                PluginCore.WriteSettings();
                TraceLogger.Write("Exit - list: " + list + ", plugin: " + plugin + ", dir: " + dir);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void updateMessageView()
        {
            try
            {
                TraceLogger.Write("Enter");
                MainView.Message_List_Clear();
                foreach (OrderData z in MessageOrder)
                {
                    MainView.Message_List_Add(z.plugin, z.enabled);
                }
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void updateColorView()
        {
            try
            {
                TraceLogger.Write("Enter");
                MainView.Color_List_Clear();
                foreach (OrderData z in ColorOrder)
                {
                    MainView.Color_List_Add(z.plugin, z.enabled);
                }
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void updateWindowView()
        {
            try
            {
                TraceLogger.Write("Enter");
                MainView.Window_List_Clear();
                foreach (OrderData z in WindowOrder)
                {
                    MainView.Window_List_Add(z.plugin, z.enabled);
                }
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }
    }
}