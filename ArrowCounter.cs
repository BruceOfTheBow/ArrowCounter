using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

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

        public static bool ArrowCounterUIEnabled = true;

        public void Awake()
        {
            isModEnabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Enable or disable this mod.");
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
            _logger = Logger;
            Attack_UseAmmo_Patch.initializeArrowCounts();
        }

        public void OnDestroy()
        {
            // this should really go on logout
            Attack_UseAmmo_Patch.saveCounts();
            _harmony?.UnpatchSelf();
        }


        [HarmonyPatch(typeof(Attack))]
        public static class Attack_UseAmmo_Patch {
            private static bool _debug = true;

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
            private static Dictionary<string, int> arrowCounts = initializeArrowCounts();            

            [HarmonyPostfix]
            [HarmonyPatch(nameof(Attack.UseAmmo))]
            
            public static void UseAmmoPostfix(Attack __instance)
            {
                string ammoType = Traverse.Create(__instance).Field("m_ammoItem").Field("m_shared").Field("m_ammoType").GetValue() as string;
                string ammoName = Traverse.Create(__instance).Field("m_ammoItem").Field("m_shared").Field("m_name").GetValue() as string;
                countAmmo(ammoType, ammoName);
            }

            private static bool countAmmo(string ammoType, string ammoName)
            {
                if(ammoType.Equals("$ammo_arrows"))
                {
                    arrowCounts[ammoName]++;
                    log(arrowCounts[ammoName].ToString() + " " + ammoName.Split('_').Last() + " arrows fired in total.");
                    return true;
                }
                return false;
            }

            private static void log(string message)
            {
                if(_debug)
                {
                    _logger.LogInfo(message);
                }
            }
            
            public static void GetCounts() {

            }

            public static void saveCounts() {
                log("Arrow count saving to file: " + getCountsFilePath());
                using (StreamWriter file = File.CreateText(getCountsFilePath())) {
                    foreach (KeyValuePair<string, int> entry in arrowCounts) {
                        file.WriteLine(entry.Key + "," + entry.Value.ToString());
                    }
                }
            }
            public static Dictionary<string, int> initializeArrowCounts() {
                Dictionary<string, int> arrowCounts = new Dictionary<string, int>();
                if (File.Exists(getCountsFilePath())) {
                    log("Reading data from " + getCountsFilePath());
                    string[] lines = File.ReadAllLines(getCountsFilePath());
                    foreach(string line in lines) {
                        arrowCounts.Add(line.Split(',')[0], int.Parse(line.Split(',')[1]));
                    }
                } else {
                    log("No stored data found for ArrowCounter. Initializing values to 0.");
                    foreach (string arrowName in arrowNames) {
                        arrowCounts.Add(arrowName, 0);
                    }
                }
                return arrowCounts;
            }

            public static string getCountsFilePath() {
                return Path.Combine(Paths.ConfigPath, countsFileName);
            }
        }
    }
}
