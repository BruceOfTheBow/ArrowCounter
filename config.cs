using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArrowCounter {
    public class ArrowCounterConfig {
        public ConfigFile ConfigFile { get; set; }
        public static ConfigEntry<KeyboardShortcut> ChangePieceColorShortcut { get; private set; }

        public bool OpenHotkeyModifierSet = true;

        public static void BindConfig(ConfigFile config) {
            //config.Bind("Hotkeys", "openHotkey", new KeyboardShortcut(KeyCode.A, KeyCode.LeftShift, KeyCode.LeftControl), "Main hotkey to show/hide the arrow totals.");
        }
        //public bool IfMenuHotkeyPressed() {
        //    if (!OpenHotkeyModifierSet) { 
        //        return Input.GetKeyDown(OpenHotkey.Value);
        //    }
        //    return false;
        //}
    }
}
