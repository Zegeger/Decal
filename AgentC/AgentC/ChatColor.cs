using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Decal.Adapter;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using Zegeger.Decal.VVS;
using Zegeger.Decal.Chat;
using Zegeger.Decal.Data;
using Zegeger.Diagnostics;
using Zegeger.Decal.Controls;
using Zegeger.Audio;
using Zegeger.Analysis;

namespace Zegeger.Decal.Plugins.AgentC
{
    class ChatColor : Component
    {
        internal const int CHECK = 0;
        internal const int NAME = 1;
        internal const int WINDOW = 2;
        internal const int COLOR = 3;
        internal const int SOUND = 4;

        ChatColorSettings ColorSetting;

        IList Chat_Color;
        ICombo Chat_Color_Color;
        ICombo Chat_Color_Sound;
        ICombo Chat_Color_Window;

        private DecalList ChatColorList;
        private SoundManager ChatSounds;
        private RuleList ColorRuleList;
        private string KillMessage;

        public ChatColor(CoreManager core, IView view, string dllPath) : base (core, view)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                Critical = false;
                ComponentName = "ChatColor";

                ChatSounds = new SoundManager(Path.Combine(dllPath, "Sounds"));
                ColorRuleList = new RuleList(Constants.ChatColorRules("RuleXML"));

                KillMessage = "Some arbitrarily weird and long statement that should not match a kill message";

                SettingsProfileHandler.registerType(typeof(ChatColorSettings));
                SettingsProfileHandler.registerType(typeof(ChatColorType));

                SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);

                Chat_Color = (IList)View["Chat_Color"];
                Chat_Color.Click += new dClickedList(Chat_Color_Click);
                Chat_Color_Color = (ICombo)View["Chat_Color_Color"];
                Chat_Color_Color.Change += new EventHandler<MVIndexChangeEventArgs>(Chat_Color_Color_Change);
                Chat_Color_Sound = (ICombo)View["Chat_Color_Sound"];
                Chat_Color_Sound.Change += new EventHandler<MVIndexChangeEventArgs>(Chat_Color_Sound_Change);
                Chat_Color_Window = (ICombo)View["Chat_Color_Window"];
                Chat_Color_Window.Change += new EventHandler<MVIndexChangeEventArgs>(Chat_Color_Window_Change);

                ChatColorList = new DecalList(Chat_Color);
                ChatColorList.HighlightColor = Constants.GUIColors("Highlight");
                ChatColorList.HighlightColumn = new int[] {NAME};
                ChatColorList.SelectedIndexChanged += new SelectedIndexChangedEvent(ChatColorList_SelectedRowChanged);
                ChatColorList.SelectedRowChanged += new SelectedRowChangedEvent(ChatColorList_SelectedRowChanged);

                Chat_Color_Color.Clear();
                Chat_Color_Color.Add("Default");
                foreach (string name in Constants.ChatColorsDictionary.Keys)
                {
                    Chat_Color_Color.Add(name);
                }

                Chat_Color_Window.Clear();
                foreach (string name in Constants.ChatWindowsDictionary.Values)
                {
                    Chat_Color_Window.Add(name);
                }

