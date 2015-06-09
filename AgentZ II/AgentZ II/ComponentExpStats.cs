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
using Decal.Filters;
using Zegeger.Decal.VVS;
using Zegeger.Data;
using Zegeger.Decal.Chat;
using Zegeger.Diagnostics;
using Zegeger.Decal.Data;
using Zegeger.Decal.Controls;

namespace Zegeger.Decal.Plugins.AgentZ_II
{
    internal class ComponentExpStats : Component
    {
        internal ExpStatsSettings ExpStatsSettings;

        private static IStaticText Char_Name;
        private static IStaticText XP_Total;
        private static IStaticText Level_Num;
        private static IStaticText XP_Next;
        private static IStaticText Session_Time;
        private static IStaticText XP_Hour;
        private static IStaticText XP_Min;
        private static IStaticText XP_Change;
        private static IStaticText XP_Session;
        private static IStaticText XP_Un;
        private static IStaticText Total_Time;
        private static IStaticText Total_XP;
        private static IStaticText Total_XP_Hour;
        private static IStaticText Total_XP_Min;
        private static IStaticText XP_5Min;
        private static IProgressBar XP_Bar;
        private static IButton Reset_Time;
        private static ICheckBox XP_Auto;
        private static ICheckBox XP_Filter;
        private static ITextBox XP_Filter_Threshold;

        private static IProgressBar Lum_Bar;
        private static IStaticText Lum_Total;
        private static IStaticText Session_Time2;
        private static IStaticText Total_Time2;
        private static IStaticText Session_Lum;
        private static IStaticText Session_Lum_Hour;
        private static IStaticText Session_Lum_Min;
        private static IStaticText Total_Lum;
        private static IStaticText Total_Lum_Hour;
        private static IStaticText Total_Lum_Min;
        private static IStaticText Lum_Change;

        internal ComponentExpStats(CoreManager core, IView view)
            : base(core, view)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ComponentName = "ExpStats";
            Critical = false;

            Char_Name = (IStaticText)View["Char_Name"];
            XP_Total = (IStaticText)View["XP_Total"];
            Level_Num = (IStaticText)View["Level_Num"];
            XP_Next = (IStaticText)View["aXP_Next"];
            Session_Time = (IStaticText)View["Session_Time"];
            XP_Hour = (IStaticText)View["XP_Hour"];
            XP_Min = (IStaticText)View["XP_Min"];
            XP_Change = (IStaticText)View["XP_Change"];
            XP_Session = (IStaticText)View["XP_Session"];
            XP_Un = (IStaticText)View["XP_Un"];
            Total_Time = (IStaticText)View["Total_Time"];
            Total_XP = (IStaticText)View["Total_XP"];
            Total_XP_Hour = (IStaticText)View["Total_XP_Hour"];
            Total_XP_Min = (IStaticText)View["Total_XP_Min"];
            XP_5Min = (IStaticText)View["XP_5Min"];
            XP_Bar = (IProgressBar)View["XP_Bar"];
            XP_Bar.PreText = "";
            Reset_Time = (IButton)View["Reset_Time"];
            Reset_Time.Click += new EventHandler<MVControlEventArgs>(Reset_Time_Click);
            XP_Auto = (ICheckBox)View["XP_Auto"];
            XP_Filter = (ICheckBox)View["XP_Filter"];
            XP_Filter_Threshold = (ITextBox)View["XP_Filter_Threshold"];

            Lum_Bar = (IProgressBar)View["Lum_Bar"];
            Lum_Bar.PreText = "";
            Lum_Total = (IStaticText)View["aLum_Total"];
            Session_Time2 = (IStaticText)View["Session_Time2"];
            Total_Time2 = (IStaticText)View["Total_Time2"];
            Session_Lum = (IStaticText)View["Session_Lum"];
            Session_Lum_Hour = (IStaticText)View["Session_Lum_Hour"];
            Session_Lum_Min = (IStaticText)View["Session_Lum_Min"];
            Total_Lum = (IStaticText)View["Total_Lum"];
            Total_Lum_Hour = (IStaticText)View["Total_Lum_Hour"];
            Total_Lum_Min = (IStaticText)View["Total_Lum_Min"];
            Lum_Change = (IStaticText)View["Lum_Change"];

            SettingsProfileHandler.registerType(typeof(ExpStatsSettings));
            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);

            XPStats.LevelUp += new EventHandler(XPStats_LeveledUp);
            XPStats.XPChange += new EventHandler<XPChangeEventArgs>(XPStats_XPChange);
            XPStats.StatsUpdated += new EventHandler(XPStats_StatsUpdated);
            
            TraceLogger.Write("Initialized Component: " + ComponentName, TraceLevel.Info);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Reset_Time_Click(object sender, MVControlEventArgs e)
        {
            XPStats.ResetSession();
        }

        void XPStats_StatsUpdated(object sender, EventArgs e)
        {
            UpdateXPStats();
        }

        void XPStats_XPChange(object sender, XPChangeEventArgs e)
        {
            if (e.Type == XPChangeType.Total)
            {
                UpdateXP();
            }
            if (e.Type == XPChangeType.Unassigned)
            {
                XP_Un.Text = Core.CharacterFilter.UnassignedXP.ToString("n0");
            }
            if (e.Type == XPChangeType.Lum)
            {

            }
        }

