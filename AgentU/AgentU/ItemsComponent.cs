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
using System.Threading;
using System.Windows.Threading;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Filters;
using Zegeger.Decal.VVS;
using Zegeger.Data;
using Zegeger.Decal.Chat;
using Zegeger.Diagnostics;
using Zegeger.Decal.Data;
using Zegeger.Decal.Controls;
using Zegeger.Decal.Spells;

namespace Zegeger.Decal.Plugins.AgentU
{
    internal partial class Items : Component
    {
        internal ItemsSettings ItemsSettings;

        private static ICheckBox Use_Gems;
        private static ICheckBox Fill_Mana;
        private static ITextBox Fill_Threshold;
        private static ICheckBox Dispell_DOT;
        private static ITextBox DOT_Damage;
        private static ICheckBox Use_Beers;
        private static ITextBox Beer_Timer;
        private static ICheckBox Beer_Str;
        private static ICheckBox Beer_End;
        private static ICheckBox Beer_Coord;
        private static ICheckBox Beer_Quick;
        private static ICheckBox Beer_Focus;
        private static ICheckBox Beer_Self;
        private static ICheckBox Beer_Use_Check;
        private static ITextBox Beer_Use_Timer;
        private static ICombo Beer_Use_Spell;
        private static ICheckBox Event_Start;

        //Dictionary<int, SpellItem> spellTable;
        SynchronizationContext context = new SynchronizationContext();

        internal Items(CoreManager core, IView view)
            : base(core, view)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ComponentName = "Items";
            Critical = false;

            Use_Gems = (ICheckBox)View["Use_Gems"];
            Fill_Mana = (ICheckBox)View["Fill_Mana"];
            Fill_Threshold = (ITextBox)View["Fill_Threshold"];
            Dispell_DOT = (ICheckBox)View["Dispell_DOT"];
            DOT_Damage = (ITextBox)View["DOT_Damage"];
            Use_Beers = (ICheckBox)View["Use_Beers"];
            Beer_Timer = (ITextBox)View["Beer_Timer"];
            Beer_Str = (ICheckBox)View["Beer_Str"];
            Beer_End = (ICheckBox)View["Beer_End"];
            Beer_Coord = (ICheckBox)View["Beer_Coord"];
            Beer_Quick = (ICheckBox)View["Beer_Quick"];
            Beer_Focus = (ICheckBox)View["Beer_Focus"];
            Beer_Self = (ICheckBox)View["Beer_Self"];
            Beer_Use_Check = (ICheckBox)View["Beer_Use_Check"];
            Beer_Use_Timer = (ITextBox)View["Beer_Use_Timer"];
            Beer_Use_Spell = (ICombo)View["Beer_Use_Spell"];
            Beer_Use_Spell.Change += new EventHandler<MVIndexChangeEventArgs>(Beer_Use_Spell_Change);
            Event_Start = (ICheckBox)View["Event_Start"];
            
            SettingsProfileHandler.registerType(typeof(ItemsSettings));
            SettingsProfileHandler.SettingActivated += new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);

            ManaStoneID = new BlockID(core);
            EquipID = new BlockID(core);

            CommandClass cc = CommandHandler.CreateCommandClass("Use Items");
            CommandGroup cg = cc.CreateCommandGroup("Fill your mana now");
            cg.AddCommand("FillMana", FillManaCommand);
            CommandGroup cg2 = cc.CreateCommandGroup("Drink beers now");
            cg2.AddCommand("DrinkBeers", DrinkBeersCommand);
            CommandGroup cg3 = cc.CreateCommandGroup("Run queued events");
            cg3.AddCommand("RunEvents", RunEventsCommand);

            manaStoneQueue = ActionQueue.CreateActionQueue(100);
            manaStoneQueue.Start();

            dispelQueue = ActionQueue.CreateActionQueue(110);
            dispelQueue.Start();

            BeerTimer = ZTimer.CreateInstance(BeerTimerCallback);
            BeerTimer.Repeat = false;
            RunTimer = ZTimer.CreateInstance(RunTimerCallback);
            RunTimer.Repeat = false;
            BeerQueue = ActionQueue.CreateActionQueue(90);
            BeerQueue.QueueCompleteEvent += new QueueComplete(BeerQueue_QueueCompleteEvent);
            Core.ChatNameClicked += new EventHandler<ChatClickInterceptEventArgs>(Core_ChatNameClicked);

