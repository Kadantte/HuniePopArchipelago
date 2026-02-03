using BepInEx;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using HuniePopArchiepelagoClient.HuniePop.UI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HuniePopArchiepelagoClient.UI
{

    [HarmonyPatch]
    public class Menu
    {

        public static LoadScreen ts;

        public static Rect archwindowrect = new Rect();

        public static Rect archnewrect = new Rect();

        public static Rect archsaverect1 = new Rect();
        public static Rect archsaverect2 = new Rect();
        public static Rect archsaverect3 = new Rect();
        public static Rect archsaverect4 = new Rect();

        public static GUIStyle buttonstyle1;
        public static GUIStyle buttonstyle2;

        public static GUIStyle labelstyle1;
        public static GUIStyle labelstyle2;
        public static GUIStyle labelstyle3;

        public static GUIStyle textstyle1;

        public static int slot=-1;
        public static bool newgame = false;
        public static bool erase = false;

        public static bool eraseedit = true;

        public static string playeruri = "ws://localhost:38281";
        public static string playername = "Player1";
        public static string playerpass = "";

        public static void archwindow()
        {
            float w = (float)((archwindowrect.width * 0.8) / 2);
            float h = (float)((archwindowrect.height * 0.8) / 2);

            float r1 = archwindowrect.x + (float)(archwindowrect.width * 0.05);
            float r2 = (float)(Screen.width - r1 - w);
            float c1 = archwindowrect.y + (float)(archwindowrect.height * 0.05);
            float c2 = (float)(Screen.height - c1 - h); ;
            //float c2 = Screen.height - (float)(archwindowrect.height - c1 - h);

            archsaverect1.Set(r1, c1, w, h);
            archsaverect2.Set(r2, c1, w, h);
            archsaverect3.Set(r1, c2, w, h);
            archsaverect4.Set(r2, c2, w, h);

            archnewrect.Set((Screen.width/2)-(w/2), (Screen.height / 2) - (h / 2), w, h);

            if (buttonstyle1 == null)
            {
                buttonstyle1 = new GUIStyle(GUI.skin.button.name);
                buttonstyle2 = new GUIStyle(GUI.skin.button.name);

                labelstyle1 = new GUIStyle(GUI.skin.label.name);
                labelstyle2 = new GUIStyle(GUI.skin.label.name);
                labelstyle3 = new GUIStyle(GUI.skin.label.name);

                textstyle1 = new GUIStyle(GUI.skin.textField.name);

                buttonstyle1.alignment = TextAnchor.MiddleCenter;
                buttonstyle2.alignment = TextAnchor.MiddleCenter;

                labelstyle1.alignment = TextAnchor.UpperCenter;
                labelstyle2.alignment = TextAnchor.MiddleLeft;
                labelstyle3.alignment = TextAnchor.MiddleLeft;

                textstyle1.alignment = TextAnchor.MiddleLeft;


                labelstyle1.fontSize = (int)(h / 15);
                labelstyle2.fontSize = (int)(h / 15);
                labelstyle3.fontSize = (int)(h / 18);

                textstyle1.fontSize = (int)(h / 15);

                buttonstyle1.fontSize = (int)(h / 15);
                buttonstyle2.fontSize = (int)(h / 10);
            }

            labelstyle1.fontSize = (int)(h / 15);
            labelstyle2.fontSize = (int)(h / 15);
            labelstyle3.fontSize = (int)(h / 18);

            textstyle1.fontSize = (int)(h / 15);

            buttonstyle1.fontSize = (int)(h / 15);
            buttonstyle2.fontSize = (int)(h / 10);


            if (Plugin.curse.fullconnection)
            {
                GUI.Window(300 + slot, archnewrect, fullconnectwindow, "");
            }
            else if (Plugin.tringtoconnect)
            {
                GUI.Window(300 + slot, archnewrect, tryingtoconnectwindow, "");
            }
            else if(newgame)
            {
                GUI.Window(200 + slot, archnewrect, newgamewindow, "");
            }
            else
            {
                GUI.Window(101, archsaverect1, savewindow, "");
                GUI.Window(102, archsaverect2, savewindow, "");
                GUI.Window(103, archsaverect3, savewindow, "");
                GUI.Window(104, archsaverect4, savewindow, "");
            }

        }

        public static void savewindow(int id)
        {
            GUI.Label(new Rect(0,0, archsaverect1.width, (float)(archsaverect1.height * 0.1)), $"SAVE SLOT #{id-100}", labelstyle1);
            if (SaveUtils.GetSaveFile(id - 101).started)
            {
                ArchipelageItemList savedlist = new ArchipelageItemList();
                if (File.Exists(Application.persistentDataPath + $"/archdata{id - 101}"))
                {
                    using (StreamReader file = File.OpenText(Application.persistentDataPath + $"/archdata{id - 101}"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        savedlist = (ArchipelageItemList)serializer.Deserialize(file, typeof(ArchipelageItemList));
                        GUI.Label(new Rect(0, 2 * (archsaverect1.height / 6), archsaverect1.width, archsaverect1.height / 6), $"host: {savedlist.host}", labelstyle1);
                        GUI.Label(new Rect(0, 3 * (archsaverect1.height / 6), archsaverect1.width, archsaverect1.height / 6), $"name: {savedlist.user}", labelstyle1);
                        GUI.Label(new Rect(0, 4 * (archsaverect1.height / 6), archsaverect1.width, archsaverect1.height / 6), $"last played: {File.GetLastWriteTime(Application.persistentDataPath + $"/archdata{id - 101}").ToString("d MMM h:m:s tt")}", labelstyle1);
                    }
                }
                else
                {
                    GUI.Label(new Rect(0, 2 * (archsaverect1.height / 6), archsaverect1.width, 3*(archsaverect1.height / 6)), $"ERROR READING SAVED CONNECTION DATA", labelstyle1);
                }


                if (!erase)
                {
                    if (savedlist.host == "")
                    {
                        if (GUI.Button(new Rect((archsaverect1.width / 2) - (archsaverect1.width / 6), 5 * (archsaverect1.height / 6), archsaverect1.width / 3, archsaverect1.height / 6), "EDIT\nCONNECTION"))
                        {
                            slot = id - 101;
                            newgame = true;
                        }
                    }
                    else
                    {
                        if (GUI.Button(new Rect((archsaverect1.width / 8), 5 * (archsaverect1.height / 6), archsaverect1.width / 4, archsaverect1.height / 6), "Continue"))
                        {
                            slot = id - 101;
                            if (Plugin.dll)
                            {
                                Plugin.tringtoconnect = true;
                                Plugin.curse.setup(savedlist.host, savedlist.user, savedlist.pass);
                                Plugin.curse.connect();
                            }
                        }
                        if (GUI.Button(new Rect((archsaverect1.width / 2) + (archsaverect1.width / 8), 5 * (archsaverect1.height / 6), archsaverect1.width / 4, archsaverect1.height / 6), "Edit\nConnection"))
                        {
                            slot = id - 101;
                            newgame = true;
                        }
                    }
                }
                else
                {
                    if (GUI.Button(new Rect((archsaverect1.width/2)-(archsaverect1.width/6), 5 * (archsaverect1.height / 6), archsaverect1.width/3, archsaverect1.height / 6), "ERASE FILE"))
                    {
                        SaveUtils.GetSaveFile(id - 101).ResetFile();
                        SaveUtils.Save();
                        if (File.Exists(Application.persistentDataPath + $"/archdata{id - 101}")) File.Delete(Application.persistentDataPath + $"/archdata{id - 101}");
                    }

                }
            }
            else
            {
                if (GUI.Button(new Rect((float)(archsaverect1.width * 0.1), (float)(archsaverect1.height * 0.1), (float)(archsaverect1.width * 0.8), (float)(archsaverect1.height * 0.8)), "Click To Connect \nand Start a New Game", buttonstyle2))
                {
                    slot = id - 101;
                    newgame = true;
                }
            }
        }

        public static void newgamewindow(int id)
        {
            string statusMessage = " Status: Disconnected";
            float t = (float)((archnewrect.height*0.8) / 5);
            float s = (float)(archnewrect.height*0.1);
            GUI.Label(new Rect(0, s, archnewrect.width, t), Plugin.APDisplayInfo + statusMessage, labelstyle1);
            GUI.Label(new Rect(5, s + t, (float)(archnewrect.width*0.2), t), "Host: ", labelstyle2);
            GUI.Label(new Rect(5, s + (t * 2), (float)(archnewrect.width * 0.2), t), "Player\nName: ", labelstyle2);
            GUI.Label(new Rect(5, s + (t * 3), (float)(archnewrect.width * 0.2), t), "Pass: ", labelstyle2);

            playeruri = GUI.TextField(new Rect((float)(archnewrect.width * 0.25), s + t, (float)(archnewrect.width * 0.7), t), playeruri, 100, textstyle1);
            playername = GUI.TextField(new Rect((float)(archnewrect.width * 0.25), s + (t * 2), (float)(archnewrect.width * 0.7), t), playername, 100, textstyle1);
            playerpass = GUI.TextField(new Rect((float)(archnewrect.width * 0.25), s + (t * 3), (float)(archnewrect.width * 0.7), t), playerpass, 100, textstyle1);


            if (GUI.Button(new Rect((archnewrect.width/2) - (archnewrect.width / 6), (float)(s + (t * 4.2)), archnewrect.width/3, (float)(t*0.8)), "Connect", buttonstyle2) && playername!="")
            {
                if (Plugin.dll)
                {
                    Plugin.tringtoconnect = true;
                    Plugin.curse.setup(playeruri, playername, playerpass);
                    Plugin.curse.connect();
                }
            }
        }

        public static void tryingtoconnectwindow(int id)
        {
            float h = archnewrect.height / 12;

            GUI.Label(new Rect(0, h, archnewrect.width, h), "-connecting to:" + Plugin.curse.url, labelstyle3);
            if (helper.readyWS(Plugin.curse.ws) == 3)
            {
                GUI.Label(new Rect(0, h*2, archnewrect.width, h), "-initial server connection established", labelstyle3);
            }
            if (Plugin.curse.recievedroominfo)
            {
                GUI.Label(new Rect(0, h*3, archnewrect.width, h), "-sending archipelago GetDataPackages packet", labelstyle3);
                if (!Plugin.curse.sendroomdatapackage)
                {
                    Plugin.curse.sendGetPackage();
                    Plugin.curse.sendroomdatapackage = true;
                }
            }
            if (Plugin.curse.recievedroomdatapackage)
            {
                GUI.Label(new Rect(0, h*4, archnewrect.width, h), "-recieved archipelago GetDataPackages packet", labelstyle3);
                if (!Plugin.curse.startprocessedroomdatapackage && !Plugin.curse.processeddatapackage)
                {
                    Plugin.curse.data.data.setup();
                    Plugin.curse.processeddatapackage = true;
                }
            }
            if (Plugin.curse.processeddatapackage)
            {
                GUI.Label(new Rect(0, h*5, archnewrect.width, h), "-processed archipelago GetDataPackages", labelstyle3);
                if (!Plugin.curse.startprocessedroomdatapackage)
                {
                    GUI.Label(new Rect(0, h*6, archnewrect.width, h), "-sending archipelago Connect Packet", labelstyle3);
                    if (!Plugin.curse.sentconnectedpacket)
                    {
                        Plugin.curse.sendConnectPacket();
                        Plugin.curse.sentconnectedpacket = true;
                    }
                }
            }
            if (Plugin.curse.recievedconnectedpacket)
            {
                GUI.Label(new Rect(0, h*7, archnewrect.width, h), "-connection to archipelago server established", labelstyle3);
                GUI.Label(new Rect(0, h*8, archnewrect.width, h), "-waiting on geting a packet to know if connection", labelstyle3);
                GUI.Label(new Rect(0, h*9, archnewrect.width, h), "is fully working", labelstyle3);
            }
            if (helper.readyWS(Plugin.curse.ws) == 2)
            {
                if (GUI.Button(new Rect(0, h*10, archnewrect.width/3, h), "RESET", buttonstyle1)) { Plugin.tringtoconnect = false; newgame = false; }
            }
        }

        public static void fullconnectwindow(int id)
        {
            if (eraseedit) { ts.Refresh(); eraseedit = false; }

            string s = "";
            string g = "";
            bool gs = false;

            if (!SaveUtils.GetSaveFile(slot).started)
            {
                s = "START GAME";
                g = "NEW GAME";
                gs = false;
            }
            else
            {
                s = "CONTINUE GAME";
                g = "GAME STARTED";
                gs = true;
            }

            GUI.Label(new Rect(0, 1 * (archsaverect1.height / 8), archsaverect1.width, archsaverect1.height / 8), $"GAME STATE: {g}", labelstyle1);

            float cl = Plugin.curse.connected.checked_locations.Count;
            float tl = Convert.ToInt32(Plugin.curse.connected.slot_data["total_loc"]);
            float pl = cl/tl;
            GUI.Label(new Rect(0, 2 * (archsaverect1.height / 8), archsaverect1.width, archsaverect1.height / 8), $"LOCATIONS:", labelstyle1);
            GUI.Label(new Rect(0, 3 * (archsaverect1.height / 8), archsaverect1.width, archsaverect1.height / 8), $"{cl} OUT OF {tl} ({pl:P})", labelstyle1);

            float ci = CursedArchipelagoClient.alist.list.Count;
            float ti = Convert.ToInt32(Plugin.curse.connected.slot_data["total_item"]);
            float pi = ci / ti;
            GUI.Label(new Rect(0, 4 * (archsaverect1.height / 8), archsaverect1.width, archsaverect1.height / 8), $"ITEMS:", labelstyle1);
            GUI.Label(new Rect(0, 5 * (archsaverect1.height / 8), archsaverect1.width, archsaverect1.height / 8), $"{ci} OUT OF {ti} ({pi:P})", labelstyle1);


            if (!gs)
            {
                if (GUI.Button(new Rect(0, 7 * (archsaverect1.height / 8), archsaverect1.width, archsaverect1.height / 8), s, buttonstyle1))
                {
                    SaveFile saveFile = SaveUtils.GetSaveFile(slot);

                    if (Plugin.curse.fullconnection)//&& !saveFile.started
                    {
                        //set some default values
                        saveFile.started = true;
                        saveFile.tutorialComplete = true;
                        saveFile.cellphoneUnlocked = true;
                        saveFile.endingSceneShown = true;
                        saveFile.settingsGender = 1;
                        saveFile.tutorialStep = 10;

                        //if a girl is disabled in logic add their panties to panties turned in to allow for completion goal
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["tiffany_enabled"])) { saveFile.pantiesTurnedIn.Add(277); saveFile.girls[1].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["aiko_enabled"])) { saveFile.pantiesTurnedIn.Add(278); saveFile.girls[2].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["kyanna_enabled"])) { saveFile.pantiesTurnedIn.Add(279); saveFile.girls[3].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["audrey_enabled"])) { saveFile.pantiesTurnedIn.Add(280); saveFile.girls[4].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["lola_enabled"])) { saveFile.pantiesTurnedIn.Add(281); saveFile.girls[5].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["nikki_enabled"])) { saveFile.pantiesTurnedIn.Add(282); saveFile.girls[6].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["jessie_enabled"])) { saveFile.pantiesTurnedIn.Add(283); saveFile.girls[7].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["beli_enabled"])) { saveFile.pantiesTurnedIn.Add(284); saveFile.girls[8].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["kyu_enabled"])) { saveFile.pantiesTurnedIn.Add(285); saveFile.girls[9].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["momo_enabled"])) { saveFile.pantiesTurnedIn.Add(286); saveFile.girls[10].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["celeste_enabled"])) { saveFile.pantiesTurnedIn.Add(287); saveFile.girls[11].gotPanties = true; }
                        if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["venus_enabled"])) { saveFile.pantiesTurnedIn.Add(288); saveFile.girls[12].gotPanties = true; }

                        //set starting girl and starting location to be bedroom
                        saveFile.currentGirl = Convert.ToInt32(Plugin.curse.connected.slot_data["start_girl"]);
                        saveFile.currentLocation = 22;

                        //process archipelago item list for any unlocked girls and set them as been met
                        for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                        {
                            if (CursedArchipelagoClient.alist.list[i].Id > Convert.ToInt32(Plugin.curse.connected.slot_data["girl_unlock_start"]) && CursedArchipelagoClient.alist.list[i].Id <= Convert.ToInt32(Plugin.curse.connected.slot_data["gift_item_start"]))
                            {
                                saveFile.girls[(int)CursedArchipelagoClient.alist.list[i].Id - Convert.ToInt32(Plugin.curse.connected.slot_data["girl_unlock_start"])].metStatus = 3;
                                CursedArchipelagoClient.alist.list[i].processed = true;
                            }
                        }

                        //throw an error if there is no item in the archipelago item list or starting girl setting being null
                        if (CursedArchipelagoClient.alist.list.Count == 0)
                        {
                            ArchipelagoConsole.LogMessage("ERROR IN SETTING UP SAVE DATA(no items recieved) PLEASE RESTART YOUR GAME");
                            return;
                        }
                        if (saveFile.girls[saveFile.currentGirl].metStatus != 3)
                        {
                            ArchipelagoConsole.LogMessage("ERROR IN SETTING UP SAVE DATA(starting girl not unlocked properly) PLEASE RESTART YOUR GAME");
                            return;
                        }

                        using (StreamWriter archfile = File.CreateText(Application.persistentDataPath + $"/archdata{Menu.slot}"))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(archfile, CursedArchipelagoClient.alist);
                        }

                        Plugin.showwindow = false;

                        MethodInfo dynMethod = ts.GetType().GetMethod("OnContinueGame", BindingFlags.NonPublic | BindingFlags.Instance);
                        dynMethod.Invoke(ts, new object[] { slot });
                    }
                }
            }
            else
            {
                if (GUI.Button(new Rect(0, 7 * (archsaverect1.height / 8), archsaverect1.width, archsaverect1.height / 8), s, buttonstyle1))
                {
                    Plugin.showwindow = false;

                    MethodInfo dynMethod = ts.GetType().GetMethod("OnContinueGame", BindingFlags.NonPublic | BindingFlags.Instance);
                    dynMethod.Invoke(ts, new object[] { slot });
                }

            }


            
        }







        /// <summary>
        /// checks for continuing a game
        /// </summary>
        [HarmonyPatch(typeof(LoadScreenSaveFile), "Refresh")]
        [HarmonyPrefix]
        public static bool loadscreenoverrite(LoadScreenSaveFile __instance, ref int ____saveFileIndex, ref SaveFile ____saveFile, ref LoadScreen ____loadScreenRef, ref SpriteObject ____eraseGameButton, ref SpriteObject ____continueGameButton)
        {
            __instance.gameObj.SetActive(false);

            return false;
        }

        /// <summary>
        /// overwite the talk button so it says "Dont Just Stare" instead of "+XX Hnnie"
        /// </summary>
        [HarmonyPatch(typeof(ActionMenuButton), "Refresh")]
        [HarmonyILManipulator]
        public static void talkedit(ILContext ctx, MethodBase orig)
        {
            bool plus = true;
            bool fifty = true;
            bool hunie = true;
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (plus && ctx.Instrs[i].OpCode == OpCodes.Ldstr && ctx.Instrs[i].Operand.ToString() == "+") { ctx.Instrs[i].Operand = ""; plus = false; continue; }
                if (hunie && ctx.Instrs[i].OpCode == OpCodes.Ldstr && ctx.Instrs[i].Operand.ToString() == " Hunie") { ctx.Instrs[i].Operand = ""; hunie = false; break; }
                if (fifty && ctx.Instrs[i].OpCode == OpCodes.Ldc_R4 && ctx.Instrs[i].Operand.ToString() == "50") { ctx.Instrs[i].OpCode = OpCodes.Ldstr; ctx.Instrs[i].Operand = "Dont Just Stare"; fifty = false; continue; }
                if (!plus)
                {
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                }
            }
        }


        [HarmonyPatch(typeof(TitleScreen), "OnTitleScreenClicked")]
        [HarmonyPrefix]
        public static void test0(TitleScreen __instance, ref LoadScreen ____loadScreen)
        {
            ts = ____loadScreen;
            Plugin.showwindow = true;
        }

        [HarmonyPatch(typeof(LoadScreen), "OnCreditsScreenClosed")]
        [HarmonyPatch(typeof(LoadScreen), "OnPhotoGalleryClosed")]
        [HarmonyPatch(typeof(LoadScreen), "OnSettingsCancelButtonPressed")]
        [HarmonyPrefix]
        public static void test()
        {
            Plugin.showwindow = true;
        }

        [HarmonyPatch(typeof(LoadScreen), "OnCreditsButtonPressed")]
        [HarmonyPatch(typeof(LoadScreen), "OnGalleryButtonPressed")]
        [HarmonyPatch(typeof(LoadScreen), "OnSettingsButtonPressed")]
        [HarmonyPrefix]
        public static void test2()
        {
            Plugin.showwindow = false;
        }

        [HarmonyPatch(typeof(LoadScreen), "OnEraseButtonPressed")]
        [HarmonyPatch(typeof(LoadScreen), "OnCancelButtonPressed")]
        [HarmonyPostfix]
        public static void test3(LoadScreen __instance)
        {
            erase = __instance.eraseMode;
        }


        [HarmonyPatch(typeof(LoadScreen), "Refresh")]
        [HarmonyPostfix]
        public static void test4(LoadScreen __instance, ref SpriteObject ____eraseButton, ref SpriteObject ____cancelButton)
        {
            if (Plugin.curse.fullconnection)
            {
                ____eraseButton.gameObj.SetActive(false);
                ____cancelButton.gameObj.SetActive(false);
                __instance.buttonContainer.localX = 600f;
            }
        }
    }
}
