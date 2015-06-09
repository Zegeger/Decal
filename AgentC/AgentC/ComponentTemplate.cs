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
    internal class CompTemplate : Component
    {
        internal const int NAME = 0;
        internal const int VIEW = 2;
        internal const int FILE = 4;

        internal CompTemplateData CompTemplateData;
        internal CompTemplateSettings CompTemplateSettings;

        DecalList CompTemplateList;

        private static IList CompTemplate_Options;  

        internal CompTemplate(CoreManager core, IView view)
            : base(core, view)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ComponentName = "CompTemplate";
            Critical = false;

            CompTemplate_Options = (IList)View["Chat_Log_Options"];

            SettingsProfileHandler.registerType(typeof(CompTemplateSettings));
            SettingsProfileHandler.registerType(typeof(CompTemplateOption));
            DataHandler.registerType(typeof(CompTemplateData));
            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            DataHandler.DataLoaded += new DataLoadedEvent(DataHandler_DataLoaded);

            CompTemplateList = new DecalList(CompTemplate_Options);
            CompTemplateList.HighlightColor = Constants.GUIColors("Highlight");
            CompTemplateList.HighlightColumn = new int[] { };
            
            TraceLogger.Write("Initialized Component: " + ComponentName, TraceLevel.Info);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Service_ChatTextComplete(ChatTextCompleteEventArgs e)
        {
            TraceLogger.Write("Enter " + e.Text, TraceLevel.Verbose);
            try
            {
                
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

            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            CompTemplateSettings tmp = (CompTemplateSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ChatLoggerSettings", TraceLevel.Info);
                CompTemplateSettings = new CompTemplateSettings();
                SettingsProfileHandler.AddSettingGroup(CompTemplateSettings);
            }
            else
            {
                TraceLogger.Write("Loading Existing ChatLoggerSettings", TraceLevel.Info);
                CompTemplateSettings = tmp;
            }
            CompTemplateList.List = CompTemplateSettings.Options;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void DataHandler_DataLoaded(DataLoadedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            CompTemplateData tmp = (CompTemplateData)DataHandler.GetDataStore(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ChatLoggerData", TraceLevel.Info);
                CompTemplateData = new CompTemplateData();
                DataHandler.AddDataStore(CompTemplateData);
            }
            else
            {
                TraceLogger.Write("Loading Existing ChatLoggerData", TraceLevel.Info);
                CompTemplateData = tmp;
            }
            
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    [Serializable]
    public class CompTemplateSettings : SettingGroup
    {
        [XmlArray("Options"), XmlArrayItem("Option", typeof(CompTemplateOption))]
        public RowItemList<CompTemplateOption> Options { get; set; }

        public CompTemplateSettings()
        {
            groupName = "CompTemplate";
            Options = new RowItemList<CompTemplateOption>();
        }
    }

    [Serializable]
    public class CompTemplateOption : IRowItem
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

        public CompTemplateOption()
        {
            DataRow = new Row();
            DataRow.AddColumn("Name", ChatLogger.NAME);
            DataRow.AddColumn("View", ChatLogger.VIEW);
            DataRow.AddColumn("File", ChatLogger.FILE);
        }
    }

    [Serializable]
    public class CompTemplateData : DataStore
    {
        
        public CompTemplateData()
        {
            base.dataStoreName = "ChatLog";
        }
    }
}
