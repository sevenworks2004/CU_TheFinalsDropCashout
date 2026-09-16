using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using CUCoreLib.Registries;
using UnityEngine.SceneManagement;
using System.Linq;

namespace ModNamespace
{
    internal class Patches
    {
        [HarmonyPatch(typeof(ConsoleScript))]
        internal static class ConsolePatch
        {
            [HarmonyPatch(nameof(ConsoleScript.Start))]
            [HarmonyPostfix]
            private static void StartPatch()
            {
                ConsoleScript.instance.LogToConsole("Load Custom analoga The finals");
            }
        }
    }
}
