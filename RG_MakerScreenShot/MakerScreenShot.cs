using System;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.IL2CPP;
using UnityEngine;
using CharaCustom;
using RG;
using Illusion.Unity.Component;
using Illusion.IO;

namespace MakerScreenShot
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInProcess("RoomGirl.exe")]
    public class MakerScreenShot : BasePlugin
    {
        public const string PluginName = "RG Maker Screenshot";
        public const string GUID = "kky.RG.Maker.ScreenShot";
        public const string Version = "1.0.0";

        internal static new ManualLogSource Log;

        private static ConfigEntry<bool> EnableScreenshot { get; set; }
        private static ConfigEntry<KeyCode> ScreenshotHotkey { get; set; }
        private static ConfigEntry<bool> ScreenshotMessage { get; set; }
        private static ConfigEntry<bool> ScreenshotSound { get; set; }
        private static ConfigEntry<string> Prefix { get; set; }

        private static GameScreenShot gameScreenshot = new GameScreenShot();

        public override void Load()
        {
            Log = base.Log;

            EnableScreenshot = Config.Bind("General", "Enable", true, "Enable screen capture in Character Creator");
            ScreenshotHotkey = Config.Bind("General", "Hotkey", KeyCode.F11, "Hotkey for screen capture");
            ScreenshotMessage = Config.Bind("General", "Show messages on screen", true, "Whether screenshot messages will be displayed on screen. Messages will still be written to the log.");
            ScreenshotSound = Config.Bind("General", "Play camera sound effect", true, "Whether camera sound plays when screenshot is taken.");
            Prefix = Config.Bind("General", "Filename Prefix", "RG_", new ConfigDescription("String to append in front of screenshot filename.", null, "Advanced"));

            Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
        }

        private static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CustomControl), nameof(CustomControl.Update))]
            private static void AddScreenshotKey(CustomControl __instance)
            {
                bool isInputFocused = __instance.customBase.IsInputFocused();
                if (!isInputFocused)
                {
                    if (Input.GetKeyDown(ScreenshotHotkey.Value))
                    {
                        if (EnableScreenshot.Value)
                        {
                            if (ScreenshotSound.Value) Illusion.Game.Utils.Sound.PlaySystemSE(SystemSE.Capture);
                            string _path = CreateCaptureFileName();
                            gameScreenshot.Capture(_path);
                            LogScreenshotMessage("Writing screenshot to " + _path.Substring(Application.dataPath.Length + 4));
                            return;
                        }
                    }
                }
            }
        }

        private static void LogScreenshotMessage(string text)
        {
            if (ScreenshotMessage.Value)
                Log.LogMessage(text);
            else
                Log.LogInfo(text);
        }
        public static string CreateCaptureFileName()
        {
            StringBuilder stringBuilder = new StringBuilder(256);
            stringBuilder.Append(UserData.Create("cap"));
            stringBuilder.Append(Prefix.Value);
            DateTime now = DateTime.Now;
            stringBuilder.Append(now.Year.ToString("0000"));
            stringBuilder.Append(now.Month.ToString("00"));
            stringBuilder.Append(now.Day.ToString("00"));
            stringBuilder.Append(now.Hour.ToString("00"));
            stringBuilder.Append(now.Minute.ToString("00"));
            stringBuilder.Append(now.Second.ToString("00"));
            stringBuilder.Append(now.Millisecond.ToString("000"));
            stringBuilder.Append(".png");
            return stringBuilder.ToString();
        }
    }
}