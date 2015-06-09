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
    internal class ChatFilter : Component
    {
        internal const int ENABLED = 0;
        internal const int TEXT = 1;
        internal const int MATCHING = 2;
        internal const int ACTION = 3;
        internal const int DELETE = 4;

        internal ChatFilterSettings ChatFilterSettings;

        DecalList ChatFilterList;

        IList Chat_Filter;
        ITextBox Filter_Box;
        IButton Filter_Add;
        ICheckBox Chat_Filter_Matching;
        ICheckBox Chat_Filter_Action;

        internal ChatFilter(CoreManager core, IView view)
            : base(core, view)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            ComponentName = "ChatFilter";
            Critical = false;

            SettingsProfileHandler.registerType(typeof(ChatFilterSettings));
            SettingsProfileHandler.registerType(typeof(ChatFilterOption));
            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            
            Chat_Filter = (IList)View["Chat_Filter"];
            Chat_Filter.Click += new dClickedList(Chat_Filter_Click);
            Filter_Box = (ITextBox)View["Filter_Box"];
            Filter_Add = (IButton)View["Filter_Add"];
            Filter_Add.Click += new EventHandler<MVControlEventArgs>(Filter_Add_Click);
            Chat_Filter_Matching = (ICheckBox)View["Chat_Filter_Matching"];
            Chat_Filter_Action = (ICheckBox)View["Chat_Filter_Action"];

            ChatFilterList = new DecalList(Chat_Filter);
            ChatFilterList.HighlightColor = Constants.GUIColors("Highlight");
            ChatFilterList.HighlightColumn = new int[] { };

            TraceLogger.Write("Initialized Component: " + ComponentName, TraceLevel.Info);
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Chat_Filter_Click(object sender, int row, int col)
        {
            TraceLogger.Write("Enter, row: " + row + ", col: " + col, TraceLevel.Noise);
            if (col == DELETE)
            {
                TraceLogger.Write("Deleting " + ChatFilterSettings.Filters[row].Text, TraceLevel.Info);
                ChatFilterSettings.Filters.RemoveAt(row);
                SettingsProfileHandler.Save();
            }
            if (col == ENABLED)
            {
                TraceLogger.Write("Toggling enabled for " + ChatFilterSettings.Filters[row].Text + " to " + (bool)Chat_Filter[row][ENABLED][0], TraceLevel.Info);
                ChatFilterSettings.Filters[row].Enabled = (bool)Chat_Filter[row][ENABLED][0];
                SettingsProfileHandler.Save();
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Filter_Add_Click(object sender, MVControlEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                ChatFilterOption cfo = new ChatFilterOption();
                cfo.Text = Filter_Box.Text;
                cfo.Enabled = true;
                if (Chat_Filter_Action.Checked)
                {
                    cfo.Action = "Replace";
                }
                else
                {
                    cfo.Action = "Block";
                }
                if (Chat_Filter_Matching.Checked)
                {
                    cfo.Matching = "Whole";
                }
                else
                {
                    cfo.Matching = "Part";
                }
                ChatFilterSettings.Filters.Add(cfo);
                SettingsProfileHandler.Save();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void ZChatWrapper_ChatBoxMessage(ChatBoxMessageEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                e.SetPriority(75);
                foreach (ChatFilterOption cfo in ChatFilterSettings.Filters)
                {
                    if (cfo.Enabled)
                    {
                        if (cfo.Matching == "Whole")
                        {
                            Regex whole = new Regex(@"[^\w](" + cfo.Text + @")[^\w]");
                            if (cfo.Action == "Replace")
                            {
                                foreach (Match m in whole.Matches(e.Text))
                                {
                                    TraceLogger.Write("Replacing on Whole Word Match", TraceLevel.Verbose);
                                    e.ReplaceMessage(m.Groups[1].Value, new String('#', m.Groups[1].Length));
                                }
                            }
                            else if (cfo.Action == "Block")
                            {
                                if (whole.IsMatch(e.Text))
                                {
                                    TraceLogger.Write("Blocking on Whole Word Match", TraceLevel.Verbose);
                                    e.Window.BlockAll();
                                }
                            }
                        }
                        else if (cfo.Matching == "Part")
                        {
                            if (e.Text.Contains(cfo.Text))
                            {
                                if (cfo.Action == "Replace")
                                {
                                    TraceLogger.Write("Replacing on Partial Word Match", TraceLevel.Verbose);
                                    e.ReplaceMessage(cfo.Text, new String('#', cfo.Text.Length));
                                }
                                else if (cfo.Action == "Block")
                                {
                                    TraceLogger.Write("Blocking on Partial Word Match", TraceLevel.Verbose);
                                    e.Window.BlockAll();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
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
            ZChatWrapper.ChatBoxMessage += new ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.ShuttingDown;
            SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            ZChatWrapper.ChatBoxMessage -= new ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            Chat_Filter = null;
            Filter_Box = null;
            Filter_Add = null;
            Chat_Filter_Matching = null;
            Chat_Filter_Action = null;

            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ChatFilterSettings tmp = (ChatFilterSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ChatFilterSettings", TraceLevel.Info);
                ChatFilterSettings = new ChatFilterSettings();
                SettingsProfileHandler.AddSettingGroup(ChatFilterSettings);
            }
            else
            {
                TraceLogger.Write("Loading Existing ChatFilterSettings", TraceLevel.Info);
                ChatFilterSettings = tmp;
            }
            ChatFilterList.List = ChatFilterSettings.Filters;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    [Serializable]
    public class ChatFilterSettings : SettingGroup
    {
        [XmlArray("Filters"), XmlArrayItem("Filter", typeof(ChatFilterOption))]
        public RowItemList<ChatFilterOption> Filters { get; set; }

        public ChatFilterSettings()
        {
            this.groupName = "ChatFilter";
            Filters = new RowItemList<ChatFilterOption>();
        }
    }

    [Serializable]
    public class ChatFilterOption : IRowItem
    {
        private string text;
        private bool enabled;
        private string matching;
        private string action;

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                DataRow["Text"].Data = new StringColumnData(value);
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

        public string Matching
        {
            get { return matching; }
            set
            {
                matching = value;
                DataRow["Matching"].Data = new StringColumnData(value);
            }
        }

        public string Action
        {
            get { return action; }
            set
            {
                action = value;
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

        private Row DataRow { get; set; }

        public ChatFilterOption()
        {
            DataRow = new Row();
            DataRow.AddColumn("Enabled", ChatFilter.ENABLED);
            DataRow.AddColumn("Text", ChatFilter.TEXT);
            DataRow.AddColumn("Matching", ChatFilter.MATCHING);
            DataRow.AddColumn("Action", ChatFilter.ACTION);
            DataRow.AddColumn("Delete", ChatFilter.DELETE);
            DataRow["Delete"].Data = new IconColumnData(0x60011F8);
        }
    }
}