                Chat_Color_Sound.Clear();
                Chat_Color_Sound.Add("None");
                foreach (string name in ChatSounds.SoundFileNames)
                {
                    Chat_Color_Sound.Add(name);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void OnKillMessageEvent(KillMessageEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            TraceLogger.Write("Setting KillMessage = " + e.KillMessage, TraceLevel.Verbose);
            KillMessage = e.KillMessage;
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void ChatColorList_SelectedRowChanged(SelectedRowChangedEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                ChatColorType setting = ColorSetting.ColorTypes[ChatColorList.SelectedIndex];
                for (int i = 0; i < Chat_Color_Color.Count; i++)
                {
                    if (Chat_Color_Color.Text[i] == setting.Color)
                    {
                        Chat_Color_Color.Selected = i;
                    }
                }
                for (int i = 0; i < Chat_Color_Window.Count; i++)
                {
                    if (Chat_Color_Window.Text[i] == setting.Window)
                    {
                        Chat_Color_Window.Selected = i;
                    }
                }
                string soundName = setting.Sound;
                if (String.IsNullOrEmpty(setting.Sound))
                {
                    soundName = "None";
                }
                for (int i = 0; i < Chat_Color_Sound.Count; i++)
                {
                    
                    if (Chat_Color_Sound.Text[i] == soundName)
                    {
                        Chat_Color_Sound.Selected = i;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void ChatColorList_SelectedRowChanged(SelectedIndexChangedEventArgs e)
        {
            
        }

        void Chat_Color_Click(object sender, int row, int col)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (col == CHECK)
                {
                    ColorSetting.ColorTypes[row].Enabled = (bool)Chat_Color[row][CHECK][0];
                    SettingsProfileHandler.Save();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Color_Window_Change(object sender, MVIndexChangeEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                ColorSetting.ColorTypes[ChatColorList.SelectedIndex].Window = Chat_Color_Window.Text[e.Index];
                SettingsProfileHandler.Save();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Color_Sound_Change(object sender, MVIndexChangeEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                ColorSetting.ColorTypes[ChatColorList.SelectedIndex].Sound = Chat_Color_Sound.Text[e.Index];
                SettingsProfileHandler.Save();
                ChatSounds.PlaySound(Chat_Color_Sound.Text[e.Index]);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Color_Color_Change(object sender, MVIndexChangeEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                ColorSetting.ColorTypes[ChatColorList.SelectedIndex].Color = Chat_Color_Color.Text[e.Index];
                SettingsProfileHandler.Save();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                ChatColorSettings tmp = (ChatColorSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
                if (tmp == null)
                {
                    TraceLogger.Write("Creating new ChatColorSettings", TraceLevel.Info);
                    ColorSetting = new ChatColorSettings();
                    SettingsProfileHandler.AddSettingGroup(ColorSetting);
                }
                else
                {
                    TraceLogger.Write("Loading Existing ChatColorSettings", TraceLevel.Info);
                    ColorSetting = tmp;
                }
                foreach (string v in Constants.ChatColorGeneralDictionary.Keys)
                {
                    bool found = false;
                    foreach (ChatColorType cct in ColorSetting.ColorTypes)
                    {
                        if (v == cct.Name)
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        TraceLogger.Write("Missing ColorType in Setting, adding one for " + v, TraceLevel.Warning);
                        ChatColorType newcct = new ChatColorType();
                        newcct.Name = v;
                        newcct.Enabled = false;
                        newcct.Color = "Default";
                        newcct.Window = "D";
                        newcct.Sound = "";
                        ColorSetting.ColorTypes.Add(newcct);
                    }
                }
                SettingsProfileHandler.Save();
                ChatColorList.List = ColorSetting.ColorTypes;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void PostPluginInit()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.Startingup;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void PostLogin()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ZChatWrapper.ChatBoxMessage += new ChatBoxMessageEvent(Service_ChatBoxMessage);
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Service_ChatBoxMessage(ChatBoxMessageEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            TraceLogger.Write("Initial Text: " + e.Text + ", Color: " + e.Color + ", Window: " + e.Window.ToString(), TraceLevel.Verbose);
            string testText = e.Text;

            if (testText.Trim() == KillMessage.Trim())
            {
                TraceLogger.Write("Text matches kill message", TraceLevel.Verbose);
                testText = "_KILL_";
            }

            string colorType = ColorRuleList.Test(e.Type, testText);
            if (!String.IsNullOrEmpty(colorType))
            {
                TraceLogger.Write("Match rule " + colorType, TraceLevel.Verbose);
                foreach (ChatColorType type in ColorSetting.ColorTypes)
                {
                    if (type.Name == colorType)
                    {
                        TraceLogger.Write("Type setting matched rule", TraceLevel.Verbose);
                        if (type.Enabled)
                        {
                            TraceLogger.Write("Rule is enabled", TraceLevel.Verbose);
                            if (type.Color == "Default")
                            {
                                TraceLogger.Write("Forcing default color", TraceLevel.Verbose);
                                e.ForceDefaultColor();
                            }
                            else
                            {
                                TraceLogger.Write("Setting color to " + type.Color, TraceLevel.Verbose);
                                e.Color = Constants.ChatColors(type.Color);
                            }

                            if (type.Window == "D")
                            {
                                TraceLogger.Write("Forcing default window", TraceLevel.Verbose);
                                e.Window.ForceDefault();
                            }
                            else if (type.Window == "B")
                            {
                                TraceLogger.Write("Blocking message", TraceLevel.Verbose);
                                e.Window.BlockAll();
                            }
                            else
                            {
                                switch (type.Window)
                                {
                                    case "M":
                                        TraceLogger.Write("Sending to main", TraceLevel.Verbose);
                                        e.Window.SetWindows(true, false, false, false, false);
                                        break;
                                    case "1":
                                        TraceLogger.Write("Sending to window 1", TraceLevel.Verbose);
                                        e.Window.SetWindows(false, true, false, false, false);
                                        break;
                                    case "2":
                                        TraceLogger.Write("Sending to window 2", TraceLevel.Verbose);
                                        e.Window.SetWindows(false, false, true, false, false);
                                        break;
                                    case "3":
                                        TraceLogger.Write("Sending to window 3", TraceLevel.Verbose);
                                        e.Window.SetWindows(false, false, false, true, false);
                                        break;
                                    case "4":
                                        TraceLogger.Write("Sending to window 4", TraceLevel.Verbose);
                                        e.Window.SetWindows(false, false, false, false, true);
                                        break;
                                }
                            }
                            if (!String.IsNullOrEmpty(type.Sound) && type.Sound != "None")
                            {
                                TraceLogger.Write("Playing sound " + type.Sound, TraceLevel.Verbose);
                                ChatSounds.PlaySound(type.Sound);
                            }
                            break;
                        }

                    }
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void Shutdown()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                State = ComponentState.ShuttingDown;
                SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
                ZChatWrapper.ChatBoxMessage -= new ChatBoxMessageEvent(Service_ChatBoxMessage);
                Chat_Color = null;
                Chat_Color_Color = null;
                Chat_Color_Sound = null;
                Chat_Color_Window = null;

                State = ComponentState.ShutDown;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    public class ChatColorSettings : SettingGroup
    {
        [XmlArray("ChatColors"), XmlArrayItem("ChatColor", typeof(ChatColorType))]
        public RowItemList<ChatColorType> ColorTypes { get; set; }

        public ChatColorSettings()
        {
            groupName = "ChatColor";
            ColorTypes = new RowItemList<ChatColorType>();
        }
    }

    public class ChatColorType : IRowItem
    {
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

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                DataRow["Name"].Data = new StringColumnData(Constants.ChatColorGeneral(value));
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

        public Row RowItem
        {
            get
            {
                return DataRow;
            }
        }

        private Row DataRow { get; set; }

        public ChatColorType()
        {
            DataRow = new Row();
            DataRow.AddColumn("Enabled", ChatColor.CHECK);
            DataRow.AddColumn("Name", ChatColor.NAME);
            DataRow.AddColumn("Window", ChatColor.WINDOW);
            DataRow.AddColumn("Color", ChatColor.COLOR);
            DataRow.AddColumn("Sound", ChatColor.SOUND);
        }
    }
}
