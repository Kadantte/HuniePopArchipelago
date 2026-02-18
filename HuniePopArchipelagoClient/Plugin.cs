using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using HuniePopArchiepelagoClient.UI;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace HuniePopArchiepelagoClient
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "Dots.Archipelago.huniepop";
        public const string PluginName = "Hunie Pop";
        public const string PluginVersion = "2.1.1";
        public static int compatworldmajor = 2;
        public static int compatworldminor = 1;
        public static int compatworldbuild = 1;


        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        public const string APDisplayInfo = $"Archipelago v{PluginVersion}";
        public static ManualLogSource BepinLogger;
        //public static ArchipelagoClient ArchipelagoClient;
        public static CursedArchipelagoClient curse;

        public static string playeruri = "ws://localhost:38281";
        public static string playername = "Player1";
        public static string playerpass = "";

        public static bool tringtoconnect = false;
        public static int connectstage = 0;
        public static bool dll;

        public static bool showwindow = false;

        private static Texture2D SolidBoxTex;

        private void Awake()
        {
            // Plugin startup logic
            BepinLogger = Logger;
            curse = new CursedArchipelagoClient();
            ArchipelagoConsole.Awake();

            new Harmony(PluginGUID).PatchAll();

            try
            {
                ArchipelagoConsole.LogMessage("DotsWebsocket.dll version:" + helper.dotsV().ToString());
                if (helper.dotsV() == 3) { dll = true; }
                else { ArchipelagoConsole.LogMessage("DotsWebsocket Not Correct Version"); }

            }
            catch
            {
                dll = false;
                ArchipelagoConsole.LogImportant("FATAL ERROR: DotsWebSocket.dll not able to be accessed");
                if (File.Exists("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll"))
                {
                    ArchipelagoConsole.LogImportant("DotsWebSocket.dll exists but errored on client.");
                }
            }
            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded! - Press F8 to Toggle Console");

        }
        void Update()
        {
            if (dll)
            {
                if (tringtoconnect && helper.hasmsg(curse.ws))
                {
                    CursedArchipelagoClient.msgCallback(Marshal.PtrToStringAnsi(helper.getmsg(curse.ws)));
                }
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ArchipelagoConsole.toggle();
            }
            if (Input.GetKeyDown(KeyCode.F7))
            {
            }
            if (Input.GetKeyDown(KeyCode.Escape) && !curse.fullconnection)
            {
                Menu.newgame = false;
            }
            if (Input.GetKeyDown(KeyCode.Return) && !ArchipelagoConsole.Hidden && !ArchipelagoConsole.CommandText.IsNullOrWhiteSpace())
            {
                Plugin.curse.sendSay(ArchipelagoConsole.CommandText);
                ArchipelagoConsole.CommandText = "";
            }
        }

        private void OnGUI()
        {
            GUI.depth = -1;
            // show the mod is currently loaded in the corner
            ArchipelagoConsole.OnGUI();

            GUI.backgroundColor = Color.black;

            // show the Archipelago Version and whether we're connected or not
            if (curse.fullconnection)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;
                GUI.Box(new Rect(Screen.width - 300, 40, 300, 20), "");
                if (curse.worldver == null)
                {
                    GUI.Label(new Rect(Screen.width - 295, 40, 300, 20), "Client V(" + PluginVersion + "), World V(ERROR VERSION NOT SUPPORTED): Status: Connected");
                }
                else
                {
                    GUI.Label(new Rect(Screen.width - 295, 40, 300, 20), "Client V(" + PluginVersion + "), World V(" + curse.worldver.major + "." + curse.worldver.minor + "." + curse.worldver.build + "): Status: Connected");
                }

            }

            if (showwindow)
            {
                Menu.archwindowrect.Set((float)(Screen.width * 0.1), (float)(Screen.height * 0.1), (float)(Screen.width * 0.8), (float)(Screen.height * 0.8));

                Menu.archwindow();
                //GUI.Window(61, Menu.archwindowrect, Menu.archwindow, "");
            }
            // this is a good place to create and add a bunch of debug buttons
        }


        public static void DrawSolidBox(Rect boxRect)
        {
            if (SolidBoxTex == null)
            {
                var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                windowBackground.SetPixel(0, 0, new Color(0, 0, 0));
                windowBackground.Apply();
                SolidBoxTex = windowBackground;
            }

            // It's necessary to make a new GUIStyle here or the texture doesn't show up
            GUI.Box(boxRect, "", new GUIStyle { normal = new GUIStyleState { background = SolidBoxTex } });
        }

    }
}