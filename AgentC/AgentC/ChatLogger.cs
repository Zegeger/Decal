using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Drawing;
using System.Media;
using Decal.Adapter;
using System.Xml.Serialization;
using System.Reflection;
using Zegeger.Decal.VVS;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using Zegeger.Decal.Chat;
using Zegeger.Diagnostics;
using Zegeger.Decal.Data;
using Zegeger.Analysis;
using Zegeger.Decal.Controls;

namespace Zegeger.Decal.Plugins.AgentC
{
    internal class ChatLogger : Component
    {
        internal const int NAME = 0;
        internal const int VIEW = 2;
        internal const int FILE = 4;

        internal ChatLoggerData LogData;
        internal ChatLoggerSettings LogSettings;
        internal string BasePath;
        private ColorConverter colorConverter = new ColorConverter();
        private RuleList TypeRules;
        private List<string[]> searchTokens;
        private bool decending = false;
        private int filterClass = -1;
        private bool FileLogEnabled = false;
        private string FileLogPath;
        private IFileLogWriter FileLogWriter;
        private int Day;

        DecalList TypeList;

        static ICombo Chat_Log_Filter;
        static IList Chat_Log_View;
        static ICheckBox Chat_Log_AFK;
        static ITextBox Chat_Log_AFK_Message;
        static ITextBox Chat_Log_Search;
        static IButton Chat_Log_Dir;
        static ICheckBox Chat_Log_Enable;
        static IList Chat_Log_Options;
        static ICheckBox Chat_Log_Timestamp;
        static ITextBox Chat_Log_Max;
        static ICheckBox Chat_Log_XML;
        static ICheckBox Chat_Log_By_Date;
        static ICheckBox Chat_Log_Auto;
        static IButton Chat_Log_Clear;
        static INotebook nblog;

        internal ChatLogger(CoreManager core, IView view) : base(core, view)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ComponentName = "ChatLog";
            Critical = false;

            SettingsProfileHandler.registerType(typeof(ChatLoggerSettings));
            SettingsProfileHandler.registerType(typeof(ChatLogOption));
            DataHandler.registerType(typeof(ChatLoggerData));
            DataHandler.registerType(typeof(LogItem));
            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            DataHandler.DataLoaded += new DataLoadedEvent(DataHandler_DataLoaded);
            
            nblog = (INotebook)View["nblog"];
            Chat_Log_Filter = (ICombo)View["Chat_Log_Filter"];
            Chat_Log_Filter.Change += new EventHandler<MVIndexChangeEventArgs>(Chat_Log_Filter_Change);
            Chat_Log_View = (IList)View["Chat_Log_View"];
            Chat_Log_View.Click += new dClickedList(Chat_Log_View_Click);
            Chat_Log_AFK = (ICheckBox)View["Chat_Log_AFK"];
            Chat_Log_AFK.Change += new EventHandler<MVCheckBoxChangeEventArgs>(Chat_Log_AFK_Change);
            Chat_Log_AFK_Message = (ITextBox)View["Chat_Log_AFK_Message"];
            Chat_Log_AFK_Message.Change += new EventHandler<MVTextBoxChangeEventArgs>(Chat_Log_AFK_Message_Change);
            Chat_Log_Search = (ITextBox)View["Chat_Log_Search"];
            Chat_Log_Search.Change += new EventHandler<MVTextBoxChangeEventArgs>(Chat_Log_Search_Change);

