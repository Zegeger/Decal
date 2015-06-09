using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Zegeger.Decal.Plugins.ZChatSystem;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Chat
{
    public delegate void ChatBoxMessageEvent(ChatBoxMessageEventArgs e);
    public delegate void ChatTextCompleteEvent(ChatTextCompleteEventArgs e);

    public enum ZChatType{
        None,
        Decal,
        ZChatSystem
    }

    public enum MsgModType
    {
        MESSAGE_REPLACE, MESSAGE_PREPEND, MESSAGE_APPEND, MESSAGE_SUBSTITUTE
    }

    public class ChatBoxMessageEventArgs : EventArgs
    {
        public int MessageID
        {
            get { return baseMessageID; }
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
        internal bool basePluginIDSet;

        internal int baseColor;
        internal bool colorChanged;
        internal string baseText;
        internal int baseType;

        internal int priority;

        internal List<MsgChange> ModMessageList;

        public ChatBoxMessageEventArgs()
        {
            baseMessageID = -1;
            basePluginIDSet = false;
            priority = 50;
        }

        internal ChatBoxMessageEventArgs(int MsgID, string MsgText, int MsgColor, int MsgType, WindowOutput MsgWindow)
        {
            try
            {
                baseMessageID = MsgID;
                
                baseText = MsgText;
                ModMessageList = new List<MsgChange>();

                baseColor = MsgColor;
                colorChanged = false;

                baseType = MsgType;
                baseWindow = MsgWindow;

                basePluginIDSet = false;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public void SetPriority(int priorityValue)
        {
            priority = priorityValue;
        }

        public void ReplaceMessage(string Find, string Replace)
        {
            try
            {
                MsgChange t = new MsgChange();
                t.ChangeType = MsgModType.MESSAGE_REPLACE;
                t.Find = Find;
                t.NewText = Replace;
                ModMessageList.Add(t);
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
                MsgChange t = new MsgChange();
                t.ChangeType = MsgModType.MESSAGE_SUBSTITUTE;
                t.NewText = NewMessage;
                ModMessageList.Add(t);
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
                MsgChange t = new MsgChange();
                t.ChangeType = MsgModType.MESSAGE_APPEND;
                t.NewText = AppendText;
                ModMessageList.Add(t);
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
                MsgChange t = new MsgChange();
                t.ChangeType = MsgModType.MESSAGE_PREPEND;
                t.NewText = PrependText;
                ModMessageList.Add(t);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
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
            if (MainWindow != Main)
                MainWindow = Main;
            if (Window1 != W1)
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

        public bool IsShown
        {
            get
            {
                return baseMainWindow || baseWindow1 || baseWindow2 || baseWindow3 || baseWindow4;
            }
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

    public class MsgChange
    {
        internal MsgModType ChangeType { get; set; }
        internal string NewText { get; set; }
        internal string Find { get; set; }
    }

    public class ChatType
    {
        public int Value { get; private set; }
        
        internal ChatType(int TypeNumber)
        {
            Value = TypeNumber;
        }
    }

    public class TypeData
    {
        internal  int Color { get; set; }
        internal WindowOutput Target { get; set; }

        internal TypeData(int inColor, bool MainWindow, bool Window1, bool Window2, bool Window3, bool Window4)
        {
            Color = inColor;
            Target = new WindowOutput(MainWindow, Window1, Window2, Window3, Window4);
        }
    }

    public static class ZChatWrapper
    {

        public static event ChatTextCompleteEvent ChatTextComplete;
        public static event ChatBoxMessageEvent ChatBoxMessage;

        public static ZChatType ChatMode {get; private set;}
        private static IDToken ZChatID {get; set;}

        private static CoreManager Core;
        //private static Decal.Adapter.Wrappers.PluginHost Host;

        private static int CurrentType;
        private static Dictionary<int, TypeData> TypeList;
        private static Dictionary<int, Zegeger.Decal.Plugins.ZChatSystem.ChatType> ZTypeList;

        static ZChatWrapper()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ChatMode = ZChatType.None;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static void Initialize(CoreManager pCore, string PluginName)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                Core = pCore;
                //Host = pHost;

                CurrentType = 100;

                TypeList = new Dictionary<int, TypeData>();

                if (ZChatSystemPresent())
                {
                    IDToken temp;
                    temp = ZChatSystem.RegisterPlugin(PluginName);
                    if (temp != null)
                    {
                        TraceLogger.Write("Using ZChatSystem", TraceLevel.Info);
                        ZChatID = temp;
                        ChatMode = ZChatType.ZChatSystem;
                        ZTypeList = new Dictionary<int, Zegeger.Decal.Plugins.ZChatSystem.ChatType>();
                        ZChatSystem.ChatBoxMessage += new Zegeger.Decal.Plugins.ZChatSystem.ChatBoxMessageEvent(Service_ChatBoxMessage);
                        ZChatSystem.ChatTextComplete += new Zegeger.Decal.Plugins.ZChatSystem.ChatTextCompleteEvent(Service_ChatTextComplete);
                    }
                    else
                    {
                        TraceLogger.Write("Using Decal Chat", TraceLevel.Info);
                        ChatMode = ZChatType.Decal;
                        Core.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(pCore_ChatBoxMessage);
                    }
                }
                else
                {
                    TraceLogger.Write("Using Decal Chat", TraceLevel.Info);
                    ChatMode = ZChatType.Decal;
                    Core.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(pCore_ChatBoxMessage);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static void Dispose()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    ZTypeList.Clear();
                    ZChatSystem.ChatBoxMessage -= new Zegeger.Decal.Plugins.ZChatSystem.ChatBoxMessageEvent(Service_ChatBoxMessage);
                    ZChatSystem.ChatTextComplete -= new Zegeger.Decal.Plugins.ZChatSystem.ChatTextCompleteEvent(Service_ChatTextComplete);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(pCore_ChatBoxMessage);
                }
                Core = null;
                TypeList.Clear();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static ChatType CreateChatType(int Color, bool MainWindow, bool Window1, bool Window2, bool Window3, bool Window4)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            try
            {
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    Zegeger.Decal.Plugins.ZChatSystem.ChatType temp = ZChatSystem.CreateChatType(ZChatID, Color, MainWindow, Window1, Window2, Window3, Window4);
                    ZTypeList.Add(temp.Value, temp);
                    TraceLogger.Write("Exit returning value for ZChatSystem", TraceLevel.Verbose);
                    return new ChatType(temp.Value);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    CurrentType += 1;
                    TypeData temp = new TypeData(Color, MainWindow, Window1, Window2, Window3, Window4);
                    TypeList.Add(CurrentType, temp);
                    TraceLogger.Write("Exit returning value for Decal Chat", TraceLevel.Verbose);
                    return new ChatType(CurrentType);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit return null", TraceLevel.Verbose);
            return null;
        }

        public static void WriteToChat(string Text, int Color)
        {
            try
            {
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    ZChatSystem.WriteToChat(Text, Color);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    Core.Actions.AddChatText(Text, Color);
                }
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
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    ZChatSystem.WriteToChat(Text, Color, Target);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    Core.Actions.AddChatText(Text, Color, Target);
                }
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
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    Zegeger.Decal.Plugins.ZChatSystem.WindowOutput temp = new Zegeger.Decal.Plugins.ZChatSystem.WindowOutput(Target.baseMainWindow, Target.baseWindow1, Target.baseWindow2, Target.baseWindow3, Target.baseWindow4);
                    ZChatSystem.WriteToChat(Text, Color, temp);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    WriteToWindows(Text, Color, Target);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, ChatType ChatType)
        {
            try
            {
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    ZChatSystem.WriteToChat(Text, ZTypeList[ChatType.Value]);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    WriteToWindows(Text, TypeList[ChatType.Value].Color, TypeList[ChatType.Value].Target);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, WindowOutput Target, ChatType ChatType)
        {
            try
            {
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    Zegeger.Decal.Plugins.ZChatSystem.WindowOutput temp = new Zegeger.Decal.Plugins.ZChatSystem.WindowOutput(Target.baseMainWindow, Target.baseWindow1, Target.baseWindow2, Target.baseWindow3, Target.baseWindow4);
                    ZChatSystem.WriteToChat(Text, temp, ZTypeList[ChatType.Value]);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    WriteToWindows(Text, TypeList[ChatType.Value].Color, Target);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, int Color, ChatType ChatType)
        {
            try
            {
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    ZChatSystem.WriteToChat(Text, Color, ZTypeList[ChatType.Value]);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    WriteToWindows(Text, Color, TypeList[ChatType.Value].Target);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, int Color, int Target, ChatType ChatType)
        {
            try
            {
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    ZChatSystem.WriteToChat(Text, Color, Target, ZTypeList[ChatType.Value]);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    Core.Actions.AddChatText(Text, Color, Target);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public static void WriteToChat(string Text, int Color, WindowOutput Target, ChatType ChatType)
        {
            try
            {
                if (ChatMode == ZChatType.ZChatSystem)
                {
                    Zegeger.Decal.Plugins.ZChatSystem.WindowOutput temp = new Zegeger.Decal.Plugins.ZChatSystem.WindowOutput(Target.baseMainWindow, Target.baseWindow1, Target.baseWindow2, Target.baseWindow3, Target.baseWindow4);
                    ZChatSystem.WriteToChat(Text, Color, temp, ZTypeList[ChatType.Value]);
                }
                else if (ChatMode == ZChatType.Decal)
                {
                    WriteToWindows(Text, Color, Target);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        private static void WriteToWindows(string Text, int Color, WindowOutput Target)
        {
            try
            {
                if (Target.baseMainWindow)
                {
                    Core.Actions.AddChatText(Text, Color, 1);
                }
                if (Target.baseWindow1)
                {
                    Core.Actions.AddChatText(Text, Color, 2);
                }
                if (Target.baseWindow2)
                {
                    Core.Actions.AddChatText(Text, Color, 3);
                }
                if (Target.baseWindow3)
                {
                    Core.Actions.AddChatText(Text, Color, 4);
                }
                if (Target.baseWindow4)
                {
                    Core.Actions.AddChatText(Text, Color, 5);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        private static void pCore_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                bool eat = false;
                string newText = e.Text;
                int newColor = e.Color;
                WindowOutput newWindow = new WindowOutput();
                TraceLogger.Write("Initial Text: " + newText + ", Color: " + newColor + ", Window: " + newWindow.ToString(), TraceLevel.Verbose);
                bool windowChanged = false;
                if (ChatBoxMessage != null)
                {
                    int colorPriority = -1;
                    int windowPriority = -1;
                    foreach (Delegate d in ChatBoxMessage.GetInvocationList())
                    {
                        bool globalend = false;
                        ChatBoxMessageEventArgs args = new ChatBoxMessageEventArgs(-1, e.Text, e.Color, e.Color, new WindowOutput(false, false, false, false, false));
                        TraceLogger.Write("Invoking " + d.Method.Name, TraceLevel.Verbose);
                        d.DynamicInvoke(new object[] { args });

                        if (args.colorChanged || args.Window.windowsChanged || args.ModMessageList.Count > 0)
                        {
                            eat = true;
                        }
                        if (args.colorChanged)
                        {
                            if (args.priority >= colorPriority)
                            {
                                newColor = args.Color;
                                colorPriority = args.priority;
                            }
                        }
                        if (args.Window.windowsChanged)
                        {
                            if (args.priority >= windowPriority)
                            {
                                windowChanged = true;
                                newWindow = args.Window;
                                windowPriority = args.priority;
                            }
                        }
                        foreach (MsgChange mc in args.ModMessageList)
                        {
                            if (globalend)
                                break;
                            switch (mc.ChangeType)
                            {
                                case MsgModType.MESSAGE_SUBSTITUTE:
                                    newText = mc.NewText;
                                    globalend = true;
                                    break;
                                case MsgModType.MESSAGE_APPEND:
                                    newText = newText.Replace(Convert.ToChar(10).ToString(), "") + mc.NewText + Convert.ToChar(10);
                                    break;
                                case MsgModType.MESSAGE_PREPEND:
                                    newText = mc.NewText + newText;
                                    break;
                                case MsgModType.MESSAGE_REPLACE:
                                    newText = newText.Replace(mc.Find, mc.NewText);
                                    break;
                            }
                        }
                    }
                }
                TraceLogger.Write("Final result.  Text: " + newText + ", Color: " + newColor + ", Window" + newWindow.ToString(), TraceLevel.Verbose);
                if (eat)
                {
                    if (windowChanged)
                    {
                        if (newWindow.MainWindow)
                        {
                            Core.Actions.AddChatText(newText, newColor, 1);
                        }
                        if (newWindow.Window1)
                        {
                            Core.Actions.AddChatText(newText, newColor, 2);
                        }
                        if (newWindow.Window2)
                        {
                            Core.Actions.AddChatText(newText, newColor, 3);
                        }
                        if (newWindow.Window3)
                        {
                            Core.Actions.AddChatText(newText, newColor, 4);
                        }
                        if (newWindow.Window4)
                        {
                            Core.Actions.AddChatText(newText, newColor, 5);
                        }
                    }
                    else
                    {
                        Core.Actions.AddChatText(newText, newColor);
                    }
                    e.Eat = true;
                }
                WindowOutputComplete newWO = new WindowOutputComplete();
                newWO.MainWindow = newWindow.MainWindow;
                newWO.Window1 = newWindow.Window1;
                newWO.Window2 = newWindow.Window2;
                newWO.Window3 = newWindow.Window3;
                newWO.Window4 = newWindow.Window4;

                ChatTextCompleteEventArgs args2 = new ChatTextCompleteEventArgs();
                args2.Color = newColor;
                args2.Text = newText;
                args2.Type = e.Color;
                args2.MessageID = -1;
                args2.Window = newWO;

                if(ChatTextComplete != null)
                    ChatTextComplete.Invoke(args2);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private static void Service_ChatBoxMessage(Zegeger.Decal.Plugins.ZChatSystem.ChatBoxMessageEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                e.PluginID = ZChatID;
                if (ChatBoxMessage != null)
                {
                    int newColor = e.Color;
                    WindowOutput newWindow = null;
                    int colorPriority = -1;
                    int windowPriority = -1;
                    foreach (Delegate d in ChatBoxMessage.GetInvocationList())
                    {
                        WindowOutput newWO = new WindowOutput(e.Window.MainWindow, e.Window.Window1, e.Window.Window2, e.Window.Window3, e.Window.Window4);
                        ChatBoxMessageEventArgs args = new ChatBoxMessageEventArgs(e.MessageID, e.Text, e.Color, e.Type, newWO);
                        TraceLogger.Write("Invoking " + d.Method.Name, TraceLevel.Noise);
                        d.DynamicInvoke(new object[] { args });
                        if (args.colorChanged)
                        {
                            if (args.priority >= colorPriority)
                            {
                                newColor = args.baseColor;
                                colorPriority = args.priority;
                            }
                        }
                        if (args.Window.windowsChanged)
                        {
                            if (args.priority >= windowPriority)
                            {
                                newWindow = args.baseWindow;
                                windowPriority = args.priority;
                            }
                        }
                        foreach (MsgChange me in args.ModMessageList)
                        {
                            switch (me.ChangeType)
                            {
                                case MsgModType.MESSAGE_SUBSTITUTE:
                                    e.SubstituteMessage(me.NewText);
                                    break;
                                case MsgModType.MESSAGE_APPEND:
                                    e.AppendMessage(me.NewText);
                                    break;
                                case MsgModType.MESSAGE_PREPEND:
                                    e.PrependMessage(me.NewText);
                                    break;
                                case MsgModType.MESSAGE_REPLACE:
                                    e.ReplaceMessage(me.Find, me.NewText);
                                    break;
                            }
                        }
                    }
                    if (colorPriority > -1)
                    {
                        e.Color = newColor;
                    }
                    if (windowPriority > -1)
                    {
                        e.Window.SetWindows(newWindow.baseMainWindow, newWindow.baseWindow1, newWindow.baseWindow2, newWindow.baseWindow3, newWindow.baseWindow4);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private static void Service_ChatTextComplete(Zegeger.Decal.Plugins.ZChatSystem.ChatTextCompleteEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter, Text: " + e.Text + ", Color: " + e.Color + ", Window: " + e.Window.ToString(), TraceLevel.Verbose);
                if (ChatTextComplete != null)
                {
                    WindowOutputComplete newWO = new WindowOutputComplete();
                    newWO.MainWindow = e.Window.MainWindow;
                    newWO.Window1 = e.Window.Window1;
                    newWO.Window2 = e.Window.Window2;
                    newWO.Window3 = e.Window.Window3;
                    newWO.Window4 = e.Window.Window4;

                    ChatTextCompleteEventArgs args = new ChatTextCompleteEventArgs();
                    args.Color = e.Color;
                    args.Text = e.Text;
                    args.Type = e.Type;
                    args.MessageID = e.MessageID;
                    args.Window = newWO;

                    ChatTextComplete.Invoke(args);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private static bool ZChatSystemPresent()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                System.Reflection.Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

                foreach (System.Reflection.Assembly a in asms)
                {
                    AssemblyName nmm = a.GetName();
                    if ((nmm.Name == "ZChatSystem") && (nmm.Version >= new System.Version("0.9.0.0")))
                    {
                        try
                        {
                            TraceLogger.Write("Exit returning " + ZChatSystem.Running, TraceLevel.Verbose);
                            return ZChatSystem.Running;
                        }
                        catch
                        {
                            TraceLogger.Write("Exit returning false", TraceLevel.Verbose);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit returning false", TraceLevel.Verbose);
            return false;
        }
    }
}
