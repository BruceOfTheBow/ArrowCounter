using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace ArrowCounter
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ArrowCounter : BaseUnityPlugin
    {
        public const string PluginGuid = "com.bruce.valheim.arrowcounter";
        public const string PluginName = "ArrowCounter";
        public const string PluginVersion = "1.0.0";

        private const string countsFileName = "arrowCounts";
        private static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "ArrowCounter.cfg"), true);

        static ManualLogSource _logger;
        Harmony _harmony;
        static ConfigEntry<bool> isModEnabled;

        private static bool _debug = true;
        public static bool ArrowCounterUIEnabled = true;

        private static Dictionary<string, int> arrowCounts = new Dictionary<string, int>();

        private static List<string> arrowNames = new List<string> {
                    "$item_arrow_wood",
                    "$item_arrow_fire",
                    "$item_arrow_flint",
                    "$item_arrow_bronze",
                    "$item_arrow_iron",
                    "$item_arrow_silver",
                    "$item_arrow_obsidian",
                    "$item_arrow_poison",
                    "$item_arrow_frost",
                    "$item_arrow_needle"
                };

        public void Awake()
        {
            isModEnabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Enable or disable this mod.");
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
            _logger = Logger;
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        public static void saveCounts(Player player) {
            if(!player.GetPlayerName().IsNullOrWhiteSpace()) {
                log("Arrow count saving for player: " + player.GetPlayerName());
                using (StreamWriter file = File.CreateText(getCountsFilePath(player))) {
                    foreach (KeyValuePair<string, int> entry in arrowCounts) {
                        file.WriteLine(entry.Key + "," + entry.Value.ToString());
                    }
                }
            }
        }
        public static string getCountsFilePath(Player player) {
            return Path.Combine(Paths.ConfigPath, countsFileName + "_" + player.GetPlayerID().ToString());
        }
        public static void initializeArrowCounts(Player player) {
            if(!player.GetPlayerName().IsNullOrWhiteSpace()) {
                arrowCounts = new Dictionary<string, int>();
                if (File.Exists(getCountsFilePath(player))) {
                    log("Loading data for player " + player.GetPlayerName());
                    string[] lines = File.ReadAllLines(getCountsFilePath(player));
                    foreach (string line in lines) {
                        arrowCounts.Add(line.Split(',')[0], int.Parse(line.Split(',')[1]));
                    }
                } else {
                    log("No stored data found for player " + player.GetPlayerName() + " ArrowCounter. Initializing values to 0.");
                    foreach (string arrowName in arrowNames) {
                        arrowCounts.Add(arrowName, 0);
                    }
                }
            } else {
                log("No player name found.");
            }
        }
        public static void log(string message) {
            if (_debug) {
                _logger.LogInfo(message);
            }
        }
        public static bool countAmmo(string ammoType, string ammoName) {
            if (ammoType.Equals("$ammo_arrows")) {
                ArrowCounter.arrowCounts[ammoName]++;
                ArrowCounter.log(ArrowCounter.arrowCounts[ammoName].ToString() + " " + ammoName.Split('_').Last() + " arrows fired in total.");
                return true;
            }
            return false;
        }
    }
}
