#define TRACE

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Zegeger.Decal.Plugins.ZChatSystem
{
    internal class Logging
    {
        private static string tracingLogFile;
        private static string errorLogFile;
        private static System.IO.StreamWriter sw;

        internal static bool TracingOn;

        internal static void initLogging(string TracingPath)
        {
            string tracingFileName, errorFileName;
            TracingOn = true;
            tracingFileName = TracingPath + "ZChatSystemTraceLog.txt";
            errorFileName = TracingPath + "ZChatSystemErrorLog.txt";
            tracingLogFile = getFileName(tracingFileName, 0);
            errorLogFile = getFileName(errorFileName, 0);
            startTracing();
            sw = new System.IO.StreamWriter(tracingLogFile, true);
        }

        internal static void destroyLogging()
        {
            sw.Close();
            sw = null;
        }

        private static string getFileName(string filename, int count)
        {
            if (count == 0)
            {
                if (IsFileLocked(new FileInfo(filename)))
                {
                    return getFileName(filename, 1);
                }
                else
                {
                    return filename;
                }
            }
            else
            {
                if (IsFileLocked(new FileInfo(filename.Substring(0, filename.Length - 4) + count + ".txt")))
                {
                    return getFileName(filename, count + 1);
                }
                else
                {
                    return filename.Substring(0, filename.Length - 4) + count + ".txt";
                }
            }
        }

        private static void startTracing()
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(tracingLogFile, false);
            sw.WriteLine(DateTime.Now.ToString() + " - PLUGIN STARTUP");
            sw.Close();
        }

        [ Conditional("TRACE") ]
        internal static void LogTracing(string message)
        {
            if (TracingOn)
            {
                StackFrame sf = new StackFrame(1, false);
                //System.IO.StreamWriter sw = new System.IO.StreamWriter(tracingLogFile, true);
                sw.WriteLine("[T]" + DateTime.Now.ToString("M/d/yyyy HH:mm:ss:fff") + " - " + sf.GetMethod() + " - " + message);
                sw.Flush();
                //PluginCore.wtcwt(DateTime.Now.ToString() + " - " + sf.GetMethod() + " - " + message);
            }
        }

        [Conditional("TRACE")]
        private static void LogErrorToTrace(string message, string source)
        {
            if (TracingOn)
            {
                sw.WriteLine("[E]" + DateTime.Now.ToString("M/d/yyyy HH:mm:ss:fff") + " - " + source + " - " + message);
                sw.Flush();
            }
        }

        [Conditional("PERF")]
        internal static void LogPerfToTrace(string message)
        {
            sw.WriteLine("[P]" + DateTime.Now.ToString("M/d/yyyy HH:mm:ss:fff") + " - " + message);
            sw.Flush();
        }

        internal static void LogError(Exception ex)
        {
            System.IO.StreamWriter ew = new System.IO.StreamWriter(errorLogFile, true);
            ew.WriteLine("============================================================================");
            ew.WriteLine(DateTime.Now.ToString("M/d/yyyy HH:mm:ss:fff"));
            ew.WriteLine("Error: " + ex.Message);
            ew.WriteLine("Source: " + ex.Source);
            ew.WriteLine("Stack: " + ex.StackTrace);
            if (ex.InnerException != null)
            {
                ew.WriteLine("Inner: " + ex.InnerException.Message);
                ew.WriteLine("Inner Stack: " + ex.InnerException.StackTrace);
            }
            ew.WriteLine("============================================================================");
            ew.WriteLine("");
            ew.Close();
            LogErrorToTrace(ex.Message, ex.Source);
        }

        protected static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

    }
}