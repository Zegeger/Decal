using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using Zegeger.Data;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Plugins.AgentU
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
                foreach (KeyValuePair<string, Value> v in RefData["GUIColors"].Values)
                {
                    int num = Int32.Parse("FF" + v.Value.ToString(), System.Globalization.NumberStyles.HexNumber);
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(num);
                    pGUIColors.Add(v.Key, color);
                }
                foreach (KeyValuePair<string, Value> v in RefData["VoidSpellDamage"].Values)
                {
                    pVoidDamage.Add(Int32.Parse(v.Key), v.Value.ToInt());
                }
                foreach (KeyValuePair<string, Value> v in RefData["VoidDOTEffects"].Values)
                {
                    pVoidDotFamilies.Add(v.Key, v.Value.ToInt());
                }
                foreach (KeyValuePair<string, Value> v in RefData["DispelGems"].Values)
                {
                    pDispelGems.Add(v.Key, v.Value.ToInt());
                }
                foreach (KeyValuePair<string, Value> v in RefData["BeerSpells"].Values)
                {
                    pBeerSpells.Add(Int32.Parse(v.Key), v.Value.ToString());
                }
                foreach (KeyValuePair<string, Value> v in RefData["BonusSpells"].Values)
                {
                    pBonusSpells.Add(v.Key, v.Value.ToInt());
                }
                foreach (KeyValuePair<string, Value> v in RefData["BonusCooldown"].Values)
                {
                    pBonusCooldown.Add(v.Key, v.Value.ToInt());
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
            pChatColors.Clear();
            pChatColorsHex.Clear();
            pChatColorsToHex.Clear();
            pChatWindows.Clear();
            pChatClasses.Clear();
            pGUIColors.Clear();
            pVoidDamage.Clear();
            pVoidDotFamilies.Clear();
            pDispelGems.Clear();
            pBeerSpells.Clear();
            pBonusSpells.Clear();
            pBonusCooldown.Clear();
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
        internal static Dictionary<string, int> ChatColorsDictionary
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


        private static Dictionary<int, int> pVoidDamage = new Dictionary<int, int>();
        internal static int VoidDamage(int key)
        {
            if (pVoidDamage.ContainsKey(key))
            {
                return pVoidDamage[key];
            }
            return -1;
        }
        internal static Dictionary<int, int> VoidDamageDictionary
        {
            get
            {
                return pVoidDamage;
            }
        }

        private static Dictionary<string, int> pVoidDotFamilies = new Dictionary<string, int>();
        internal static int VoidDotFamily(string key)
        {
            if (pVoidDotFamilies.ContainsKey(key))
            {
                return pVoidDotFamilies[key];
            }
            return -1;
        }
        internal static Dictionary<string, int> VoidDotFamilyDictionary
        {
            get
            {
                return pVoidDotFamilies;
            }
        }

        private static Dictionary<string, int> pDispelGems = new Dictionary<string, int>();
        internal static int DispelGems(string key)
        {
            if (pDispelGems.ContainsKey(key))
            {
                return pDispelGems[key];
            }
            return -1;
        }
        internal static Dictionary<string, int> DispelGemsDictionary
        {
            get
            {
                return pDispelGems;
            }
        }

        private static Dictionary<string, int> pBonusSpells = new Dictionary<string, int>();
        internal static int BonusSpells(string key)
        {
            if (pBonusSpells.ContainsKey(key))
            {
                return pBonusSpells[key];
            }
            return -1;
        }
        internal static Dictionary<string, int> BonusSpellsDictionary
        {
            get
            {
                return pBonusSpells;
            }
        }

        private static Dictionary<string, int> pBonusCooldown = new Dictionary<string, int>();
        internal static int BonusCooldown(string key)
        {
            if (pBonusCooldown.ContainsKey(key))
            {
                return pBonusCooldown[key];
            }
            return -1;
        }
        internal static Dictionary<string, int> BonusCooldownDictionary
        {
            get
            {
                return pBonusCooldown;
            }
        }

        private static Dictionary<int, string> pBeerSpells = new Dictionary<int, string>();
        internal static string BeerSpells(int key)
        {
            if (pBeerSpells.ContainsKey(key))
            {
                return pBeerSpells[key];
            }
            return "";
        }
        internal static Dictionary<int, string> BeerSpellsDictionary
        {
            get
            {
                return pBeerSpells;
            }
        }
    }
}