            //spellTable = new Dictionary<int, SpellItem>();

            TraceLogger.Write("Initialized Component: " + ComponentName, TraceLevel.Info);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Enchantments_EffectiveSpellChangedEvent(object sender, EffectiveSpellChangedEventArgs e)
        {
            TraceLogger.Write("Enter Spell Family " + e.Family + ", Type: " + e.Type, TraceLevel.Verbose);
            if (e.Type == ChangeType.Added)
            {
                int index = AddToSpellControl(e.NewEffectiveSpell.Name);
                if (e.Family == ItemsSettings.NoBeersSpell)
                {
                    TraceLogger.Write("Setting selected to " + index, TraceLevel.Verbose);
                    Beer_Use_Spell.Selected = index;
                }
            }
            else if (e.Type == ChangeType.Changed)
            {
                if (!e.PreviousSpellWasSameId)
                {
                    RemoveFromSpellControl(e.PreviousEffectiveSpell.Name);
                    int index = AddToSpellControl(e.NewEffectiveSpell.Name);
                    if (e.Family == ItemsSettings.NoBeersSpell)
                    {
                        TraceLogger.Write("Setting selected to " + index, TraceLevel.Verbose);
                        Beer_Use_Spell.Selected = index;
                    }
                }
            }
            else if (e.Type == ChangeType.Deleted)
            {
                RemoveFromSpellControl(e.PreviousEffectiveSpell.Name);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        int RemoveFromSpellControl(string spell)
        {
            TraceLogger.Write("Enter " + spell, TraceLevel.Verbose);
            bool removed = false;
            int i;
            for (i = 1; i < Beer_Use_Spell.Count; i++)
            {
                string name = Beer_Use_Spell.Text[i];
                if (spell == name)
                {
                    TraceLogger.Write("Removing spell from control at " + i, TraceLevel.Verbose);
                    Beer_Use_Spell.Remove(i);
                    removed = true;
                    break;
                }
            }
            if (removed)
            {
                TraceLogger.Write("Selected is " + Beer_Use_Spell.Selected, TraceLevel.Verbose);
                if (i < Beer_Use_Spell.Selected)
                {
                    TraceLogger.Write("Decremented selected since spell was removed before selected", TraceLevel.Verbose);
                    Beer_Use_Spell.Selected--;
                }
                else if (i == Beer_Use_Spell.Selected)
                {
                    TraceLogger.Write("Setting selected to 0 since selected spell was removed", TraceLevel.Verbose);
                    Beer_Use_Spell.Selected = 0;
                }
            }
            else
            {
                TraceLogger.Write("Spell was not found, so it couldn't be removed", TraceLevel.Verbose);
                i = -1;
            }
            
            TraceLogger.Write("Exit with " + i, TraceLevel.Verbose);
            return i;
        }

        int AddToSpellControl(string spell)
        {
            TraceLogger.Write("Enter " + spell, TraceLevel.Verbose);
            bool added = false;
            int i;
            for (i = 1; i < Beer_Use_Spell.Count; i++)
            {
                string name = Beer_Use_Spell.Text[i];
                int pos = spell.CompareTo(name);
                if (pos < 0)
                {
                    TraceLogger.Write("Adding spell to control at " + i, TraceLevel.Verbose);
                    Beer_Use_Spell.Insert(i, spell);
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                TraceLogger.Write("Adding spell to control at end", TraceLevel.Verbose);
                Beer_Use_Spell.Add(spell);
                i = Beer_Use_Spell.Count;
            }
            TraceLogger.Write("Selected is " + Beer_Use_Spell.Selected, TraceLevel.Verbose);
            if (i <= Beer_Use_Spell.Selected)
            {
                TraceLogger.Write("Incrementing selected since spell was added before selected", TraceLevel.Verbose);
                Beer_Use_Spell.Selected++;
            }
            TraceLogger.Write("Exit with " + i, TraceLevel.Verbose);
            return i;
        }

        private void PopulateSpellControl()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            SortedDictionary<string, EnchantmentSpell> sortedSpellTable = new SortedDictionary<string, EnchantmentSpell>();
            foreach (EnchantmentFamily ef in Enchantments.SpellFamilies.Values)
            {
                sortedSpellTable.Add(ef.EffectiveSpell.Name, ef.EffectiveSpell);
            }
            Beer_Use_Spell.Clear();
            Beer_Use_Spell.Add("Not set/No match");
            foreach (EnchantmentSpell ef in sortedSpellTable.Values)
            {
                Beer_Use_Spell.Add(ef.Name);
                TraceLogger.Write("Checking if " + ef.Name + " with family " + ef.Family + " equals " + ItemsSettings.NoBeersSpell, TraceLevel.Noise);
                if (ef.Family == ItemsSettings.NoBeersSpell)
                {
                    TraceLogger.Write("Setting Spell Dropdown to " + ef.Name + ", Index: " + (Beer_Use_Spell.Count - 1), TraceLevel.Verbose);
                    Beer_Use_Spell.Selected = Beer_Use_Spell.Count - 1;
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Beer_Use_Spell_Change(object sender, MVIndexChangeEventArgs e)
        {
            TraceLogger.Write("Enter " + Beer_Use_Spell.Text[e.Index], TraceLevel.Verbose);
            ItemsSettings.NoBeersSpell = PluginCore.fileService.SpellTable.GetByName(Beer_Use_Spell.Text[e.Index]).Family;
            SettingsProfileHandler.Save();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void Core_ChatNameClicked(object sender, ChatClickInterceptEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (e.Id == 463074 && e.Text == "[Beers]")
            {
                StartBeerQueue();
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void FillManaCommand(CommandIssuedCallbackArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            FillMana(true);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void DrinkBeersCommand(CommandIssuedCallbackArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            CheckBeerSpells();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void RunEventsCommand(CommandIssuedCallbackArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            BeerQueue.Start();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void CharacterFilter_ChangeEnchantments(object sender, ChangeEnchantmentsEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter spell ID " + e.Enchantment.SpellId + ", Change Type: " + e.Type, TraceLevel.Verbose);
                if (e.Type == AddRemoveEventType.Delete)
                {
                    if (ItemsSettings.UseGems.Value)
                    {
                        if (Constants.BonusSpellsDictionary.ContainsValue(e.Enchantment.SpellId) || Constants.BonusCooldownDictionary.ContainsValue(e.Enchantment.SpellId))
                        {
                            TraceLogger.Write("Gem Related Spell Removed: " + e.Enchantment.SpellId, TraceLevel.Info);
                            CheckGemSpells();
                        }
                    }
                    if (ItemsSettings.UseBeers.Value)
                    {
                        if (Constants.BeerSpellsDictionary.ContainsKey(e.Enchantment.SpellId))
                        {
                            TraceLogger.Write("Beer Related Spell Removed: " + e.Enchantment.SpellId, TraceLevel.Info);
                            RunBeers();
                        }
                    }
                }
                if (e.Type == AddRemoveEventType.Add)
                {
                    if (ItemsSettings.DispelDot.Value)
                    {
                        if (Constants.VoidDamageDictionary.ContainsKey(e.Enchantment.SpellId))
                        {
                            TraceLogger.Write("DOT Spell Added: " + e.Enchantment.SpellId, TraceLevel.Info);
                            UseDispellForDOTIfNeeded();
                        }
                    }
                    if (ItemsSettings.UseBeers.Value && ItemsSettings.NoBeersIFBuffsLow.Value)
                    {
                        if (e.Enchantment.Family == ItemsSettings.NoBeersSpell)
                        {
                            TraceLogger.Write("Beer Buff Check Spell Added: " + e.Enchantment.SpellId, TraceLevel.Info);
                            RunBeers();
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

        private void RunBeers()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (ItemsSettings.NoBeersIFBuffsLow.Value)
            {
                TraceLogger.Write("Buff check for beers is enabled", TraceLevel.Verbose);
                if (Enchantments.SpellFamilies.ContainsKey(ItemsSettings.NoBeersSpell))
                {
                    TraceLogger.Write("Spell family is found " + ItemsSettings.NoBeersSpell + " with time remaining of " + Enchantments.SpellFamilies[ItemsSettings.NoBeersSpell].EffectiveSpell.TimeRemaining, TraceLevel.Verbose);
                    if (Enchantments.SpellFamilies[ItemsSettings.NoBeersSpell].EffectiveSpell.TimeRemaining > ItemsSettings.NoBeersTime.Value)
                    {
                        CheckBeerSpells();
                    }
                }
            }
            else
            {
                TraceLogger.Write("Buff check for beers is disabled", TraceLevel.Verbose);
                CheckBeerSpells();
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void ZChatWrapper_ChatBoxMessage(ChatBoxMessageEventArgs e)
        {
            TraceLogger.Write("Enter " + e.Text, TraceLevel.Verbose);
            try
            {
                if (ItemsSettings.FillMana.Value && e.Text.Contains("is low on Mana.") && e.Color == Constants.ChatClasses("Spells"))
                {
                    if (ItemsSettings.FillMana.Value)
                    {
                        FillMana();
                    }
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
            State = ComponentState.Startingup;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal override void PostLogin()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            Enchantments.EffectiveSpellChangedEvent += new EventHandler<EffectiveSpellChangedEventArgs>(Enchantments_EffectiveSpellChangedEvent);

            Core.CharacterFilter.ChangeEnchantments += new EventHandler<ChangeEnchantmentsEventArgs>(CharacterFilter_ChangeEnchantments);
            ZChatWrapper.ChatBoxMessage += new ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            FindGems();
            PopulateSpellControl();
            if (ItemsSettings.UseGems.Value)
                CheckGemSpells();
            if(ItemsSettings.UseBeers.Value)
                RunBeers();
            if(ItemsSettings.FillMana.Value)
                FillMana();
            State = ComponentState.Running;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
 
        internal override void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            State = ComponentState.ShuttingDown;
            SettingsProfileHandler.SettingActivated -= new SettingActivatedEvent(SettingsProfileHandler_SettingActivated);
            Enchantments.EffectiveSpellChangedEvent -= new EventHandler<EffectiveSpellChangedEventArgs>(Enchantments_EffectiveSpellChangedEvent);
            Core.CharacterFilter.ChangeEnchantments -= new EventHandler<ChangeEnchantmentsEventArgs>(CharacterFilter_ChangeEnchantments);
            ZChatWrapper.ChatBoxMessage -= new ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            ActionQueue.DestroyActionQueue(manaStoneQueue);
            ActionQueue.DestroyActionQueue(dispelQueue);
            ActionQueue.DestroyActionQueue(BeerQueue);
            State = ComponentState.ShutDown;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void SettingsProfileHandler_SettingActivated(SettingActivatedEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ItemsSettings tmp = (ItemsSettings)SettingsProfileHandler.GetSettingGroup(ComponentName);
            if (tmp == null)
            {
                TraceLogger.Write("Creating new ItemSettings", TraceLevel.Info);
                ItemsSettings = new ItemsSettings();
                SettingsProfileHandler.AddSettingGroup(ItemsSettings);
            }
            else
            {
                TraceLogger.Write("Loading Existing ItemSettings", TraceLevel.Info);
                ItemsSettings = tmp;
            }
            ItemsSettings.UseGems.AttachControl(Use_Gems);
            ItemsSettings.UseGems.ControlSettingChanged += new ControlSettingChangedEvent<bool>(UseGems_ControlSettingChanged);
            ItemsSettings.FillMana.AttachControl(Fill_Mana);
            ItemsSettings.DispelDot.AttachControl(Dispell_DOT);
            ItemsSettings.DotDamage.AttachControl(DOT_Damage);
            ItemsSettings.FillThreshold.AttachControl(Fill_Threshold);
            ItemsSettings.UseBeers.AttachControl(Use_Beers);
            ItemsSettings.UseBeers.ControlSettingChanged += new ControlSettingChangedEvent<bool>(UseBeers_ControlSettingChanged);
            ItemsSettings.Strength.AttachControl(Beer_Str);
            ItemsSettings.Strength.ControlSettingChanged += new ControlSettingChangedEvent<bool>(Beers_ControlSettingChanged);
            ItemsSettings.Coordination.AttachControl(Beer_Coord);
            ItemsSettings.Coordination.ControlSettingChanged += new ControlSettingChangedEvent<bool>(Beers_ControlSettingChanged);
            ItemsSettings.Quickness.AttachControl(Beer_Quick);
            ItemsSettings.Quickness.ControlSettingChanged += new ControlSettingChangedEvent<bool>(Beers_ControlSettingChanged);
            ItemsSettings.Endurance.AttachControl(Beer_End);
            ItemsSettings.Endurance.ControlSettingChanged += new ControlSettingChangedEvent<bool>(Beers_ControlSettingChanged);
            ItemsSettings.Focus.AttachControl(Beer_Focus);
            ItemsSettings.Focus.ControlSettingChanged += new ControlSettingChangedEvent<bool>(Beers_ControlSettingChanged);
            ItemsSettings.Self.AttachControl(Beer_Self);
            ItemsSettings.Self.ControlSettingChanged += new ControlSettingChangedEvent<bool>(Beers_ControlSettingChanged);
            ItemsSettings.NoBeersIFBuffsLow.AttachControl(Beer_Use_Check);
            ItemsSettings.UseBeersTime.AttachControl(Beer_Timer);
            ItemsSettings.NoBeersTime.AttachControl(Beer_Use_Timer);
            ItemsSettings.RunQueueAutomatically.AttachControl(Event_Start);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void UseBeers_ControlSettingChanged(ControlSettingChangedEventArgs<bool> e)
        {
            if (e.NewValue)
                CheckBeerSpells();
        }

        void Beers_ControlSettingChanged(ControlSettingChangedEventArgs<bool> e)
        {
            if (ItemsSettings.UseBeers.Value)
                CheckBeerSpells();
        }

        void UseGems_ControlSettingChanged(ControlSettingChangedEventArgs<bool> e)
        {
            if (e.NewValue)
                CheckGemSpells();
        }
    }

    [Serializable]
    public class ItemsSettings : SettingGroup
    {
        public CheckboxSetting UseGems { get; set; }
        public CheckboxSetting FillMana { get; set; }
        public TextBoxIntSetting FillThreshold { get; set; }
        public CheckboxSetting DispelDot { get; set; }
        public TextBoxIntSetting DotDamage { get; set; }
        public CheckboxSetting UseBeers { get; set; }
        public CheckboxSetting Strength { get; set; }
        public CheckboxSetting Endurance { get; set; }
        public CheckboxSetting Coordination { get; set; }
        public CheckboxSetting Quickness { get; set; }
        public CheckboxSetting Focus { get; set; }
        public CheckboxSetting Self { get; set; }
        public TextBoxIntSetting UseBeersTime { get; set; }
        public CheckboxSetting NoBeersIFBuffsLow { get; set; }
        public TextBoxIntSetting NoBeersTime { get; set; }
        public int NoBeersSpell { get; set; }

        public CheckboxSetting RunQueueAutomatically { get; set; }

        public ItemsSettings()
        {
            groupName = "Items";
            UseGems = new CheckboxSetting(false);
            FillMana = new CheckboxSetting(false);
            FillThreshold = new TextBoxIntSetting(50000);
            DispelDot = new CheckboxSetting(false);
            DotDamage = new TextBoxIntSetting(100);
            UseBeers = new CheckboxSetting(false);
            Strength = new CheckboxSetting(true);
            Endurance = new CheckboxSetting(true);
            Coordination = new CheckboxSetting(true);
            Quickness = new CheckboxSetting(true);
            Focus = new CheckboxSetting(true);
            Self = new CheckboxSetting(true);
            UseBeersTime = new TextBoxIntSetting(120);
            NoBeersIFBuffsLow = new CheckboxSetting(true);
            NoBeersTime = new TextBoxIntSetting(120);
            RunQueueAutomatically = new CheckboxSetting(false);
            NoBeersSpell = -1;
        }
    }
}