        void XPStats_LeveledUp(object sender, EventArgs e)
        {
            Level_Num.Text = Core.CharacterFilter.Level.ToString();
            UpdateXP();
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
            Char_Name.Text = Core.CharacterFilter.Name;
            Level_Num.Text = Core.CharacterFilter.Level.ToString();
            UpdateXP();
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal void UpdateXP()
        {
            XP_Total.Text = Core.CharacterFilter.TotalXP.ToString("n0");
            XP_Next.Text = Core.CharacterFilter.XPToNextLevel.ToString("n0");
            XP_Un.Text = Core.CharacterFilter.UnassignedXP.ToString("n0");
            XP_Change.Text = XPStats.LastTotalChange.ToString("n0");
            int percent = (int)Math.Round((((double)XPStats.XPForThisLevel - (double)XPStats.XPToNextLevel) / (double)XPStats.XPForThisLevel) * 100);
            if (percent < 0)
                XP_Bar.Position = 0;
            else if (percent > 100)
                XP_Bar.Position = 100;
            else
                XP_Bar.Position = percent;

            Lum_Total.Text = String.Format("{0} / {1}", XPStats.TotalLum.ToString("n0"), XPStats.MaxLum.ToString("n0"));
            Lum_Change.Text = XPStats.LastLumChange.ToString("n0");
            int percent2 = (int)Math.Round((((double)XPStats.CurrentLum) / (double)XPStats.MaxLum) * 100);
            if (percent2 < 0)
                Lum_Bar.Position = 0;
            else if (percent2 > 100)
                Lum_Bar.Position = 100;
            else
                Lum_Bar.Position = percent2;

            UpdateXPStats();
        }

        internal void UpdateXPStats()
        {
            Total_Time.Text = TimeSpanToString(DateTime.Now - XPStats.TotalStartTime);
            Total_XP.Text = XPStats.TotalXP.ToString("n0");
            Total_XP_Hour.Text = XPStats.TotalXPPerHour.ToString("n0");
            Total_XP_Min.Text = (Math.Round(XPStats.TotalXPPerHour / 60)).ToString("n0");

            Session_Time.Text = TimeSpanToString(DateTime.Now - XPStats.SessionStartTime);
            XP_Session.Text = XPStats.SessionXP.ToString("n0");
            XP_Hour.Text = XPStats.SessionXPPerHour.ToString("n0");
            XP_Min.Text = (Math.Round(XPStats.SessionXPPerHour / 60)).ToString("n0");

            Total_Time2.Text = TimeSpanToString(DateTime.Now - XPStats.TotalStartTime);
            Total_Lum.Text = XPStats.TotalLum.ToString("n0");
            Total_Lum_Hour.Text = XPStats.TotalLumPerHour.ToString("n0");
            Total_Lum_Min.Text = (Math.Round(XPStats.TotalLumPerHour / 60)).ToString("n0");

            Session_Time2.Text = TimeSpanToString(DateTime.Now - XPStats.SessionStartTime);
            Session_Lum.Text = XPStats.SessionLum.ToString("n0");
            Session_Lum_Hour.Text = XPStats.SessionLumPerHour.ToString("n0");
            Session_Lum_Min.Text = (Math.Round(XPStats.SessionLumPerHour / 60)).ToString("n0");

            XP_5Min.Text = XPStats.FiveMinAvg.ToString("n0");
        }

        private string TimeSpanToString(TimeSpan ts)
        {
            return ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
        }

        internal override void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.ShuttingDown;
            SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            XPStats.LevelUp -= new EventHandler(XPStats_LeveledUp);
            XPStats.XPChange -= new EventHandler<XPChangeEventArgs>(XPStats_XPChange);
            XPStats.StatsUpdated -= new EventHandler(XPStats_StatsUpdated);
            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ExpStatsSettings tmp = (ExpStatsSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ChatLoggerSettings", TraceLevel.Info);
                ExpStatsSettings = new ExpStatsSettings();
                SettingsProfileHandler.AddSettingGroup(ExpStatsSettings);
            }
            else
            {
                TraceLogger.Write("Loading Existing ChatLoggerSettings", TraceLevel.Info);
                ExpStatsSettings = tmp;
            }

            ExpStatsSettings.AutoReset.AttachControl(XP_Auto);
            XPStats.AutoReset = ExpStatsSettings.AutoReset.Value;
            ExpStatsSettings.AutoReset.ControlSettingChanged += new ControlSettingChangedEvent<bool>(AutoReset_ControlSettingChanged);

            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void AutoReset_ControlSettingChanged(ControlSettingChangedEventArgs<bool> e)
        {
            XPStats.AutoReset = e.NewValue;
        }
    }

    [Serializable]
    public class ExpStatsSettings : SettingGroup
    {
        public CheckboxSetting AutoReset { get; set; }

        public ExpStatsSettings()
        {
            groupName = "ExpStats";
            AutoReset = new CheckboxSetting(false);
        }
    }
}
