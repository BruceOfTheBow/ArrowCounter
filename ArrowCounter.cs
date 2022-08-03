
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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

        static ManualLogSource _logger;
        Harmony _harmony;
        static ConfigEntry<bool> isModEnabled;

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


        [HarmonyPatch(typeof(Attack))]
        public static class Attack_UseAmmo_Patch
        {
            private static bool _debug = true;
            private static int woodArrowCount = 0;
            private static int fireArrowCount = 0;
            private static int flintArrowCount = 0;
            private static int bronzeArrowCount = 0;
            private static int ironArrowCount = 0;
            private static int silverArrowCount = 0;
            private static int obsidianArrowCount = 0;
            private static int poisonArrowCount = 0;
            private static int frostArrowCount = 0;
            private static int needleArrowCount = 0;
            
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
                    if (ammoName.Equals("$item_arrow_wood"))
                    {
                        woodArrowCount++;
                        log(woodArrowCount.ToString() + " wood arrows fired this session.");
                    }
                    else if(ammoName.Equals("$item_arrow_fire"))
                    {
                        fireArrowCount++;
                        log(fireArrowCount.ToString() + " fire arrows fired this session.");
                    } 
                    else if(ammoName.Equals("$item_arrow_flint"))
                    {
                        flintArrowCount++;
                        log(flintArrowCount.ToString() + " flint arrows fired this session.");
                    }
                    else if (ammoName.Equals("$item_arrow_bronze"))
                    {
                        bronzeArrowCount++;
                        log(bronzeArrowCount.ToString() + " bronze arrows fired this session.");
                    }
                    else if (ammoName.Equals("$item_arrow_iron"))
                    {
                        ironArrowCount++;
                        log(ironArrowCount.ToString() + " iron arrows fired this session.");
                    }
                    else if (ammoName.Equals("$item_arrow_silver"))
                    {
                        silverArrowCount++;
                        log(silverArrowCount.ToString() + " silver arrows fired this session.");
                    }
                    else if (ammoName.Equals("$item_arrow_obsidian"))
                    {
                        obsidianArrowCount++;
                        log(obsidianArrowCount.ToString() + " obsidian arrows fired this session.");
                    }
                    else if (ammoName.Equals("$item_arrow_poison"))
                    {
                        poisonArrowCount++;
                        log(poisonArrowCount.ToString() + " poison arrows fired this session.");
                    }
                    else if (ammoName.Equals("$item_arrow_frost"))
                    {
                        frostArrowCount++;
                        log(frostArrowCount.ToString() + " frost arrows fired this session.");
                    }
                    else if (ammoName.Equals("$item_arrow_needle"))
                    {
                        needleArrowCount++;
                        log(needleArrowCount.ToString() + " needle arrows fired this session.");
                    }
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
        }
    }
}
