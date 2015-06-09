using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Drawing;
using System.Media;
using System.Xml.Serialization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using Decal.Adapter;
using Zegeger.Decal.VVS;
using Zegeger.Decal.Chat;
using Zegeger.Diagnostics;
using Zegeger.Decal.Data;
using Zegeger.Analysis;
using Zegeger.Decal.Controls;

namespace Zegeger.Decal.Plugins.AgentC
{
    internal class FriendMsg
    {
        internal string Text { get; set; }
        internal int Color { get; set; }
        internal WindowOutput Window { get; set; }

        internal FriendMsg(string text, int color, WindowOutput window)
        {
            Text = text;
            Color = color;
            Window = window;
        }
    }

    internal class ChatOptions : Component
    {
        internal const int ENABLED = 0;
        internal const int TEXT = 1;

        private const string CHAT_REMOVE_GREEN_NAMES = "REMOVE_GREEN_NAMES";
        private const string CHAT_IMPROVED_VITAE_MSG = "IMPROVED_VITAE_MSG";
        private const string CHAT_BLAST_RARE_MSG = "BLAST_RARE_MSG";
        private const string CHAT_SHOW_FRIENDS_ONLINE = "SHOW_FRIENDS_ONLINE";

        internal ChatOptionsSettings ChatOptionsSettings;

        DecalList ChatOptionsList;

        private IList Chat_Options;

        private ZTimer startUpTimer;
        private ZTimer friendsOnlineTimer;

        private bool friendsOnlineCalled;
        private bool friendStartSeen;
        private List<FriendMsg> friendsOnlineList;

        internal ChatOptions(CoreManager core, IView view)
            : base(core, view)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ComponentName = "ChatOptions";
            Critical = false;

            friendsOnlineCalled = false;
            friendStartSeen = false;
            friendsOnlineList = new List<FriendMsg>();
            friendsOnlineTimer = ZTimer.CreateInstance(friendsOnlineTimer_Elapsed);
            friendsOnlineTimer.Repeat = false;

            startUpTimer = ZTimer.CreateInstance(startUpTimer_Elapsed);
            startUpTimer.Repeat = false;

            Chat_Options = (IList)View["Chat_Options"];
            Chat_Options.Click += new dClickedList(Chat_Options_Click);

            SettingsProfileHandler.registerType(typeof(ChatOptionsSettings));
            SettingsProfileHandler.registerType(typeof(ChatOptionsOption));
            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            
            ChatOptionsList = new DecalList(Chat_Options);
            ChatOptionsList.HighlightColor = Constants.GUIColors("Highlight");
            ChatOptionsList.HighlightColumn = new int[] { };

