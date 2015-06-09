using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using Zegeger.Diagnostics;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Interop.Core;
using Decal.Filters;

namespace Zegeger.Decal.Spells
{
    public static class Enchantments
    {
        private static EnchantmentTable _table;

        public static void Initialize(CoreManager core)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (_table == null)
            {
                TraceLogger.Write("Creating Enchantment Table", TraceLevel.Verbose);
                _table = new EnchantmentTable(core);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static void Shutdown()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            _table.Dispose();
            _table = null;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private static EnchantmentTable Table
        {
            get
            {
                if (_table == null)
                    throw new InvalidOperationException("Enchantments has not been Intialized!");
                return _table;
            }
        }

        public static Dictionary<int, EnchantmentFamily> SpellFamilies
        {
            get
            {
                return Table.SpellFamilies;
            }
        }

        public static event EventHandler<EffectiveSpellChangedEventArgs> EffectiveSpellChangedEvent
        {
            add
            {
                Table.EffectiveSpellChangedEvent += value;
            }
            remove
            {
                Table.EffectiveSpellChangedEvent -= value;
            }
        }
    }

    public class EffectiveSpellChangedEventArgs : EventArgs
    {
        public ChangeType Type { get; private set; }
        public int Family { get; private set; }
        public EnchantmentSpell NewEffectiveSpell { get; private set; }
        public EnchantmentSpell PreviousEffectiveSpell { get; private set; }
        public bool PreviousSpellWasSameId
        {
            get
            {
                if (NewEffectiveSpell == (EnchantmentSpell)null || PreviousEffectiveSpell == (EnchantmentSpell)null)
                    return false;
                return NewEffectiveSpell.SpellId == PreviousEffectiveSpell.SpellId;
            }
        }

        internal EffectiveSpellChangedEventArgs(ChangeType type, int family, EnchantmentSpell spell, EnchantmentSpell prevSpell)
        {
            Type = type;
            Family = family;
            NewEffectiveSpell = spell;
            PreviousEffectiveSpell = prevSpell;
        }
    }

    public enum ChangeType
    {
        Added,
        Changed,
        Deleted
    }

    public class EnchantmentTable : IDisposable
    {
        Dictionary<int, EnchantmentFamily> families;
        CoreManager Core;
        internal static FileService fileService;

        internal event EventHandler<EffectiveSpellChangedEventArgs> EffectiveSpellChangedEvent;

        internal EnchantmentTable(CoreManager core)
        {
            Core = core;
            families = new Dictionary<int, EnchantmentFamily>();
            if(fileService == null)
                fileService = Core.FileService as FileService;
            Core.CharacterFilter.LoginComplete += CharacterFilter_LoginComplete;
        }

        public Dictionary<int, EnchantmentFamily> SpellFamilies
        {
            get
            {
                return new Dictionary<int, EnchantmentFamily>(families);
            }
        }

