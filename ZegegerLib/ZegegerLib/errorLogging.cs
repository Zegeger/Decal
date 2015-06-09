using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Threading;

namespace Zegeger.Diagnostics
{
    public enum TraceLevel
    {
        Off,
        Fatal,
        Error,
        Warning,
        Info,
        Verbose,
        Noise
    }

    public static class TraceLogger
    {
        private static System.Timers.Timer logFlusher;
        private static TraceLevel loggingLevel = TraceLevel.Verbose;

        private static string pathBase;
        private static string path;
        private static int logRollOverByteSize = 20 * 1024 * 1024;
        private static int maxLogCount = 50;
        private static int logNumber = 1;
        private static bool autoFlush = false;

        private static StreamWriter fileStream;

        public static TraceLevel LoggingLevel
        {
            get { return loggingLevel; }
            set { loggingLevel = value; }
        }

        public static int LogRollOverByteSize
        {
            get { return logRollOverByteSize; }
            set { logRollOverByteSize = value; }
        }

        public static int MaxLogCount
        {
            get { return maxLogCount; }
            set { maxLogCount = value; }
        }

        public static double FlushInterval
        {
            get { return logFlusher.Interval; }
            set { logFlusher.Interval = value; }
        }

        public static bool AutoFlush
        {
            get { return autoFlush; }
            set
            {
                autoFlush = value;
                fileStream.AutoFlush = value;
            }
        }

        public static void StartUp(string folderPath)
        {
            try
            {
                string datetime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                pathBase = Path.Combine(folderPath, "TraceLog_" + datetime);
                path = pathBase + "_" + logNumber + ".txt";
                logFlusher = new System.Timers.Timer(10000);
                logFlusher.AutoReset = true;
                logFlusher.Elapsed += new ElapsedEventHandler(logFlusher_Elapsed);
                logFlusher.Start();
                Start();
                ThreadPool.QueueUserWorkItem(CleanUp);
            }
            catch (Exception ex)
            {
                Write(ex);
            }
        }

        private static void logFlusher_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(Flush);
            }
            catch (Exception ex)
            {
                Write(ex);
            }
        }

        private static void Start()
        {
            fileStream = new StreamWriter(path);
            fileStream.AutoFlush = AutoFlush;
            fileStream.WriteLine("TraceStart");
            fileStream.Flush();
        }

        private static void End()
        {
            fileStream.WriteLine("TraceEnd");
            fileStream.Close();
        }

        private static void Flush(object state)
        {
            try
            {
                fileStream.Flush();
                FileInfo logInfo = new FileInfo(path);
                Write("File Size: " + logInfo.Length + " > " + logRollOverByteSize, TraceLevel.Verbose);
                if (logInfo.Length > logRollOverByteSize)
                {
                    Write("Rolling over...", TraceLevel.Info);
                    RollOver();
                }
            }
            catch (Exception ex)
            {
                Write(ex);
            }
        }

        private static void RollOver()
        {
            try
            {
                logNumber++;
                path = pathBase + "_" + logNumber + ".txt";
                lock (fileStream)
                {
                    End();
                    Start();
                }
                ThreadPool.QueueUserWorkItem(CleanUp);
            }
            catch (Exception ex)
            {
                Write(ex);
            }
        }

        private static void CleanUp(object state)
        {
            try
            {
                Write("Enter", TraceLevel.Verbose);
                string folder = Path.GetDirectoryName(pathBase);
                Write("Cleaning up folder " + folder, TraceLevel.Info);
                DirectoryInfo di = new DirectoryInfo(folder);
                SortedDictionary<DateTime, FileInfo> sortedFiles = new SortedDictionary<DateTime, FileInfo>();
                foreach (FileInfo fi in di.GetFiles("*.txt"))
                {
                    Write("Found file " + fi.Name, TraceLevel.Verbose);
                    sortedFiles.Add(fi.CreationTime, fi);
                }
                if (sortedFiles.Count > 0)
                {
                    FileInfo[] sortedFI = new FileInfo[sortedFiles.Count];
                    sortedFiles.Values.CopyTo(sortedFI, 0);
                    for (int i = sortedFI.Length - 1; i >= 0; i--)
                    {
                        if (i < sortedFI.Length - maxLogCount)
                        {
                            Write("Deleting file " + sortedFI[i].Name, TraceLevel.Info);
                            sortedFI[i].Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Write(ex);
            }
            Write("Exit");
        }

        public static void ShutDown()
        {
            try
            {
                loggingLevel = TraceLevel.Off;
                End();
                logFlusher.Elapsed -= new ElapsedEventHandler(logFlusher_Elapsed);
                logFlusher.Stop();
            }
            catch
            {
                
            }
        }

        public static void Write(Exception ex, TraceLevel level = TraceLevel.Error)
        {
            try
            {
                StringWriter sw = new StringWriter();
                sw.WriteLine("");
                sw.WriteLine("Begin Exception ============================================================");
                sw.WriteLine("Message: " + ex.Message);
                sw.WriteLine("Source: " + ex.Source);
                sw.WriteLine("Stack: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    sw.WriteLine("Inner: " + ex.InnerException.Message);
                    sw.WriteLine("Inner Stack: " + ex.InnerException.StackTrace);
                }
                sw.WriteLine("End Exception ==============================================================");
                Write(sw.ToString(), level);
                //PluginCore.WriteToChat(sw.ToString());
                sw.Close();
            }
            catch
            {
                //PluginCore.WriteToChat(ex2.ToString());
                //Write(ex2.ToString());
            }
        }

        public static void Write(string text, TraceLevel level = TraceLevel.Info)
        {
            try
            {
                if (level == TraceLevel.Off)
                    return;
                if (level > loggingLevel)
                    return;
                StackFrame sf = new StackFrame(1, true);
                string output = "";
                switch (level)
                {
                    case TraceLevel.Fatal:
                        output = "FATAL   ";
                        break;
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
                    case TraceLevel.Noise:
                        output = "NOISE   ";
                        break;
                }
                output += DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:fff") + " ";
                output += "[ " + Path.GetFileName(sf.GetFileName()) + "(" + sf.GetFileLineNumber() + ") " + sf.GetMethod() + " ] ";
                output += text;
                fileStream.WriteLine(output);
                //PluginCore.WriteToChat(output);
            }
            catch
            {
                //Write(ex);
            }
        }

        public static void Write(string format, TraceLevel level, params object[] args)
        {
            Write(String.Format(format, args), level);
        }
    }
}