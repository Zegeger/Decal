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
using Decal.Interop.Core;
using Decal.Adapter.Wrappers;
using Zegeger.Decal.VVS;
using Zegeger.Data;
using Zegeger.Decal.Chat;
using Zegeger.Diagnostics;
using Zegeger.Decal.Data;
using Zegeger.Decal.Controls;
using Zegeger.Audio;

namespace Zegeger.Decal.Plugins.AgentU
{
    internal class Util : Component
    {
        internal UtilSettings UtilSettings;

        private static ICheckBox Level_SS;
        private static ICheckBox Status_Text;
        private static ICheckBox Show_Errors;
        private static ICheckBox Logout_Timer;
        private static ICheckBox Util_checkVitalHealthOn;
        private static ICheckBox Util_checkVitalStaminaOn;
        private static ICheckBox Util_checkVitalManaOn;
        private static ITextBox Util_txtVitalHealthPer;
        private static ITextBox Util_txtVitalStaminaPer;
        private static ITextBox Util_txtVitalManaPer;
        private static ICombo Util_cboVitalHealthSound;
        private static ICombo Util_cboVitalStaminaSound;
        private static ICombo Util_cboVitalManaSound;

        ZTimer LevelSSTimer;
        PluginHost Host;
        SoundManager Sounds;
        private Dictionary<CharFilterVitalType, string> SoundPlaying;

        internal Util(CoreManager core, PluginHost host, IView view, string dllPath)
            : base(core, view)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ComponentName = "Util";
            Critical = false;

            Host = host;

            Level_SS = (ICheckBox)View["Level_SS"];
            Status_Text = (ICheckBox)View["Status_Text"];
            Show_Errors = (ICheckBox)View["Show_Errors"];
            Logout_Timer = (ICheckBox)View["Logout_Timer"];
            Util_checkVitalHealthOn = (ICheckBox)View["Util_checkVitalHealthOn"];
            Util_checkVitalStaminaOn = (ICheckBox)View["Util_checkVitalStaminaOn"];
            Util_checkVitalManaOn = (ICheckBox)View["Util_checkVitalManaOn"];
            Util_txtVitalHealthPer = (ITextBox)View["Util_txtVitalHealthPer"];
            Util_txtVitalStaminaPer = (ITextBox)View["Util_txtVitalStaminaPer"];
            Util_txtVitalManaPer = (ITextBox)View["Util_txtVitalManaPer"];
            Util_cboVitalHealthSound = (ICombo)View["Util_cboVitalHealthSound"];
            Util_cboVitalHealthSound.Add("");
            Util_cboVitalStaminaSound = (ICombo)View["Util_cboVitalStaminaSound"];
            Util_cboVitalStaminaSound.Add("");
            Util_cboVitalManaSound = (ICombo)View["Util_cboVitalManaSound"];
            Util_cboVitalManaSound.Add("");

            Sounds = new SoundManager(Path.Combine(dllPath, Path.Combine("Sounds", "Loop")));
            foreach (string sound in Sounds.SoundFileNames)
            {
                Util_cboVitalHealthSound.Add(sound);
                Util_cboVitalStaminaSound.Add(sound);
                Util_cboVitalManaSound.Add(sound);
            }

            SoundPlaying = new Dictionary<CharFilterVitalType, string>();
            SoundPlaying.Add(CharFilterVitalType.Health, "");
            SoundPlaying.Add(CharFilterVitalType.Stamina, "");
            SoundPlaying.Add(CharFilterVitalType.Mana, "");
            
            SettingsProfileHandler.registerType(typeof(UtilSettings));
            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);

            LevelSSTimer = ZTimer.CreateInstance(LevelTimerCallback);
            LevelSSTimer.Repeat = false;

            CommandClass cc = CommandHandler.CreateCommandClass("Util");
            CommandGroup cg = cc.CreateCommandGroup("Take Screenshot");
            cg.AddCommand("TakeSS", TakeSSCommand);
            
