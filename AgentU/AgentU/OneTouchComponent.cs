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
using Decal.Adapter.Wrappers;
using Decal.Filters;
using Zegeger.Decal.VVS;
using Zegeger.Data;
using Zegeger.Decal.Chat;
using Zegeger.Diagnostics;
using Zegeger.Decal.Data;
using Zegeger.Decal.Controls;
using Zegeger.Decal.Hotkey;

namespace Zegeger.Decal.Plugins.AgentU
{
    internal class OneTouch : Component
    {
        internal const int NAME = 0;
        internal const int DELETE = 1;

        internal OneTouchSettings OneTouchSettings;

        DecalList OneTouchExceptions;

        private static IList Vitals_listExceptions;
        private static ICheckBox Vitals_OneTouch_Enabled;
        private static ITextBox Vitals_txtBlock;
        private static ITextBox Vitals_txtSuccess;
        private static ITextBox Vitals_txtCheap;
        private static ITextBox Vitals_txtFallback;
        private static ISlider Vitals_sldrBlock;
        private static ISlider Vitals_sldrSuccess;
        private static ISlider Vitals_sldrCheap;
        private static ISlider Vitals_sldrFallback;
        private static ITextBox Vitals_txtAddException;
        private static IButton Vitals_btnAddException;
        private static IButton Vitals_btnAddSelected;

        internal OneTouch(CoreManager core, IView view)
            : base(core, view)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ComponentName = "OneTouch";
            Critical = false;

            Vitals_listExceptions = (IList)View["Vitals_listExceptions"];
            Vitals_listExceptions.Click += new dClickedList(Vitals_listExceptions_Click);
            Vitals_OneTouch_Enabled = (ICheckBox)View["Vitals_OneTouch_Enabled"];
            Vitals_txtBlock = (ITextBox)View["Vitals_txtBlock"];
            Vitals_txtSuccess = (ITextBox)View["Vitals_txtSuccess"];
            Vitals_txtCheap = (ITextBox)View["Vitals_txtCheap"];
            Vitals_txtFallback = (ITextBox)View["Vitals_txtFallback"];
            Vitals_sldrBlock = (ISlider)View["Vitals_sldrBlock"];
            Vitals_sldrBlock.Change += new EventHandler<MVIndexChangeEventArgs>(Vitals_sldrBlock_Change);
            Vitals_sldrSuccess = (ISlider)View["Vitals_sldrSuccess"];
            Vitals_sldrSuccess.Change += new EventHandler<MVIndexChangeEventArgs>(Vitals_sldrSuccess_Change);
            Vitals_sldrCheap = (ISlider)View["Vitals_sldrCheap"];
            Vitals_sldrCheap.Change += new EventHandler<MVIndexChangeEventArgs>(Vitals_sldrCheap_Change);
            Vitals_sldrFallback = (ISlider)View["Vitals_sldrFallback"];
            Vitals_sldrFallback.Change += new EventHandler<MVIndexChangeEventArgs>(Vitals_sldrFallback_Change);
            Vitals_txtAddException = (ITextBox)View["Vitals_txtAddException"];
            Vitals_btnAddException = (IButton)View["Vitals_btnAddException"];
            Vitals_btnAddException.Click += new EventHandler<MVControlEventArgs>(Vitals_btnAddException_Click);
            Vitals_btnAddSelected = (IButton)View["Vitals_btnAddSelected"];
            Vitals_btnAddSelected.Click += new EventHandler<MVControlEventArgs>(Vitals_btnAddSelected_Click);

            SettingsProfileHandler.registerType(typeof(OneTouchSettings));
            SettingsProfileHandler.registerType(typeof(OneTouchException));

            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);

            OneTouchExceptions = new DecalList(Vitals_listExceptions);
            Core.WorldFilter.CreateObject += new EventHandler<CreateObjectEventArgs>(WorldFilter_CreateObject);

