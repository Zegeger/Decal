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
using Zegeger.Decal.Controls;

namespace Zegeger.Decal.Plugins.AgentC
{
    internal class Alias : Component
    {
        internal const int ENABLED = 0;
        internal const int ABBR = 1;
        internal const int FULL = 2;
        internal const int DELETE = 3;

        internal AliasSettings AliasSettings;

        DecalList AliasList;

        IList Chat_Alias;
        ITextBox Alias_Abbr;
        ITextBox Alias_Full;
        IButton Alias_Add;

        internal Alias(CoreManager core, IView view)
            : base(core, view)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ComponentName = "Alias";
            Critical = false;

            core.CommandLineText += new EventHandler<ChatParserInterceptEventArgs>(core_CommandLineText);

            Chat_Alias = (IList)View["Chat_Alias"];
            Chat_Alias.Click += new dClickedList(Chat_Alias_Click);
            Alias_Abbr = (ITextBox)View["Alias_Abbr"];
            Alias_Full = (ITextBox)View["Alias_Full"];
            Alias_Add = (IButton)View["Alias_Add"];
            Alias_Add.Click += new EventHandler<MVControlEventArgs>(Alias_Add_Click);
            
            SettingsProfileHandler.registerType(typeof(AliasSettings));
            SettingsProfileHandler.registerType(typeof(AliasOption));
            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);

            AliasList = new DecalList(Chat_Alias);
            AliasList.HighlightColor = Constants.GUIColors("Highlight");
            AliasList.HighlightColumn = new int[] { };
            
            TraceLogger.Write("Initialized Component: " + ComponentName, TraceLevel.Info);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void core_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter, Text: " + e.Text, TraceLevel.Noise);
                bool eat = false;
                bool commandMatched = false;
                string command = e.Text;
                foreach(AliasOption alias in AliasSettings.Aliases)
                {
                    if (alias.Enabled)
                    {
                        if (alias.Abbr.StartsWith("/") || alias.Abbr.StartsWith("@"))
                        {
                            if (!commandMatched && command.StartsWith(alias.Abbr))
                            {
                                TraceLogger.Write("Matched command alias " + alias.Abbr + ", replacing with " + alias.Full, TraceLevel.Verbose);
                                eat = true;
                                commandMatched = true;
                                command = alias.Full + command.Substring(alias.Abbr.Length);
                            }
                        }
                        else
                        {
                            if (command.Contains(alias.Abbr))
                            {
                                TraceLogger.Write("Matched alias " + alias.Abbr + ", replacing with " + alias.Full, TraceLevel.Verbose);
                                eat = true;
                                command = command.Replace(alias.Abbr, alias.Full);
                            }
                        }
                    }
                }
                if (eat)
                {
                    TraceLogger.Write("Eating command and sending: " + command, TraceLevel.Info);
                    e.Eat = true;
                    Core.Actions.InvokeChatParser(command);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Alias_Add_Click(object sender, MVControlEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                if (!String.IsNullOrEmpty(Alias_Abbr.Text) && !String.IsNullOrEmpty(Alias_Full.Text))
                {
                    bool found = false;
                    foreach (AliasOption opt in AliasSettings.Aliases)
                    {
                        if (opt.Abbr == Alias_Abbr.Text)
                        {
                            TraceLogger.Write("Updating Alias" + opt.Abbr + " with " + Alias_Full.Text, TraceLevel.Info);
                            opt.Full = Alias_Full.Text;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        TraceLogger.Write("Adding New Alias", TraceLevel.Info);
                        AliasOption newOption = new AliasOption();
                        newOption.Enabled = true;
                        newOption.Abbr = Alias_Abbr.Text;
                        newOption.Full = Alias_Full.Text;
                        AliasSettings.Aliases.Add(newOption);
                    }
                    SettingsProfileHandler.Save();
                }
                else
                {
                    TraceLogger.Write("Failed to add new Alias since the abbr or full field is empty.", TraceLevel.Warning);
                    PluginCore.WriteToChatError("Cannot add Alias entry because one of the fields is empty.");
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void Chat_Alias_Click(object sender, int row, int col)
        {
            try
            {
                TraceLogger.Write("Enter, row: " + row + ", col: " + col, TraceLevel.Noise);
                if (col == ENABLED)
                {
                    TraceLogger.Write("Toggling enabled for " + AliasSettings.Aliases[row].Abbr + " to " + (bool)Chat_Alias[row][ENABLED][0], TraceLevel.Info);
                    AliasSettings.Aliases[row].Enabled = (bool)Chat_Alias[row][ENABLED][0];
                    SettingsProfileHandler.Save();
                }
                else if (col == DELETE)
                {
                    TraceLogger.Write("Deleting " + AliasSettings.Aliases[row].Abbr, TraceLevel.Info);
                    AliasSettings.Aliases.RemoveAt(row);
                    SettingsProfileHandler.Save();
                }
                else
                {
                    TraceLogger.Write("Clicked " + AliasSettings.Aliases[row].Abbr, TraceLevel.Info);
                    Alias_Abbr.Text = AliasSettings.Aliases[row].Abbr;
                    Alias_Full.Text = AliasSettings.Aliases[row].Full;
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
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.ShuttingDown;
            SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            Core.CommandLineText -= new EventHandler<ChatParserInterceptEventArgs>(core_CommandLineText);
            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            AliasSettings tmp = (AliasSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new AliasSettings", TraceLevel.Info);
                AliasSettings = new AliasSettings();
                SettingsProfileHandler.AddSettingGroup(AliasSettings);
            }
            else
            {
                TraceLogger.Write("Loading Existing AliasSettings", TraceLevel.Info);
                AliasSettings = tmp;
            }
            AliasList.List = AliasSettings.Aliases;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    [Serializable]
    public class AliasSettings : SettingGroup
    {
        [XmlArray("Aliases"), XmlArrayItem("Alias", typeof(AliasOption))]
        public RowItemList<AliasOption> Aliases { get; set; }

        public AliasSettings()
        {
            Aliases = new RowItemList<AliasOption>();
            groupName = "Alias";
        }
    }

    [Serializable]
    public class AliasOption : IRowItem
    {
        private bool enabled;
        private string abbr;
        private string full;

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                DataRow["Enabled"].Data = new BoolColumnData(value);
            }
        }
        
        public string Abbr
        {
            get { return abbr; }
            set
            {
                abbr = value;
                DataRow["Abbr"].Data = new StringColumnData(value);
            }
        }

        public string Full
        {
            get { return full; }
            set
            {
                full = value;
                DataRow["Full"].Data = new StringColumnData(value);
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

        public AliasOption()
        {
            DataRow = new Row();
            DataRow.AddColumn("Enabled", Alias.ENABLED);
            DataRow.AddColumn("Abbr", Alias.ABBR);
            DataRow.AddColumn("Full", Alias.FULL);
            DataRow.AddColumn("Delete", Alias.DELETE);
            DataRow["Delete"].Data = new IconColumnData(0x60011F8);
        }
    }
}
