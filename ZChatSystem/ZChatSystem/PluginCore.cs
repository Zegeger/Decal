using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Interop.Inject;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;
using Zegeger.Decal.Plugins.ZChatSystem.Diagnostics;


namespace Zegeger.Decal.Plugins.ZChatSystem
{
    internal partial class PluginCore : PluginBase
    {
         
        internal static PluginHost MyHost;
        internal static CoreManager MyCore;
        internal static bool initComplete;

        protected override void Startup()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Zegeger\ZChatSystem\";
            TraceLogger.StartUp(appDataPath + @"Logs");
            try
            {
                TraceLogger.Write("Enter");
                initComplete = false;
                MyHost = Host;
                MyCore = Core;
                MainView.ViewInit();
                ZChatSystem.initPerf();
                initEchoFilter();
                initCharStats();
                initChatEvents();
                ZChatSystem.initService();
                ReadSettings();
                Core.PluginInitComplete += new EventHandler<EventArgs>(MyCore_PluginInitComplete);
                ZChatSystem.Running = true;
                TraceLogger.Write( "Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        void MyCore_PluginInitComplete(object sender, EventArgs e)
        {
            TraceLogger.Write("Enter");
            initComplete = true;
            TraceLogger.Write("Exit");
        }

        protected override void Shutdown()
        {
            try
            {
                TraceLogger.Write( "Enter");
                WriteSettings();
                Core.PluginInitComplete -= new EventHandler<EventArgs>(MyCore_PluginInitComplete);
                ZChatSystem.destroyPerf();
                ZChatSystem.destroyService();
                destroyChatEvents();
                destroyCharStats();
                destroyEchoFilter();
                MainView.ViewDestroy();
                MyHost = null;
                MyCore = null;
                TraceLogger.Write( "Exit");
                //TODO: Code for shutdown events
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.ShutDown();
        }

        internal static void WriteSettings()
        {
            try
            {
                TraceLogger.Write("Enter");
                string filename = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Decal Plugins\\Zegeger\\ZChatSystem\\settings.xml";
                TraceLogger.Write("Filename: " + filename);
                TraceLogger.Write("Start writing to temp: " + filename + ".tmp");

                XmlDocument xmlWriter = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlWriter.CreateXmlDeclaration("1.0", "utf-8", null); 
                xmlWriter.AppendChild(xmlWriter.CreateElement("ZChatSystem"));
                xmlWriter.InsertBefore(xmlDeclaration, xmlWriter.DocumentElement);
                
                XmlElement oPlugin;

                XmlElement zo = xmlWriter.CreateElement("Settings");
                xmlWriter.DocumentElement.AppendChild(zo);
                TraceLogger.Write("Writing Settings Logging");
                oPlugin = xmlWriter.CreateElement("Logging");
                oPlugin.SetAttribute("tracingLevel", TraceLogger.LoggingLevel.ToString());
                zo.AppendChild(oPlugin);

                XmlElement oi = xmlWriter.CreateElement("OrderInfo");
                xmlWriter.DocumentElement.AppendChild(oi);

                XmlElement mo = xmlWriter.CreateElement("MessageOrder");
                oi.AppendChild(mo);
                foreach (OrderData messagedata in ZChatSystem.MessageOrder)
                {
                    TraceLogger.Write("Writing Message" + messagedata.plugin);
                    oPlugin = xmlWriter.CreateElement("Plugin");
                    oPlugin.SetAttribute("enabled", messagedata.enabled.ToString());
                    oPlugin.SetAttribute("name", messagedata.plugin);
                    mo.AppendChild(oPlugin);
                }

                XmlElement co = xmlWriter.CreateElement("ColorOrder");
                oi.AppendChild(co);
                foreach (OrderData colordata in ZChatSystem.ColorOrder)
                {
                    TraceLogger.Write("Writing Color" + colordata.plugin);
                    oPlugin = xmlWriter.CreateElement("Plugin");
                    oPlugin.SetAttribute("enabled", colordata.enabled.ToString());
                    oPlugin.SetAttribute("name", colordata.plugin);
                    co.AppendChild(oPlugin);
                }

                XmlElement wo = xmlWriter.CreateElement("WindowOrder");
                oi.AppendChild(wo);
                foreach (OrderData windowdata in ZChatSystem.WindowOrder)
                {
                    TraceLogger.Write("Writing Window" + windowdata.plugin);
                    oPlugin = xmlWriter.CreateElement("Plugin");
                    oPlugin.SetAttribute("enabled", windowdata.enabled.ToString());
                    oPlugin.SetAttribute("name", windowdata.plugin);
                    wo.AppendChild(oPlugin);
                }

                xmlWriter.Save(filename + ".tmp");
                TraceLogger.Write("Finish writing to temp: " + filename + ".tmp");
                TraceLogger.Write("Deleting old file: " + filename);
                System.IO.File.Delete(filename);
                TraceLogger.Write("Renaming \"" + filename + ".tmp" + "\" to \"" + filename + "\"");
                System.IO.File.Move(filename + ".tmp", filename);
                


                /*XmlTextWriter textWriter = new XmlTextWriter(filename + ".tmp", null);
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("ZChatSystem");
                textWriter.WriteStartElement("PluginOrder");
                foreach (KeyValuePair<System.Guid, string> plugin in Plugins)
                {
                    TraceLogger.Write("Writing " + plugin.Value);
                    textWriter.WriteStartElement("Plugin");
                    textWriter.WriteValue(plugin.Value);
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
                textWriter.Close();
                TraceLogger.Write("Finish writing to temp: " + filename + ".tmp");
                TraceLogger.Write("Deleting old file: " + filename);
                System.IO.File.Delete(filename);
                TraceLogger.Write("Renaming \"" + filename + ".tmp" + "\" to \"" + filename + "\"");
                System.IO.File.Move(filename + ".tmp", filename);
                textWriter = null;*/
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal void ReadSettings()
        {
            try
            {
                TraceLogger.Write("Enter");
                string filename = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Decal Plugins\\Zegeger\\ZChatSystem\\settings.xml";
                TraceLogger.Write("Filename: " + filename);
                XmlDocument doc = new XmlDocument();
                TraceLogger.Write("Loading: " + filename);
                doc.Load(filename);
                XmlNode DocNode = doc.DocumentElement;
                XmlNodeList Settings = DocNode.ChildNodes;
                for (int w = 0; w < Settings.Count; w++)
                {
                    if (Settings[w].Name.Equals("Settings"))
                    {
                        TraceLogger.Write("Handling Settings");
                        XmlNodeList SettingsList = Settings[w].ChildNodes;
                        for (int v = 0; v < SettingsList.Count; v++)
                        {
                            TraceLogger.Write("Handling: " + SettingsList[v].Name);
                            switch (SettingsList[v].Name)
                            {
                                case "Logging":
                                    string level = SettingsList[v].Attributes["tracingLevel"].Value;
                                    switch(level)
                                    {
                                        case "Off":
                                            TraceLogger.LoggingLevel = TraceLevel.Off;
                                            break;
                                        case "Noise":
                                            TraceLogger.LoggingLevel = TraceLevel.Noise;
                                            break;
                                        case "Verbose":
                                            TraceLogger.LoggingLevel = TraceLevel.Verbose;
                                            break;
                                        case "Info":
                                            TraceLogger.LoggingLevel = TraceLevel.Info;
                                            break;
                                        case "Warning":
                                            TraceLogger.LoggingLevel = TraceLevel.Warning;
                                            break;
                                        case "Error":
                                            TraceLogger.LoggingLevel = TraceLevel.Error;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }

                    if (Settings[w].Name.Equals("OrderInfo"))
                    {
                        TraceLogger.Write("Handling OrderInfo");
                        XmlNodeList OrderInfo = Settings[w].ChildNodes;
                        for (int x = 0; x < OrderInfo.Count; x++)
                        {
                            TraceLogger.Write("Handling: " + OrderInfo.Item(x).Name);
                            XmlNodeList XmlPlugins = OrderInfo[x].ChildNodes;
                            for (int y = 0; y < XmlPlugins.Count; y++)
                            {
                                TraceLogger.Write("Adding: " + XmlPlugins.Item(y).Attributes["name"].Value);

                                if (!ZChatSystem.PluginsName.ContainsKey(XmlPlugins.Item(y).Attributes["name"].Value))
                                    ZChatSystem.PluginsName.Add(XmlPlugins.Item(y).Attributes["name"].Value, Guid.Empty);

                                OrderData neworder = new OrderData();
                                neworder.id = Guid.Empty;
                                neworder.plugin = XmlPlugins.Item(y).Attributes["name"].Value;
                                neworder.enabled = (XmlPlugins.Item(y).Attributes["enabled"].Value == "True");

                                switch (OrderInfo.Item(x).Name)
                                {
                                    case "MessageOrder":
                                        ZChatSystem.MessageOrder.Add(neworder);
                                        break;
                                    case "ColorOrder":
                                        ZChatSystem.ColorOrder.Add(neworder);
                                        break;
                                    case "WindowOrder":
                                        ZChatSystem.WindowOrder.Add(neworder);
                                        break;
                                }
                            }
                        }
                    }
                }
                doc = null;
                ZChatSystem.updateMessageView();
                ZChatSystem.updateWindowView();
                ZChatSystem.updateColorView();
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }

        }
    }
}