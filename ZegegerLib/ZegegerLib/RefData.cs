using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using Zegeger.Diagnostics;

namespace Zegeger.Data
{
    public delegate void DataReferenceLoadedEvent(DataReferenceLoadedEventArgs e);

    public class DataReferenceLoadedEventArgs : EventArgs
    {

    }

    public class DataReference
    {
        private string folderPath;
        private Dictionary<string, Table> tables;
        private FileSystemWatcher watcher;
        private Dictionary<string, string> xmlFiles;

        public event DataReferenceLoadedEvent DataReferenceLoaded;

        public DataReference(string path)
        {
            TraceLogger.Write("Enter with path " + path, TraceLevel.Verbose);
            try
            {
                watcher = new FileSystemWatcher(path, "*.xml");
                watcher.EnableRaisingEvents = true;
                watcher.Created += watcher_Changed;
                watcher.Deleted += watcher_Changed;
                tables = new Dictionary<string, Table>();
                xmlFiles = new Dictionary<string, string>();
                folderPath = path;
                Load();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                Load();
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public bool TableExists(string table)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (tables.ContainsKey(table))
                {
                    TraceLogger.Write("Exit true", TraceLevel.Verbose);
                    return true;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit false", TraceLevel.Verbose);
            return false;
        }

        public Table GetTable(string table)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (tables.ContainsKey(table))
                {
                    TraceLogger.Write("Exit", TraceLevel.Verbose);
                    return tables[table];
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
            return null;
        }

        public Table this[string table]
        {
            get
            {
                return GetTable(table);
            }
        }

        public Dictionary<string, string> FileInfo
        {
            get
            {
                return new Dictionary<string, string>(xmlFiles);
            }
        }

        private void Load()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                tables.Clear();
                xmlFiles.Clear();
                DirectoryInfo DI = new DirectoryInfo(folderPath);
                FileInfo[] files = DI.GetFiles("*.xml");
                foreach (FileInfo file in files)
                {
                    TraceLogger.Write("Importing reference file " + file.FullName);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file.FullName);
                    XmlElement root = doc.DocumentElement;
                    XmlNode refs = root.FirstChild;
                    string version = refs.Attributes["version"].Value;
                    xmlFiles.Add(file.Name, version);
                    foreach (XmlNode reference in refs.ChildNodes)
                    {
                        if (reference.Name == "Reference")
                        {
                            if (!tables.ContainsKey(reference.Attributes["name"].Value))
                            {
                                Table tmp = new Table();
                                tmp.tableName = reference.Attributes["name"].Value;
                                TraceLogger.Write("Reference Node " + tmp.TableName);
                                foreach (XmlNode value in reference.ChildNodes)
                                {
                                    if (value.Name == "Value")
                                    {
                                        if (!tmp.values.ContainsKey(value.Attributes["name"].Value))
                                        {
                                            Value val = new Value();
                                            val.name = value.Attributes["name"].Value;
                                            val.data = value.InnerXml;
                                            if (value.Attributes["type"] != null)
                                            {
                                                val.type = value.Attributes["type"].Value;
                                            }
                                            if (value.Attributes["ref"] != null)
                                            {
                                                val.reference = value.Attributes["ref"].Value;
                                            }
                                            if (value.Attributes["array"] != null)
                                            {
                                                val.isArray = (value.Attributes["array"].Value.ToLower() == "true");
                                            }
                                            TraceLogger.Write("Value Node: " + val.Name + ", Type: " + val.Type + ", Value: " + val.Data + ", Ref: " + val.Reference + ", Array: " + val.IsArray, TraceLevel.Verbose);
                                            tmp.values.Add(val.Name, val);
                                        }
                                        else
                                        {
                                            TraceLogger.Write("Value already exists " + value.Attributes["name"].Value, TraceLevel.Warning);
                                        }
                                    }
                                }
                                tables.Add(tmp.TableName, tmp);
                            }
                            else
                            {
                                TraceLogger.Write("Reference already exists " + reference.Attributes["name"].Value, TraceLevel.Warning);
                            }
                        }
                    }
                }
                WalkRefs();
                WalkEmbeddedRefs();
                if (DataReferenceLoaded != null)
                    DataReferenceLoaded(new DataReferenceLoadedEventArgs());
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void WalkRefs()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                int changeCount;
                do
                {
                    changeCount = 0;
                    foreach (Table r in tables.Values)
                    {
                        foreach (Value v in r.values.Values)
                        {
                            if (!String.IsNullOrEmpty(v.Reference))
                            {
                                if (tables.ContainsKey(v.Reference))
                                {
                                    if (tables[v.Reference].values.ContainsKey(v.Data))
                                    {
                                        Value newVal = tables[v.Reference][v.Data];
                                        TraceLogger.Write("Ref copied to " + v.Name + ", old val: " + v.Data + ", old ref: " + v.Reference + ", new val: " + newVal.Data + ", new ref: " + newVal.Reference, TraceLevel.Verbose);
                                        v.data = newVal.Data;
                                        v.reference = newVal.Reference;
                                        changeCount++;
                                    }
                                    else
                                    {
                                        TraceLogger.Write("Invalid ref value: " + v.Data + ", to ref: " + v.Reference + ", in node: " + v.Name, TraceLevel.Warning);
                                        tables[v.Reference].values.Remove(v.Name);
                                    }
                                }
                                else
                                {
                                    TraceLogger.Write("Invalid ref target: " + v.Reference + ", with value: " + v.Data + ", in node: " + v.Name, TraceLevel.Warning);
                                    tables[v.Reference].values.Remove(v.Name);
                                }
                            }
                        }
                    }
                    TraceLogger.Write("WalkRef ChangeCount: " + changeCount);
                } while (changeCount > 0);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void WalkEmbeddedRefs()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                foreach (Table r in tables.Values)
                {
                    foreach (Value v in r.values.Values)
                    {
                        string result = "";
                        string data = v.Data;
                        int pointer = 0;
                        int start = data.IndexOf("%%", pointer);
                        bool match = false;
                        while (start != -1)
                        {
                            TraceLogger.Write("Start " + start, TraceLevel.Verbose);
                            result += data.Substring(pointer, start - pointer);
                            TraceLogger.Write("Adding string " + data.Substring(pointer, start - pointer), TraceLevel.Verbose);
                            int end = data.IndexOf("%%", start + 2);
                            TraceLogger.Write("End " + end, TraceLevel.Verbose);
                            if (end != -1)
                            {
                                string variable = data.Substring(start, end - start + 2);
                                TraceLogger.Write("Variable " + variable, TraceLevel.Verbose);
                                string[] splitVar = variable.Trim('%').Split('%');
                                if (splitVar.Length == 2)
                                {
                                    string reference = splitVar[0];
                                    string value = splitVar[1];
                                    TraceLogger.Write("Reference " + reference + " Value " + value, TraceLevel.Verbose);
                                    if (tables.ContainsKey(reference) && tables[reference].values.ContainsKey(value))
                                    {
                                        match = true;
                                        result += tables[reference][value].Data;
                                        TraceLogger.Write("Replacing with data " + tables[reference][value].Data, TraceLevel.Verbose);
                                    }
                                    else
                                    {
                                        result += variable;
                                        TraceLogger.Write("No match in tables", TraceLevel.Verbose);
                                    }
                                    pointer = end + 2;
                                    TraceLogger.Write("Pointer " + pointer, TraceLevel.Verbose);
                                }
                                else
                                {
                                    result += variable;
                                    TraceLogger.Write("Incorrect split", TraceLevel.Verbose);
                                    pointer = end + 2;
                                }
                            }
                            else
                            {
                                break;
                            }
                            start = data.IndexOf("%%", pointer);
                        }
                        if (match)
                        {
                            result += data.Substring(pointer);
                            TraceLogger.Write("Adding tailing string " + data.Substring(pointer), TraceLevel.Verbose);
                            v.data = result;
                            TraceLogger.Write("Start data " + data, TraceLevel.Verbose);
                            TraceLogger.Write("End result " + result, TraceLevel.Verbose);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    public class Table
    {
        internal string tableName;
        public string TableName
        {
            get { return tableName; }
        }

        internal Dictionary<string, Value> values = new Dictionary<string, Value>();
        public Dictionary<string, Value> Values
        {
            get { return new Dictionary<string,Value>(values); }
        }

        public Value this[string key]
        {
            get
            {
                if (values.ContainsKey(key))
                {
                    return values[key];
                }
                return null;
            }
        }
    }

    public class Value
    {
        internal string name;
        public string Name
        {
            get { return name; }
        }

        internal string type;
        public string Type
        {
            get { return type; }
        }

        internal string data;
        public string Data
        {
            get { return this.data; }
        }

        internal string reference;
        public string Reference
        {
            get { return reference; }
        }

        internal bool isArray;
        public bool IsArray
        {
            get { return isArray; }
        }

        public override string ToString()
        {
            return Data;
        }

        public List<string> ToStringList()
        {
            List<string> tmp = new List<string>();
            string[] elements = data.Split('|');
            foreach (string e in elements)
            {
                tmp.Add(e);
            }
            return tmp;
        }

        public byte[] ToByte()
        {
            return System.Text.Encoding.UTF8.GetBytes(data);
        }

        public Bitmap ToBitmap()
        {
            MemoryStream m = new MemoryStream(System.Convert.FromBase64String(data));
            return new Bitmap(m);
        }

        public short ToShort()
        {
            return Int16.Parse(Data);
        }

        public List<short> ToShortList()
        {
            List<short> tmp = new List<short>();
            string[] elements = data.Split('|');
            foreach (string e in elements)
            {
                tmp.Add(Int16.Parse(e));
            }
            return tmp;
        }

        public int ToInt()
        {
            return Int32.Parse(Data);
        }

        public List<int> ToIntList()
        {
            List<int> tmp = new List<int>();
            string[] elements = data.Split('|');
            foreach (string e in elements)
            {
                tmp.Add(Int32.Parse(e));
            }
            return tmp;
        }

        public long ToLong()
        {
            return Int64.Parse(Data);
        }

        public List<long> ToLongList()
        {
            List<long> tmp = new List<long>();
            string[] elements = data.Split('|');
            foreach (string e in elements)
            {
                tmp.Add(Int64.Parse(e));
            }
            return tmp;
        }

        public ushort ToUShort()
        {
            return UInt16.Parse(Data);
        }

        public List<ushort> ToUShortList()
        {
            List<ushort> tmp = new List<ushort>();
            string[] elements = data.Split('|');
            foreach (string e in elements)
            {
                tmp.Add(UInt16.Parse(e));
            }
            return tmp;
        }

        public uint ToUInt()
        {
            return UInt32.Parse(Data);
        }

        public List<uint> ToUIntList()
        {
            List<uint> tmp = new List<uint>();
            string[] elements = data.Split('|');
            foreach (string e in elements)
            {
                tmp.Add(UInt32.Parse(e));
            }
            return tmp;
        }

        public ulong ToULong()
        {
            return UInt64.Parse(Data);
        }

        public List<ulong> ToULongList()
        {
            List<ulong> tmp = new List<ulong>();
            string[] elements = data.Split('|');
            foreach (string e in elements)
            {
                tmp.Add(UInt64.Parse(e));
            }
            return tmp;
        }

        public bool ToBool()
        {
            return Boolean.Parse(Data);
        }

        public List<bool> ToBoolList()
        {
            List<bool> tmp = new List<bool>();
            string[] elements = data.Split('|');
            foreach (string e in elements)
            {
                tmp.Add(Boolean.Parse(e));
            }
            return tmp;
        }

        public double ToDouble()
        {
            return Double.Parse(Data);
        }

        public List<double> ToDoubleList()
        {
            List<double> tmp = new List<double>();
            string[] elements = data.Split('|');
            foreach (string e in elements)
            {
                tmp.Add(Double.Parse(e));
            }
            return tmp;
        }
    }


}
