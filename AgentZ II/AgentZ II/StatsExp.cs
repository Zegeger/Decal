using System;
using System.Collections.Generic;
using System.Text;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using Zegeger.Decal.Chat;

namespace Zegeger.Decal.Plugins.AgentZ_II
{
    public enum XPChangeType
    {
        Total,
        Unassigned,
        Monster,
        Lum
    }
    public class XPChangeEventArgs : EventArgs
    {
        public XPChangeType Type { get; private set; }
        public long Amount { get; private set; }

        public XPChangeEventArgs(XPChangeType type, long amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    internal static class XPStats
    {
        static CoreManager Core;
        static public long LastTotalChange { get; private set; }
        static public long LastUnassignedChange { get; private set; }
        static public long LastLumChange { get; private set; }
        static public long MonsterXP { get; private set; }
        static public int KillsToLevel { get; private set; }

        static public long XPAtThisLevel { get; private set; }
        static public long XPAtNextLevel { get; private set; }
        static public long XPForThisLevel { get; private set; }
        static public long XPToNextLevel
        {
            get
            {
                if (Core != null)
                    return Core.CharacterFilter.XPToNextLevel;
                return 0;
            }
        }

        //Totals
        static public double TotalXPStart { get; private set; }
        static public DateTime TotalStartTime { get; private set; }
        static public TimeSpan TotalDuration { get; private set; }
        static public double TotalXP { get; private set; }
        static public double TotalXPPerHour { get; private set; }
        static public double TotalLumStart { get; private set; }
        static public double TotalLum { get; private set; }
        static public double TotalLumPerHour { get; private set; }

        //Session
        static public double SessionXPStart { get; private set; }
        static public DateTime SessionStartTime { get; private set; }
        static public TimeSpan SessionDuration { get; private set; }
        static public double SessionXP { get; private set; }
        static public double SessionXPPerHour { get; private set; }
        static public double SessionLumStart { get; private set; }
        static public double SessionLum { get; private set; }
        static public double SessionLumPerHour { get; private set; }

        static public long FiveMinAvg { get; private set; }

        static private EchoFilter Echo;
        static public long MaxLum { get; private set; }
        static public long CurrentLum { get; private set; }

        static public bool AutoReset { get; set; }

        //Events
        static public event EventHandler LevelUp;
        static public event EventHandler<XPChangeEventArgs> XPChange;
        static public event EventHandler StatsUpdated;

        static bool TotalChange = false;
        static bool UnassChange = false;

        static ZTimer XPTimer;

        static List<long> FiveMinList;

        static public void StartUp(CoreManager core, EchoFilter echo)
        {
            Core = core;
            XPTimer = ZTimer.CreateInstance(XPTimerCallback);
            Core.CharacterFilter.ChangeExperience += new EventHandler<ChangeExperienceEventArgs>(CharacterFilter_ChangeExperience);
            Core.CharacterFilter.LoginComplete += new EventHandler(CharacterFilter_LoginComplete);
            FiveMinList = new List<long>();
            FiveMinList.AddRange(new long[] { 0, 0, 0, 0, 0, 0 });
            AutoReset = false;
            echo.LumChange += new EventHandler<LumChangeEventArgs>(echo_LumChange);
            Echo = echo;
        }

        static void echo_LumChange(object sender, LumChangeEventArgs e)
        {
            LastLumChange = e.TotalLum - CurrentLum;
            CurrentLum = e.TotalLum;
            UpdateLum();
            RaiseXPChange(XPChangeType.Lum, LastLumChange);
        }

        static void XPTimerCallback(object state)
        {
            TotalDuration = DateTime.Now - TotalStartTime;
            SessionDuration = DateTime.Now - SessionStartTime;
            UpdateXPPerHour();
            Update5MinXP();
            UpdateLumPerHour();
            if (StatsUpdated != null)
                StatsUpdated(null, new EventArgs());
        }

        static long lastMin = 0;

        static void Update5MinXP()
        {
            if (TotalDuration.Minutes != lastMin)
            {
                lastMin = TotalDuration.Minutes;
                FiveMinList.RemoveAt(5);
                FiveMinList.Insert(0, 0);
                long XP = 0;
                for (int i = 1; i <= 5; i++)
                {
                    XP += FiveMinList[i];
                }
                FiveMinAvg = XP * 12;
            }
        }

        static void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            ZChatWrapper.ChatBoxMessage += new ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            
            TotalStartTime = DateTime.Now;

            TotalXP = 0;
            TotalXPStart = Core.CharacterFilter.TotalXP;
            TotalXPPerHour = 0;

            CurrentLum = Echo.TotalLum;
            MaxLum = Echo.MaxLum;

            TotalLum = 0;
            TotalLumStart = CurrentLum;
            TotalLumPerHour = 0;

            Level();
            ResetSession();
            XPTimer.Start(1000);
        }

        static void ZChatWrapper_ChatBoxMessage(ChatBoxMessageEventArgs e)
        {
            if (e.Text.StartsWith("You are now level") && e.Color == Constants.ChatClasses("Level"))
            {
                Level();
                if (LevelUp != null)
                    LevelUp(null, new EventArgs());
            }
        }

        static public void ResetSession()
        {
            SessionStartTime = DateTime.Now;
            SessionDuration = DateTime.Now - SessionStartTime;
            
            SessionXP = 0;
            SessionXPStart = Core.CharacterFilter.TotalXP;
            SessionXPPerHour = 0;

            SessionLum = 0;
            SessionLumStart = CurrentLum;
            SessionLumPerHour = 0;

            if (StatsUpdated != null)
                StatsUpdated(null, new EventArgs());
        }

        static void Level()
        {
            FileService fs = Core.FileService as FileService;
            XPAtThisLevel = fs.LevelTables.CharacterXP[Core.CharacterFilter.Level];
            XPAtNextLevel = fs.LevelTables.CharacterXP[Core.CharacterFilter.Level + 1];
            XPForThisLevel = XPAtNextLevel - XPAtThisLevel;
        }

        static void UpdateXP()
        {
            if (AutoReset && 0.1 * LastTotalChange > SessionXP)
            {
                ResetSession();
                SessionXPStart -= LastTotalChange;
            }
            TotalXP = Core.CharacterFilter.TotalXP - TotalXPStart;
            SessionXP = Core.CharacterFilter.TotalXP - SessionXPStart;
            UpdateXPPerHour();
        }

        static void UpdateLum()
        {
            TotalLum = CurrentLum - TotalLumStart;
            SessionLum = CurrentLum - SessionLumStart;
            UpdateLumPerHour();
        }

        static void UpdateXPPerHour()
        {
            if(XPStats.TotalDuration.TotalSeconds >= 1)
                TotalXPPerHour = (TotalXP / Math.Round(XPStats.TotalDuration.TotalSeconds) * 60 * 60);
            if (XPStats.SessionDuration.TotalSeconds >= 1)
                SessionXPPerHour = (SessionXP / Math.Round(XPStats.SessionDuration.TotalSeconds) * 60 * 60);
        }

        static void UpdateLumPerHour()
        {
            if (XPStats.TotalDuration.TotalSeconds >= 1)
                TotalLumPerHour = (TotalLum / Math.Round(XPStats.TotalDuration.TotalSeconds) * 60 * 60);
            if (XPStats.SessionDuration.TotalSeconds >= 1)
                SessionLumPerHour = (SessionLum / Math.Round(XPStats.SessionDuration.TotalSeconds) * 60 * 60);
        }

        private static void RaiseXPChange(XPChangeType type, long amount)
        {
            if (XPChange != null)
            {
                XPChange(null, new XPChangeEventArgs(type, amount));
            }
        }

        static void CharacterFilter_ChangeExperience(object sender, ChangeExperienceEventArgs e)
        {
            bool xpdone = false;
            if (e.Type == PlayerXPEventType.Total)
            {
                LastTotalChange = e.Amount;
                FiveMinList[0] += LastTotalChange;
                TotalChange = true;
                UpdateXP();
                RaiseXPChange(XPChangeType.Total, LastTotalChange);
            }
            if (e.Type == PlayerXPEventType.Unassigned)
            {
                LastUnassignedChange = e.Amount;
                UnassChange = true;
                if (LastUnassignedChange < 0)
                {
                    xpdone = true;
                }
                RaiseXPChange(XPChangeType.Unassigned, LastUnassignedChange);
            }
            if (TotalChange && UnassChange)
            {
                xpdone = true;
                if (LastTotalChange > 0)
                {
                    MonsterXP = (long)(LastUnassignedChange - (Math.Ceiling(0.1 * (LastTotalChange - LastUnassignedChange))));
                    if (MonsterXP < 0)
                    {
                        MonsterXP = 0;
                    }
                    if (MonsterXP != 0)
                    {
                        KillsToLevel = (int)Math.Ceiling((double)Core.CharacterFilter.XPToNextLevel / MonsterXP);
                    }
                    RaiseXPChange(XPChangeType.Monster, MonsterXP);
                }
            }
            if (xpdone)
            {
                TotalChange = false;
                UnassChange = false;
            }
        }
    }
}
