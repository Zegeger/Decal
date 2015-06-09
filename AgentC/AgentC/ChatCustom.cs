using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Decal.Adapter;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.ObjectModel;
using Zegeger.Decal.VVS;
using Zegeger.Decal.Chat;
using Zegeger.Decal.Data;
using Zegeger.Diagnostics;
using Zegeger.Decal.Controls;
using Zegeger.Audio;
using Zegeger.Analysis;

namespace Zegeger.Decal.Plugins.AgentC
{
    class ChatCustom : Component
    {
        internal const int CHECK = 0;
        internal const int TEXT = 1;
        internal const int WINDOW = 2;
        internal const int TYPE = 3;
        internal const int COLOR = 4;
        internal const int SOUND = 5;
        internal const int ACTION = 6;

        ChatCustomSettings CustomSetting;

        IList Chat_Custom;
        ITextBox Chat_Custom_Text;
        ICombo Chat_Custom_Type;
        ICombo Chat_Custom_Color;
        ICombo Chat_Custom_Sound;
        ITextBox Chat_Custom_Window;
        IButton Chat_Custom_Down;
        IButton Chat_Custom_Up;
        IButton Chat_Custom_Delete;
        IButton Chat_Custom_Add;
        IButton Chat_Custom_Edit;
        ITextBox Chat_Custom_Action;

        private DecalList ChatCustomList;
        private SoundManager ChatSounds;

        public ChatCustom(CoreManager core, IView view, string dllPath) : base (core, view)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                Critical = false;
                ComponentName = "ChatCustom";

                ChatSounds = new SoundManager(Path.Combine(dllPath, "Sounds"));

                SettingsProfileHandler.registerType(typeof(ChatCustomSettings));
                SettingsProfileHandler.registerType(typeof(ChatCustomType));

                SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);

                Chat_Custom = (IList)View["Chat_Custom"];
                Chat_Custom.Click +=new dClickedList(Chat_Custom_Click);
                Chat_Custom_Text = (ITextBox)View["Chat_Custom_Text"];
                Chat_Custom_Type = (ICombo)View["Chat_Custom_Type"];
                Chat_Custom_Color = (ICombo)View["Chat_Custom_Color"];
                Chat_Custom_Action = (ITextBox)View["Chat_Custom_Action"];
                Chat_Custom_Window = (ITextBox)View["Chat_Custom_Window"];
                Chat_Custom_Sound = (ICombo)View["Chat_Custom_Sound"];
                Chat_Custom_Sound.Change +=new EventHandler<MVIndexChangeEventArgs>(Chat_Custom_Sound_Change);
                Chat_Custom_Down = (IButton)View["Chat_Custom_Down"];
                Chat_Custom_Down.Click += new EventHandler<MVControlEventArgs>(Chat_Custom_Down_Click);
                Chat_Custom_Up = (IButton)View["Chat_Custom_Up"];
                Chat_Custom_Up.Click += new EventHandler<MVControlEventArgs>(Chat_Custom_Up_Click);
                Chat_Custom_Delete = (IButton)View["Chat_Custom_Delete"];
                Chat_Custom_Delete.Click += new EventHandler<MVControlEventArgs>(Chat_Custom_Delete_Click);
                Chat_Custom_Add = (IButton)View["Chat_Custom_Add"];
                Chat_Custom_Add.Click += new EventHandler<MVControlEventArgs>(Chat_Custom_Add_Click);
                Chat_Custom_Edit = (IButton)View["Chat_Custom_Edit"];
                Chat_Custom_Edit.Click += new EventHandler<MVControlEventArgs>(Chat_Custom_Edit_Click);

                ChatCustomList = new DecalList(Chat_Custom);
                ChatCustomList.HighlightColor = Constants.GUIColors("Highlight");
                ChatCustomList.HighlightColumn = new int[] { TEXT };
                ChatCustomList.SelectedRowChanged += new SelectedRowChangedEvent(ChatCustomList_SelectedRowChanged);

                Chat_Custom_Color.Clear();
                Chat_Custom_Color.Add("Default");
                foreach (string name in Constants.ChatColorsDictionary.Keys)
                {
                    Chat_Custom_Color.Add(name);
                }

