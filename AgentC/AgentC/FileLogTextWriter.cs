using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Plugins.AgentC
{
    class FileLogTextWriter : IFileLogWriter
    {
        private string FilePath;
        private static TextWriter _stream;
        private Timer writeTimer;
        private int writesPending;
        private bool closeRequested;

        public FileLogTextWriter(string path)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            writeTimer = new Timer(Timer_Elapse);
            writeTimer.Change(10000, 10000);
            FilePath = path;
            writesPending = 0;
            closeRequested = false;
            AttachFile(path);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        ~FileLogTextWriter()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            Close();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void Timer_Elapse(object sender)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            _stream.Flush();
            if (closeRequested)
            {
                doClose();
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void AttachFile(string path)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                _stream = TextWriter.Synchronized(new StreamWriter(path, true));
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void WriteLine(int type, string color, int tag, DateTime time, string line)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (!closeRequested)
                {
                    writesPending++;
                    ThreadPool.QueueUserWorkItem(WriteLineThread, line);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void WriteLineThread(object line)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                _stream.WriteLine((string)line);
                writesPending--;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void ChangePath(string path)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            _stream.Close();
            FilePath = path;
            AttachFile(path);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void Close()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            closeRequested = true;
            writeTimer.Change(100, 100);
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void doClose()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (writesPending <= 0)
                {
                    if (_stream != null)
                        _stream.Close();
                    writeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }
}