            TraceLogger.Write("Initialized Component: " + ComponentName, TraceLevel.Info);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void CharacterFilter_ChangeVital(object sender, ChangeVitalEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (UtilSettings.HealthVitalAlarm.Value && e.Type == CharFilterVitalType.Health)
            {
                TraceLogger.Write("Current Health " + Core.Actions.Vital[VitalType.CurrentHealth], TraceLevel.Verbose);
                double percent = (double)Core.Actions.Vital[VitalType.CurrentHealth] / (double)Core.Actions.Vital[VitalType.MaximumHealth];
                if (percent <= UtilSettings.HealthPercentage.Value)
                {
                    TraceLogger.Write("Percent " + percent + " <= " + UtilSettings.HealthPercentage.Value, TraceLevel.Verbose);
                    if (!String.IsNullOrEmpty(UtilSettings.HealthSound.Value))
                    {
                        TraceLogger.Write("Sound File" + UtilSettings.HealthSound.Value, TraceLevel.Verbose);
                        if (String.IsNullOrEmpty(SoundPlaying[CharFilterVitalType.Health]))
                        {
                            TraceLogger.Write("Sound Not Playing " + UtilSettings.HealthSound.Value, TraceLevel.Verbose);
                            Sounds.LoopSound(UtilSettings.HealthSound.Value);
                            SoundPlaying[CharFilterVitalType.Health] = UtilSettings.HealthSound.Value;
                        }
                    }
                }
                else
                {
                    TraceLogger.Write("Percent " + percent + " > " + UtilSettings.HealthPercentage.Value, TraceLevel.Verbose);
                    if (!String.IsNullOrEmpty(SoundPlaying[CharFilterVitalType.Health]))
                    {
                        TraceLogger.Write("Sound Playing " + SoundPlaying[CharFilterVitalType.Health], TraceLevel.Verbose);
                        Sounds.StopSound(SoundPlaying[CharFilterVitalType.Health]);
                        SoundPlaying[CharFilterVitalType.Health] = "";
                    }
                }
            }
            if (UtilSettings.StaminaVitalAlarm.Value && e.Type == CharFilterVitalType.Stamina)
            {
                TraceLogger.Write("Current Stamina " + Core.Actions.Vital[VitalType.CurrentStamina], TraceLevel.Verbose);
                double percent = (double)Core.Actions.Vital[VitalType.CurrentStamina] / (double)Core.Actions.Vital[VitalType.MaximumStamina];
                if (percent <= UtilSettings.StaminaPercentage.Value)
                {
                    TraceLogger.Write("Percent " + percent + " <= " + UtilSettings.StaminaPercentage.Value, TraceLevel.Verbose);
                    if (!String.IsNullOrEmpty(UtilSettings.StaminaSound.Value))
                    {
                        TraceLogger.Write("Sound File" + UtilSettings.StaminaSound.Value, TraceLevel.Verbose);
                        if (String.IsNullOrEmpty(SoundPlaying[CharFilterVitalType.Stamina]))
                        {
                            TraceLogger.Write("Sound Not Playing " + UtilSettings.StaminaSound.Value, TraceLevel.Verbose);
                            Sounds.LoopSound(UtilSettings.StaminaSound.Value);
                            SoundPlaying[CharFilterVitalType.Stamina] = UtilSettings.StaminaSound.Value;
                        }
                    }
                }
                else
                {
                    TraceLogger.Write("Percent " + percent + " > " + UtilSettings.StaminaPercentage.Value, TraceLevel.Verbose);
                    if (!String.IsNullOrEmpty(SoundPlaying[CharFilterVitalType.Stamina]))
                    {
                        TraceLogger.Write("Sound Playing " + SoundPlaying[CharFilterVitalType.Stamina], TraceLevel.Verbose);
                        Sounds.StopSound(SoundPlaying[CharFilterVitalType.Stamina]);
                        SoundPlaying[CharFilterVitalType.Stamina] = "";
                    }
                }
            }
            if (UtilSettings.ManaVitalAlarm.Value && e.Type == CharFilterVitalType.Mana)
            {
                TraceLogger.Write("Current Mana " + Core.Actions.Vital[VitalType.CurrentMana], TraceLevel.Verbose);
                double percent = (double)Core.Actions.Vital[VitalType.CurrentMana] / (double)Core.Actions.Vital[VitalType.MaximumMana];
                if (percent <= UtilSettings.ManaPercentage.Value)
                {
                    TraceLogger.Write("Percent " + percent + " <= " + UtilSettings.ManaPercentage.Value, TraceLevel.Verbose);
                    if (!String.IsNullOrEmpty(UtilSettings.ManaSound.Value))
                    {
                        TraceLogger.Write("Sound File" + UtilSettings.ManaSound.Value, TraceLevel.Verbose);
                        if (String.IsNullOrEmpty(SoundPlaying[CharFilterVitalType.Mana]))
                        {
                            TraceLogger.Write("Sound Not Playing " + UtilSettings.ManaSound.Value, TraceLevel.Verbose);
                            Sounds.LoopSound(UtilSettings.ManaSound.Value);
                            SoundPlaying[CharFilterVitalType.Mana] = UtilSettings.ManaSound.Value;
                        }
                    }
                }
                else
                {
                    TraceLogger.Write("Percent " + percent + " > " + UtilSettings.ManaPercentage.Value, TraceLevel.Verbose);
                    if (!String.IsNullOrEmpty(SoundPlaying[CharFilterVitalType.Mana]))
                    {
                        TraceLogger.Write("Sound Playing " + SoundPlaying[CharFilterVitalType.Mana], TraceLevel.Verbose);
                        Sounds.StopSound(SoundPlaying[CharFilterVitalType.Mana]);
                        SoundPlaying[CharFilterVitalType.Mana] = "";
                    }
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Hooks_StatusTextIntercept(string bstrText, ref bool bEat)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (UtilSettings.StatusText.Value)
            {
                TraceLogger.Write("Eating Status Text " + bstrText, TraceLevel.Info);
                bEat = true;
                if (UtilSettings.ShowErrors.Value)
                {
                    //TODO
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void TakeSSCommand(CommandIssuedCallbackArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            LevelSSTimer.Start(100);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void LevelTimerCallback(object sender)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            uint key = (uint)Core.QueryKeyBoardMap("CaptureScreenshot");
            TraceLogger.Write("Capture Screenshot key " + key, TraceLevel.Info);
            Zegeger.Input.VirtualKeyboard.SendKey(key);
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
            ZChatWrapper.ChatBoxMessage += new ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            Host.Underlying.Hooks.StatusTextIntercept += new IACHooksEvents_StatusTextInterceptEventHandler(Hooks_StatusTextIntercept);
            Core.CharacterFilter.ChangeVital += new EventHandler<ChangeVitalEventArgs>(CharacterFilter_ChangeVital);
            if (UtilSettings.ExtendLogout.Value)
            {
                TraceLogger.Write("Extending log out timer.", TraceLevel.Info);
                Core.Actions.SetIdleTime(double.MaxValue);
            }
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void ZChatWrapper_ChatBoxMessage(ChatBoxMessageEventArgs e)
        {
            if (UtilSettings.LevelSS.Value)
            {
                if (e.Text.StartsWith("You are now level") && e.Color == Constants.ChatClasses("Level"))
                {
                    LevelSSTimer.Start(1000);
                }
            }
        }

        internal override void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.ShuttingDown;
            SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            ZChatWrapper.ChatBoxMessage -= new ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            Host.Underlying.Hooks.StatusTextIntercept -= new IACHooksEvents_StatusTextInterceptEventHandler(Hooks_StatusTextIntercept);
            Core.CharacterFilter.ChangeVital -= new EventHandler<ChangeVitalEventArgs>(CharacterFilter_ChangeVital);
            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            UtilSettings tmp = (UtilSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ChatLoggerSettings", TraceLevel.Info);
                UtilSettings = new UtilSettings();
                SettingsProfileHandler.AddSettingGroup(UtilSettings);
            }
            else
            {
                TraceLogger.Write("Loading Existing ChatLoggerSettings", TraceLevel.Info);
                UtilSettings = tmp;
            }
            //UtilList.List = UtilSettings.Options;
            UtilSettings.LevelSS.AttachControl(Level_SS);
            UtilSettings.StatusText.AttachControl(Status_Text);
            UtilSettings.ShowErrors.AttachControl(Show_Errors);
            UtilSettings.ExtendLogout.AttachControl(Logout_Timer);
            UtilSettings.ExtendLogout.ControlSettingChanged += new ControlSettingChangedEvent<bool>(ExtendLogout_ControlSettingChanged);
            UtilSettings.HealthVitalAlarm.AttachControl(Util_checkVitalHealthOn);
            UtilSettings.StaminaVitalAlarm.AttachControl(Util_checkVitalStaminaOn);
            UtilSettings.ManaVitalAlarm.AttachControl(Util_checkVitalManaOn);
            UtilSettings.HealthPercentage.AttachControl(Util_txtVitalHealthPer);
            UtilSettings.StaminaPercentage.AttachControl(Util_txtVitalStaminaPer);
            UtilSettings.ManaPercentage.AttachControl(Util_txtVitalManaPer);
            UtilSettings.HealthSound.AttachControl(Util_cboVitalHealthSound);
            UtilSettings.StaminaSound.AttachControl(Util_cboVitalStaminaSound);
            UtilSettings.ManaSound.AttachControl(Util_cboVitalManaSound);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void ExtendLogout_ControlSettingChanged(ControlSettingChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                TraceLogger.Write("Extending log out timer.", TraceLevel.Info);
                Core.Actions.SetIdleTime(double.MaxValue);
            }
        }
    }

    [Serializable]
    public class UtilSettings : SettingGroup
    {
        //[XmlArray("Options"), XmlArrayItem("Option", typeof(UtilOption))]
        //public RowItemList<UtilOption> Options { get; set; }
        public CheckboxSetting LevelSS { get; set; }
        public CheckboxSetting StatusText { get; set; }
        public CheckboxSetting ShowErrors { get; set; }
        public CheckboxSetting ExtendLogout { get; set; }
        public CheckboxSetting HealthVitalAlarm { get; set; }
        public CheckboxSetting StaminaVitalAlarm { get; set; }
        public CheckboxSetting ManaVitalAlarm { get; set; }
        public TextBoxDoubleSetting HealthPercentage { get; set; }
        public TextBoxDoubleSetting StaminaPercentage { get; set; }
        public TextBoxDoubleSetting ManaPercentage { get; set; }
        public ComboSetting HealthSound { get; set; }
        public ComboSetting StaminaSound { get; set; }
        public ComboSetting ManaSound { get; set; }

        public UtilSettings()
        {
            groupName = "Util";
            //Options = new RowItemList<UtilOption>();
            LevelSS = new CheckboxSetting(false);
            StatusText = new CheckboxSetting(false);
            ShowErrors = new CheckboxSetting(false);
            ExtendLogout = new CheckboxSetting(false);
            HealthVitalAlarm = new CheckboxSetting(false);
            StaminaVitalAlarm = new CheckboxSetting(false);
            ManaVitalAlarm = new CheckboxSetting(false);
            HealthPercentage = new TextBoxDoubleSetting(5);
            HealthPercentage.IsPercentage = true;
            StaminaPercentage = new TextBoxDoubleSetting(5);
            StaminaPercentage.IsPercentage = true;
            ManaPercentage = new TextBoxDoubleSetting(5);
            ManaPercentage.IsPercentage = true;
            HealthSound = new ComboSetting();
            StaminaSound = new ComboSetting();
            ManaSound = new ComboSetting();
        }
    }
}
