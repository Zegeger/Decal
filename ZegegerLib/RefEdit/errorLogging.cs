using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Threading;

namespace Zegeger.Data
{
    internal class traceLogger
    {
        private static System.Timers.Timer logFlusher = new System.Timers.Timer(10000);
        internal static TraceLevel loggingLevel = TraceLevel.Verbose;
        internal static string LogPath;

        internal static void StartUp(string path)
        {
            LogPath = path;
            Trace.Listeners.Clear();
            TextWriterTraceListener twtl = new TextWriterTraceListener(path);
            Trace.Listeners.Add(twtl);
            logFlusher.AutoReset = true;
            logFlusher.Elapsed += new ElapsedEventHandler(logFlusher_Elapsed);
            logFlusher.Start();
        }

        static void logFlusher_Elapsed(object sender, ElapsedEventArgs e)
        {
            Trace.Flush();
        }

        internal static void ShutDown()
        {
            Trace.Close();
        }

        internal static void Write(Exception ex)
        {
            StringWriter sw = new StringWriter();
            sw.WriteLine("============================================================================");
            sw.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:fff"));
            sw.WriteLine("Error: " + ex.Message);
            sw.WriteLine("Source: " + ex.Source);
            sw.WriteLine("Stack: " + ex.StackTrace);
            if (ex.InnerException != null)
            {
                sw.WriteLine("Inner: " + ex.InnerException.Message);
                sw.WriteLine("Inner Stack: " + ex.InnerException.StackTrace);
            }
            sw.WriteLine("============================================================================");
            Trace.WriteLine(sw.ToString());
            sw.Close();
        }

        internal static void Write(string text, TraceLevel level = TraceLevel.Info)
        {
            if (level == TraceLevel.Off)
                return;
            if (loggingLevel == TraceLevel.Info && level == TraceLevel.Verbose)
                return;
            if (loggingLevel == TraceLevel.Warning && (level == TraceLevel.Verbose || level == TraceLevel.Info))
                return;
            if (loggingLevel == TraceLevel.Error && level != TraceLevel.Error)
                return;
            StackFrame sf = new StackFrame(1, true);
            string output = "";
            switch (level)
            {
                case TraceLevel.Error:
                    output = "ERROR   ";
                    break;
                case TraceLevel.Warning:
                    output = "WARN    ";
                    break;
                case TraceLevel.Info:
                    output = "INFO    ";
                    break;
                case TraceLevel.Verbose:
                    output = "VERBOSE ";
                    break;
            }
            output += DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:fff") + " ";
            output += "[ " + Path.GetFileName(sf.GetFileName()) + "(" + sf.GetFileLineNumber() + ") " + sf.GetMethod() + " ] ";
            output += text;
            Trace.WriteLine(output);
        }

        internal static void Write(string format, TraceLevel level, params object[] args)
        {
            Write(String.Format(format, args), level);
        }
    }
}