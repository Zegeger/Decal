using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Threading;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Data
{
    public delegate void DataLoadedEvent(DataLoadedEventArgs e);

    public class DataLoadedEventArgs : EventArgs
    {

    }

    public static class DataHandler
    {
        public static event DataLoadedEvent DataLoaded;
        private static string folderPath;
        private static string charName;
        private static string serverName;
        private static CharData data;
        private static List<Type> typeList;
        private static bool saving;

        public static void StartUp(string FolderPath)
        {
            folderPath = FolderPath;
            saving = false;
            data = new CharData();
            typeList = new List<Type>();
        }

        public static void PostLogin(string charname, string servername)
        {
            try
            {
                TraceLogger.Write("Enter");
                charName = charname;
                serverName = servername;
                Load();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        public static void Shutdown()
        {
            try
            {
                TraceLogger.Write("Enter");
                SaveThread(null);
                typeList.Clear();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
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

        public static void AddDataStore(DataStore store)
        {
            try
            {
                TraceLogger.Write("Enter");
                if (!data.dataStores.Contains(store))
                {
                    TraceLogger.Write("Adding " + store.dataStoreName);
                    data.dataStores.Add(store);
                    Save();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }

        public static DataStore GetDataStore(string name)
        {
            try
            {
                TraceLogger.Write("Enter");
                foreach (DataStore ds in data.dataStores)
                {
                    if (ds.dataStoreName == name)
                    {
                        TraceLogger.Write("Exit returning datastore: " + ds.dataStoreName);
                        return ds;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit returning null");
            return null;
        }

        public static void Save()
        {
            try
            {
                if (!saving)
                {
                    saving = true;
                    ThreadPool.QueueUserWorkItem(SaveThread);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        private static void SaveThread(object state)
        {
            try
            {
                TraceLogger.Write("Enter");
                XmlSerializer serializer = new XmlSerializer(typeof(CharData), typeList.ToArray());
                TextWriter textWriter = new StreamWriter(folderPath + serverName + "_" + charName + ".xml");
                serializer.Serialize(textWriter, data);
                textWriter.Close();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            saving = false;
            TraceLogger.Write("Exit");
        }

        private static void Load()
        {
            try
            {
                TraceLogger.Write("Enter");
                if (File.Exists(folderPath + serverName + "_" + charName + ".xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CharData), typeList.ToArray());
                    TraceLogger.Write("Reading " + folderPath + serverName + "_" + charName + ".xml");
                    TextReader reader = null;
                    XmlReader xmlReader = null;
                    try
                    {
                        reader = new StreamReader(folderPath + serverName + "_" + charName + ".xml");
                        xmlReader = XmlReader.Create(reader);
                        if (serializer.CanDeserialize(xmlReader))
                        {
                            TraceLogger.Write("Deserializing");
                            data = (CharData)serializer.Deserialize(xmlReader);
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Write(ex, TraceLevel.Warning);
                    }
                    xmlReader.Close();
                    reader.Close();
                }
                if(DataLoaded != null)
                    DataLoaded(new DataLoadedEventArgs());
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit");
        }
    }

    [Serializable, XmlRoot("CharData")]
    public class CharData
    {
        [XmlArray("DataEntries"), XmlArrayItem("DataEntry", typeof(DataStore))]
        public List<DataStore> dataStores;

        public CharData()
        {
            dataStores = new List<DataStore>();
        }
    }

    [Serializable]
    public class DataStore
    {
        public string dataStoreName;
    }
}
