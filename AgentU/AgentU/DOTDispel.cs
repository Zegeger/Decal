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
        private ActionQueue dispelQueue;

        private void UseDispellForDOTIfNeeded()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            FileService fs = Core.FileService as FileService;

            Dictionary<int, int> DotDmg = new Dictionary<int, int>();
            int maxLevel = 0;

            for (int i = 0; i < Core.CharacterFilter.Enchantments.Count; i++)
            {
                EnchantmentWrapper ew = Core.CharacterFilter.Enchantments[i];
                if (Constants.VoidDamageDictionary.ContainsKey(ew.SpellId))
                {
                    TraceLogger.Write("Dot Spell Found: " + ew.SpellId + ", Family: " + ew.Family, TraceLevel.Noise);
                    int tickDmg = Constants.VoidDamage(ew.SpellId);
                    if (DotDmg.ContainsKey(ew.Family))
                    {
                        if (DotDmg[ew.Family] < tickDmg)
                        {
                            TraceLogger.Write("Updating Family Tick Damage to " + tickDmg, TraceLevel.Noise);
                            DotDmg[ew.Family] = tickDmg;
                        }
                    }
                    else
                    {
                        TraceLogger.Write("Adding Family Tick Damage " + tickDmg, TraceLevel.Noise);
                        DotDmg.Add(ew.Family, tickDmg);
                    }
                    int level = LazySpellLevel(ew.SpellId);
                    if (maxLevel < level)
                        maxLevel = level;
                }
            }

            int total = 0;
            foreach (int dmg in DotDmg.Values)
            {
                total += dmg;
            }
            TraceLogger.Write("Total tick damage = " + total, TraceLevel.Info);
            if (total > ItemsSettings.DotDamage.Value)
            {
                TraceLogger.Write("Tick damage exceeds setting of " + ItemsSettings.DotDamage, TraceLevel.Info);
                Dictionary<int, WorldObject> Gems = new Dictionary<int, WorldObject>();
                foreach (KeyValuePair<string, int> x in Constants.DispelGemsDictionary)
                {
                    if (x.Value >= maxLevel)
                    {
                        TraceLogger.Write("Possible Gem matched " + x.Key, TraceLevel.Noise);
                        foreach (WorldObject wo in Core.WorldFilter.GetByName(x.Key))
                        {
                            TraceLogger.Write("Gem found in inventory " + x.Key, TraceLevel.Noise);
                            if (!Gems.ContainsKey(x.Value))
                                Gems.Add(x.Value, wo);
                            break;
                        }
                    }
                }
                int bestlevel = 999;
                WorldObject bestObject = null;
                foreach (KeyValuePair<int, WorldObject> x in Gems)
                {
                    if (x.Key >= maxLevel && x.Key < bestlevel)
                    {
                        TraceLogger.Write("Best gem changed to " + x.Value.Name, TraceLevel.Noise);
                        bestObject = x.Value;
                        bestlevel = x.Key;
                    }
                }

                if (bestObject != null)
                {
                    TraceLogger.Write("Preparing to use " + bestObject.Name, TraceLevel.Info);
                    dispelQueue.Add(new UseStackOnTargetAction(bestObject, Core.CharacterFilter.Id));
                }
            }

            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private int LazySpellLevel(int id)
        {
            FileService fs = Core.FileService as FileService;
            Spell spl = fs.SpellTable.GetById(id);
            if (spl.Name.Contains("Incantation"))
                return 8;
            if (spl.Name.Contains("VII"))
                return 7;
            if (spl.Name.Contains("VI"))
                return 6;
            if (spl.Name.Contains("V"))
                return 5;
            if (spl.Name.Contains("IV"))
                return 4;
            if (spl.Name.Contains("III"))
                return 3;
            if (spl.Name.Contains("II"))
                return 2;
            if (spl.Name.Contains("I"))
                return 1;
            return 0;
        }
    }
}