            TraceLogger.Write("Initialized Component: " + ComponentName, TraceLevel.Info);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void startUpTimer_Elapsed(object state)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                friendsOnlineCalled = true;
                Core.Actions.InvokeChatParser("/friends online");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void friendsOnlineTimer_Elapsed(object state)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (friendsOnlineCalled)
                {
                    friendsOnlineCalled = false;
                    foreach (FriendMsg e in friendsOnlineList)
                    {
                        ZChatWrapper.WriteToChat(e.Text, e.Color, e.Window);
                    }
                    friendsOnlineList.Clear();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Options_Click(object sender, int row, int col)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (col == ENABLED)
                {
                    ChatOptionsSettings.Options[row].Enabled = (bool)Chat_Options[row][ENABLED][0];
                    SettingsProfileHandler.Save();
                }
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
            ZChatWrapper.ChatBoxMessage += new ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            ZChatWrapper.ChatTextComplete += new ChatTextCompleteEvent(ZChatWrapper_ChatTextComplete);
            State = ComponentState.Startingup;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void PostLogin()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (SettingEnabled(CHAT_SHOW_FRIENDS_ONLINE))
            {
                startUpTimer.Start(3000);
            }
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void ZChatWrapper_ChatTextComplete(ChatTextCompleteEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (State == ComponentState.Running)
                {
                    if (SettingEnabled(CHAT_BLAST_RARE_MSG))
                    {
                        if (e.Type == Constants.ChatClasses("Rare") && e.Text.Contains(" has discovered "))
                        {
                            TraceLogger.Write("Adding additional rare messages.", TraceLevel.Verbose);
                            Core.Actions.AddChatText(e.Text, e.Color);
                            Core.Actions.AddChatText(e.Text, e.Color);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void ZChatWrapper_ChatBoxMessage(ChatBoxMessageEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (e.Text.Contains("You have entered the ") && e.Type == Constants.ChatClasses("System"))
                {
                    e.SetPriority(100);
                    e.Window.BlockAll();
                }
                if (State == ComponentState.Running)
                {
                    foreach (ChatOptionsOption coo in ChatOptionsSettings.Options)
                    {
                        if (coo.Enabled)
                        {
                            switch (coo.Id)
                            {
                                case CHAT_REMOVE_GREEN_NAMES:
                                    string clickable = Util.ClickableText(e.Text);
                                    if (!String.IsNullOrEmpty(clickable))
                                    {
                                        TraceLogger.Write("Removing clickable name.", TraceLevel.Verbose);
                                        e.ReplaceMessage(clickable, Util.RemoveClickableText(clickable));
                                    }
                                    break;
                                case CHAT_IMPROVED_VITAE_MSG:
                                    if (e.Text.StartsWith("Your experience has reduced your Vitae penalty!") && e.Type == Constants.ChatClasses("Vitae"))
                                    {
                                        TraceLogger.Write("Adding vitae percentage to vitae message.", TraceLevel.Verbose);
                                        e.ReplaceMessage("!", "");
                                        e.AppendMessage(" " + Core.CharacterFilter.Vitae + "%");
                                    }
                                    break;
                                case CHAT_BLAST_RARE_MSG:
                                    if (e.Type == Constants.ChatClasses("Rare") && e.Text.Contains(" has discovered "))
                                    {
                                        TraceLogger.Write("Blasing rare message.", TraceLevel.Verbose);
                                        e.Window.SetWindows(true, true, true, true, true);
                                    }
                                    break;
                                case CHAT_SHOW_FRIENDS_ONLINE:
                                    if (friendsOnlineCalled && friendStartSeen && e.Type == Constants.ChatClasses("System"))
                                    {
                                        TraceLogger.Write("Friend name.", TraceLevel.Verbose);
                                        e.SetPriority(100);
                                        friendsOnlineList.Add(new FriendMsg(e.Text, e.Type, e.Window.Clone()));
                                        e.Window.BlockAll();
                                    }
                                    if (friendsOnlineCalled && e.Type == Constants.ChatClasses("System") && e.Text.Contains("Your friends:"))
                                    {
                                        TraceLogger.Write("Friends start seen.", TraceLevel.Verbose);
                                        friendStartSeen = true;
                                        e.SetPriority(100);
                                        friendsOnlineList.Add(new FriendMsg(e.Text, e.Type, e.Window.Clone()));
                                        e.Window.BlockAll();
                                        friendsOnlineTimer.Start(1000);
                                    }
                                    if (friendsOnlineCalled && e.Type == Constants.ChatClasses("System") && (e.Text.Contains("You have no friends that are online") || e.Text.Contains("Your friends list is empty!")))
                                    {
                                        TraceLogger.Write("Blank friends list, hiding.", TraceLevel.Verbose);
                                        e.SetPriority(100);
                                        friendsOnlineList.Clear();
                                        friendsOnlineCalled = false;
                                        e.Window.BlockAll();
                                        friendsOnlineTimer.Stop();
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.ShuttingDown;
            SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            ZChatWrapper.ChatBoxMessage -= new ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            ZChatWrapper.ChatTextComplete -= new ChatTextCompleteEvent(ZChatWrapper_ChatTextComplete);
            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ChatOptionsSettings tmp = (ChatOptionsSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ChatLoggerSettings", TraceLevel.Info);
                ChatOptionsSettings = new ChatOptionsSettings();
                SettingsProfileHandler.AddSettingGroup(ChatOptionsSettings);
            }
            else
            {
                TraceLogger.Write("Loading Existing ChatLoggerSettings", TraceLevel.Info);
                ChatOptionsSettings = tmp;
            }
            foreach (string v in Constants.ChatOptionsDictionary.Keys)
            {
                bool found = false;
                foreach (ChatOptionsOption coo in ChatOptionsSettings.Options)
                {
                    if (v == coo.Id)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    TraceLogger.Write("Missing ChatOptionsOption in Setting, adding one for " + v, TraceLevel.Warning);
                    ChatOptionsOption newcoo = new ChatOptionsOption();
                    newcoo.Enabled = false;
                    newcoo.Id = v;
                    ChatOptionsSettings.Options.Add(newcoo);
                }
            }
            SettingsProfileHandler.Save();
            ChatOptionsList.List = ChatOptionsSettings.Options;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        bool SettingEnabled(string SettingId)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            foreach (ChatOptionsOption coo in ChatOptionsSettings.Options)
            {
                if (coo.Id == SettingId)
                {
                    TraceLogger.Write("Exit return " + coo.Enabled, TraceLevel.Verbose);
                    return coo.Enabled;
                }
            }
            TraceLogger.Write("Exit return false since Setting not found", TraceLevel.Verbose);
            return false;
        }
    }

    [Serializable]
    public class ChatOptionsSettings : SettingGroup
    {
        [XmlArray("Options"), XmlArrayItem("Option", typeof(ChatOptionsOption))]
        public RowItemList<ChatOptionsOption> Options { get; set; }

        public ChatOptionsSettings()
        {
            groupName = "ChatOptions";
            Options = new RowItemList<ChatOptionsOption>();
        }
    }

    [Serializable]
    public class ChatOptionsOption : IRowItem
    {
        private string id;
        private bool enabled;

        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                DataRow["Text"].Data = new StringColumnData(Constants.ChatOptions(value));
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                DataRow["Enabled"].Data = new BoolColumnData(value);
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

        public ChatOptionsOption()
        {
            DataRow = new Row();
            DataRow.AddColumn("Enabled", ChatOptions.ENABLED);
            DataRow.AddColumn("Text", ChatOptions.TEXT);
        }
    }
}
