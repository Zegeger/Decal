using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Data
{
    public delegate void SettingsLoadedEvent(SettingsLoadedEventArgs e);
    public delegate void SettingActivatedEvent(SettingActivatedEventArgs e);

    public class SettingsLoadedEventArgs : EventArgs
    {
        public List<SettingsProfile> Profiles;

        internal SettingsLoadedEventArgs(List<SettingsProfile> settings)
        {
            Profiles = settings;
        }
    }

    public class SettingActivatedEventArgs : EventArgs
    {
        public SettingsProfile newProfile;

        internal SettingActivatedEventArgs(SettingsProfile newProf)
        {
            newProfile = newProf;
        }
    }

    public static class SettingsProfileHandler
    {
        public static Guid DefaultGuid = new Guid("{DEEB016B-47A1-4520-98C9-BBABD3FEDE04}");
        private static Guid NullGuid = new Guid("{20CDA0A3-A94E-4AAB-AD5D-F771A2B179FB}");
        public static event SettingsLoadedEvent SettingsLoaded;
        public static event SettingActivatedEvent SettingActivated;

        private static string folderPath;
        private static SettingsProfile activeSetting = new SettingsProfile("Null", NullGuid);
        private static Dictionary<Guid, SettingsProfile> settings;
        private static Guid requestedSetting = DefaultGuid;
        private static List<Type> typeList;

        public static List<SettingsProfile> SettingsList
        {
            get
            {
                List<SettingsProfile> tmp = new List<SettingsProfile>();
                foreach (SettingsProfile sp in settings.Values)
                    tmp.Add(sp);
                return tmp;
            }
        }

        public static void StartUp(string FolderPath)
        {
            folderPath = FolderPath;
            settings = new Dictionary<Guid, SettingsProfile>();
            typeList = new List<Type>();
        }

        public static void PostLogin(Guid requestedId)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                TraceLogger.Write("ActiveSetting Name: " + activeSetting.profileName);
                if (requestedId != null)
                {
                    TraceLogger.Write("Requested Setting Guid " + requestedId.ToString());
                    requestedSetting = requestedId;
                }
                Load();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static void Shutdown()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                SaveThread(activeSetting);
                settings.Clear();
                typeList.Clear();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static bool NewProfile(string name)
        {
            try
            {
                TraceLogger.Write("Enter, name " + name);
                if (!NameExists(name))
                {
                    Guid newGuid = Guid.NewGuid();
                    SettingsProfile newProfile = new SettingsProfile(name, newGuid);
                    newProfile.settingGroups = activeSetting.settingGroups;
                    settings.Add(newGuid, newProfile);
                    Save(newProfile);
                    requestedSetting = newGuid;
                    Load();
                    TraceLogger.Write("Exit true");
                    return true;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit false");
            return false;
        }

        public static bool RenameProfile(string profile, string name)
        {
            try
            {
                TraceLogger.Write("Enter, Profile " + profile + ", name " + name);
                if (!NameExists(name))
                {
                    foreach (SettingsProfile sp in settings.Values)
                    {
                        if (sp.profileName == profile)
                        {
                            File.Delete(folderPath + profile + ".xml");
                            sp.profileName = name;
                            Save(sp);
                            Load();
                            TraceLogger.Write("Exit true");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit false");
            return false;
        }

        public static bool DeleteProfile(string profile)
        {
            try
            {
                TraceLogger.Write("Enter, Profile " + profile);
                foreach (SettingsProfile sp in settings.Values)
                {
                    if (sp.profileName == profile)
                    {
                        File.Delete(folderPath + profile + ".xml");
                        settings.Remove(sp.profileId);
                        Load();
                        TraceLogger.Write("Exit true");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit false");
            return false;
        }

        private static bool NameExists(string name)
        {
            try
            {
                TraceLogger.Write("Enter Name " + name);
                foreach (SettingsProfile sp in settings.Values)
                {
                    if (sp.profileName == name)
                    {
                        TraceLogger.Write("Exit true");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit false");
            return false;
        }

        public static void registerType(Type type)
        {
            try
            {
                TraceLogger.Write("Enter, Type: " + type.ToString());
                typeList.Add(type);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        public static void requestSettingProfile(string name)
        {
            try
            {
                TraceLogger.Write("Enter - name: " + name);
                foreach (SettingsProfile sp in settings.Values)
                {
                    if (sp.profileName == name)
                    {
                        TraceLogger.Write("Setting name valid");
                        requestedSetting = sp.profileId;
                        EnableSetting();
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        public static void requestSettingProfile(Guid id)
        {
            try
            {
                TraceLogger.Write("Enter id: " + id);
                if (settings.ContainsKey(id))
                {
                    TraceLogger.Write("Setting id valid");
                    requestedSetting = id;
                    EnableSetting();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        private static void EnableSetting()
        {
            try
            {
                TraceLogger.Write("Enter");
                TraceLogger.Write("RequestedSetting: " + requestedSetting.ToString());
                TraceLogger.Write("ActiveSetting: " + activeSetting.profileId.ToString());
                if (requestedSetting != activeSetting.profileId)
                {
                    if (settings.ContainsKey(requestedSetting))
                    {
                        TraceLogger.Write("Activating Requested Setting");
                        activeSetting = settings[requestedSetting];
                    }
                    else
                    {
                        if (settings.ContainsKey(DefaultGuid))
                        {
                            TraceLogger.Write("Activating Default Setting");
                            activeSetting = settings[DefaultGuid];
                        }
                        else
                        {
                            TraceLogger.Write("Creating new Default Setting and activating");
                            SettingsProfile newDefault = new SettingsProfile("Default", DefaultGuid);
                            settings.Add(DefaultGuid, newDefault);
                            activeSetting = newDefault;
                            requestedSetting = DefaultGuid;
                            TraceLogger.Write("Raising SettingsLoadedEvent");
                            SettingsLoaded(new SettingsLoadedEventArgs(SettingsList));
                        }
                    }
                    TraceLogger.Write("Raising SettingsActivatedEvent");
                    SettingActivated(new SettingActivatedEventArgs(activeSetting));
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        public static void Save()
        {
            Save(activeSetting);
        }

        public static void Save(SettingsProfile profile)
        {
            try
            {
                TraceLogger.Write("Enter");
                ThreadPool.QueueUserWorkItem(SaveThread, profile);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        private static void SaveThread(object state)
        {
            try
            {
                TraceLogger.Write("Enter");
                SettingsProfile profile = state as SettingsProfile;
                if (profile.profileId != NullGuid)
                {
                    lock (profile)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(SettingsProfile), typeList.ToArray());
                        TextWriter textWriter = new StreamWriter(folderPath + profile.profileName + ".xml");
                        TraceLogger.Write("Saved " + folderPath + profile.profileName + ".xml");
                        serializer.Serialize(textWriter, profile);
                        textWriter.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        private static void Load()
        {
            try
            {
                TraceLogger.Write("Enter");
                settings.Clear();
                activeSetting = new SettingsProfile("Null", NullGuid);
                XmlSerializer serializer = new XmlSerializer(typeof(SettingsProfile), typeList.ToArray());
                DirectoryInfo DI = new DirectoryInfo(folderPath);
                FileInfo[] files = DI.GetFiles("*.xml");
                foreach (FileInfo file in files)
                {
                    TextReader reader = null;
                    XmlReader xmlReader = null;
                    try
                    {
                        reader = new StreamReader(file.FullName);
                        xmlReader = XmlReader.Create(reader);
                        if (serializer.CanDeserialize(xmlReader))
                        {
                            SettingsProfile tmp = (SettingsProfile)serializer.Deserialize(xmlReader);
                            settings.Add(tmp.profileId, tmp);
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Write(ex, TraceLevel.Warning);
                    }
                    xmlReader.Close();
                    reader.Close();
                }
                TraceLogger.Write("Raising SettingsLoadedEvent");
                if(SettingsLoaded != null)
                    SettingsLoaded(new SettingsLoadedEventArgs(SettingsList));
                EnableSetting();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        public static void AddSettingGroup(SettingGroup group)
        {
            try
            {
                TraceLogger.Write("Enter adding group " + group.groupName);
                if (!activeSetting.settingGroups.Contains(group))
                {
                    TraceLogger.Write("Group does not exist, adding");
                    activeSetting.settingGroups.Add(group);
                    Save();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        public static SettingGroup GetSettingGroup(string name)
        {
            try
            {
                TraceLogger.Write("Enter getting group " + name);
                foreach (SettingGroup sg in activeSetting.settingGroups)
                {
                    if (sg.groupName == name)
                    {
                        TraceLogger.Write("Exit return group: " + sg.groupName);
                        return sg;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit return null");
            return null;
        }
    }

    [XmlRoot("SettingsProfile")]
    public class SettingsProfile
    {
        public string profileName;
        public Guid profileId;

        [XmlArray("SettingGroups"), XmlArrayItem("SettingGroup", typeof(SettingGroup))]
        public List<SettingGroup> settingGroups;

        public SettingsProfile()
        {
            TraceLogger.Write("Enter");
            settingGroups = new List<SettingGroup>();
            profileId = Guid.NewGuid();
            profileName = "Default";
            TraceLogger.Write("Exit");
        }

        public SettingsProfile(string name) : this()
        {
            TraceLogger.Write("Enter Name:" + name);
            profileName = name;
            TraceLogger.Write("Exit");
        }

        public SettingsProfile(string name, Guid id) : this(name)
        {
            TraceLogger.Write("Enter Name:" + name + ", Id:" + id.ToString());
            profileId = id;
            TraceLogger.Write("Exit");
        }
    }

    public class SettingGroup
    {
        public string groupName { get; set; }

        public SettingGroup()
        {
            groupName = "";
        }
    }
}
