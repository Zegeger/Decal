using System;
using System.Collections.Generic;
using System.Text;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Zegeger.Diagnostics;

namespace Zegeger.Decal
{
    public delegate void CompletedIDsEvent(object sender, CompletedIDsEventArgs e);

    public class CompletedIDsEventArgs:EventArgs
    {
        public List<WorldObject> IDedObjects { get; private set; }

        public CompletedIDsEventArgs(List<WorldObject> ObjectList)
        {
            IDedObjects = ObjectList;
        }
    }

    public class BlockID
    {
        CoreManager Core;
        List<int> IDQueue;
        List<WorldObject> CompletedObjects;
        bool running = false;

        public event CompletedIDsEvent CompletedIDs;

        public BlockID(CoreManager core)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            IDQueue = new List<int>();
            CompletedObjects = new List<WorldObject>();
            Core = core;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void Add(int Id)
        {
            TraceLogger.Write("Enter id=" + Id, TraceLevel.Verbose);
            if (!running)
            {
                IDQueue.Add(Id);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void Start()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (!running)
                {
                    running = true;
                    CompletedObjects = new List<WorldObject>();
                    Core.WorldFilter.ChangeObject += new EventHandler<ChangeObjectEventArgs>(WorldFilter_ChangeObject);
                    TraceLogger.Write("Number of Items to ID: " + IDQueue.Count, TraceLevel.Info);
                    foreach (int obj in IDQueue)
                    {
                        Core.Actions.RequestId(obj);
                    }
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
                TraceLogger.Write("Enter", TraceLevel.Noise);
                if (e.Change == WorldChangeType.IdentReceived)
                {
                    if (IDQueue.Contains(e.Changed.Id))
                    {
                        TraceLogger.Write("ID completed for " + e.Changed.Id, TraceLevel.Info);
                        IDQueue.Remove(e.Changed.Id);
                        CompletedObjects.Add(e.Changed);
                        if (IDQueue.Count == 0)
                        {
                            TraceLogger.Write("All ID's Done", TraceLevel.Info);
                            running = false;
                            Core.WorldFilter.ChangeObject -= new EventHandler<ChangeObjectEventArgs>(WorldFilter_ChangeObject);
                            CompletedIDs(this, new CompletedIDsEventArgs(CompletedObjects));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }
    }
}
