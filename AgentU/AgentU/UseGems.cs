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

namespace Zegeger.Decal.Plugins.AgentU
{
    internal partial class Items : Component
    {
        private int BenGem = 0;
        private int FavGem = 0;
        private int LessBenGem = 0;

        private void FindGems()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            WorldObjectCollection woc = Core.WorldFilter.GetInventory();
            foreach (WorldObject obj in woc)
            {
                if (obj.Name == "Blackmoor's Favor")
                {
                    TraceLogger.Write("Found Blackmoor's Favor", TraceLevel.Info);
                    FavGem = obj.Id;
                }
                if (obj.Name == "Asheron's Benediction")
                {
                    TraceLogger.Write("Found Asheron's Benediction", TraceLevel.Info);
                    BenGem = obj.Id;
                }
                if (obj.Name == "Asheron's Lesser Benediction")
                {
                    TraceLogger.Write("Found Asheron's Lesser Benediction", TraceLevel.Info);
                    LessBenGem = obj.Id;
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void CheckGemSpells()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            bool foundBen = false; bool foundFav = false; bool foundLessBen = false;
            bool foundBenCoolDown = false; bool foundFavCoolDown = false;

            for (int i = 0; i < Core.CharacterFilter.Enchantments.Count; i++)
            {
                EnchantmentWrapper ew = Core.CharacterFilter.Enchantments[i];
                if (ew.SpellId == Constants.BonusSpells("Asheron's Benediction"))
                {
                    TraceLogger.Write("Found Asheron's Benediction Spell", TraceLevel.Verbose);
                    foundBen = true;
                }
                if (ew.SpellId == Constants.BonusSpells("Blackmoor's Favor"))
                {
                    TraceLogger.Write("Found Blackmoor's Favor Spell", TraceLevel.Verbose);
                    foundFav = true;
                }
                if (ew.SpellId == Constants.BonusSpells("Asheron's Lesser Benediction"))
                {
                    TraceLogger.Write("Found Asheron's Lesser Benediction Spell", TraceLevel.Verbose);
                    foundLessBen = true;
                }
                if (ew.SpellId == Constants.BonusCooldown("Asheron's Benediction"))
                {
                    TraceLogger.Write("Found Asheron's Benediction CoolDown", TraceLevel.Verbose);
                    foundBenCoolDown = true;
                }
                if (ew.SpellId == Constants.BonusCooldown("Blackmoor's Favor"))
                {
                    TraceLogger.Write("Found Blackmoor's Favor CoolDown", TraceLevel.Verbose);
                    foundFavCoolDown = true;
                }
            }

            if (!foundBen && BenGem != 0 && !foundBenCoolDown)
            {
                TraceLogger.Write("Using Asheron's Benediction Gem", TraceLevel.Info);
                Core.Actions.UseItem(BenGem, 0);
            }
            if (!foundLessBen && LessBenGem != 0 && !foundBenCoolDown)
            {
                TraceLogger.Write("Using Asheron's Lesser Benediction Gem", TraceLevel.Info);
                Core.Actions.UseItem(LessBenGem, 0);
            }
            if (!foundFav && FavGem != 0 && !foundFavCoolDown)
            {
                TraceLogger.Write("Using Blackmoor's Favor Gem", TraceLevel.Info);
                Core.Actions.UseItem(FavGem, 0);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }
}