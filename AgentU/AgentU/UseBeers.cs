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
        ZTimer BeerTimer;
        ZTimer RunTimer;
        ActionQueue BeerQueue;
        bool Warned = false;
        List<string> BeerAdded = new List<string>();

        void BeerQueue_QueueCompleteEvent(QueueCompleteEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            BeerQueue.Pause();
            BeerAdded.Clear();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void CheckBeerSpells()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            Dictionary<int, DateTime> BeerSpellExpires = new Dictionary<int, DateTime>();
            foreach (int s in Constants.BeerSpellsDictionary.Keys)
            {
                BeerSpellExpires.Add(s, DateTime.MinValue);
            }

            foreach (EnchantmentWrapper ew in Core.CharacterFilter.Enchantments)
            {
                if (Constants.BeerSpellsDictionary.ContainsKey(ew.SpellId))
                {
                    TraceLogger.Write("Setting spell " + ew.SpellId + " expires " + ew.Expires, TraceLevel.Verbose);
                    BeerSpellExpires[ew.SpellId] = ew.Expires;
                }
            }

            double shortest = double.MaxValue;
            foreach (KeyValuePair<int, DateTime> bse in BeerSpellExpires)
            {
                TraceLogger.Write("Spell " + bse.Key + " expires at " + bse.Value + ", threshold is " + DateTime.Now.AddSeconds(ItemsSettings.UseBeersTime.Value), TraceLevel.Info);
                if (bse.Value == DateTime.MinValue)
                {
                    TraceLogger.Write("Spell " + bse.Key + " is not present, so attempting to add beer", TraceLevel.Info);
                    AddBeer(bse.Key);
                }
                else if (bse.Value < DateTime.Now.AddSeconds(ItemsSettings.UseBeersTime.Value))
                {
                    TraceLogger.Write("Spell " + bse.Key + " expires at " + bse.Value + " which is less than " + DateTime.Now.AddSeconds(ItemsSettings.UseBeersTime.Value) + ", so attempting to add beer", TraceLevel.Info);
                    AddBeer(bse.Key);
                }
                else
                {
                    double duration = (bse.Value - DateTime.Now).TotalSeconds;
                    if (duration < shortest)
                    {
                        TraceLogger.Write("Spell " + bse.Key + " is now the soonest to expire at " + duration, TraceLevel.Info);
                        shortest = duration;
                    }
                }
            }

            if (shortest != double.MaxValue)
            {
                int duration = ((int)shortest - ItemsSettings.UseBeersTime.Value + 20) * 1000;
                TraceLogger.Write("Starting Beer Timer with duration of " + duration, TraceLevel.Info);
                BeerTimer.Start(duration);
            }

            RunQueue();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void BeerTimerCallback(object state)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            CheckBeerSpells();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void RunTimerCallback(object state)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            RunQueue();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void RunQueue()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (BeerQueue.Count > 0)
            {
                if (ItemsSettings.RunQueueAutomatically.Value)
                {
                    if (Core.Actions.CombatMode == CombatState.Peace)
                    {
                        StartBeerQueue();
                    }
                    else
                    {
                        if (!Warned)
                        {
                            PluginCore.WriteToChat("Please enter peace mode to drink your beers");
                            Warned = true;
                        }
                        TraceLogger.Write("Staring Run Timer", TraceLevel.Verbose);
                        RunTimer.Start(1000);
                    }
                }
                else
                {
                    if (!Warned)
                    {
                        PluginCore.WriteToChat("Beers have been added to the queue.  Please <Tell:IIDString:463074:[Beers]>click here<\\Tell> to start drinking them.");
                        Warned = true;
                    }
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void StartBeerQueue()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (BeerQueue.Count > 0)
            {
                TraceLogger.Write("Starting Beer Queue", TraceLevel.Info);
                BeerQueue.Start();
                Warned = false;
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void AddBeer(int beer)
        {
            TraceLogger.Write("Enter beer " + beer, TraceLevel.Verbose);
            switch (Constants.BeerSpells(beer))
            {
                case "KETNANEYE":
                    if (ItemsSettings.Focus.Value)
                    {
                        AddBeer("Tusker Spit Ale");
                    }
                    break;
                case "BRIGHTEYE":
                    if (ItemsSettings.Coordination.Value)
                    {
                        AddBeer("Amber Ape");
                    }
                    break;
                case "BOBOQUICK":
                    if (ItemsSettings.Quickness.Value)
                    {
                        AddBeer("Bobo's Stout");
                    }
                    break;
                case "ZONGOFIST":
                    if (ItemsSettings.Strength.Value)
                    {
                        AddBeer("Apothecary Zongo's Stout");
                    }
                    break;
                case "RAOULPRIDE":
                    if (ItemsSettings.Self.Value)
                    {
                        AddBeer("Duke Raoul's Distillation");
                    }
                    break;
                case "HUNTERHARD":
                    if (ItemsSettings.Endurance.Value)
                    {
                        AddBeer("Hunter's Stock Amber");
                    }
                    break;
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void AddBeer(string name)
        {
            TraceLogger.Write("Enter beer " + name, TraceLevel.Verbose);
            if (!BeerAdded.Contains(name))
            {
                foreach (WorldObject wo in Core.WorldFilter.GetInventory())
                {
                    if (wo.Name == name)
                    {
                        TraceLogger.Write("Found beer " + name, TraceLevel.Info);
                        PluginCore.WriteToChat("Adding " + name + " to the queue");
                        BeerAdded.Add(name);
                        BeerQueue.Add(new UseStackOnTargetAction(wo, Core.CharacterFilter.Id));
                        break;
                    }
                }
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }
}
