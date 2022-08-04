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
        public const string PluginVersion = "1.0.0";

        private const string countsFileName = "arrowCounts";
        private static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "ArrowCounter.cfg"), true);

        static ManualLogSource _logger;
        Harmony _harmony;
        static ConfigEntry<bool> isModEnabled;

        private static GameObject infoPanel;
        public static GameObject infoPrefab;

        private static bool _debug = true;
        public static bool ArrowCounterUIEnabled = true;
        private static bool isInfoPanelActive = false;

        private static Dictionary<string, int> sessionArrowCounts = new Dictionary<string, int>();
        private static Dictionary<string, int> totalArrowCounts = new Dictionary<string, int>();
        private static Dictionary<string, GameObject> arrowCountSessionTexts = new Dictionary<string, GameObject>();
        private static Dictionary<string, GameObject> arrowCountTotalTexts = new Dictionary<string, GameObject>();

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
            BindConfig(configFile);
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        public void Update() {
            var player = Player.m_localPlayer;
            if (player != null) {
                if(player.TakeInput()) {
                    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl)) {
                        if (Input.GetKeyDown(KeyCode.A)) {
                            log("Toggling Arrow Counter info panel.");
                            toggleInfoPanel();
                        }
                    }
                }
            }
        }

        public static void toggleInfoPanel() {
            if(!infoPanel) {
                infoPanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.96f, 0.5f),
                anchorMax: new Vector2(0.96f, 0.5f),
                position: new Vector2(0, 0),
                width: 150,
                height: 500,
                draggable: false);

                float dataHeight = -35f - 45f;

                GameObject titleObject = GUIManager.Instance.CreateText(
                    text: "Arrow Counter",
                    parent: infoPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(5f, -45f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 18,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 135f,
                    height: 45f,
                    addContentSizeFitter: false);


                GameObject header1Object = GUIManager.Instance.CreateText(
                    text: "Today's Usage",
                    parent: infoPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(25f, -75f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 14,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 135f,
                    height: 45f,
                    addContentSizeFitter: false);

                // Create 
                foreach (string arrowName in arrowNames) {
                    GameObject arrowNameObject = GUIManager.Instance.CreateText(
                    text: arrowName.Split('_').Last().First().ToString().ToUpper() + String.Join("", arrowName.Split('_').Last().Skip(1)),
                    parent: infoPanel.transform,
                    anchorMin: new Vector2(0.05f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(35f, dataHeight),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 12,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 50f,
                    height: 15f,
                    addContentSizeFitter: false);

                    GameObject countTextObject = GUIManager.Instance.CreateText(
                    text: sessionArrowCounts[arrowName].ToString(),
                    parent: infoPanel.transform,
                    anchorMin: new Vector2(0.05f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(130f, dataHeight),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 12,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 50f,
                    height: 15f,
                    addContentSizeFitter: false);

                    arrowCountSessionTexts[arrowName] = countTextObject;
                  
                   dataHeight -= 15;
                }

                dataHeight -= 30;
                GameObject header2Object = GUIManager.Instance.CreateText(
                    text: "Total Usage",
                    parent: infoPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(25f, dataHeight),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 14,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 135f,
                    height: 45f,
                    addContentSizeFitter: false);

                foreach (string arrowName in arrowNames) {
                    GameObject arrowNameObject = GUIManager.Instance.CreateText(
                    text: arrowName.Split('_').Last().First().ToString().ToUpper() + String.Join("", arrowName.Split('_').Last().Skip(1)),
                    parent: infoPanel.transform,
                    anchorMin: new Vector2(0.05f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(35f, dataHeight),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 12,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 50f,
                    height: 15f,
                    addContentSizeFitter: false);

                    GameObject countTextObject = GUIManager.Instance.CreateText(
                    text: totalArrowCounts[arrowName].ToString(),
                    parent: infoPanel.transform,
                    anchorMin: new Vector2(0.05f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(130f, dataHeight),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 12,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 50f,
                    height: 15f,
                    addContentSizeFitter: false);

                    arrowCountTotalTexts[arrowName] = countTextObject;

                    dataHeight -= 15;
                }
            }

            isInfoPanelActive = !isInfoPanelActive;
            log("Setting state of info panel to " + (isInfoPanelActive).ToString());
            infoPanel.SetActive(isInfoPanelActive);
        }

        public static void updateInfoPanel() {
            foreach(string arrowName in arrowNames) {
                arrowCountSessionTexts[arrowName].GetComponent<Text>().text = sessionArrowCounts[arrowName].ToString();
            }
        }

        public static void saveCounts(Player player) {
            if(!player.GetPlayerName().IsNullOrWhiteSpace()) {
                log("Arrow count saving for player: " + player.GetPlayerName());
                using (StreamWriter file = File.CreateText(getCountsFilePath(player))) {
                    foreach (KeyValuePair<string, int> entry in sessionArrowCounts) {
                        file.WriteLine(entry.Key + "," + (entry.Value + totalArrowCounts[entry.Key]).ToString());
                    }
                }
            }
        }
        public static string getCountsFilePath(Player player) {
            return Path.Combine(Paths.ConfigPath, countsFileName + "_" + player.GetPlayerID().ToString());
        }
        public static void initializeArrowCounts(Player player) {
            if(!player.GetPlayerName().IsNullOrWhiteSpace()) {
                sessionArrowCounts = setZeroArrowCounts();
                totalArrowCounts = setZeroArrowCounts();

                if (File.Exists(getCountsFilePath(player))) {
                    log("Loading data for player " + player.GetPlayerName());
                    string[] lines = File.ReadAllLines(getCountsFilePath(player));
                    foreach (string line in lines) {
                        totalArrowCounts[line.Split(',')[0]] = int.Parse(line.Split(',')[1]);
                    }
                } else {
                    log("No stored data found for player " + player.GetPlayerName() + " ArrowCounter. Initializing values to 0.");
                }
            } else {
                log("No player name found.");
            }
        }
        private static Dictionary<string, int> setZeroArrowCounts() {
            Dictionary<string, int> arrowCounts = new Dictionary<string, int>();
            foreach (string arrowName in arrowNames) {
                arrowCounts.Add(arrowName, 0);
            }
            return arrowCounts;
        }

        public static void log(string message) {
            if (_debug) {
                _logger.LogInfo(message);
            }
        }
        public static bool countAmmo(string ammoType, string ammoName) {
            if (ammoType.Equals("$ammo_arrows")) {
                ArrowCounter.sessionArrowCounts[ammoName]++;
                ArrowCounter.log(ArrowCounter.sessionArrowCounts[ammoName].ToString() + " " + ammoName.Split('_').Last() + " arrows fired this session.");
                ArrowCounter.log((ArrowCounter.sessionArrowCounts[ammoName] + ArrowCounter.totalArrowCounts[ammoName]).ToString() + " " + ammoName.Split('_').Last() + " arrows fired in total.");
                if (infoPanel) {
                    updateInfoPanel();
                }                
                return true;
            }
            return false;
        }
    }
}
