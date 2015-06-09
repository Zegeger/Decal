using System;
using System.Collections.Generic;
using System.Text;
using Zegeger.Decal;
using Zegeger.Diagnostics;
using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace Zegeger.Decal.Plugins.AgentU
{
    /*
    public class UseItemAction : Action
    {
        public WorldObject Item { get; private set; }
        public WorldObject Target { get; private set; }

        public UseItemAction(WorldObject item, WorldObject target)
        {
            Item = item;
            Target = target;
        }

        public override void Run()
        {
            Step = ActionStep.Running;
            ActionQueue.Core.Actions.ApplyItem(Item.Id, Target.Id);
        }

        public override ResultState CheckIfSuccessful()
        {
            if (!ActionQueue.Core.Actions.IsValidObject(Item.Id) || !ActionQueue.Core.Actions.IsValidObject(Target.Id))
            {
                Step = ActionStep.Completed;
                return ResultState.Successful;
            }
            Step = ActionStep.RetryNeeded;
            return ResultState.Failed;
        }
    }
    */
    public class UseStackOnTargetAction : Action
    {
        public WorldObject Item { get; private set; }
        public int Target { get; private set; }

        private int StackCount = -1;
        private bool hookedChangeObjectEvent = false;

        public UseStackOnTargetAction(WorldObject item, int target)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            Item = item;
            Target = target;
            
            try
            {
                if (Item.Exists(LongValueKey.StackMax) && Item.Values(LongValueKey.StackMax) > 1)
                {
                    StackCount = Item.Values(LongValueKey.StackCount);
                    TraceLogger.Write("Stack count for " + item.Name + " is " + StackCount, TraceLevel.Verbose);
                }
                
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        override protected void Execute()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (!CheckIfSuccessful())
                {
                    if (!hookedChangeObjectEvent)
                    {
                        hookedChangeObjectEvent = true;
                        Core.WorldFilter.ChangeObject += new EventHandler<ChangeObjectEventArgs>(WorldFilter_ChangeObject);
                    }
                    TraceLogger.Write("Applying " + Item.Id + " on to " + Target, TraceLevel.Info);
                    Core.Actions.ApplyItem(Item.Id, Target);
                }
                else
                {
                    Complete(ResultState.Successful);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        protected override void CancelInternal()
        {
            Complete(ResultState.Canceled);
        }

        void WorldFilter_ChangeObject(object sender, ChangeObjectEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                if (e.Change == WorldChangeType.SizeChange && e.Changed == Item)
                {
                    TraceLogger.Write("Stack Size change for " + e.Changed.Name, TraceLevel.Verbose);
                    if (CheckIfSuccessful())
                    {
                        Complete(ResultState.Successful);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        private void Complete(ResultState result)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                Core.WorldFilter.ChangeObject -= new EventHandler<ChangeObjectEventArgs>(WorldFilter_ChangeObject);
                Done(result);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private bool CheckIfSuccessful()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (!Core.Actions.IsValidObject(Item.Id) || !Core.Actions.IsValidObject(Target))
            {
                TraceLogger.Write("Exit true", TraceLevel.Verbose);
                return true;
            }
            else
            {
                if (Item.Values(LongValueKey.StackCount) < StackCount)
                {
                    TraceLogger.Write("Exit true", TraceLevel.Verbose);
                    return true;
                }
            }
            TraceLogger.Write("Exit false", TraceLevel.Verbose);
            return false;
        }
    }
    /*
    public class UseItemOnStackAction : Action
    {
        public WorldObject Item { get; private set; }
        public WorldObject Target { get; private set; }

        private int StackCount = -1;

        public UseItemOnStackAction(WorldObject item, WorldObject target)
        {
            Item = item;
            Target = target;
        }

        public override void Run()
        {
            if (Target.Exists(LongValueKey.StackMax) && Target.Values(LongValueKey.StackMax) > 1)
            {
                StackCount = Target.Values(LongValueKey.StackCount);
            }
            Step = ActionStep.Running;
            ActionQueue.Core.Actions.ApplyItem(Item.Id, Target.Id);
        }

        public override ResultState CheckIfSuccessful()
        {
            if (!ActionQueue.Core.Actions.IsValidObject(Item.Id) || !ActionQueue.Core.Actions.IsValidObject(Target.Id))
            {
                Step = ActionStep.Completed;
                return ResultState.Successful;
            }
            else
            {
                if (Target.Values(LongValueKey.StackCount) < StackCount)
                {
                    Step = ActionStep.Completed;
                    return ResultState.Successful;
                }
            }
            Step = ActionStep.RetryNeeded;
            return ResultState.Failed;
        }
    }
    */
    public class UseManaStoneAction : Action
    {
        private WorldObject ManaStone { get; set; }
        private int Target { get; set; }
        private bool _disposed;

        public UseManaStoneAction(WorldObject item)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ManaStone = item;
            _disposed = false;
            Zegeger.Decal.Chat.ZChatWrapper.ChatBoxMessage += new Chat.ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
            Core.WorldFilter.ReleaseObject += new EventHandler<ReleaseObjectEventArgs>(WorldFilter_ReleaseObject);
            ManaStone.Disposing += new EventHandler(ManaStone_Disposing);
            Target = Core.CharacterFilter.Id;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void ManaStone_Disposing(object sender, EventArgs e)
        {
            TraceLogger.Write("ManaStone WO Disposed", TraceLevel.Verbose);
            _disposed = true;
            ManaStone.Disposing -= new EventHandler(ManaStone_Disposing);
        }

        override protected void Execute()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                Apply();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        protected override void CancelInternal()
        {
            Complete(ResultState.Canceled);
        }

        void WorldFilter_ReleaseObject(object sender, ReleaseObjectEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (e.Released.Id == ManaStone.Id)
                {
                    TraceLogger.Write("Mana Stone was applied", TraceLevel.Info);
                    Complete(ResultState.Successful);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void ZChatWrapper_ChatBoxMessage(Chat.ChatBoxMessageEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (e.Color == Constants.ChatClasses("System") && e.Text.Contains("The Mana Stone gives"))
                {
                    TraceLogger.Write("Mana Stone chat message", TraceLevel.Info);
                    Zegeger.Decal.Chat.ZChatWrapper.ChatBoxMessage -= new Chat.ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
                    Core.WorldFilter.ChangeObject += new EventHandler<ChangeObjectEventArgs>(WorldFilter_ChangeObject);
                    Core.Actions.RequestId(ManaStone.Id);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        void WorldFilter_ChangeObject(object sender, ChangeObjectEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (e.Change == WorldChangeType.IdentReceived && e.Changed.Id == ManaStone.Id)
                {
                    Core.WorldFilter.ChangeObject -= new EventHandler<ChangeObjectEventArgs>(WorldFilter_ChangeObject);
                    TraceLogger.Write("ID of Mana Stone Complete", TraceLevel.Info);
                    Apply();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void Complete(ResultState result)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                Zegeger.Decal.Chat.ZChatWrapper.ChatBoxMessage -= new Chat.ChatBoxMessageEvent(ZChatWrapper_ChatBoxMessage);
                Core.WorldFilter.ChangeObject -= new EventHandler<ChangeObjectEventArgs>(WorldFilter_ChangeObject);
                Core.WorldFilter.ReleaseObject -= new EventHandler<ReleaseObjectEventArgs>(WorldFilter_ReleaseObject);
                Done(result);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void Apply()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (CheckIfValid())
                {
                    TraceLogger.Write("Applying manastone " + ManaStone.Id + " onto your character", TraceLevel.Info);
                    Core.Actions.ApplyItem(ManaStone.Id, Target);
                }
                else
                {
                    TraceLogger.Write("Mana Stone was applied", TraceLevel.Info);
                    Complete(ResultState.Successful);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private bool CheckIfValid()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (!Core.Actions.IsValidObject(ManaStone.Id) || _disposed)
                {
                    TraceLogger.Write("Exit false on item destroyed", TraceLevel.Verbose);
                    return false;
                }
                else
                {
                    TraceLogger.Write("Mana Stone Mana = " + ManaStone.Values(LongValueKey.CurrentMana), TraceLevel.Verbose);
                    if (ManaStone.Values(LongValueKey.CurrentMana) == 0)
                    {
                        TraceLogger.Write("Exit false on mana emptied", TraceLevel.Verbose);
                        return false;
                    }
                }
                TraceLogger.Write("Exit true", TraceLevel.Verbose);
                return true;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit false on exception", TraceLevel.Verbose);
            return false;
        }
    }
}
