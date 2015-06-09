using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using Zegeger.Data;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Plugins.AgentC
{
    internal static class Constants
    {
        private static DataReference RefData;
        public static string UpdateURL = "http://www.zegeger.net/lib/decalUpdater.php";
        public static bool AutoUpdate = true;
        public static string DownloadURL = "http://www.zegeger.net/ac/decal/agentz/";

        internal static void StartUp(string path)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                RefData = new DataReference(path);
                
                UpdateURL = RefData["Updater"].Values["URL"].ToString();
                AutoUpdate = RefData["Updater"].Values["AutoUpdate"].ToBool();
                DownloadURL = RefData["Updater"].Values["DownloadURL"].ToString();

                foreach (KeyValuePair<string, Value> v in RefData["ChatColors"].Values)
                {
                    pChatColors.Add(v.Key, v.Value.ToInt());
                }
                foreach (KeyValuePair<string, Value> v in RefData["ChatColorsHex"].Values)
                {
                    pChatColorsHex.Add(v.Key, v.Value.ToString());
                }
                foreach (KeyValuePair<string, Value> v in RefData["ChatColorsToHex"].Values)
                {
                    pChatColorsToHex.Add(v.Key, v.Value.ToString());
                }
                foreach (KeyValuePair<string, Value> v in RefData["ChatClasses"].Values)
                {
                    pChatClasses.Add(v.Key, v.Value.ToInt());
                }
                foreach (KeyValuePair<string, Value> v in RefData["ChatWindows"].Values)
                {
                    pChatWindows.Add(v.Key, v.Value.ToString());
                }
                foreach (KeyValuePair<string, Value> v in RefData["LogTypes"].Values)
                {
                    pLogTypes.Add(v.Key, v.Value.ToInt());
                    pLogTypesInverse.Add(v.Value.ToInt(), v.Key);
                }

                foreach (KeyValuePair<string, Value> v in RefData["LogClassRules"].Values)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(v.Value.ToString());
                    pLogClassRules.Add(v.Key, doc);
                }
                foreach (KeyValuePair<string, Value> v in RefData["ChatColorGeneral"].Values)
                {
                    pChatColorGeneral.Add(v.Key, v.Value.ToString());
                }
                foreach (KeyValuePair<string, Value> v in RefData["ChatColorRules"].Values)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(v.Value.ToString());
                    pChatColorRules.Add(v.Key, doc);
                }
                foreach (KeyValuePair<string, Value> v in RefData["ChatColorIcons"].Values)
                {
                    pChatColorIcons.Add(v.Key, v.Value.ToBitmap());
                }
                foreach (KeyValuePair<string, Value> v in RefData["GUIColors"].Values)
                {
                    int num = Int32.Parse("FF" + v.Value.ToString(), System.Globalization.NumberStyles.HexNumber);
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(num);
                    pGUIColors.Add(v.Key, color);
                }
                foreach (KeyValuePair<string, Value> v in RefData["ChatOptions"].Values)
                {
                    pChatOptions.Add(v.Key, v.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        public static void Shutdown()
        {
            pChatClasses.Clear();
            pChatColorGeneral.Clear();
            pChatColorIcons.Clear();
            pChatColorRules.Clear();
            pChatColors.Clear();
            pChatColorsHex.Clear();
            pChatColorsToHex.Clear();
            pChatWindows.Clear();
            pGUIColors.Clear();
            pLogClassRules.Clear();
            pLogTypes.Clear();
            pLogTypesInverse.Clear();
            pChatOptions.Clear();
        }

        public static Dictionary<string, string> FileInfo
        {
            get
            {
                return RefData.FileInfo;
            }
        }

        private static Dictionary<string, int> pChatColors = new Dictionary<string, int>();
        internal static int ChatColors(string key)
        {
            if (pChatColors.ContainsKey(key))
            {
                return pChatColors[key];
            }
            return -1;
        }
        internal static Dictionary<string,int> ChatColorsDictionary
        {
            get
            {
                return pChatColors;
            }
        }

        private static Dictionary<string, string> pChatColorsHex = new Dictionary<string, string>();
        internal static string ChatColorsHex(string key)
        {
            if (pChatColorsHex.ContainsKey(key))
            {
                return pChatColorsHex[key];
            }
            return null;
        }
        internal static Dictionary<string, string> ChatColorsHexDictionary
        {
            get
            {
                return pChatColorsHex;
            }
        }

        private static Dictionary<string, string> pChatColorsToHex = new Dictionary<string, string>();
        internal static string ChatColorsToHex(string key)
        {
            if (pChatColorsToHex.ContainsKey(key))
            {
                return pChatColorsToHex[key];
            }
            return null;
        }
        internal static Dictionary<string, string> ChatColorsToHexDictionary
        {
            get
            {
                return pChatColorsToHex;
            }
        }

        private static Dictionary<string, int> pChatClasses = new Dictionary<string, int>();
        internal static int ChatClasses(string key)
        {
            if (pChatClasses.ContainsKey(key))
            {
                return pChatClasses[key];
            }
            return -1;
        }
        internal static Dictionary<string, int> ChatClassesDictionary
        {
            get
            {
                return pChatClasses;
            }
        }

        private static Dictionary<string, string> pChatWindows = new Dictionary<string, string>();
        internal static string ChatWindows(string key)
        {
            if (pChatWindows.ContainsKey(key))
            {
                return pChatWindows[key];
            }
            return null;
        }
        internal static Dictionary<string, string> ChatWindowsDictionary
        {
            get
            {
                return pChatWindows;
            }
        }

        private static Dictionary<string, int> pLogTypes = new Dictionary<string, int>();
        private static Dictionary<int, string> pLogTypesInverse = new Dictionary<int, string>();
        internal static int LogTypes(string key)
        {
            if (pLogTypes.ContainsKey(key))
            {
                return pLogTypes[key];
            }
            return -1;
        }
        internal static string LogTypesInverse(int key)
        {
            if (pLogTypesInverse.ContainsKey(key))
            {
                return pLogTypesInverse[key];
            }
            return null;
        }
        internal static Dictionary<string, int> LogTypesDictionary
        {
            get
            {
                return pLogTypes;
            }
        }
        internal static Dictionary<int, string> LogTypesInverseDictionary
        {
            get
            {
                return pLogTypesInverse;
            }
        }

        private static Dictionary<string, XmlDocument> pLogClassRules = new Dictionary<string, XmlDocument>();
        internal static XmlDocument LogClassRules(string key)
        {
            if (pLogClassRules.ContainsKey(key))
            {
                return pLogClassRules[key];
            }
            return null;
        }
        internal static Dictionary<string, XmlDocument> LogClassRulesDictionary
        {
            get
            {
                return pLogClassRules;
            }
        }

        private static Dictionary<string, string> pChatColorGeneral = new Dictionary<string, string>();
        internal static string ChatColorGeneral(string key)
        {
            if (pChatColorGeneral.ContainsKey(key))
            {
                return pChatColorGeneral[key];
            }
            return null;
        }
        internal static Dictionary<string, string> ChatColorGeneralDictionary
        {
            get
            {
                return pChatColorGeneral;
            }
        }

        private static Dictionary<string, XmlDocument> pChatColorRules = new Dictionary<string, XmlDocument>();
        internal static XmlDocument ChatColorRules(string key)
        {
            if (pChatColorRules.ContainsKey(key))
            {
                return pChatColorRules[key];
            }
            return null;
        }
        internal static Dictionary<string, XmlDocument> ChatColorRulesDictionary
        {
            get
            {
                return pChatColorRules;
            }
        }

        private static Dictionary<string, System.Drawing.Bitmap> pChatColorIcons = new Dictionary<string, System.Drawing.Bitmap>();
        internal static System.Drawing.Bitmap ChatColorIcons(string key)
        {
            if (pChatColorIcons.ContainsKey(key))
            {
                return pChatColorIcons[key];
            }
            return null;
        }
        internal static Dictionary<string, System.Drawing.Bitmap> ChatColorIconsDictionary
        {
            get
            {
                return pChatColorIcons;
            }
        }

        private static Dictionary<string, System.Drawing.Color> pGUIColors = new Dictionary<string, System.Drawing.Color>();
        internal static System.Drawing.Color GUIColors(string key)
        {
            if (pGUIColors.ContainsKey(key))
            {
                return pGUIColors[key];
            }
            return System.Drawing.Color.Empty;
        }
        internal static Dictionary<string, System.Drawing.Color> GUIColorsDictionary
        {
            get
            {
                return pGUIColors;
            }
        }

        private static Dictionary<string, string> pChatOptions = new Dictionary<string, string>();
        internal static string ChatOptions(string key)
        {
            if (pChatOptions.ContainsKey(key))
            {
                return pChatOptions[key];
            }
            return null;
        }
        internal static Dictionary<string, string> ChatOptionsDictionary
        {
            get
            {
                return pChatOptions;
            }
        }
    }
}
