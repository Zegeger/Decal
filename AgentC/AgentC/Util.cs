using System;
using System.Collections.Generic;
using System.Text;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Plugins.AgentC
{
    internal static class Util
    {
        internal static string RemoveClickableText(string text)
        {
            TraceLogger.Write("Enter, text: " + text, TraceLevel.Noise);
            string rtn = text;
            int start = text.IndexOf("<Tell:IIDString:");
            while (start != -1)
            {
                int finish = rtn.IndexOf(">") + 1;
                rtn = rtn.Remove(start, finish - start);
                rtn = rtn.Remove(rtn.IndexOf(@"<\Tell>"), 7);
                start = rtn.IndexOf("<Tell:IIDString:");
            }
            TraceLogger.Write("Exit returning " + rtn, TraceLevel.Noise);
            return rtn;
        }

        internal static string ClickableText(string text)
        {
            TraceLogger.Write("Enter, text: " + text, TraceLevel.Noise);
            string rtn = "";
            int start = text.IndexOf("<Tell:IIDString:");
            if (start != -1)
            {
                int finish = text.IndexOf(@"<\Tell>") + 7;
                rtn = text.Substring(start, finish - start);
            }
            TraceLogger.Write("Exit returning " + rtn, TraceLevel.Noise);
            return rtn;
        }
    }
}
