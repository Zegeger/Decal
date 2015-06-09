using Decal.Adapter;
using Decal.Adapter.Wrappers;
//using Decal.Interop.Inject;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using Zegeger.Data;
using Zegeger.Decal;
using Zegeger.Diagnostics;
using Zegeger.Decal.Chat;
using Zegeger.Decal.Data;
using Zegeger.Decal.VVS;
using Zegeger.Versioning;

namespace Zegeger.Decal.Plugins.AgentC
{
    public enum PluginState
    {
        Init,
        Starting,
        Running, 
        ShuttingDown,
        ShutDown,
        Unknown
    }

    public partial class PluginCore : PluginBase
    {
        internal static PluginHost MyHost;
        internal static IView View;
        List<Component> ComponentList;
        SettingsComponent settingsComp;
        PluginState State;
        EchoFilter Echo;
        Updater UpdateChecker;

        internal static ChatType NormalType;
        internal static ChatType ErrorType;

        internal const string PluginName = "AgentC";
        internal const string PluginVersion = "1.0.0.0";
        internal const string PluginAuthor = "Zegeger of Harvestgain";

        private int filesUpdated;
        private VersionNumber latestVersion;

        protected override void Startup()
        {
            try
            {
                State = PluginState.Init;
                MyHost = Host;
                ComponentList = new List<Component>();
                
                string dllPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6) + @"\";
                string profilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\Zegeger\AgentC\";
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Zegeger\AgentC\";

                TraceLogger.StartUp(appDataPath + @"Logs");
                TraceLogger.LoggingLevel = TraceLevel.Noise;
                TraceLogger.Write("START");
                TraceLogger.Write("dllPath: " + dllPath, TraceLevel.Verbose);
                TraceLogger.Write("profilePath: " + profilePath, TraceLevel.Verbose);
                TraceLogger.Write("appDataPath: " + appDataPath, TraceLevel.Verbose);

                Constants.StartUp(appDataPath + @"RefData\");
                if (Constants.AutoUpdate)
                {
                    Update(appDataPath);
                }

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Zegeger.Decal.Plugins.AgentC.ViewXML.mainView.xml"))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        View = Zegeger.Decal.VVS.ViewSystemSelector.CreateViewResource(PluginCore.MyHost, result);
                    }
                }

                ZTimer.StartUp(Core);
                DataHandler.StartUp(profilePath + @"Data\");
                SettingsProfileHandler.StartUp(profilePath + @"Settings\");
                Core.PluginInitComplete += new EventHandler<EventArgs>(Core_PluginInitComplete);
                Core.CharacterFilter.LoginComplete += new EventHandler(CharacterFilter_LoginComplete);
                CommandHandler.StartUp(Core, "ac", new string[] { PluginName + " " + PluginVersion, "By " + PluginAuthor }, WriteToChat);

                Echo = new EchoFilter(Core);

                settingsComp = new SettingsComponent(Core, View);
                settingsComp.ComponentStateChange += new ComponentStateChangeEvent(ComponentStateChange);
                ComponentList.Add(settingsComp);
                ChatLogger chatLogger = new ChatLogger(Core, View);
                chatLogger.BasePath = profilePath + @"Chat Logs\";
                chatLogger.ComponentStateChange += new ComponentStateChangeEvent(ComponentStateChange);
                ComponentList.Add(chatLogger);
                ChatColor chatColor = new ChatColor(Core, View, dllPath);
                chatColor.ComponentStateChange += new ComponentStateChangeEvent(ComponentStateChange);
                Echo.KillMessage += chatColor.OnKillMessageEvent;
                ComponentList.Add(chatColor);
                ChatCustom chatCustom = new ChatCustom(Core, View, dllPath);
                chatCustom.ComponentStateChange += new ComponentStateChangeEvent(ComponentStateChange);
                ComponentList.Add(chatCustom);
                ChatFilter chatFilter = new ChatFilter(Core, View);
                chatCustom.ComponentStateChange += new ComponentStateChangeEvent(ComponentStateChange);
                ComponentList.Add(chatFilter);
                Alias alias = new Alias(Core, View);
                alias.ComponentStateChange += new ComponentStateChangeEvent(ComponentStateChange);
                ComponentList.Add(alias);
                ChatOptions chatOptions = new ChatOptions(Core, View);
                chatOptions.ComponentStateChange += new ComponentStateChangeEvent(ComponentStateChange);
                ComponentList.Add(chatOptions);
                State = PluginState.Starting;
                TraceLogger.Write("Startup End");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
                WriteToChat(ex.ToString());
            }
        }

