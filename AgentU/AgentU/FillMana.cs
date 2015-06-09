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
        private ActionQueue manaStoneQueue;
        private BlockID ManaStoneID;
        private BlockID EquipID;
        private int neededMana;
        private bool forceManaFill = false;

        public void FillMana(bool Force = false)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            forceManaFill = Force;
            ScanEquipment();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void ScanEquipment()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                EquipID.CompletedIDs += new CompletedIDsEvent(EquipID_CompletedIDs);
                foreach (WorldObject wo in Core.WorldFilter.GetByOwner(Core.CharacterFilter.Id))
                {
                    if (wo.Values(LongValueKey.EquippedSlots) != 0)
                    {
                        TraceLogger.Write("Equiped item " + wo.Name, TraceLevel.Verbose);
                        EquipID.Add(wo.Id);
                    }
                }
                EquipID.Start();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void EquipID_CompletedIDs(object sender, CompletedIDsEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                EquipID.CompletedIDs -= new CompletedIDsEvent(EquipID_CompletedIDs);
                neededMana = 0;
                bool willExpireSoon = false;
                foreach (WorldObject wo in e.IDedObjects)
                {
                    int maxMana = wo.Values(LongValueKey.MaximumMana);
                    int curMana = wo.Values(LongValueKey.CurrentMana);
                    neededMana += maxMana - curMana;
                    TraceLogger.Write(String.Format("Item mana for {0} is {1} of {2}, rate of change is {3}", wo.Name, curMana, maxMana, wo.Values(DoubleValueKey.ManaRateOfChange)), TraceLevel.Verbose);
                    if (curMana * (1.0 / Math.Abs(wo.Values(DoubleValueKey.ManaRateOfChange))) < 120)
                    {
                        TraceLogger.Write(wo.Name + " mana will expire soon.", TraceLevel.Info);
                        willExpireSoon = true;
                    }
                }
                if (willExpireSoon || forceManaFill)
                {
                    TraceLogger.Write("Needed mana: " + neededMana, TraceLevel.Info);
                    ScanManaStones();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void ScanManaStones()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                ManaStoneID.CompletedIDs += new CompletedIDsEvent(ManaStoneID_CompletedIDs);
                foreach (WorldObject wo in Core.WorldFilter.GetInventory())
                {
                    if (wo.ObjectClass == ObjectClass.ManaStone)
                    {
                        TraceLogger.Write("Mana Stone: " + wo.Name + ", id " + wo.Id , TraceLevel.Verbose);
                        ManaStoneID.Add(wo.Id);
                    }
                }
                ManaStoneID.Start();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void ManaStoneID_CompletedIDs(object sender, CompletedIDsEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                ManaStoneID.CompletedIDs -= new CompletedIDsEvent(ManaStoneID_CompletedIDs);
                WorldObject bestMatch = null;
                int diffMana = Int32.MaxValue;
                foreach (WorldObject wo in e.IDedObjects)
                {
                    int mana = wo.Values(LongValueKey.CurrentMana);
                    long value = wo.Values(LongValueKey.Value, -1);
                    TraceLogger.Write("Mana Stone after ID: " + wo.Id + ", mana: " + mana + ", value: " + value, TraceLevel.Verbose);
                    if (mana > 0 && value < ItemsSettings.FillThreshold.Value)
                    {
                        int diff = Math.Abs(neededMana - mana);
                        if (diff < diffMana)
                        {
                            bestMatch = wo;
                            diffMana = diff;
                            TraceLogger.Write("New best match " + diffMana, TraceLevel.Verbose);
                        }
                    }
                }
                if (bestMatch != null)
                {
                    TraceLogger.Write("Best Mana Stone " + bestMatch.Id + " diffMana: " + diffMana, TraceLevel.Info);
                    manaStoneQueue.Add(new UseManaStoneAction(bestMatch));
                }
                forceManaFill = false;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }
}