using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Jotunn.Managers;
using UnityEngine.UI;

using static ArrowCounter.ArrowCounterConfig;

namespace ArrowCounter
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ArrowCounter : BaseUnityPlugin
    {
        public const string PluginGuid = "com.bruce.valheim.arrowcounter";
        public const string PluginName = "ArrowCounter";
        public const string PluginVersion = "1.1.0";

        private const string countsFileName = "arrowCounts";
        private static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "ArrowCounter.cfg"), true);

        static ManualLogSource _logger;
        Harmony _harmony;
        static ConfigEntry<bool> isModEnabled;

        private static InfoPanel infoPanel;

        private static bool _debug = false;
        public static bool ArrowCounterUIEnabled = true;

        public void Awake()
        {
            isModEnabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Enable or disable this mod.");
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
            _logger = Logger;
            BindConfig(configFile);
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        public static void Initialize(Player player) {
            infoPanel = new InfoPanel();
            initializeArrowCounts(player);
            infoPanel.Build();
        }

        public void Update() {
            var player = Player.m_localPlayer;
            if (player != null) {
                if(player.TakeInput()) {
                    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl)) {
                        if (Input.GetKeyDown(KeyCode.A)) {
                            bool state = infoPanel.Toggle();
                            log("Setting state of info panel to " + (state).ToString());
                        }
                    }
                }
            }
        }

        public static void saveCounts(Player player) {
            if(!player.GetPlayerName().IsNullOrWhiteSpace()) {
                log("Arrow count saving for player: " + player.GetPlayerName());
                using (StreamWriter file = File.CreateText(getCountsFilePath(player))) {
                    infoPanel.Write(file);
                }
            }
        }
        public static string getCountsFilePath(Player player) {
            return Path.Combine(Paths.ConfigPath, countsFileName + "_" + player.GetPlayerID().ToString());
        }
        public static void initializeArrowCounts(Player player) {
                infoPanel.SetZeroArrowCounts();
                if (File.Exists(getCountsFilePath(player))) {
                    log("Loading data for player " + player.GetPlayerName());
                    string[] lines = File.ReadAllLines(getCountsFilePath(player));
                    infoPanel.InitializeCountsForPlayer(lines);
                } else {
                    log("No stored data found for player " + player.GetPlayerName() + " ArrowCounter. Initializing values to 0.");
                }
            } 
        
        public static void log(string message) {
            if (_debug) {
                _logger.LogInfo(message);
            }
        }
        public static bool countAmmo(string ammoType, string ammoName) {
            if (ammoType.Equals("$ammo_arrows")) {
                infoPanel.Count(ammoName);
                log(infoPanel.GetSessionCount(ammoName).ToString() + " " + ammoName.Split('_').Last() + " arrows fired this session.");
                log((infoPanel.GetSessionCount(ammoName) + infoPanel.GetTotalCount(ammoName)).ToString() + " " + ammoName.Split('_').Last() + " arrows fired in total.");
                if (infoPanel.isActive) {
                    infoPanel.Update();
                }                
                return true;
            }
            return false;
        }

        public static bool IsInfoPanelActive() {
            return infoPanel.isActive;
        }

        public static void DeactivateInfoPanel() {
            infoPanel.Deactivate();
        }
    }
}