            Chat_Log_Dir = (IButton)View["Chat_Log_Dir"];
            Chat_Log_Dir.Click += new EventHandler<MVControlEventArgs>(Chat_Log_Dir_Click);
            Chat_Log_Enable = (ICheckBox)View["Chat_Log_Enable"];
            Chat_Log_Enable.Change += new EventHandler<MVCheckBoxChangeEventArgs>(Chat_Log_Enable_Change);
            Chat_Log_Options = (IList)View["Chat_Log_Options"];
            Chat_Log_Options.Click += new dClickedList(Chat_Log_Options_Click);
            Chat_Log_Timestamp = (ICheckBox)View["Chat_Log_Timestamp"];
            Chat_Log_Timestamp.Change += new EventHandler<MVCheckBoxChangeEventArgs>(Chat_Log_Timestamp_Change);
            Chat_Log_Max = (ITextBox)View["Chat_Log_Max"];
            Chat_Log_Max.Change += new EventHandler<MVTextBoxChangeEventArgs>(Chat_Log_Max_Change);
            Chat_Log_XML = (ICheckBox)View["Chat_Log_XML"];
            Chat_Log_XML.Change += new EventHandler<MVCheckBoxChangeEventArgs>(Chat_Log_XML_Change);
            Chat_Log_By_Date = (ICheckBox)View["Chat_Log_By_Date"];
            Chat_Log_By_Date.Change += new EventHandler<MVCheckBoxChangeEventArgs>(Chat_Log_By_Date_Change);
            Chat_Log_Auto = (ICheckBox)View["Chat_Log_Auto"];
            Chat_Log_Auto.Change += new EventHandler<MVCheckBoxChangeEventArgs>(Chat_Log_Auto_Change);
            Chat_Log_Clear = (IButton)View["Chat_Log_Clear"];
            Chat_Log_Clear.Click += new EventHandler<MVControlEventArgs>(Chat_Log_Clear_Click);

            TypeList = new DecalList(Chat_Log_Options);
            TypeList.HighlightColor = Constants.GUIColors("Highlight");
            TypeList.HighlightColumn = new int[] { };
            TypeRules = new RuleList(Constants.LogClassRules("RuleXML"));
            
            searchTokens = new List<string[]>();

            CommandClass logClass = CommandHandler.CreateCommandClass("Logging");
            CommandGroup logStartGroup = logClass.CreateCommandGroup("Starts file logging to the specified file name");
            List<string> logStartArgs = new List<string>();
            logStartArgs.Add("file name");
            logStartGroup.AddCommand("log start", logStartArgs, Cmd_LogStart);

