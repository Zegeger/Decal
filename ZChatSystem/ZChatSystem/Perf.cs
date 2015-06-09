using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Zegeger.Decal.Plugins.ZChatSystem.Diagnostics;

namespace Zegeger.Decal.Plugins.ZChatSystem
{
    public partial class ZChatSystem
    {
        internal class PerfData
        {
            internal DateTime startTime { get; set; }
            internal DateTime finishTime { get; set; }
            internal string originalText { get; set; }
            internal List<string> checkText;

            public PerfData(DateTime start, string txt)
            {
                startTime = start;
                originalText = txt;
                checkText = new List<string>();
            }
        }

        static Dictionary<int, PerfData> PerfDataList;
        static int PerfCounter;

        internal static void initPerf()
        {
            PerfDataList = new Dictionary<int,PerfData>();
            PerfCounter = 0;
        }

        internal static void destroyPerf()
        {
            PerfDataList = null;
            PerfCounter = 0;
        }

        internal static int startPerfMsg(string originalText)
        {
            PerfCounter += 1;
            //Logging.LogPerfToTrace("Start - PerfId: " + PerfCounter);
            PerfDataList.Add(PerfCounter, new PerfData(DateTime.Now, originalText));
            return PerfCounter;
        }

        internal static void finishPerfMsg(int PerfId)
        {
            PerfDataList[PerfId].finishTime = DateTime.Now;
            int length = (PerfDataList[PerfId].finishTime - PerfDataList[PerfId].startTime).Milliseconds;
            foreach (string ct in PerfDataList[PerfId].checkText)
            {
                TraceLogger.Write(ct);
            }
            TraceLogger.Write("Finished - PerfId: " + PerfId + ", Duration: " + length + ", Text: " + PerfDataList[PerfId].originalText);
            PerfDataList[PerfId].checkText = null;
            PerfDataList.Remove(PerfId);
        }

        [Conditional("PERF"), Conditional("TRACE")]
        internal static void checkPerfMsg(int PerfId, string note)
        {
            PerfDataList[PerfId].finishTime = DateTime.Now;
            int length = (PerfDataList[PerfId].finishTime - PerfDataList[PerfId].startTime).Milliseconds;
            PerfDataList[PerfId].checkText.Add("Check - PerfId: " + PerfId + ", Duration: " + length + ", Note: " + note);
        }
    }
}
