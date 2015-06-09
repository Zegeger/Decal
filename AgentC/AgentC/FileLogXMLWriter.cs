using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Plugins.AgentC
{
    class FileLogXMLWriter : IFileLogWriter
    {
        private string FilePath;
        private XmlDocument Document;
        private Timer writeTimer;
        private XmlNode root;

        public FileLogXMLWriter(string path)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            writeTimer = new Timer(Timer_Elapse);
            writeTimer.Change(10000, 10000);
            FilePath = path;
            Open();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        ~FileLogXMLWriter()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            Close();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void Timer_Elapse(object sender)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            Write();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void ChangePath(string path)
        {
            TraceLogger.Write("Enter path: " + path, TraceLevel.Verbose);
            FilePath = path;
            Open();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void WriteLine(int type, string color, int tag, DateTime time, string message)
        {
            TraceLogger.Write("Enter message " + message, TraceLevel.Verbose);
            try
            {
                XmlElement newNode = Document.CreateElement("Message");
                XmlAttribute typeNode = Document.CreateAttribute("type");
                typeNode.Value = type.ToString();
                newNode.Attributes.Append(typeNode);
                XmlAttribute colorNode = Document.CreateAttribute("color");
                colorNode.Value = color;
                newNode.Attributes.Append(colorNode);
                XmlAttribute tagNode = Document.CreateAttribute("tag");
                tagNode.Value = type.ToString();
                newNode.Attributes.Append(tagNode);
                XmlAttribute timeNode = Document.CreateAttribute("date");
                timeNode.Value = time.ToString();
                newNode.Attributes.Append(timeNode);
                newNode.InnerText = message;
                root.AppendChild(newNode);
                root.AppendChild(Document.CreateTextNode(Environment.NewLine));
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void Open()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            try
            {
                Document = new XmlDocument();
                if (File.Exists(FilePath))
                {
                    try
                    {
                        TraceLogger.Write("Loading log file " + FilePath, TraceLevel.Info);
                        Document.Load(FilePath);
                        XmlNodeList list = Document.GetElementsByTagName("Log");
                        if (list.Count > 0)
                        {
                            root = list[0];
                        }
                        else
                        {
                            CreateNew();
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Write(ex.ToString(), TraceLevel.Info);
                        CreateNew();
                    }
                }
                else
                {
                    CreateNew();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void CreateNew()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            try
            {
                TraceLogger.Write("Creating new Log file", TraceLevel.Info);
                XmlProcessingInstruction xpi = Document.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"style/agentzlog.xsl\"");
                Document.AppendChild(xpi);
                root = Document.CreateElement("Log");
                Document.AppendChild(root);
                Write();
            }
            catch(Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void Write()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            try
            {
                if (Document != null)
                {
                    TraceLogger.Write("Writing file " + FilePath, TraceLevel.Info);
                    XmlWriter writer = XmlWriter.Create(FilePath);
                    Document.Save(writer);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void Close()
        {
            Write();
            writeTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
