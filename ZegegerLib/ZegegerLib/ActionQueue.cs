using System;
using System.Collections.Generic;
using System.Text;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Zegeger.Diagnostics;
using Zegeger.Decal;

namespace Zegeger.Decal
{
    public enum ResultState
    {
        Successful,
        Failure,
        Timeout,
        Canceled
    }

    public delegate void ActionDone(ActionDoneEventArgs e);
    public delegate void QueueComplete(QueueCompleteEventArgs e);

    public class ActionDoneEventArgs: EventArgs
    {
        public ResultState Result { get; private set; }

        internal ActionDoneEventArgs(ResultState result)
        {
            Result = result;
        }
    }

    public class QueueCompleteEventArgs : EventArgs
    {
        
    }

    public abstract class Action
    {
        protected static CoreManager Core;

        public static void StartUp(CoreManager core)
        {
            Core = core;
        }

        protected abstract void Execute();
        protected abstract void CancelInternal();
        private ActionDone DoneCallback;

        internal void Run(ActionDone callback)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                DoneCallback = callback;
                Execute();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal void Cancel()
        {
            CancelInternal();
        }

        protected void Done(ResultState result)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                DoneCallback(new ActionDoneEventArgs(result));
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }
    
    public enum ActionQueueState
    {
        Init,
        Running,
        Paused,
        Destroyed
    }

    public delegate void ActionReadyEvent(ActionReadyEventArgs e);

    public class ActionReadyEventArgs : EventArgs
    {

    }

    public class ActionQueue
    {
        static bool running = false;
        static Action currentAction = null;
        static List<ActionQueue> queueList = new List<ActionQueue>();
        static int attempt = 0;
        static int maxAttempts = 10;
        static int retryDelay = 1000;
        static ZTimer retryTimer = ZTimer.CreateInstance(timerCallback);

        static ActionQueue()
        {

            TraceLogger.Write("Enter", TraceLevel.Verbose);
            retryTimer.Repeat = true;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static ActionQueue CreateActionQueue(int priority)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ActionQueue newQueue = new ActionQueue(priority);
            newQueue.ActionReady += new ActionReadyEvent(newQueue_ActionReady);
            queueList.Add(newQueue);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
            return newQueue;
        }

        public static void DestroyActionQueue(ActionQueue queue)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            queue.Destroy();
            queue.ActionReady -= new ActionReadyEvent(newQueue_ActionReady);
            queueList.Remove(queue);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        static void newQueue_ActionReady(ActionReadyEventArgs e)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (!running)
                Process();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        static void Process()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                currentAction = null;
                ActionQueue tmpActionQueue = null;
                retryTimer.Stop();
                foreach (ActionQueue queue in queueList)
                {
                    TraceLogger.Write("Queue with priority " + queue.Priority, TraceLevel.Noise);
                    if (queue.State == ActionQueueState.Running && queue.actionQueue.Count > 0 && queue.Priority > (tmpActionQueue != null ? tmpActionQueue.Priority : 0))
                    {
                        TraceLogger.Write("Found a queue with a better Action", TraceLevel.Noise);
                        tmpActionQueue = queue;
                    }
                }
                if (tmpActionQueue != null)
                {
                    running = true;
                    currentAction = tmpActionQueue.Dequeue();
                    maxAttempts = tmpActionQueue.MaxAttempts;
                    retryDelay = tmpActionQueue.RetryDelay;
                    attempt = 1;
                    TraceLogger.Write("Starting Action " + currentAction.GetType().ToString(), TraceLevel.Info);
                    currentAction.Run(actionCallback);
                    retryTimer.Start(retryDelay);
                }
                else
                {
                    TraceLogger.Write("No actions found", TraceLevel.Info);
                    running = false;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        static void timerCallback(object state)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            retryCycle();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        static void actionCallback(ActionDoneEventArgs args)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                TraceLogger.Write("Result = " + args.Result, TraceLevel.Info);
                if (args.Result != ResultState.Failure)
                {
                    Process();
                }
                else
                {
                    retryCycle();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        static void retryCycle()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                attempt++;
                if (attempt < maxAttempts)
                {
                    TraceLogger.Write("Retrying with attempt " + attempt, TraceLevel.Verbose);
                    currentAction.Run(actionCallback);
                }
                else
                {
                    TraceLogger.Write("Max Attempts reached, so moving on", TraceLevel.Info);
                    Process();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        Queue<Action> actionQueue = new Queue<Action>();
        ActionQueueState State = ActionQueueState.Init;
        private int Priority;
        public int MaxAttempts { get; set; }
        public int RetryDelay { get; set; }
        public int Count
        {
            get
            {
                return actionQueue.Count;
            }
        }
        public event QueueComplete QueueCompleteEvent;

        private event ActionReadyEvent ActionReady;

        private ActionQueue(int priority)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            Priority = priority;
            MaxAttempts = 10;
            RetryDelay = 1000;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private ActionQueue(int priority, int maxAttempts, int retryDelay)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            Priority = priority;
            MaxAttempts = maxAttempts;
            RetryDelay = retryDelay;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void Add(Action action)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            if (State != ActionQueueState.Destroyed)
            {
                actionQueue.Enqueue(action);
                RaiseEvent();
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void Start()
        {
            try
            {
                TraceLogger.Write("Enter current state=" + State, TraceLevel.Verbose);
                if (State != ActionQueueState.Destroyed)
                {
                    State = ActionQueueState.Running;
                    RaiseEvent();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void RaiseEvent()
        {
            TraceLogger.Write("Enter current state=" + State, TraceLevel.Verbose);
            if (State == ActionQueueState.Running && actionQueue.Count > 0)
            {
                if (ActionReady != null)
                    ActionReady(new ActionReadyEventArgs());
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private Action Dequeue()
        {
            Action rtn = actionQueue.Dequeue();
            if (actionQueue.Count == 0)
                if (QueueCompleteEvent != null)
                    QueueCompleteEvent(new QueueCompleteEventArgs());
            return rtn;
        }

        public void Pause()
        {
             try
            {
                TraceLogger.Write("Enter current state="+State, TraceLevel.Verbose);
                if (State != ActionQueueState.Destroyed)
                {
                    State = ActionQueueState.Paused;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void Destroy()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                State = ActionQueueState.Destroyed;
                actionQueue.Clear();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }
}
