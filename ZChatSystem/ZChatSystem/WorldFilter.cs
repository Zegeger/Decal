using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Zegeger.Decal.Plugins.ZChatSystem.Diagnostics;

namespace Zegeger.Decal.Plugins.ZChatSystem
{
    internal partial class PluginCore
    {
        private void initWorldFilter()
        {
            TraceLogger.Write( "Enter");
            //////////////////////////////////////////////////////////////
            // To initialize any of the World Filter Events,            //
            // simply uncomment the appropriate initialization          //
            // statement(s) below to enable the event handler           //
            //////////////////////////////////////////////////////////////

            // Initialize the ResetTrade event handler
            //Core.WorldFilter.ResetTrade += new EventHandler<Decal.Adapter.Wrappers.ResetTradeEventArgs>(WorldFilter_ResetTrade);

            // Initialize the ReleaseObject event handler
            //Core.WorldFilter.ReleaseObject += new EventHandler<Decal.Adapter.Wrappers.ReleaseObjectEventArgs>(WorldFilter_ReleaseObject);

            // Initialize the ReleaseDone event handler
            //Core.WorldFilter.ReleaseDone += new EventHandler(WorldFilter_ReleaseDone);

            // Initialize the MoveObject event handler
            //Core.WorldFilter.MoveObject += new EventHandler<Decal.Adapter.Wrappers.MoveObjectEventArgs>(WorldFilter_MoveObject);

            // Initialize the FailToCompleteTrade event handler
            //Core.WorldFilter.FailToCompleteTrade += new EventHandler(WorldFilter_FailToCompleteTrade);

            // Initialize the FailToAddTradeItem event handler
            //Core.WorldFilter.FailToAddTradeItem += new EventHandler<Decal.Adapter.Wrappers.FailToAddTradeItemEventArgs>(WorldFilter_FailToAddTradeItem);

            // Initialize the EnterTrade event handler
            //Core.WorldFilter.EnterTrade += new EventHandler<Decal.Adapter.Wrappers.EnterTradeEventArgs>(WorldFilter_EnterTrade);

            // Initialize the EndTrade event handler
            //Core.WorldFilter.EndTrade += new EventHandler<Decal.Adapter.Wrappers.EndTradeEventArgs>(WorldFilter_EndTrade);

            // Initialize the DeclineTrade event handler
            //Core.WorldFilter.DeclineTrade += new EventHandler<Decal.Adapter.Wrappers.DeclineTradeEventArgs>(WorldFilter_DeclineTrade);

            // Initialize the CreateObject event handler
            //Core.WorldFilter.CreateObject += new EventHandler<Decal.Adapter.Wrappers.CreateObjectEventArgs>(WorldFilter_CreateObject);

            // Initialize the ChangeObject event handler
            //Core.WorldFilter.ChangeObject += new EventHandler<Decal.Adapter.Wrappers.ChangeObjectEventArgs>(WorldFilter_ChangeObject);

            // Initialize the ApproachVendor event handler
            //Core.WorldFilter.ApproachVendor += new EventHandler<Decal.Adapter.Wrappers.ApproachVendorEventArgs>(WorldFilter_ApproachVendor);

            // Initialize the AddTradeItem event handler
            //Core.WorldFilter.AddTradeItem += new EventHandler<Decal.Adapter.Wrappers.AddTradeItemEventArgs>(WorldFilter_AddTradeItem);

            // Initialize the AcceptTrade event handler
            //Core.WorldFilter.AcceptTrade += new EventHandler<Decal.Adapter.Wrappers.AcceptTradeEventArgs>(WorldFilter_AcceptTrade);
            TraceLogger.Write( "Exit");
        }

