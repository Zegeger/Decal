using System;
using System.Collections.Generic;
using System.Text;
using Zegeger.Decal.VVS;
using Decal.Adapter;
using Zegeger.Diagnostics;
using Zegeger.Decal.Data;

namespace Zegeger.Decal.Plugins.AgentZ_II
{
    class SettingsComponent : Component
    {
        private SettingsData data = new SettingsData();

        static ICombo Settings_Drop;
        static ITextBox Settings_Text;
        static IButton Settings_Add;
        static IButton Settings_Rename;
        static IButton Settings_Delete;
        static IStaticText Settings_Active;
        static IButton Settings_Load;

        public Guid SettingId
        {
            get { return data.SettingsProfile; }
        }

        public SettingsComponent(CoreManager core, IView view)
            : base(core, view)
        {
            TraceLogger.Write("Enter");
            ComponentName = "Settings";
            Critical = true;

            DataHandler.registerType(typeof(SettingsData));
            DataHandler.DataLoaded += new DataLoadedEvent(DataHandler_DataLoaded);
            SettingsProfileHandler.SettingsLoaded += new SettingsLoadedEvent(SettingsProfileHandler_SettingsLoaded);
            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);

            Settings_Active = (IStaticText)View["Settings_Active"];
            Settings_Drop = (ICombo)View["Settings_Drop"];
            Settings_Drop.Change += new EventHandler<MVIndexChangeEventArgs>(Settings_Drop_Change);
            Settings_Text = (ITextBox)View["Settings_Text"];
            Settings_Add = (IButton)View["Settings_Add"];
            Settings_Add.Click += new EventHandler<MVControlEventArgs>(Settings_Add_Click);
            Settings_Rename = (IButton)View["Settings_Rename"];
            Settings_Rename.Click += new EventHandler<MVControlEventArgs>(Settings_Rename_Click);
            Settings_Delete = (IButton)View["Settings_Delete"];
            Settings_Delete.Click += new EventHandler<MVControlEventArgs>(Settings_Delete_Click);
            Settings_Load = (IButton)View["Settings_Load"];
            Settings_Load.Click += new EventHandler<MVControlEventArgs>(Settings_Load_Click);
            TraceLogger.Write("Exit");
        }

        void Settings_Load_Click(object sender, MVControlEventArgs e)
        {
            TraceLogger.Write("Enter, Profile " + Settings_Drop.Text[Settings_Drop.Selected]);
            SettingsProfileHandler.requestSettingProfile(Settings_Drop.Text[Settings_Drop.Selected]);
            TraceLogger.Write("Exit");
        }

        void Settings_Delete_Click(object sender, MVControlEventArgs e)
        {
            TraceLogger.Write("Enter, Profile " + Settings_Drop.Text[Settings_Drop.Selected]);
            if (!SettingsProfileHandler.DeleteProfile(Settings_Drop.Text[Settings_Drop.Selected]))
            {
                PluginCore.WriteToChatError("Error, failed to delete profile");
            }
            TraceLogger.Write("Exit");
        }

        void Settings_Rename_Click(object sender, MVControlEventArgs e)
        {
            TraceLogger.Write("Enter, Profile " + Settings_Drop.Text[Settings_Drop.Selected] + ", New Name " + Settings_Text.Text);
            if (!SettingsProfileHandler.RenameProfile(Settings_Drop.Text[Settings_Drop.Selected], Settings_Text.Text))
            {
                PluginCore.WriteToChatError("Error, failed to rename profile");
            }
            TraceLogger.Write("Exit");
        }

        void Settings_Add_Click(object sender, MVControlEventArgs e)
        {
            TraceLogger.Write("Enter, New Name " + Settings_Text.Text);
            if (!SettingsProfileHandler.NewProfile(Settings_Text.Text))
            {
                PluginCore.WriteToChatError("Error, failed to add profile");
            }
            TraceLogger.Write("Exit");
        }

        void Settings_Drop_Change(object sender, MVIndexChangeEventArgs e)
        {
            TraceLogger.Write("Enter, Selected " + Settings_Drop.Text[e.Index]);
            Settings_Text.Text = Settings_Drop.Text[e.Index];
            TraceLogger.Write("Exit");
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter, Activating " + e.newProfile.profileName);
            Settings_Active.Text = e.newProfile.profileName;
            Settings_Text.Text = e.newProfile.profileName;
            TraceLogger.Write("Exit");
        }

        void SettingsProfileHandler_SettingsLoaded(SettingsLoadedEventArgs e)
        {
            TraceLogger.Write("Enter");
            Settings_Drop.Clear();
            foreach (SettingsProfile sp in e.Profiles)
            {
                Settings_Drop.Add(sp.profileName);
            }
            TraceLogger.Write("Exit");
        }

        void DataHandler_DataLoaded(DataLoadedEventArgs e)
        {
            TraceLogger.Write("Enter");
            SettingsData tmp = (SettingsData)DataHandler.GetDataStore(ComponentName);
            if (tmp == null)
            {
                data = new SettingsData();
                DataHandler.AddDataStore(data);
            }
            else
            {
                data = tmp;
            }
            State = ComponentState.Running;
            TraceLogger.Write("Exit");
        }

        internal override void PostPluginInit()
        {
            TraceLogger.Write("Enter");
            TraceLogger.Write("Exit");
        }

        internal override void PostLogin()
        {
            TraceLogger.Write("Enter");
            TraceLogger.Write("Exit");
        }

        internal override void Shutdown()
        {
            TraceLogger.Write("Enter");
            State = ComponentState.ShuttingDown;
            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit");
        }
    }

    public class SettingsData : DataStore
    {
        public Guid SettingsProfile;

        public SettingsData()
        {
            dataStoreName = "SettingsData";
            SettingsProfile = SettingsProfileHandler.DefaultGuid;
        }
    }
}
