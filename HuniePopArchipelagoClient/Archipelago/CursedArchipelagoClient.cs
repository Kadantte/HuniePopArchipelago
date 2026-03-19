using HuniePopArchiepelagoClient.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HuniePopArchiepelagoClient.Archipelago
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void callback(string msg);

    public class CursedArchipelagoClient
    {

        public RoomInfoPacket room;
        public DataPackagePacket data;
        public ConnectedPacket connected;
        public NetworkVersion worldver;

        public static Dictionary<int, GirlDefinition> girldata;

        public static Dictionary<int, int> giftshopcost;
        public static Dictionary<int, int> gifthunniecost;
        public static Dictionary<string, int> archshopcost;
        public static Dictionary<int, int> dateshopcost;

        public static int deathlink = -1;
        public static List<BouncedPacket> deathlinkdeaths = [];

        public string seed = "";
        public string error = null;

        public const string APVersion = "0.5.0";
        private const string Game = "Hunie Pop";

        public string url;
        public string username;
        public string password;

        public bool recievedroominfo = false;
        public bool sendroomdatapackage = false;
        public bool recievedroomdatapackage = false;
        public bool processedroomdatapackage = false;
        public bool startprocessedroomdatapackage = false;
        public bool processeddatapackage = false;
        public bool sentconnectedpacket = false;
        public bool recievedconnectedpacket = false;
        public bool fullconnection = false;

        public static ArchipelageItemList alist = new ArchipelageItemList();

        public IntPtr ws;

        public void setup(string h, string u, string p)
        {
            url = h.Trim();
            username = u.Trim();
            password = p;

            if (url != "ws://localhost:38281")
            {
                if (!url.StartsWith("wss://") && !url.StartsWith("ws://"))
                {
                    url = "wss://" + url;
                }

                if (!url.Substring(url.IndexOf("://", StringComparison.Ordinal) + 3).Contains(":"))
                {
                    url = url + ":38281";
                }

                if (url.EndsWith(":"))
                    url += 38281;
            }

            ws = helper.getWS();
            helper.seturlWS(ws, url);
            helper.setcallWS(ws);

        }

        public void connect()
        {
            if (ws == null || url == null || username == null) { return; }
            if (helper.readyWS(ws) == 3) { return; }
            if (!url.StartsWith("ws://") && !url.StartsWith("wss://")) { return; }
            helper.startWS(ws);
        }

        public void code(string msg)
        {
            ArchipelagoConsole.LogMessage($"CODE ENTERED: {msg}");
            if (msg == "$archdata")
            {
                ArchipelagoConsole.LogMessage("-------DEBUG DATA-------");
                ArchipelagoConsole.LogMessage("List size: " + alist.list.Count.ToString());
                ArchipelagoConsole.LogMessage("------------------------");
                for (int i = 0; i < alist.list.Count; i++)
                {
                    ArchipelagoConsole.LogMessage("#" + i.ToString());
                    ArchipelagoConsole.LogMessage("ID:" + alist.list[i].Id.ToString());
                    ArchipelagoConsole.LogMessage("NAME:" + alist.list[i].itemname);
                    ArchipelagoConsole.LogMessage("LOCID:" + alist.list[i].LocationId.ToString());
                    ArchipelagoConsole.LogMessage("LOCNAME:" + alist.list[i].locationname);
                    ArchipelagoConsole.LogMessage("PROCESSED:" + alist.list[i].processed.ToString());
                    ArchipelagoConsole.LogMessage("PUTINSHOP:" + alist.list[i].putinshop.ToString());
                    ArchipelagoConsole.LogMessage("------------------------");
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
            if (msg == "$resetitems")
            {
                CursedArchipelagoClient.alist = new ArchipelageItemList();
                sendJson("{\"cmd\":\"Sync\"}");
                ArchipelagoConsole.LogMessage("Items reset please move locations to re-process them");
            }
            if (msg == "$sync")
            {
                sendJson("{\"cmd\":\"Sync\"}");
            }
            if (msg == "$resync")
            {
                sendJson("{\"cmd\":\"Sync\"}");
                resync();
            }
            if (msg == "$deldata")
            {
                File.Delete(Application.persistentDataPath + "/archdata");
            }
            if (msg == "$goal")
            {
                SaveFile saveFile = SaveUtils.GetSaveFile(Menu.slot);
                PlayerManager player = GameManager.System.Player;
                if (saveFile == null || !saveFile.started || GameManager.System.GameState == GameState.LOADING || GameManager.System.GameState == GameState.TITLE) { return; }

                int offset = 0;
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["tiffany_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["aiko_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["kyanna_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["audrey_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["lola_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["nikki_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["jessie_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["beli_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["kyu_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["momo_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["celeste_enabled"])) { offset++; }
                if (!Convert.ToBoolean(Plugin.curse.connected.slot_data["venus_enabled"])) { offset++; }

                if (Convert.ToBoolean(Plugin.curse.connected.slot_data["goal"]))
                {
                    int p = 0;
                    string s = "";
                    ArchipelagoConsole.LogMessage("Your goal is to give kyu panties");

                    for (int i = 1; i < 13; i++)
                    {
                        if (player.pantiesTurnedIn.Count() == 0 || !player.pantiesTurnedIn.Contains(i + 276))
                        {
                            if (s != "") { s += ", "; }
                            s += GameManager.Data.Girls.Get(i).firstName;
                        }
                        else
                        {
                            p++;
                        }
                    }

                    ArchipelagoConsole.LogMessage($"Number of panties turned in: {p - offset}");
                    ArchipelagoConsole.LogMessage($"Girls panties left to give to kyu: {s}");

                }
                else
                {
                    int p = 0;
                    string s = "";
                    ArchipelagoConsole.LogMessage("Your goal is to have sex with all the girls");

                    foreach (GirlPlayerData girl in player.girls)
                    {
                        if (!girl.gotPanties)
                        {
                            if (s != "") { s += ", "; }
                            s += girl.GetGirlDefinition().firstName;
                        }
                        else
                        {
                            p++;
                        }
                    }

                    ArchipelagoConsole.LogMessage($"Number of girls that you have had sex with: {p - offset}");
                    ArchipelagoConsole.LogMessage($"Girls you still need to have sex with: {s}");
                }
            }
            if (msg == "$test")
            {
                ArchipelagoConsole.LogMessage($"Your gamestate is {GameManager.System.GameState}");
            }


            //pulling game data
            if (msg == "$girls")
            {
                List<GirlDefinition> l = GameManager.Data.Girls.GetAll();
                ArchipelagoConsole.LogMessage("-------GIRL DATA-------");
                foreach (GirlDefinition g in l)
                {
                    ArchipelagoConsole.LogMessage("GIRL ID:" + g.id + " | GIRL NAME:" + g.name);
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
            if (msg == "$items")
            {
                List<ItemDefinition> list = GameManager.Data.Items.GetAllOfType(ItemType.FOOD, false);
                List<ItemDefinition> list2 = GameManager.Data.Items.GetAllOfType(ItemType.DRINK, false);
                List<ItemDefinition> list3 = GameManager.Data.Items.GetAllOfType(ItemType.GIFT, false);
                List<ItemDefinition> list4 = GameManager.Data.Items.GetAllOfType(ItemType.UNIQUE_GIFT, false);
                List<ItemDefinition> list5 = GameManager.Data.Items.GetAllOfType(ItemType.DATE_GIFT, false);
                List<ItemDefinition> list6 = GameManager.Data.Items.GetAllOfType(ItemType.ACCESSORY, false);
                List<ItemDefinition> list7 = GameManager.Data.Items.GetAllOfType(ItemType.PANTIES, false);
                List<ItemDefinition> list8 = GameManager.Data.Items.GetAllOfType(ItemType.PRESENT, false);
                List<ItemDefinition> list9 = GameManager.Data.Items.GetAllOfType(ItemType.MISC, false);
                ArchipelagoConsole.LogMessage("-------FOOD DATA-------");
                foreach (ItemDefinition i in list)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------DRINK DATA-------");
                foreach (ItemDefinition i in list2)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------GIFT DATA-------");
                foreach (ItemDefinition i in list3)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------UNIQUE_GIFT DATA-------");
                foreach (ItemDefinition i in list4)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------DATE_GIFT DATA-------");
                foreach (ItemDefinition i in list5)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------ACCESSORY DATA-------");
                foreach (ItemDefinition i in list6)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------PANTIES DATA-------");
                foreach (ItemDefinition i in list7)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------PRESENT DATA-------");
                foreach (ItemDefinition i in list8)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------MISC DATA-------");
                foreach (ItemDefinition i in list9)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
            if (msg == "$location")
            {
                List<LocationDefinition> nor = GameManager.Data.Locations.GetLocationsByType(LocationType.NORMAL);
                List<LocationDefinition> date = GameManager.Data.Locations.GetLocationsByType(LocationType.DATE);

                ArchipelagoConsole.LogMessage("-------LOCATION DATA-------");
                ArchipelagoConsole.LogMessage("-------NORMAL LOCATION DATA-------");
                foreach (LocationDefinition l1 in nor)
                {
                    ArchipelagoConsole.LogMessage($"LOC ID: {l1.id} | NAME: {l1.name} | {l1.fullName}");
                }
                ArchipelagoConsole.LogMessage("-------END NORMAL LOCATION DATA-------");
                ArchipelagoConsole.LogMessage("-------DATE LOCATION DATA-------");
                foreach (LocationDefinition l2 in date)
                {
                    ArchipelagoConsole.LogMessage($"LOC ID: {l2.id} | NAME: {l2.name} | {l2.fullName}");
                }
                ArchipelagoConsole.LogMessage("-------END DATE LOCATION DATA-------");
            }
            if (msg == "$msg")
            {
                int i = 0;
                bool c = true;
                MessageDefinition data = GameManager.Data.Messages.Get(i);
                ArchipelagoConsole.LogMessage("-------MESSAGE DATA-------");
                while (c)
                {
                    data = GameManager.Data.Messages.Get(i);
                    if (data != null)
                    {
                        ArchipelagoConsole.LogMessage($"MSG ID: {data.id} | sender: {data.sender.name} | text: {data.messageText}");
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage($"MSG ID: {i} | NULL DATA");
                        if (i > 43) { c = false; }
                    }
                    i++;
                }
                ArchipelagoConsole.LogMessage("-------END MESSAGE DATA-------");
            }
        }

        public void sendConnectPacket()
        {
            if (helper.readyWS(ws) != 3) { return; }
            string pack = "{\"cmd\":\"Connect\",\"game\":\"" + Game + "\",\"name\":\"" + username + "\",\"password\":\"" + password + "\",\"uuid\":\"" + Guid.NewGuid().ToString() + "\",\"version\":{\"major\":0,\"minor\":5,\"build\":1,\"class\":\"Version\"},\"tags\":[\"AP\"],\"items_handling\":7,\"slot_data\":true}";
            helper.sendWS(ws, pack);
        }

        public void sendGetPackage()
        {
            if (helper.readyWS(ws) != 3) { return; }
            string games = "";
            for (int i = 0; i < room.games.Count(); i++)
            {
                if (i == 0) { games = "\"" + room.games[i] + "\""; continue; }
                games += ",\"" + room.games[i] + "\"";
            }
            helper.sendWS(ws, "{\"cmd\":\"GetDataPackage\", \"games\":[" + games + "]}");
        }

        public void sendJson(string json)
        {
            helper.sendWS(ws, json);
        }

        public void sendLoc(int loc)
        {
            if (!this.connected.checked_locations.Contains(loc)) { this.connected.checked_locations.Add(loc); }
            helper.sendWS(ws, "{\"cmd\":\"LocationChecks\",\"locations\":[" + loc + "]}");
        }

        public void sendLoc(string set, int offset)
        {
            Plugin.BepinLogger.LogMessage($"SENDING LOC:\nSET:{set}, OFFSET:{offset}");
            sendLoc(Convert.ToInt32(Plugin.curse.connected.slot_data[set]) + offset);
        }

        public void sendCompletion()
        {
            helper.sendWS(ws, "{\"cmd\":\"StatusUpdate\",\"status\":" + 30 + "}");
        }

        public void sendSay(string msg)
        {
            if (msg.StartsWith("$")) { code(msg); return; }
            helper.sendWS(ws, "{\"cmd\":\"Say\",\"text\":\"" + msg + "\"}");
        }

        public void setupdata()
        {
            alist.seed = room.seed_name;

            if (File.Exists(Application.persistentDataPath + $"/archdata{Menu.slot}"))
            {
                using (StreamReader file = File.OpenText(Application.persistentDataPath + $"/archdata{Menu.slot}"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ArchipelageItemList savedlist = (ArchipelageItemList)serializer.Deserialize(file, typeof(ArchipelageItemList));
                    if (savedlist.listversion != 2)
                    {
                        ArchipelagoConsole.LogMessage($"ARCHDATA version missmatch({savedlist.listversion}!=2) using new archdata");
                    }
                    else if (savedlist.seed != alist.seed)
                    {
                        ArchipelagoConsole.LogMessage($"ARCHDATA seed missmatch(archdata:{savedlist.seed}!=server:{alist.seed}) using new archdata");
                    }
                    else if (alist.seed == savedlist.seed && savedlist.listversion == 2)
                    {
                        ArchipelagoConsole.LogMessage("archdata file found restoring session");
                        if (alist.merge(savedlist.list))
                        {
                            ArchipelagoConsole.LogMessage("ERROR LOADING SAVED ARCHDATA RESETING ARCHDATA");
                            alist = new ArchipelageItemList();
                            alist.seed = room.seed_name;
                            alist.host = url;
                            alist.user = username;
                            alist.pass = password;
                            sendJson("{\"cmd\":\"Sync\"}");
                        }
                        else
                        {
                            alist.host = savedlist.host;
                            alist.user = savedlist.user;
                            alist.pass = savedlist.pass;
                            alist.seed = savedlist.seed;
                        }
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage("ARCHDATA found but an error occoured just going to use new archdata");
                        alist.seed = room.seed_name;
                        alist.host = url;
                        alist.user = username;
                        alist.pass = password;
                    }
                }
            }
            else
            {
                ArchipelagoConsole.LogMessage("archdata file not found creating new session");
                alist.seed = room.seed_name;
                alist.host = url;
                alist.user = username;
                alist.pass = password;
            }
            ArchipelagoConsole.CommandText = "!help";
        }

        public static void msgCallback(string msg)
        {
            if (!msg.StartsWith("{") && !msg.StartsWith("["))
            {
                ArchipelagoConsole.LogImportant(msg);
                Plugin.curse.error = msg;
                return;
            }

            string cmd = "";
            JObject msgjson = JObject.Parse(msg);
            if (msgjson.ContainsKey("cmd"))
            {
                cmd = (string)msgjson["cmd"];
            }

            //ArchipelagoConsole.LogMessage("MESSAGE GOTTEN\n" + msg);
            if (cmd == "RoomInfo")
            {
                Plugin.BepinLogger.LogMessage("RoomInfo PACKET GOTTEN");
                Plugin.BepinLogger.LogMessage(msg);
                Plugin.curse.room = JsonConvert.DeserializeObject<RoomInfoPacket>(msg);
                Plugin.curse.recievedroominfo = true;
            }
            else if (cmd == "ConnectionRefused")
            {
                ArchipelagoConsole.LogMessage("ConnectionRefused PACKET GOTTEN");
                ArchipelagoConsole.LogImportant(msg);

            }
            else if (cmd == "Connected")
            {
                Plugin.BepinLogger.LogMessage("Connected PACKET GOTTEN");
                Plugin.BepinLogger.LogMessage(msg);
                Plugin.curse.connected = JsonConvert.DeserializeObject<ConnectedPacket>(msg);
                List<int> wv = JsonConvert.DeserializeObject<List<int>>(msgjson["slot_data"]["world_version"].ToString());
                Plugin.curse.worldver = NetworkVersion.gen(wv);
                if (Plugin.curse.worldver.major != Plugin.compatworldmajor || Plugin.curse.worldver.minor != Plugin.compatworldminor || Plugin.curse.worldver.build != Plugin.compatworldbuild)
                {

                    ArchipelagoConsole.LogMessage("WARNING EXPECTED APWORLD VERSION MISSMATCH");
                    ArchipelagoConsole.LogImportant($"EXPECTED V{Plugin.compatworldmajor}.{Plugin.compatworldminor}.{Plugin.compatworldbuild} GOT V{Plugin.curse.worldver.major}.{Plugin.curse.worldver.minor}.{Plugin.curse.worldver.build}");
                }

                setupgirldata();

                giftshopcost = JsonConvert.DeserializeObject<Dictionary<int, int> >(msgjson["slot_data"]["shop_gift_cost"].ToString());
                gifthunniecost = JsonConvert.DeserializeObject<Dictionary<int, int> >(msgjson["slot_data"]["hunie_gift_cost"].ToString());
                archshopcost = JsonConvert.DeserializeObject<Dictionary<string, int> >(msgjson["slot_data"]["shop_item_cost"].ToString());
                dateshopcost = JsonConvert.DeserializeObject<Dictionary<int, int>>(msgjson["slot_data"]["shop_date_gift_cost"].ToString());

                if (Convert.ToInt32(msgjson["slot_data"]["deathlink"].ToString())!=0)
                {
                    Plugin.BepinLogger.LogMessage($"DEATHLINK ENABLED ({Convert.ToInt32(msgjson["slot_data"]["deathlink"].ToString())}) SENDING UPDATE PACKET");
                    deathlink = Convert.ToInt32(msgjson["slot_data"]["deathlink"].ToString());
                    helper.sendWS(Plugin.curse.ws, $"{{\"cmd\":\"ConnectUpdate\",\"items_handling\":7,\"tags\":[\"AP\",\"DeathLink\"]}}");
                }
                else
                {
                    Plugin.BepinLogger.LogMessage("DEATHLINK DISABLED DOING NOTHING");
                }

                Plugin.curse.setupdata();
                Plugin.curse.recievedconnectedpacket = true;

            }
            else if (cmd == "ReceivedItems")
            {
                //ArchipelagoConsole.LogMessage("ReceivedItems PACKET GOTTEN");
                Plugin.BepinLogger.LogMessage("itempacketmsg:\n" + msg);
                if (!Plugin.curse.fullconnection)
                {
                    Plugin.curse.fullconnection = true;
                }
                ReceivedItemsPacket pack = JsonConvert.DeserializeObject<ReceivedItemsPacket>(msg);
                Plugin.BepinLogger.LogMessage("adding items");
                foreach (NetworkItem item in pack.items)
                {
                    try
                    {
                        alist.add(item);
                    }
                    catch (Exception)
                    {
                        ArchipelagoConsole.LogImportant($"EXCEPTION RECIEVING ITEM:(id:{item.item}, location:{item.location}, player:{item.player}, flags:{item.flags}) please send details to dev for fixing");
                    }
                }

            }
            else if (cmd == "LocationInfo")
            {
                ArchipelagoConsole.LogMessage("LocationInfo PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(msg);

            }
            else if (cmd == "RoomUpdate")
            {
                //ArchipelagoConsole.LogMessage("RoomUpdate PACKET GOTTEN");
                //ArchipelagoConsole.LogMessage(msg);
                JsonConvert.DeserializeObject<RoomUpdatePacket>(msg).update();

            }
            else if (cmd == "PrintJSON")
            {
                //ArchipelagoConsole.LogMessage("PrintJSON PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(JsonConvert.DeserializeObject<PrintJSONPacket>(msg).print());

            }
            else if (cmd == "DataPackage")
            {
                Plugin.BepinLogger.LogMessage("DataPackage PACKET GOTTEN");
                Plugin.BepinLogger.LogMessage(msg);
                Plugin.curse.data = JsonConvert.DeserializeObject<DataPackagePacket>(msg);
                //ArchipelagoConsole.LogMessage(Plugin.curse.data.data.games.ToString());
                Plugin.curse.recievedroomdatapackage = true;

            }
            else if (cmd == "Bounced")
            {
                Plugin.BepinLogger.LogMessage("Bounced PACKET GOTTEN");
                Plugin.BepinLogger.LogMessage(msg);
                BouncedPacket p = JsonConvert.DeserializeObject<BouncedPacket>(msg);
                if (p.tags.Count > 0 && p.tags.Contains("DeathLink"))
                {
                    Plugin.BepinLogger.LogMessage("DEATHLINK PACKET GOTTEN");
                    if (deathlink > 1 && (GameManager.System.GameState == GameState.PUZZLE || deathlink == 3))
                    {
                        if (p.data.ContainsKey("source") && p.data["source"] == Plugin.curse.username)
                        {
                            Plugin.BepinLogger.LogMessage("DEATHLINK PACKET SKIPPED (SAME SOURCE)");
                        }
                        else
                        {
                            Plugin.BepinLogger.LogMessage("ADDED DEATHLINK PACKET TO LIST");
                            deathlinkdeaths.Add(p);
                        }
                    }
                }
                else
                {
                    ArchipelagoConsole.LogMessage("Unknown Bounced PACKET GOTTEN");
                    ArchipelagoConsole.LogMessage(msg);
                }

            }
            else if (cmd == "InvalidPacket")
            {
                ArchipelagoConsole.LogMessage("InvalidPacket PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(msg);

            }
            else if (cmd == "Retrieved")
            {
                ArchipelagoConsole.LogMessage("Retrieved PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(msg);

            }
            else if (cmd == "SetReply")
            {
                ArchipelagoConsole.LogMessage("SetReply PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(msg);

            }
            else
            {
                ArchipelagoConsole.LogMessage("---MESSAGE ERROR PRINTING MESSAGE---");
                ArchipelagoConsole.LogMessage($"{msg}");
            }

        }

        public void resync()
        {
            SaveFile saveFile = SaveUtils.GetSaveFile(3);
            if (saveFile == null || !saveFile.started) { return; }
            ArchipelagoConsole.LogMessage($"re-sending all locations");

            for (int i = 0; i < 13; i++)
            {
                if (!saveFile.girls.ContainsKey(i)) { continue; }

                GirlSaveData girl = saveFile.girls[i];
                if (girl == null || girl.metStatus != 3) { continue; }

                GirlDefinition girldef = GameManager.Data.Girls.Get(i);
                ArchipelagoConsole.LogMessage($"re-sending all {girldef.firstName.ToLower()}'s locations aquired");
                if (girl.relationshipLevel > 1) { sendLoc($"{girldef.firstName.ToLower()}_date_loc_start", 1); }
                if (girl.relationshipLevel > 2) { sendLoc($"{girldef.firstName.ToLower()}_date_loc_start", 2); }
                if (girl.relationshipLevel > 3) { sendLoc($"{girldef.firstName.ToLower()}_date_loc_start", 3); }
                if (girl.relationshipLevel > 4) { sendLoc($"{girldef.firstName.ToLower()}_date_loc_start", 4); }
                if (girl.gotPanties) { sendLoc("sleep_loc_start", (girldef.id - 1)); }

                foreach (int gift in girl.collection)
                {
                    sendLoc($"{girldef.firstName.ToLower()}_gift_loc_start", IDs.giftidtooffset(gift));
                }

                for (int j = 0; j < 12; j++)
                {
                    if (girl.details[j]) { sendLoc($"{girldef.firstName.ToLower()}_question_loc_start", j); }
                }
            }
            ArchipelagoConsole.LogMessage($"all girl location re-sent");
            ArchipelagoConsole.LogMessage($"re-sending all panties turned in locations");

            for (int i = 0; i < 12; i++)
            {
                if (saveFile.pantiesTurnedIn.Contains(i + 277)) { sendLoc("kyu_panties_loc_start", i); }
            }
            ArchipelagoConsole.LogMessage($"all panties turned in locations re-sent");
            ArchipelagoConsole.LogMessage($"all locations re-sent");
        }

        public static void setupgirldata()
        {
            List<GirlDefinition> olddata = GameManager.Data.Girls.GetAll();
            girldata = new Dictionary<int, GirlDefinition>();

            Dictionary<string, Dictionary<string, int>> data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(Plugin.curse.connected.slot_data["girldata"].ToString());

            foreach (GirlDefinition g in olddata)
            {
                g.mostDesiredTrait = GameManager.Data.Traits.Get(data[g.firstName.ToLower()]["loves"]+1);
                g.leastDesiredTrait = GameManager.Data.Traits.Get(data[g.firstName.ToLower()]["hates"]+1);

                g.collection =
                [
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift1"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift2"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift3"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift4"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift5"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift6"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift7"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift8"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift9"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift10"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift11"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift12"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift13"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift14"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift15"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift16"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift17"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift18"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift19"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift20"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift21"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift22"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift23"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift24"]),
                ];

                g.uniqueGiftList =
                    [
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift19"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift20"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift21"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift22"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift23"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift24"]),

                    ];

                girldata.Add(g.id, g);
            }

        }

    }


    public class helper
    {

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern IntPtr getWS();

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern void startWS(IntPtr ws);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern void seturlWS(IntPtr ws, string url);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern void setcallWS(IntPtr ws);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern void sendWS(IntPtr ws, string msg);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern int readyWS(IntPtr ws);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern int dotsV();

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern bool hasmsg(IntPtr ws);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern IntPtr getmsg(IntPtr ws);


    }
}