        void WorldFilter_AcceptTrade(object sender, AcceptTradeEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_AddTradeItem(object sender, AddTradeItemEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_ApproachVendor(object sender, ApproachVendorEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_ChangeObject(object sender, ChangeObjectEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_CreateObject(object sender, CreateObjectEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_DeclineTrade(object sender, DeclineTradeEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_EndTrade(object sender, EndTradeEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_EnterTrade(object sender, EnterTradeEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_FailToAddTradeItem(object sender, FailToAddTradeItemEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_FailToCompleteTrade(object sender, EventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_MoveObject(object sender, MoveObjectEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_ReleaseDone(object sender, EventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_ReleaseObject(object sender, ReleaseObjectEventArgs e)
        {
            // DO STUFF HERE
        }

        void WorldFilter_ResetTrade(object sender, ResetTradeEventArgs e)
        {
            // DO STUFF HERE
        }

        private void destroyWorldFilter()
        {
            TraceLogger.Write( "Enter");
            // UnInitialize the ResetTrade event handler
            // Core.WorldFilter.ResetTrade -= new EventHandler<Decal.Adapter.Wrappers.ResetTradeEventArgs>(WorldFilter_ResetTrade);

            // UnInitialize the ReleaseObject event handler
            // Core.WorldFilter.ReleaseObject -= new EventHandler<Decal.Adapter.Wrappers.ReleaseObjectEventArgs>(WorldFilter_ReleaseObject);

            // UnInitialize the ReleaseDone event handler
            // Core.WorldFilter.ReleaseDone -= new EventHandler(WorldFilter_ReleaseDone);

            // UnInitialize the MoveObject event handler
            // Core.WorldFilter.MoveObject -= new EventHandler<Decal.Adapter.Wrappers.MoveObjectEventArgs>(WorldFilter_MoveObject);

            // UnInitialize the FailToCompleteTrade event handler
            // Core.WorldFilter.FailToCompleteTrade -= new EventHandler(WorldFilter_FailToCompleteTrade);

            // UnInitialize the FailToAddTradeItem event handler
            // Core.WorldFilter.FailToAddTradeItem -= new EventHandler<Decal.Adapter.Wrappers.FailToAddTradeItemEventArgs>(WorldFilter_FailToAddTradeItem);

            // UnInitialize the EnterTrade event handler
            // Core.WorldFilter.EnterTrade -= new EventHandler<Decal.Adapter.Wrappers.EnterTradeEventArgs>(WorldFilter_EnterTrade);

            // UnInitialize the EndTrade event handler
            // Core.WorldFilter.EndTrade -= new EventHandler<Decal.Adapter.Wrappers.EndTradeEventArgs>(WorldFilter_EndTrade);

            // UnInitialize the DeclineTrade event handler
            // Core.WorldFilter.DeclineTrade -= new EventHandler<Decal.Adapter.Wrappers.DeclineTradeEventArgs>(WorldFilter_DeclineTrade);

            // UnInitialize the CreateObject event handler
            //Core.WorldFilter.CreateObject -= new EventHandler<Decal.Adapter.Wrappers.CreateObjectEventArgs>(WorldFilter_CreateObject);

            // UnInitialize the ChangeObject event handler
            // Core.WorldFilter.ChangeObject -= new EventHandler<Decal.Adapter.Wrappers.ChangeObjectEventArgs>(WorldFilter_ChangeObject);

            // UnInitialize the ApproachVendor event handler
            // Core.WorldFilter.ApproachVendor -= new EventHandler<Decal.Adapter.Wrappers.ApproachVendorEventArgs>(WorldFilter_ApproachVendor);

            // UnInitialize the AddTradeItem event handler
            // Core.WorldFilter.AddTradeItem -= new EventHandler<Decal.Adapter.Wrappers.AddTradeItemEventArgs>(WorldFilter_AddTradeItem);

            // UnInitialize the AcceptTrade event handler
            // Core.WorldFilter.AcceptTrade -= new EventHandler<Decal.Adapter.Wrappers.AcceptTradeEventArgs>(WorldFilter_AcceptTrade);
            TraceLogger.Write( "Exit");
        }
    }
}