        void Update(string appDataPath)
        {
            TraceLogger.Write("Enter.  Beginning up check to " + Constants.UpdateURL);
            try
            {
                UpdateChecker = new Updater(Constants.UpdateURL);
                UpdateChecker.BeginCheckVersions(PluginName, PluginVersion, ir =>
                {
                    try
                    {
                        TraceLogger.Write("Check Versions Complete");
                        UpdateCheckResults results = UpdateChecker.EndCheckVersions(ir);
                        TraceLogger.Write("Check Versions result: " + results.Status);
                        if (results.Status == ResultStatus.Success)
                        {
                            List<UpdateResult> removalList = new List<UpdateResult>();
                            foreach (UpdateResult result in results.Results)
                            {
                                if (result.action == UpdateAction.Application)
                                {
                                    TraceLogger.Write("Current application version: " + result.newVersion);
                                    latestVersion = new VersionNumber(result.newVersion);
                                    removalList.Add(result);
                                }
                                else
                                {
                                    if (Constants.FileInfo.ContainsKey(result.fileName))
                                    {
                                        if (result.action == UpdateAction.CreateUpdate)
                                        {
                                            if (new VersionNumber(Constants.FileInfo[result.fileName]) >= new VersionNumber(result.newVersion))
                                            {
                                                TraceLogger.Write("Existing file is up-to-date: " + result.fileName);
                                                removalList.Add(result);
                                            }
                                        }
                                        else if (result.action == UpdateAction.Delete)
                                        {
                                            TraceLogger.Write("Existing file should be removed: " + result.fileName);
                                            File.Move(appDataPath + @"RefData\" + result.fileName, appDataPath + @"RefData\" + result.fileName + ".bak");
                                            removalList.Add(result);
                                        }
                                    }
                                }
                            }
                            foreach (UpdateResult file in removalList)
                            {
                                results.Results.Remove(file);
                            }
                            filesUpdated = results.Results.Count;
                            if (results.Results.Count > 0)
                            {
                                TraceLogger.Write("Beginning to fetch files, count: " + results.Results.Count);
                                UpdateChecker.BeginFetchFiles(results.Results, ir2 =>
                                {
                                    try
                                    {
                                        TraceLogger.Write("Fetch Files Complete");
                                        FilesFetchedResults fileResult = UpdateChecker.EndFetchFiles(ir2);
                                        TraceLogger.Write("Fetch Files result: " + fileResult.Status);
                                        if (fileResult.Status == ResultStatus.Success)
                                        {
                                            bool restart = false;
                                            foreach (FetchedFile file in fileResult.Files)
                                            {
                                                if (file.fileName != "Application" && file.file.Length > 0)
                                                {
                                                    TraceLogger.Write("Deleting existing backup file: " + appDataPath + @"RefData\" + file.fileName + ".bak");
                                                    File.Delete(appDataPath + @"RefData\" + file.fileName + ".bak");
                                                    TraceLogger.Write("Backing up existing file: " + appDataPath + @"RefData\" + file.fileName);
                                                    File.Move(appDataPath + @"RefData\" + file.fileName, appDataPath + @"RefData\" + file.fileName + ".bak");
                                                    TraceLogger.Write("Writing new file: " + appDataPath + @"RefData\" + file.fileName);
                                                    using (FileStream fileStream = new FileStream(appDataPath + @"RefData\" + file.fileName, FileMode.Create))
                                                    {
                                                        fileStream.Write(file.file, 0, file.file.Length);
                                                        fileStream.Close();
                                                        TraceLogger.Write("File written");
                                                        restart = true;
                                                    }
                                                }
                                            }
                                            if (restart)
                                            {
                                                TraceLogger.Write("Restarting plugin after updating data files.", TraceLevel.Warning);
                                                Shutdown();
                                                Startup();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        TraceLogger.Write(ex);
                                    }
                                }, null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Write(ex);
                    }
                }, null);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        void ComponentStateChange(object sender, ComponentStateChangeEventArgs e)
        {
            Component component = (Component)sender;
            TraceLogger.Write("Enter Component: " + component.ComponentName + ", State: " + e.NewState.ToString());
            if (State == PluginState.Running && e.NewState != ComponentState.Running && component.Critical == true )
            {
                TraceLogger.Write("Critical component in incorrect state! Shutting down plugin!", TraceLevel.Fatal);
                Termiante("Critical component transitioned to an incorrect state " + component.ComponentName);
            }
            TraceLogger.Write("Exit");
        }

        void  Core_PluginInitComplete(object sender, EventArgs e)
        {
            TraceLogger.Write("Enter");
            try
            {
                ZChatWrapper.Initialize(Core, "AgentC");
                TraceLogger.Write("ChatMode: " + ZChatWrapper.ChatMode.ToString());
                NormalType = ZChatWrapper.CreateChatType(Constants.ChatColors("Purple"), true, false, false, false, false);
                ErrorType = ZChatWrapper.CreateChatType(Constants.ChatColors("DarkRed"), true, false, false, false, false);
                foreach (Component c in ComponentList)
                {
                    c.PostPluginInit();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            TraceLogger.Write("Enter");
            try
            {
                DataHandler.PostLogin(Core.CharacterFilter.Name, Core.CharacterFilter.Server);
                SettingsProfileHandler.PostLogin(settingsComp.SettingId);
                foreach (Component c in ComponentList)
                {
                    c.PostLogin();
                }
                foreach (Component c in ComponentList)
                {
                    if (c.Critical && c.State != ComponentState.Running)
                    {
                        TraceLogger.Write("Critical component in incorrect state! Shutting down plugin!", TraceLevel.Fatal);
                        Termiante("Critical component failed to load properly " + c.ComponentName);
                        return;
                    }
                }
                State = PluginState.Running;
                WriteToChat("Plugin Loaded! Type /ac help for commands.");
                if (latestVersion != null)
                {
                    if (new VersionNumber(PluginVersion) < latestVersion)
                    {
                        WriteToChat("There is a newer version (" + latestVersion.ToString() + ") available. Please download it from " + Constants.DownloadURL);
                    }
                }
                if (filesUpdated > 0)
                {
                    WriteToChat(filesUpdated.ToString() + " files updated.");
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        private void Termiante(string error = "")
        {
            WriteToChatError("An error occured: " + error + ". Terminating plugin operation...");
            Shutdown();
        }

        protected override void Shutdown()
        {
            TraceLogger.Write("Shutdown Start");
            try
            {
                if (State != PluginState.ShutDown && State != PluginState.ShuttingDown)
                {
                    State = PluginState.ShuttingDown;
                    foreach (Component c in ComponentList)
                    {
                        c.Shutdown();
                    }
                    ComponentList.Clear();
                    SettingsProfileHandler.Shutdown();
                    DataHandler.Shutdown();
                    CommandHandler.Shutdown();
                    Core.PluginInitComplete -= new EventHandler<EventArgs>(Core_PluginInitComplete);
                    Core.CharacterFilter.LoginComplete -= new EventHandler(CharacterFilter_LoginComplete);
                    ZChatWrapper.Dispose();
                    Constants.Shutdown();
                    MyHost = null;
                    TraceLogger.Write("Shutdown End");
                    TraceLogger.ShutDown();
                    State = PluginState.ShutDown;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void WriteToChat(string message)
        {
            TraceLogger.Write("Enter message: " + message, TraceLevel.Verbose);
            try
            {
                ZChatWrapper.WriteToChat("<AgentC> " + message, NormalType);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal static void WriteToChatError(string message)
        {
            TraceLogger.Write("Enter message: " + message, TraceLevel.Error);
            try
            {
                ZChatWrapper.WriteToChat("<AgentC> " + message, ErrorType);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }
}