﻿using HarmonyLib;

namespace SwapItems
{
    [HarmonyPatch(typeof(MainMenu), "Start")]
    internal class Patch
    {
        static void Postfix()
        {
            BepinPlugin.Log.LogInfo("Example Patch Executed");
        }
    }
}