            CommandGroup logStopGroup = logClass.CreateCommandGroup("Stops file logging");
            logStopGroup.AddCommand("log stop", Cmd_LogEnd);
            logStopGroup.AddCommand("log end", Cmd_LogEnd);
            TraceLogger.Write("Initialized Component: " + ComponentName, TraceLevel.Info);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Cmd_LogStart(CommandIssuedCallbackArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            string file = e.Args;
            TraceLogger.Write("Starting file log with file name " + file, TraceLevel.Info);
            PluginCore.WriteToChat("Writing log to " + file);
            FileLogPath = BasePath + file;
            if (FileLogWriter == null)
            {
                TraceLogger.Write("File log null, creating...", TraceLevel.Info);
                CreateFileLog();
            }
            else
            {
                TraceLogger.Write("File log is not null, changing path...", TraceLevel.Info);
                FileLogWriter.ChangePath(FileLogPath);
            }
            FileLogEnabled = true;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Cmd_LogEnd(CommandIssuedCallbackArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            TraceLogger.Write("Ending file log", TraceLevel.Info);
            PluginCore.WriteToChat("Ending log.");
            FileLogEnabled = false;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_AFK_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            if (e.Checked)
            {
                Core.Actions.InvokeChatParser("*afk*");
            }
        }

        void Chat_Log_View_Click(object sender, int row, int col)
        {
            TraceLogger.Write("Enter row " + row, TraceLevel.Verbose);
            string id = (string)Chat_Log_View[row][1][0];
            TraceLogger.Write("Clicked guid " + id, TraceLevel.Verbose);
            foreach (LogItem li in LogData.recentLines)
            {
                if (li.id.ToString() == id)
                {
                    TraceLogger.Write("Matched log item: " + li.message, TraceLevel.Info);
                    string time = li.timestamp.ToString("G");
                    ZChatWrapper.WriteToChat("---<" + time + "> " + li.message, li.color, 1);
                    break;
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_Clear_Click(object sender, MVControlEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            LogData.recentLines.Clear();
            Chat_Log_View.Clear();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_Dir_Click(object sender, MVControlEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            decending = !decending;
            if (decending)
            {
                Chat_Log_Dir.Text = "D";
            }
            else
            {
                Chat_Log_Dir.Text = "A";
            }
            ShowLog();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_Filter_Change(object sender, MVIndexChangeEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            string filterName = (string)Chat_Log_Filter.Text[Chat_Log_Filter.Selected];
            if (filterName == "All")
            {
                filterClass = -1;
            }
            else
            {
                filterClass = Constants.LogTypes(filterName);
            }
            ShowLog();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_Search_Change(object sender, MVTextBoxChangeEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            try
            {
                lock (searchTokens)
                {
                    searchTokens = new List<string[]>();
                    if (!String.IsNullOrEmpty(Chat_Log_Search.Text))
                    {
                        string search = Chat_Log_Search.Text;
                        search = search.Replace(" OR ", "|");
                        search = search.Replace(" AND ", "+");
                        string[] orSearch = search.Split('|');
                        foreach (string orItem in orSearch)
                        {
                            string[] andSearch = orItem.Trim().Split('+');
                            TraceLogger.Write("Search token " + andSearch.ToString(), TraceLevel.Verbose);
                            searchTokens.Add(andSearch);
                        }
                    }
                }
                ShowLog();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_Enable_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            LogSettings.RecentEnabled = e.Checked;
            SettingsProfileHandler.Save();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_Auto_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            LogSettings.AutoLog = e.Checked;
            SettingsProfileHandler.Save();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_By_Date_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            LogSettings.LogPerDay = e.Checked;
            SettingsProfileHandler.Save();
            GenerateFileLogPath();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_XML_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            LogSettings.UseXML = e.Checked;
            SettingsProfileHandler.Save();
            GenerateFileLogPath();
            if (FileLogWriter != null)
            {
                FileLogWriter.Close();
            }
            CreateFileLog();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_Max_Change(object sender, MVTextBoxChangeEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            int max;
            if (Int32.TryParse(e.Text, out max))
                LogSettings.RecentLines = max;
            SettingsProfileHandler.Save();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_Timestamp_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            LogSettings.IncludeTimestamp = e.Checked;
            SettingsProfileHandler.Save();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_AFK_Message_Change(object sender, MVTextBoxChangeEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            LogSettings.AFKMessage = e.Text;
            SettingsProfileHandler.Save();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Chat_Log_Options_Click(object sender, int row, int col)
        {
            TraceLogger.Write("Enter row " + row, TraceLevel.Verbose);
            try
            {
                string className = (string)Chat_Log_Options[row][NAME][0];
                TraceLogger.Write("Updating log class " + className, TraceLevel.Info);
                int classId = Constants.LogTypes(className);
                foreach (ChatLogOption clo in LogSettings.Options)
                {
                    if(clo.Id == classId)
                    {
                        clo.View = (bool)Chat_Log_Options[row][VIEW][0];
                        clo.File = (bool)Chat_Log_Options[row][FILE][0];
                        SettingsProfileHandler.Save();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private int GetChatClass(string text, int type)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            string rtn = TypeRules.Test(type, text);
            if (!String.IsNullOrEmpty(rtn))
            {
                int rtnint = Int32.Parse(rtn);
                TraceLogger.Write("Exit return " + rtnint, TraceLevel.Verbose);
                return rtnint;
            }
            TraceLogger.Write("Exit return 0", TraceLevel.Verbose);
            return 0;
        }

        void Service_ChatTextComplete(ChatTextCompleteEventArgs e)
        {
            TraceLogger.Write("Enter " + e.Text, TraceLevel.Verbose);
            try
            {
                if (LogSettings.RecentEnabled || FileLogEnabled)
                {
                    if (e.Window.IsShown)
                    {
                        int Class = GetChatClass(e.Text, e.Type);
                        foreach (ChatLogOption clo in LogSettings.Options)
                        {
                            if (clo.Id == Class)
                            {
                                string fixedText = Util.RemoveClickableText(e.Text);
                                if (LogSettings.RecentEnabled && clo.View)
                                {
                                    TraceLogger.Write("Logging to Recent " + fixedText, TraceLevel.Info);
                                    LogItem newItem = new LogItem(fixedText, e.Type, e.Color, Class);
                                    AddRecentLog(newItem);
                                }
                                if (FileLogEnabled && clo.File)
                                {
                                    TraceLogger.Write("Logging to File " + fixedText, TraceLevel.Info);
                                    if (Day != DateTime.Now.DayOfYear)
                                    {
                                        GenerateFileLogPath();
                                        FileLogWriter.ChangePath(FileLogPath);
                                    }
                                    string text = "";
                                    if (LogSettings.IncludeTimestamp && !LogSettings.UseXML)
                                    {
                                        text = "<" + DateTime.Now.ToString("G") + "> ";
                                    }
                                    text += fixedText;
                                    FileLogWriter.WriteLine(e.Type, Constants.ChatColorsToHex(e.Color.ToString()), Class, DateTime.Now, text);
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

        private void AddRecentLog(LogItem newItem)
        {
            TraceLogger.Write("Enter " + newItem.message, TraceLevel.Verbose);
            LogData.recentLines.Add(newItem);
            while (LogData.recentLines.Count > LogSettings.RecentLines)
            {
                TraceLogger.Write("Removing oldest line that exceeds max line count", TraceLevel.Verbose);
                LogData.recentLines.RemoveAt(0);
                if (!decending)
                {
                    Chat_Log_View.RemoveRow(0);
                }
                else
                {
                    Chat_Log_View.RemoveRow(Chat_Log_View.RowCount - 1);
                }
            }
            DataHandler.Save();
            if (ShowItem(newItem))
                AddRecentLogList(newItem);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void AddRecentLogList(LogItem newItem)
        {
            TraceLogger.Write("Enter " + newItem.message, TraceLevel.Verbose);
            IListRow row;
            if (!decending)
            {
                row = Chat_Log_View.Add();
            }
            else
            {
                row = Chat_Log_View.Insert(0);
            }
            row[0][0] = newItem.message;
            row[0].Color = (Color)colorConverter.ConvertFromString("#FF" + Constants.ChatColorsToHex(newItem.color.ToString()));
            row[1][0] = newItem.id.ToString();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void ShowLog()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            try
            {
                Chat_Log_View.Clear();
                foreach (LogItem li in LogData.recentLines)
                {
                    if (ShowItem(li))
                    {
                        AddRecentLogList(li);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private bool ShowItem(LogItem li)
        {
            TraceLogger.Write("Enter message " + li.message, TraceLevel.Verbose);
            try
            {
                bool searchValid = false;
                if (searchTokens.Count == 0)
                    searchValid = true;
                bool filterValid = false;
                lock (searchTokens)
                {
                    foreach (string[] andItems in searchTokens)
                    {
                        bool match = true;
                        foreach (string andItem in andItems)
                        {
                            if (!li.message.Contains(andItem.Trim()))
                                match = false;
                        }
                        if (match)
                        {
                            searchValid = true;
                            break;
                        }
                    }
                }
                if (filterClass == -1 || filterClass == li.tag)
                    filterValid = true;
                TraceLogger.Write("Exit searchValid: " + searchValid + ", filterValid: " + filterValid, TraceLevel.Verbose);
                return searchValid && filterValid;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            return true;
        }

        internal override void PostPluginInit()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.Startingup;
            IntegerTest test = new IntegerTest(0, PluginCore.NormalType.Value, IntegerTestType.Equals);
            List<ITestable> tests = new List<ITestable>();
            tests.Add(test);
            Any any = new Any(tests);
            Rule rule = new Rule(any, "1000");
            TypeRules.Add(rule);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void PostLogin()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ZChatWrapper.ChatTextComplete += new ChatTextCompleteEvent(Service_ChatTextComplete);
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.ShuttingDown;
            SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            DataHandler.DataLoaded -= new DataLoadedEvent(DataHandler_DataLoaded);
            FileLogWriter.Close();
            nblog = null;
            Chat_Log_Filter = null;
            Chat_Log_View = null;
            Chat_Log_AFK = null;
            Chat_Log_AFK_Message = null;
            Chat_Log_Search = null;

            Chat_Log_Dir = null;
            Chat_Log_Enable = null;
            Chat_Log_Options = null;
            Chat_Log_Timestamp = null;
            Chat_Log_Max = null;
            Chat_Log_XML = null;
            Chat_Log_By_Date = null;
            Chat_Log_Auto = null;
            Chat_Log_Clear = null;
            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ChatLoggerSettings tmp = (ChatLoggerSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ChatLoggerSettings", TraceLevel.Info);
                LogSettings = new ChatLoggerSettings();
                SettingsProfileHandler.AddSettingGroup(LogSettings);
            }
            else
            {
                TraceLogger.Write("Loading Existing ChatLoggerSettings", TraceLevel.Info);
                LogSettings = tmp;
            }
            foreach (KeyValuePair<string, int> v in Constants.LogTypesDictionary)
            {
                bool found = false;
                foreach (ChatLogOption clo in LogSettings.Options)
                {
                    if (v.Value == clo.Id)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    TraceLogger.Write("Missing LogType in Setting, adding one for " + v.Value, TraceLevel.Warning);
                    ChatLogOption newclo = new ChatLogOption();
                    newclo.Id = v.Value;
                    newclo.View = false;
                    newclo.File = false;
                    LogSettings.Options.Add(newclo);
                    SettingsProfileHandler.Save();
                }
            }
            TypeList.List = LogSettings.Options;
            Chat_Log_Enable.Checked = LogSettings.RecentEnabled;
            Chat_Log_Max.Text = LogSettings.RecentLines.ToString();
            Chat_Log_Timestamp.Checked = LogSettings.IncludeTimestamp;
            Chat_Log_Auto.Checked = LogSettings.AutoLog;
            Chat_Log_By_Date.Checked = LogSettings.LogPerDay;
            Chat_Log_XML.Checked = LogSettings.UseXML;
            Chat_Log_AFK_Message.Text = LogSettings.AFKMessage;
            Day = DateTime.Now.DayOfYear;
            GenerateFileLogPath();
            if (LogSettings.AutoLog)
            {
                CreateFileLog();
            }
            FileLogEnabled = LogSettings.AutoLog;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void GenerateFileLogPath()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            FileLogPath = BasePath + Core.CharacterFilter.Server + "_" + Core.CharacterFilter.Name;
            if (LogSettings.LogPerDay)
            {
                FileLogPath += "_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day;
            }
            if (LogSettings.UseXML)
            {
                FileLogPath += ".xml";
            }
            else
            {
                FileLogPath += ".txt";
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void CreateFileLog()
        {
            TraceLogger.Write("Enter, File Log Path: " + FileLogPath, TraceLevel.Info);
            if (LogSettings.UseXML)
            {
                TraceLogger.Write("XML Log", TraceLevel.Verbose);
                FileLogWriter = new FileLogXMLWriter(FileLogPath);
            }
            else
            {
                TraceLogger.Write("Text Log", TraceLevel.Verbose);
                FileLogWriter = new FileLogTextWriter(FileLogPath);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void DataHandler_DataLoaded(DataLoadedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ChatLoggerData tmp = (ChatLoggerData)DataHandler.GetDataStore(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ChatLoggerData", TraceLevel.Info);
                LogData = new ChatLoggerData();
                DataHandler.AddDataStore(LogData);
            }
            else
            {
                TraceLogger.Write("Loading Existing ChatLoggerData", TraceLevel.Info);
                LogData = tmp;
            }
            foreach(LogItem li in LogData.recentLines)
            {
                AddRecentLogList(li);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    internal class ChatTypeRule
    {
        public int logType;
        private int chatType;
        private string match;
        private bool startsWith;

        internal ChatTypeRule(int LogType, int ChatType, string Match, bool StartsWith)
        {
            logType = LogType;
            chatType = ChatType;
            match = Match;
            startsWith = StartsWith;
        }

        internal bool Test(string text, int type)
        {
            TraceLogger.Write("Enter text " + text + ", type " + type, TraceLevel.Verbose);
            if (chatType == type)
            {
                if (startsWith)
                {
                    bool rtn = text.StartsWith(match);
                    TraceLogger.Write("Exit starts with " + rtn, TraceLevel.Verbose);
                    return rtn;
                }
                else
                {
                    Regex r = new Regex(match);
                    bool rtn = r.IsMatch(text);
                    TraceLogger.Write("Exit regex " + rtn, TraceLevel.Verbose);
                    return rtn;
                }
            }
            TraceLogger.Write("Exit false", TraceLevel.Verbose);
            return false;
        }
    }

    [Serializable]
    public class ChatLoggerSettings : SettingGroup
    {
        public bool RecentEnabled { get; set; }
        public int RecentLines { get; set; }
        public bool AutoLog { get; set; }
        public bool IncludeTimestamp { get; set; }
        public bool UseXML { get; set; }
        public bool LogPerDay { get; set; }
        public string AFKMessage { get; set; }
        [XmlArray("Options"), XmlArrayItem("Option", typeof(ChatLogOption))]
        public RowItemList<ChatLogOption> Options { get; set; }

        public ChatLoggerSettings()
        {
            groupName = "ChatLog";
            RecentEnabled = true;
            RecentLines = 500;
            AutoLog = false;
            IncludeTimestamp = false;
            UseXML = false;
            LogPerDay = false;
            AFKMessage = "";
            Options = new RowItemList<ChatLogOption>();
        }
    }

    [Serializable]
    public class ChatLogOption : IRowItem
    {
        private int id;
        private bool view;
        private bool file;

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                DataRow["Name"].Data = new StringColumnData(Constants.LogTypesInverse(value));
            }
        }
        
        public bool View
        {
            get { return view; }
            set
            {
                view = value;
                DataRow["View"].Data = new BoolColumnData(value);
            }
        }

        public bool File
        {
            get { return file; }
            set
            {
                file = value;
                DataRow["File"].Data = new BoolColumnData(value);
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

        public ChatLogOption()
        {
            DataRow = new Row();
            DataRow.AddColumn("Name", ChatLogger.NAME);
            DataRow.AddColumn("View", ChatLogger.VIEW);
            DataRow.AddColumn("File", ChatLogger.FILE);
        }
    }

    [Serializable]
    public class ChatLoggerData : DataStore
    {
        [XmlArray("RecentLines"), XmlArrayItem("Line", typeof(LogItem))]
        public List<LogItem> recentLines;

        public ChatLoggerData()
        {
            base.dataStoreName = "ChatLog";
            recentLines = new List<LogItem>();
        }
    }

    [Serializable]
    public class LogItem
    {
        public string message;
        public int type;
        public int color;
        public int tag;
        public DateTime timestamp;
        internal Guid id;

        public LogItem()
        {
            message = "";
            type = -1;
            color = -1;
            tag = -1;
            timestamp = DateTime.Now;
            id = Guid.NewGuid();
        }

        public LogItem(string Message, int Type, int Color, int Tag) : this()
        {
            message = Message;
            type = Type;
            color = Color;
            tag = Tag;
        }

        public LogItem(string Message, int Type, int Color, int Tag, DateTime Timestamp) : this(Message, Type, Color, Tag)
        {
            timestamp = Timestamp;
        }
    }
}