                Chat_Custom_Sound.Clear();
                Chat_Custom_Sound.Add("None");
                foreach (string name in ChatSounds.SoundFileNames)
                {
                    Chat_Custom_Sound.Add(name);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void ModifyCustomType(ChatCustomType changeType)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                if (String.IsNullOrEmpty(Chat_Custom_Text.Text))
                {
                    TraceLogger.Write("Text is empty.", TraceLevel.Warning);
                    PluginCore.WriteToChatError("Unable to add entry because the text field is empty.");
                    return;
                }

                try
                {
                    Regex test = new Regex(Chat_Custom_Text.Text);
                    test = null;
                }
                catch (Exception ex)
                {
                    TraceLogger.Write("Regex parse failure: " + ex.Message + " for message " + Chat_Custom_Text.Text, TraceLevel.Warning);
                    PluginCore.WriteToChatError("Unable to add entry due to an issue with the text regular expression");
                    return;
                }
                
                string window = "D";
                if (!String.IsNullOrEmpty(Chat_Custom_Window.Text) && Chat_Custom_Window.Text.ToUpper().IndexOfAny(new char[] { 'D', 'B', 'M', '1', '2', '3', '4' }) != -1)
                {
                    window = Chat_Custom_Window.Text.ToUpper();
                }
                TraceLogger.Write(String.Format("Text: {0}, Window: {1}, Color: {2}, Sound: {3}, Type: {4}, Action: {5}", Chat_Custom_Text.Text, window, Chat_Custom_Color.Text[Chat_Custom_Color.Selected], Chat_Custom_Sound.Text[Chat_Custom_Sound.Selected], Chat_Custom_Type.Text[Chat_Custom_Type.Selected], Chat_Custom_Action.Text), TraceLevel.Info);
                changeType.Text = Chat_Custom_Text.Text;
                changeType.Window = window;
                changeType.Color = Chat_Custom_Color.Text[Chat_Custom_Color.Selected];
                changeType.Sound = Chat_Custom_Sound.Text[Chat_Custom_Sound.Selected];
                changeType.Type = Chat_Custom_Type.Text[Chat_Custom_Type.Selected];
                changeType.Action = Chat_Custom_Action.Text;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Chat_Custom_Edit_Click(object sender, MVControlEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                ChatCustomType changeType = CustomSetting.CustomTypes[ChatCustomList.SelectedIndex];
                ModifyCustomType(changeType);
                SettingsProfileHandler.Save();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Chat_Custom_Add_Click(object sender, MVControlEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                ChatCustomType newType = new ChatCustomType();
                newType.Enabled = true;
                ModifyCustomType(newType);
                CustomSetting.CustomTypes.Add(newType);
                SettingsProfileHandler.Save();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Chat_Custom_Delete_Click(object sender, MVControlEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                CustomSetting.CustomTypes.RemoveAt(ChatCustomList.SelectedIndex);
                SettingsProfileHandler.Save();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Chat_Custom_Up_Click(object sender, MVControlEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                if (ChatCustomList.SelectedIndex > 0)
                {
                    CustomSetting.CustomTypes.Move(ChatCustomList.SelectedIndex, ChatCustomList.SelectedIndex - 1);
                }
                SettingsProfileHandler.Save();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Chat_Custom_Down_Click(object sender, MVControlEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                if (ChatCustomList.SelectedIndex < CustomSetting.CustomTypes.Count - 1)
                {
                    CustomSetting.CustomTypes.Move(ChatCustomList.SelectedIndex, ChatCustomList.SelectedIndex + 1);
                }
                SettingsProfileHandler.Save();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void ChatCustomList_SelectedRowChanged(SelectedRowChangedEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                ChatCustomType setting = (ChatCustomType)e.NewRow;
                Chat_Custom_Text.Text = setting.Text;
                Chat_Custom_Action.Text = setting.Action;
                Chat_Custom_Window.Text = setting.Window;

                for (int i = 0; i < Chat_Custom_Color.Count; i++)
                {
                    if (Chat_Custom_Color.Text[i] == setting.Color)
                    {
                        Chat_Custom_Color.Selected = i;
                    }
                }

                for (int i = 0; i < Chat_Custom_Type.Count; i++)
                {
                    if (Chat_Custom_Type.Text[i] == setting.Type)
                    {
                        Chat_Custom_Type.Selected = i;
                    }
                }
                
                string soundName = setting.Sound;
                if (String.IsNullOrEmpty(setting.Sound))
                {
                    soundName = "None";
                }
                for (int i = 0; i < Chat_Custom_Sound.Count; i++)
                {
                    
                    if (Chat_Custom_Sound.Text[i] == soundName)
                    {
                        Chat_Custom_Sound.Selected = i;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Chat_Custom_Click(object sender, int row, int col)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                if (col == CHECK)
                {
                    CustomSetting.CustomTypes[row].Enabled = (bool)Chat_Custom[row][CHECK][0];
                    SettingsProfileHandler.Save();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Chat_Custom_Sound_Change(object sender, MVIndexChangeEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                ChatSounds.PlaySound(Chat_Custom_Sound.Text[e.Index]);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                ChatCustomSettings tmp = (ChatCustomSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
                if (tmp == null)
                {
                    TraceLogger.Write("Creating new ChatColorSettings", TraceLevel.Info);
                    CustomSetting = new ChatCustomSettings();
                    SettingsProfileHandler.AddSettingGroup(CustomSetting);
                }
                else
                {
                    TraceLogger.Write("Loading Existing ChatColorSettings", TraceLevel.Info);
                    CustomSetting = tmp;
                }
                ChatCustomList.List = CustomSetting.CustomTypes;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        internal override void PostPluginInit()
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            State = ComponentState.Startingup;
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        internal override void PostLogin()
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            ZChatWrapper.ChatBoxMessage += new ChatBoxMessageEvent(Service_ChatBoxMessage);
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Service_ChatBoxMessage(ChatBoxMessageEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            TraceLogger.Write("Initial Text: " + e.Text + ", Color: " + e.Color + ", Window: " + e.Window.ToString(), TraceLevel.Noise);
            string testText = e.Text;
            e.SetPriority(60);
            foreach (ChatCustomType type in CustomSetting.CustomTypes)
            {
                if (type.Test(e.Type.ToString(), e.Text))
                {
                    if (type.Color == "Default")
                    {
                        TraceLogger.Write("Forcing default color", TraceLevel.Noise);
                        e.ForceDefaultColor();
                    }
                    else
                    {
                        TraceLogger.Write("Setting color to " + type.Color, TraceLevel.Noise);
                        e.Color = Constants.ChatColors(type.Color);
                    }

                    if (type.Window.Contains("D"))
                    {
                        TraceLogger.Write("Forcing default window", TraceLevel.Noise);
                        e.Window.ForceDefault();
                    }
                    else if (type.Window.Contains("B"))
                    {
                        TraceLogger.Write("Blocking message", TraceLevel.Noise);
                        e.Window.BlockAll();
                    }
                    else
                    {
                        if(type.Window.Contains("M"))
                        {
                            TraceLogger.Write("Sending to main", TraceLevel.Noise);
                            e.Window.MainWindow = true;
                        }
                        if(type.Window.Contains("1"))
                        {
                            TraceLogger.Write("Sending to window 1", TraceLevel.Noise);
                            e.Window.Window1 = true;
                        }
                        if(type.Window.Contains("2"))
                        {
                            TraceLogger.Write("Sending to window 2", TraceLevel.Noise);
                            e.Window.Window2 = true;
                        }
                        if(type.Window.Contains("3"))
                        {
                            TraceLogger.Write("Sending to window 3", TraceLevel.Noise);
                            e.Window.Window3 = true;
                        }
                        if(type.Window.Contains("4"))
                        {
                            TraceLogger.Write("Sending to window 4", TraceLevel.Noise);
                            e.Window.Window4 = true;
                        }
                    }

                    if (!String.IsNullOrEmpty(type.Sound) && type.Sound != "None")
                    {
                        TraceLogger.Write("Playing sound " + type.Sound, TraceLevel.Noise);
                        ChatSounds.PlaySound(type.Sound);
                    }
                    if (!String.IsNullOrEmpty(type.Action))
                    {
                        ProcessActions(type.Action);
                    }
                    break;
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        private void ProcessActions(string Actions)
        {
            string[] actionArr = Actions.Split(';');
            foreach(string action in actionArr)
            {
                string fixedAction = action.Trim();
                if (fixedAction.StartsWith("["))
                {
                    int color = Constants.ChatColors("Purple");
                    int window = 1;
                    string text = "";
                    if (fixedAction.StartsWith("[]"))
                    {
                        text = fixedAction.Replace("[]", "");
                    }
                    else
                    {
                        int pos = fixedAction.IndexOf(']');
                        string subString = fixedAction.Substring(1, pos - 1);
                        text = fixedAction.Substring(pos + 1);
                        string[] chatArr = subString.Split(',');

                        if (chatArr.Length == 1)
                        {
                            if (!Int32.TryParse(chatArr[0], out color))
                            {
                                color = Constants.ChatColors("Purple");
                            }
                        }
                        else if (chatArr.Length == 2)
                        {
                            if (!Int32.TryParse(chatArr[0], out color))
                            {
                                color = Constants.ChatColors("Purple");
                            }
                            if (!Int32.TryParse(chatArr[1], out window))
                            {
                                window = 1;
                            }
                        }
                    }
                    ZChatWrapper.WriteToChat(text, color, window);
                }
                else
                {
                    Core.Actions.InvokeChatParser(fixedAction);
                }
            }
        }

        internal override void Shutdown()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                State = ComponentState.ShuttingDown;
                SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
                ZChatWrapper.ChatBoxMessage -= new ChatBoxMessageEvent(Service_ChatBoxMessage);
                Chat_Custom = null;
                Chat_Custom_Text = null;
                Chat_Custom_Type = null;
                Chat_Custom_Color = null;
                Chat_Custom_Sound = null;
                Chat_Custom_Window = null;
                Chat_Custom_Down = null;
                Chat_Custom_Up = null;
                Chat_Custom_Delete = null;
                Chat_Custom_Add = null;
                Chat_Custom_Edit = null;
                Chat_Custom_Action = null;

                State = ComponentState.ShutDown;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }
    }

    public class ChatCustomSettings : SettingGroup
    {
        [XmlArray("ChatCustomItems"), XmlArrayItem("ChatCustomItem", typeof(ChatCustomType))]
        public RowItemList<ChatCustomType> CustomTypes { get; set; }

        public ChatCustomSettings()
        {
            groupName = "ChatCustom";
            CustomTypes = new RowItemList<ChatCustomType>();
        }
    }

    public class ChatCustomType : IRowItem
    {
        private Regex regex;

        private bool _enabled;
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                DataRow["Enabled"].Data = new BoolColumnData(value);
            }
        }

        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                regex = new Regex(value);
                DataRow["Text"].Data = new StringColumnData(value);
            }
        }

        private string _type;
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                DataRow["Type"].Data = new StringColumnData(value);
            }
        }

        private string _window;
        public string Window
        {
            get
            {
                return _window;
            }
            set
            {
                _window = value;
                DataRow["Window"].Data = new StringColumnData(value);
            }
        }

        private string _color;
        public string Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                DataRow["Color"].Data = new ImageColumnData(Constants.ChatColorIcons(value));
            }
        }

        private string _sound;
        public string Sound
        {
            get
            {
                return _sound;
            }
            set
            {
                _sound = value;
                DataRow["Sound"].Data = new StringColumnData(value);
            }
        }

        private string _action;
        public string Action
        {
            get
            {
                return _action;
            }
            set
            {
                _action = value;
                DataRow["Action"].Data = new StringColumnData(value);
            }
        }

        public Row RowItem
        {
            get
            {
                return DataRow;
            }
        }

        private Row DataRow {get; set;}

        public ChatCustomType()
        {
            DataRow = new Row();
            DataRow.AddColumn("Enabled", ChatCustom.CHECK);
            DataRow.AddColumn("Text", ChatCustom.TEXT);
            DataRow.AddColumn("Type", ChatCustom.TYPE);
            DataRow.AddColumn("Window", ChatCustom.WINDOW);
            DataRow.AddColumn("Color", ChatCustom.COLOR);
            DataRow.AddColumn("Sound", ChatCustom.SOUND);
            DataRow.AddColumn("Action", ChatCustom.ACTION);
        }

        internal bool Test(string type, string text)
        {
            if (Enabled)
            {
                if (Type == "All" || type == Type)
                {
                    if (regex.IsMatch(text))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
