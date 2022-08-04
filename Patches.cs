using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ArrowCounter {

    [HarmonyPatch(typeof(Player), "Load")]
    public static class PlayerLoadPatch {

        [HarmonyPostfix]
        public static void PlayerLoadPostfix(Player __instance) {
            if (!string.IsNullOrEmpty(__instance.GetPlayerName())) {
                ArrowCounter.Initialize(__instance);
            } else {
                ArrowCounter.log("No player name found.");
            }
        }
    }

    [HarmonyPatch(typeof(Player), "Save")]
    public static class PlayerOnDestroyPatch {

        [HarmonyPrefix]
        public static bool PlayerDestroyPrefix(Player __instance) {
            ArrowCounter.saveCounts(__instance);
            if (ArrowCounter.IsInfoPanelActive()) {
                ArrowCounter.DeactivateInfoPanel();
            }
            return true;
        }
        
    }
    [HarmonyPatch(typeof(Attack), "UseAmmo")]
    public static class Attack_UseAmmo_Patch {
        [HarmonyPostfix]
        public static void UseAmmoPostfix(Attack __instance) {
            //string ammoType = __instance.m_ammoItem.m_shared.m_ammoType;
            //string ammoName = __instance.m_ammoItem.m_shared.m_name;
            // Non Publicized way of accessing properties
            string ammoType = Traverse.Create(__instance).Field("m_ammoItem").Field("m_shared").Field("m_ammoType").GetValue() as string;
            string ammoName = Traverse.Create(__instance).Field("m_ammoItem").Field("m_shared").Field("m_name").GetValue() as string;
            if (!string.IsNullOrWhiteSpace(ammoType) && !string.IsNullOrWhiteSpace(ammoName)) {
                ArrowCounter.countAmmo(ammoType, ammoName);
            }
        }
    }
}