            TraceLogger.Write("Initialized Component: " + ComponentName, TraceLevel.Info);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Vitals_listExceptions_Click(object sender, int row, int col)
        {
            try
            {
                TraceLogger.Write("Enter, row: " + row + ", col: " + col, TraceLevel.Noise);
                if (col == DELETE)
                {
                    TraceLogger.Write("Deleting " + OneTouchSettings.Exceptions[row].Name, TraceLevel.Info);
                    OneTouchSettings.Exceptions.RemoveAt(row);
                    SettingsProfileHandler.Save();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void WorldFilter_CreateObject(object sender, CreateObjectEventArgs e)
        {
            if (e.New.ObjectClass == ObjectClass.Food)
            {
                TraceLogger.Write("ID Food " + e.New.Name, TraceLevel.Verbose);
                Core.Actions.RequestId(e.New.Id);
            }
            if (e.New.ObjectClass == ObjectClass.HealingKit)
            {
                TraceLogger.Write("ID Kit " + e.New.Name, TraceLevel.Verbose);
                Core.Actions.RequestId(e.New.Id);
            }
        }

        private const string HKOneKeyHealth = "OneKeyHealth";
        private const string HKOneKeyStamina = "OneKeyStamina";
        private const string HKOneKeyMana = "OneKeyMana";
        private const string HKOneKeyHealthKit = "OneKeyHealthKit";
        private const string HKOneKeyStaminaKit = "OneKeyStaminaKit";
        private const string HKOneKeyManaKit = "OneKeyManaKit";
        private const string HKOneKeySmartHeal = "OneKeySmartHeal";
        private const string HKOneKeySmartStamina = "OneKeySmartStamina";
        private const string HKOneKeySmartMana = "OneKeySmartMana";

        private bool HotkeyPressed(string name)
        {
            TraceLogger.Write("Enter name " + name, TraceLevel.Verbose);
            bool eat = false;
            if (OneTouchSettings.Enabled.Value)
            {
                switch (name)
                {
                    case HKOneKeyHealth:
                        eat = UseFood(CharFilterVitalType.Health);
                        break;
                    case HKOneKeyStamina:
                        eat = UseFood(CharFilterVitalType.Stamina);
                        break;
                    case HKOneKeyMana:
                        eat = UseFood(CharFilterVitalType.Mana);
                        break;
                    case HKOneKeyHealthKit:
                        eat = UseKit(CharFilterVitalType.Health);
                        break;
                    case HKOneKeyStaminaKit:
                        eat = UseKit(CharFilterVitalType.Stamina);
                        break;
                    case HKOneKeyManaKit:
                        eat = UseKit(CharFilterVitalType.Mana);
                        break;
                    case HKOneKeySmartHeal:
                        eat = UseKit(CharFilterVitalType.Health, true);
                        break;
                    case HKOneKeySmartStamina:
                        eat = UseKit(CharFilterVitalType.Stamina, true);
                        break;
                    case HKOneKeySmartMana:
                        eat = UseKit(CharFilterVitalType.Mana, true);
                        break;
                }
            }
            TraceLogger.Write("Exit " + eat, TraceLevel.Verbose);
            return eat;
        }

        private bool UseFood(CharFilterVitalType type)
        {
            TraceLogger.Write("Enter for type " + type, TraceLevel.Verbose);
            int needed = 0; int curr = 0; int max = 0;
            switch(type)
            {
                case CharFilterVitalType.Health:
                    max = Core.Actions.Vital[VitalType.MaximumHealth];
                    curr = Core.Actions.Vital[VitalType.CurrentHealth];
                    needed = max - curr;
                    break;
                case CharFilterVitalType.Stamina:
                    max = Core.Actions.Vital[VitalType.MaximumStamina];
                    curr = Core.Actions.Vital[VitalType.CurrentStamina];
                    needed = max - curr;
                    break;
                case CharFilterVitalType.Mana:
                    max = Core.Actions.Vital[VitalType.MaximumMana];
                    curr = Core.Actions.Vital[VitalType.CurrentMana];
                    needed = max - curr;
                    break;
            }
            TraceLogger.Write("Needed Vital: " + needed, TraceLevel.Info);
            if (needed == 0)
            {
                TraceLogger.Write("Exit false. Needed vital is 0", TraceLevel.Verbose);
                return false;
            }
            if (curr > (double)max * OneTouchSettings.BlockThreshold.Value)
            {
                TraceLogger.Write("Exit false. Current vital higher then block threshold.", TraceLevel.Verbose);
                return false;
            }

            int bestfitid = 0;
            int bestfitdiff = 9999;

            List<string> exchNames = new List<string>();
            foreach (OneTouchException ote in OneTouchSettings.Exceptions)
            {
                exchNames.Add(ote.Name);
            }

            foreach (WorldObject wo in Core.WorldFilter.GetInventory())
            {
                if (wo.ObjectClass == ObjectClass.Food && wo.Exists(LongValueKey.AffectsVitalId) && wo.Values(LongValueKey.AffectsVitalId) == (int)type)
                {
                    if (exchNames.Contains(wo.Name))
                        continue;
                    TraceLogger.Write("Food Match: "+ wo.Name + ", Restores " + wo.Values(LongValueKey.AffectsVitalAmt), TraceLevel.Verbose);
                    int diff = Math.Abs(needed - wo.Values(LongValueKey.AffectsVitalAmt));
                    if (diff < bestfitdiff)
                    {
                        TraceLogger.Write("New best fit: " + diff, TraceLevel.Verbose);
                        bestfitid = wo.Id;
                        bestfitdiff = diff;
                    }
                }
            }
            if (bestfitid != 0)
            {
                TraceLogger.Write("Applying Food " + bestfitid, TraceLevel.Info);
                Core.Actions.UseItem(bestfitid, 0);
                TraceLogger.Write("Exit true", TraceLevel.Verbose);
                return true;
            }
            TraceLogger.Write("Exit false", TraceLevel.Verbose);
            return false;
        }

        bool UseKit(CharFilterVitalType type, bool smart = false)
        {
            TraceLogger.Write("Enter for type " + type, TraceLevel.Verbose);
            int needed = 0; int curr = 0; int max = 0;
            switch (type)
            {
                case CharFilterVitalType.Health:
                    max = Core.Actions.Vital[VitalType.MaximumHealth];
                    curr = Core.Actions.Vital[VitalType.CurrentHealth];
                    needed = max - curr;
                    break;
                case CharFilterVitalType.Stamina:
                    max = Core.Actions.Vital[VitalType.MaximumStamina];
                    curr = Core.Actions.Vital[VitalType.CurrentStamina];
                    needed = max - curr;
                    break;
                case CharFilterVitalType.Mana:
                    max = Core.Actions.Vital[VitalType.MaximumMana];
                    curr = Core.Actions.Vital[VitalType.CurrentMana];
                    needed = max - curr;
                    break;
            }
            TraceLogger.Write("Needed Vital: " + needed, TraceLevel.Info);
            if (needed == 0)
            {
                TraceLogger.Write("Exit false. Needed vital is 0", TraceLevel.Verbose);
                return false;
            }
            if (curr > (double)max * OneTouchSettings.BlockThreshold.Value)
            {
                TraceLogger.Write("Exit false. Current vital higher then block threshold.", TraceLevel.Verbose);
                return false;
            }

            double diffmultiplyer = 2.0;
            if (Core.Actions.CombatMode != CombatState.Peace)
                diffmultiplyer = 2.6;
            double skilldivisor = 1.1;
            if (Core.CharacterFilter.Skills[CharFilterSkillType.Healing].Training == TrainingType.Specialized)
                skilldivisor = 1.5;

            double difficulty = ((needed * diffmultiplyer) / skilldivisor);
            if (smart)
            {
                if (Core.Actions.Vital[VitalType.CurrentStamina] < Core.Actions.Vital[VitalType.MaximumStamina] * 0.03)
                    difficulty += 1000;
            }
            TraceLogger.Write("Healing - Needed: " + needed + ", Difficulty: " + difficulty, TraceLevel.Info);
            int bestfitid = 0;
            double bestfitregen = 0;
            int bestfitvalue = 99999;
            double bestfitsuccess = 0;
            int bestfituses = 999;

            foreach (WorldObject wo in Core.WorldFilter.GetInventory())
            {
                if (wo.ObjectClass == ObjectClass.HealingKit)
                {
                    CharFilterVitalType vitalType = CharFilterVitalType.Health;
                    if (wo.Name.Contains("Stamina"))
                    {
                        vitalType = CharFilterVitalType.Stamina;
                    }
                    else if (wo.Name.Contains("Mana"))
                    {
                        vitalType = CharFilterVitalType.Mana;
                    }
                    if (vitalType == type)
                    {
                        TraceLogger.Write("Kit Match: " + wo.Name, TraceLevel.Verbose);
                        double success = 1.0 - 1.0 / (1.0 + Math.Exp(0.03 * ((double)Core.CharacterFilter.EffectiveSkill[CharFilterSkillType.Healing] + (double)wo.Values(LongValueKey.HealKitSkillBonus) - difficulty)));
                        TraceLogger.Write("Skill: " + Core.CharacterFilter.EffectiveSkill[CharFilterSkillType.Healing] + ", Bonus: " + wo.Values(LongValueKey.HealKitSkillBonus) + ", Success: " + success, TraceLevel.Verbose);
                        if (bestfitregen == wo.Values(DoubleValueKey.HealingKitRestoreBonus) && bestfitvalue == wo.Values(LongValueKey.Value) && bestfitsuccess == success && wo.Values(LongValueKey.UsesRemaining) < bestfituses)
                        {
                            TraceLogger.Write("Similar Kit found with lower uses remaining " + wo.Values(LongValueKey.UsesRemaining), TraceLevel.Verbose);
                            bestfitid = wo.Id;
                            bestfitregen = wo.Values(DoubleValueKey.HealingKitRestoreBonus);
                            bestfitvalue = wo.Values(LongValueKey.Value);
                            bestfitsuccess = success;
                            bestfituses = wo.Values(LongValueKey.UsesRemaining);
                        }
                        else
                        {
                            if (OneTouchSettings.HealCheapoThreshold.Value < (curr / max))
                            {
                                if (success > OneTouchSettings.HealSuccessThreshold.Value && ((wo.Values(DoubleValueKey.HealingKitRestoreBonus) * 100) + wo.Values(LongValueKey.HealKitSkillBonus) < ((bestfitregen * 100) + bestfitvalue)))
                                {
                                    TraceLogger.Write("Conservation path, least 'valuable' kit");
                                    bestfitid = wo.Id;
                                    bestfitregen = wo.Values(DoubleValueKey.HealingKitRestoreBonus);
                                    bestfitvalue = wo.Values(LongValueKey.HealKitSkillBonus);
                                    bestfitsuccess = success;
                                    bestfituses = wo.Values(LongValueKey.UsesRemaining);
                                }
                            }
                            else
                            {
                                if (success > OneTouchSettings.HealSuccessThreshold.Value && wo.Values(DoubleValueKey.HealingKitRestoreBonus) > bestfitregen)
                                {
                                    TraceLogger.Write("Best success path, best restore bonus");
                                    bestfitid = wo.Id;
                                    bestfitregen = wo.Values(DoubleValueKey.HealingKitRestoreBonus);
                                    bestfitvalue = wo.Values(LongValueKey.HealKitSkillBonus);
                                    bestfitsuccess = success;
                                    bestfituses = wo.Values(LongValueKey.UsesRemaining);
                                }
                            }
                        }
                    }
                }
            }
            if (bestfitid == 0)
            {
                TraceLogger.Write("No kits match success cutoff, finding best succes rate");
                foreach (WorldObject wo in Core.WorldFilter.GetInventory())
                {
                    if (wo.ObjectClass == ObjectClass.HealingKit)
                    {
                        CharFilterVitalType vitalType = CharFilterVitalType.Health;
                        if (wo.Name.Contains("Stamina"))
                        {
                            vitalType = CharFilterVitalType.Stamina;
                        }
                        else if (wo.Name.Contains("Mana"))
                        {
                            vitalType = CharFilterVitalType.Mana;
                        }
                        if (vitalType == type)
                        {
                            double success = 1.0 - 1.0 / (1.0 + Math.Exp(0.03 * ((double)Core.CharacterFilter.EffectiveSkill[CharFilterSkillType.Healing] + (double)wo.Values(LongValueKey.HealKitSkillBonus) - difficulty))); 
                            TraceLogger.Write("Skill: " + Core.CharacterFilter.EffectiveSkill[CharFilterSkillType.Healing] + ", Bonus: " + wo.Values(LongValueKey.HealKitSkillBonus) + ", Success: " + success, TraceLevel.Verbose);
                            if (bestfitregen == wo.Values(DoubleValueKey.HealingKitRestoreBonus) && bestfitvalue == wo.Values(LongValueKey.Value) && bestfitsuccess == success && wo.Values(LongValueKey.UsesRemaining) < bestfituses)
                            {
                                TraceLogger.Write("Similar Kit found with lower uses remaining " + wo.Values(LongValueKey.UsesRemaining), TraceLevel.Verbose);
                                bestfitid = wo.Id;
                                bestfitregen = wo.Values(DoubleValueKey.HealingKitRestoreBonus);
                                bestfitvalue = wo.Values(LongValueKey.Value);
                                bestfitsuccess = success;
                                bestfituses = wo.Values(LongValueKey.UsesRemaining);
                            }
                            else
                            {
                                if (success > bestfitsuccess)
                                {
                                    bestfitid = wo.Id;
                                    bestfitregen = wo.Values(DoubleValueKey.HealingKitRestoreBonus);
                                    bestfitvalue = wo.Values(LongValueKey.Value);
                                    bestfitsuccess = success;
                                    bestfituses = wo.Values(LongValueKey.UsesRemaining);
                                }
                            }
                        }
                    }
                }
            }
            TraceLogger.Write("Healing Best Fit - Success: " + bestfitsuccess + ", Regen: " + bestfitregen + " , Bonus: " + bestfitvalue, TraceLevel.Verbose);
            bool smartfail = false;
            bool result = false;
            if (smart && bestfitsuccess < OneTouchSettings.SmartFallbackThreshold.Value)
            {
                TraceLogger.Write("Healing Smart Mode - Too low chance of success, using food", TraceLevel.Verbose);
                smartfail = true;
                result = UseFood(type);
            }
            if ((bestfitid != 0 && !smartfail) || (bestfitid != 0 && smartfail && !result))
            {
                TraceLogger.Write("Healing with kit " + bestfitid, TraceLevel.Verbose);
                Core.Actions.ApplyItem(bestfitid, Core.CharacterFilter.Id);
                result = true;
            }
            TraceLogger.Write("Exit " + result, TraceLevel.Verbose);
            return result;
        }

        void Vitals_btnAddSelected_Click(object sender, MVControlEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (Core.Actions.CurrentSelection != 0)
            {
                WorldObject wo = Core.WorldFilter[Core.Actions.CurrentSelection];
                if (wo != null)
                {
                    foreach (OneTouchException ote in OneTouchSettings.Exceptions)
                    {
                        if (ote.Name == wo.Name)
                        {
                            TraceLogger.Write("Exit, Item already exists", TraceLevel.Verbose);
                            return;
                        }
                    }
                    OneTouchException newEx = new OneTouchException();
                    newEx.Name = wo.Name;
                    OneTouchSettings.Exceptions.Add(newEx);
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Vitals_btnAddException_Click(object sender, MVControlEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            foreach(OneTouchException ote in OneTouchSettings.Exceptions)
            {
                if (ote.Name == Vitals_txtAddException.Text)
                {
                    TraceLogger.Write("Exit, Item already exists", TraceLevel.Verbose);
                    return;
                }
            }
            OneTouchException newEx = new OneTouchException();
            newEx.Name = Vitals_txtAddException.Text;
            OneTouchSettings.Exceptions.Add(newEx);
            Vitals_txtAddException.Text = "";
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Vitals_sldrFallback_Change(object sender, MVIndexChangeEventArgs e)
        {
            OneTouchSettings.SmartFallbackThreshold.Value = (double)e.Index / 100.0;
        }

        void Vitals_sldrCheap_Change(object sender, MVIndexChangeEventArgs e)
        {
            OneTouchSettings.HealCheapoThreshold.Value = (double)e.Index / 100.0;
        }

        void Vitals_sldrSuccess_Change(object sender, MVIndexChangeEventArgs e)
        {
            OneTouchSettings.HealSuccessThreshold.Value = (double)e.Index / 100.0;
        }

        void Vitals_sldrBlock_Change(object sender, MVIndexChangeEventArgs e)
        {
            OneTouchSettings.BlockThreshold.Value = (double)e.Index / 100.0;
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
            ZHotkeyWrapper.AddHotkey(HKOneKeyHealth, "AgentU: One key eat health food", 0, false, false, false, HotkeyPressed);
            ZHotkeyWrapper.AddHotkey(HKOneKeyStamina, "AgentU: One key eat stamina food", 0, false, false, false, HotkeyPressed);
            ZHotkeyWrapper.AddHotkey(HKOneKeyMana, "AgentU: One key eat mana food", 0, false, false, false, HotkeyPressed);
            ZHotkeyWrapper.AddHotkey(HKOneKeyHealthKit, "AgentU: One key heal self with health kit", 0, false, false, false, HotkeyPressed);
            ZHotkeyWrapper.AddHotkey(HKOneKeyStaminaKit, "AgentU: One key heal self with stamina kit", 0, false, false, false, HotkeyPressed);
            ZHotkeyWrapper.AddHotkey(HKOneKeyManaKit, "AgentU: One key heal self with mana kit", 0, false, false, false, HotkeyPressed);
            ZHotkeyWrapper.AddHotkey(HKOneKeySmartHeal, "AgentU: One key heal self with health kit, but falls back to food if health is too low", 0, false, false, false, HotkeyPressed);
            ZHotkeyWrapper.AddHotkey(HKOneKeySmartStamina, "AgentU: One key heal self withstamina kit, but falls back to food if stamina is too low", 0, false, false, false, HotkeyPressed);
            ZHotkeyWrapper.AddHotkey(HKOneKeySmartMana, "AgentU: One key heal self with mana kit, but falls back to food if mana is too low", 0, false, false, false, HotkeyPressed);
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.ShuttingDown;
            SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            Core.WorldFilter.CreateObject -= new EventHandler<CreateObjectEventArgs>(WorldFilter_CreateObject);
            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            OneTouchSettings tmp = (OneTouchSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ChatLoggerSettings", TraceLevel.Info);
                OneTouchSettings = new OneTouchSettings();
                SettingsProfileHandler.AddSettingGroup(OneTouchSettings);
            }
            else
            {
                TraceLogger.Write("Loading Existing ChatLoggerSettings", TraceLevel.Info);
                OneTouchSettings = tmp;
            }
            OneTouchExceptions.List = OneTouchSettings.Exceptions;
            OneTouchSettings.Enabled.AttachControl(Vitals_OneTouch_Enabled);
            OneTouchSettings.BlockThreshold.AttachControl(Vitals_txtBlock);
            OneTouchSettings.HealCheapoThreshold.AttachControl(Vitals_txtCheap);
            OneTouchSettings.HealSuccessThreshold.AttachControl(Vitals_txtSuccess);
            OneTouchSettings.SmartFallbackThreshold.AttachControl(Vitals_txtFallback);
            Vitals_sldrBlock.Position = (int)(OneTouchSettings.BlockThreshold.Value * 100);
            Vitals_sldrCheap.Position = (int)(OneTouchSettings.HealCheapoThreshold.Value * 100);
            Vitals_sldrSuccess.Position = (int)(OneTouchSettings.HealSuccessThreshold.Value * 100);
            Vitals_sldrFallback.Position = (int)(OneTouchSettings.SmartFallbackThreshold.Value * 100);
            
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    [Serializable]
    public class OneTouchSettings : SettingGroup
    {
        [XmlArray("Exceptions"), XmlArrayItem("Exception", typeof(OneTouchException))]
        public RowItemList<OneTouchException> Exceptions { get; set; }

        public CheckboxSetting Enabled { get; set; }
        public TextBoxDoubleSetting BlockThreshold { get; set; }
        public TextBoxDoubleSetting HealSuccessThreshold { get; set; }
        public TextBoxDoubleSetting HealCheapoThreshold { get; set; }
        public TextBoxDoubleSetting SmartFallbackThreshold { get; set; }

        public OneTouchSettings()
        {
            groupName = "OneTouch";
            Exceptions = new RowItemList<OneTouchException>();
            Enabled = new CheckboxSetting(false);
            BlockThreshold = new TextBoxDoubleSetting(1.0);
            BlockThreshold.IsPercentage = true;
            HealSuccessThreshold = new TextBoxDoubleSetting(0.9);
            HealSuccessThreshold.IsPercentage = true;
            HealCheapoThreshold = new TextBoxDoubleSetting(0.8);
            HealCheapoThreshold.IsPercentage = true;
            SmartFallbackThreshold = new TextBoxDoubleSetting(0.75);
            SmartFallbackThreshold.IsPercentage = true;
        }
    }

    [Serializable]
    public class OneTouchException : IRowItem
    {
        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                DataRow["Name"].Data = new StringColumnData(name.ToString());
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

        public OneTouchException()
        {
            DataRow = new Row();
            DataRow.AddColumn("Name", 0);
            DataRow.AddColumn("Delete", 1);
            DataRow["Delete"].Data = new IconColumnData(0x60011F8);
        }
    }
}