        void CharacterFilter_ChangeEnchantments(object sender, ChangeEnchantmentsEventArgs e)
        {
            TraceLogger.Write("Enter Spell "+ e.Enchantment.SpellId + ", Type " + e.Type, TraceLevel.Verbose);
            UpdateSpellList(e.Enchantment, e.Type);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            Core.CharacterFilter.LoginComplete -= CharacterFilter_LoginComplete;
            PopulateSpellList();
            Core.CharacterFilter.ChangeEnchantments += CharacterFilter_ChangeEnchantments;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void PopulateSpellList()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            foreach (EnchantmentWrapper ew in Core.CharacterFilter.Enchantments)
            {
                UpdateSpellList(ew);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void UpdateSpellList(EnchantmentWrapper ew, AddRemoveEventType eventType = AddRemoveEventType.Add)
        {
            TraceLogger.Write("Enter SpellID: " + ew.SpellId + " Type " + eventType, TraceLevel.Verbose);
            if (eventType == AddRemoveEventType.Add)
            {
                if (families.ContainsKey(ew.Family))
                {
                    TraceLogger.Write("Adding spell to existing family " + ew.Family, TraceLevel.Verbose);
                    EnchantmentSpell currentEffective = families[ew.Family].EffectiveSpell;
                    EnchantmentSpell newSpell = families[ew.Family].AddSpell(ew);
                    if (families[ew.Family].EffectiveSpell != currentEffective)
                    {
                        TraceLogger.Write("Raising event since effective spell changed for " + ew.Family, TraceLevel.Verbose);
                        if(EffectiveSpellChangedEvent != null)
                            EffectiveSpellChangedEvent(this, new EffectiveSpellChangedEventArgs(ChangeType.Changed, ew.Family, families[ew.Family].EffectiveSpell, currentEffective));
                    }
                }
                else
                {
                    TraceLogger.Write("Creating and adding new family " + ew.Family, TraceLevel.Verbose);
                    EnchantmentFamily newFamily = new EnchantmentFamily(ew.Family);
                    EnchantmentSpell newSpell = newFamily.AddSpell(ew);
                    families.Add(ew.Family, newFamily);
                    TraceLogger.Write("Raising event since effective spell added for " + ew.Family, TraceLevel.Verbose);
                    if (EffectiveSpellChangedEvent != null)
                        EffectiveSpellChangedEvent(this, new EffectiveSpellChangedEventArgs(ChangeType.Added, ew.Family, newSpell, null));
                }
            }
            else if (eventType == AddRemoveEventType.Delete)
            {
                if (families.ContainsKey(ew.Family))
                {
                    TraceLogger.Write("Removing Spell from family " + ew.Family, TraceLevel.Verbose);
                    EnchantmentSpell currentEffective = families[ew.Family].EffectiveSpell;
                    families[ew.Family].RemoveSpell(ew);
                    if (families[ew.Family].EffectiveSpell == (EnchantmentSpell)null)
                    {
                        TraceLogger.Write("Raising event since effective spell deleted for " + ew.Family, TraceLevel.Verbose);
                        if (EffectiveSpellChangedEvent != null)
                            EffectiveSpellChangedEvent(this, new EffectiveSpellChangedEventArgs(ChangeType.Deleted, ew.Family, null, currentEffective));
                        families.Remove(ew.Family);
                    
                    }
                    else if (families[ew.Family].EffectiveSpell != currentEffective)
                    {
                        TraceLogger.Write("Raising event since effective spell changed for " + ew.Family, TraceLevel.Verbose);
                        if (EffectiveSpellChangedEvent != null)
                            EffectiveSpellChangedEvent(this, new EffectiveSpellChangedEventArgs(ChangeType.Changed, ew.Family, families[ew.Family].EffectiveSpell, currentEffective));
                    }
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void Dispose()
        {
            Core.CharacterFilter.ChangeEnchantments -= CharacterFilter_ChangeEnchantments;
            Core = null;
            families.Clear();
        }
    }

    public class EnchantmentFamily
    {
        public int FamilyId { get; private set; }
        List<EnchantmentSpell> SpellLayers;

        internal EnchantmentFamily(int familyId)
        {
            FamilyId = familyId;
            SpellLayers = new List<EnchantmentSpell>();
        }

        public EnchantmentSpell EffectiveSpell
        {
            get
            {
                if (SpellLayers.Count > 0)
                {
                    return SpellLayers[0];
                }
                return null;
            }
        }

        public IEnumerable<EnchantmentSpell> Spells
        {
            get
            {
                return new List<EnchantmentSpell>(SpellLayers);
            }
        }

        internal EnchantmentSpell AddSpell(EnchantmentWrapper ew)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            EnchantmentSpell spell = new EnchantmentSpell(ew);
            TraceLogger.Write("Adding spell " + spell.Name, TraceLevel.Info);
            bool added = false;
            for (int i = 0; i < SpellLayers.Count; i++)
            {
                if (spell.Difficulty > SpellLayers[i].Difficulty)
                {
                    TraceLogger.Write("Spell has a higher diff, inserting at index " + i, TraceLevel.Verbose);
                    SpellLayers.Insert(i, spell);
                    added = true;
                    break;
                }
                else if (spell.Difficulty == SpellLayers[i].Difficulty)
                {
                    if (ew.TimeRemaining > SpellLayers[i].TimeRemaining)
                    {
                        TraceLogger.Write("Spell has a higher time remaining, inserting at index " + i, TraceLevel.Verbose);
                        SpellLayers.Insert(i, spell);
                        added = true;
                        break;
                    }
                }
            }
            if (!added)
            {
                TraceLogger.Write("Adding the spell to the end of the list", TraceLevel.Verbose);
                SpellLayers.Add(spell);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
            return spell;
        }

        internal EnchantmentSpell RemoveSpell(EnchantmentWrapper ew)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            TraceLogger.Write("Removing spell " + ew.SpellId, TraceLevel.Info);
            for (int i = 0; i < SpellLayers.Count; i++)
            {
                if (SpellLayers[i] == ew)
                {
                    EnchantmentSpell temp = SpellLayers[i];
                    TraceLogger.Write("Removed spell at index " + i, TraceLevel.Verbose);
                    SpellLayers.RemoveAt(i);
                    TraceLogger.Write("Exit success", TraceLevel.Verbose);
                    return temp;
                }
            }
            TraceLogger.Write("Exit null", TraceLevel.Verbose);
            return null;
        }
    }

    public class EnchantmentSpell
    {
        Spell _spell;
        int _spellId;
        int _family;
        int _timeRemaining;
        int _difficulty;
        double _duration;
        DateTime _expires;
        string _name;
        int _spellHash;

        public Spell Spell
        {
            get
            {
                return _spell;
            }
        }

        public int SpellId
        {
            get
            {
                return _spellId;
            }
        }

        public int Family
        {
            get
            {
                return _family;
            }
        }

        public int TimeRemaining
        {
            get
            {
                return _timeRemaining;
            }
        }

        public int Difficulty
        {
            get
            {
                return _difficulty;
            }
        }

        public double Duration
        {
            get
            {
                return _duration;
            }
        }

        public DateTime Expires
        {
            get
            {
                return _expires;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        internal EnchantmentSpell(EnchantmentWrapper wrapper)
        {
            _spell = EnchantmentTable.fileService.SpellTable.GetById(wrapper.SpellId);
            _name = _spell.Name.Replace("’", "'");
            _difficulty = _spell.Difficulty;
            _duration = wrapper.Duration;
            _expires = new DateTime(wrapper.Expires.Ticks);
            _family = wrapper.Family;
            _spellId = wrapper.SpellId;
            _timeRemaining = wrapper.TimeRemaining;
            _spellHash = wrapper.GetHashCode();
        }

        public static bool operator ==(EnchantmentSpell spell, EnchantmentWrapper wrapper)
        {
            if (System.Object.ReferenceEquals(spell, wrapper))
            {
                return true;
            }
            if (((object)spell == null) || ((object)wrapper == null))
            {
                return false;
            }
            return spell._spellHash.Equals(wrapper.GetHashCode());
        }

        public static bool operator ==(EnchantmentSpell spell1, EnchantmentSpell spell2)
        {
            if (System.Object.ReferenceEquals(spell1, spell2))
            {
                return true;
            }
            if (((object)spell1 == null) || ((object)spell2 == null))
            {
                return false;
            }
            return spell1._spellHash.Equals(spell2._spellHash);
        }

        public static bool operator !=(EnchantmentSpell spell, EnchantmentWrapper wrapper)
        {
            return !(spell == wrapper);
        }

        public static bool operator !=(EnchantmentSpell spell1, EnchantmentSpell spell2)
        {
            return !(spell1 == spell2);
        }

        public override int GetHashCode()
        {
            return _spellHash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() == typeof(EnchantmentSpell))
                return this == ((EnchantmentSpell)obj);
            if (obj.GetType() == typeof(EnchantmentWrapper))
                return this == ((EnchantmentWrapper)obj);
            return false;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
