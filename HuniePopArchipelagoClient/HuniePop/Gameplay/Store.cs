using Boomlagoon.JSON;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Store
    {
        /// <summary>
        /// when buying an item in the shopif its an archipelago location sent it otherwise if its an item make sure it dosent get put back in the shop
        /// </summary>
        [HarmonyPatch(typeof(StoreCellApp), "OnStoreItemSlotPressed")]
        [HarmonyPrefix]
        public static bool storepurchase(StoreItemSlot storeItemSlot, StoreCellApp __instance, ref int ____currentStoreTab)
        {
            if (____currentStoreTab == 1)
            {
                if (storeItemSlot.itemDefinition.id > Convert.ToInt32(Plugin.curse.connected.slot_data["shop_loc_start"]))
                {
                    ArchipelagoConsole.LogMessage($"PURCHASED ITEM:{storeItemSlot.itemDefinition.name}");
                    ArchipelagoConsole.LogMessage($"SENDING LOCATION {storeItemSlot.itemDefinition.id}");
                    Plugin.curse.sendLoc(storeItemSlot.itemDefinition.id);
                    return true;
                }
                //long a = IDs.itemidtoarchid(storeItemSlot.itemDefinition.id);
                //for (int i = 0; i < CursedArchipelagoClient.alist.list.Count; i++)
                //{
                //    if (CursedArchipelagoClient.alist.list[i].Id == a && CursedArchipelagoClient.alist.list[i].putinshop)
                //    {
                //        CursedArchipelagoClient.alist.list[i].putinshop = false;
                //        return true;
                //    }
                //}
            }
            return true;
        }

        /// <summary>
        /// populate the store with stuff we want
        /// </summary>
        [HarmonyPatch(typeof(PlayerManager), "RollNewStoreList")]
        [HarmonyPrefix]
        public static bool storeoverite(StoreItemPlayerData[] storeList, ItemType itemType, PlayerManager __instance)
        {
            //populate the gift tab of the shop
            if (itemType == ItemType.GIFT)
            {
                //get all gift items that have been recieved by the client
                List<ItemDefinition> p0 = new List<ItemDefinition>();
                List<ItemDefinition> p1 = new List<ItemDefinition>();
                foreach(int id in IDs.giftids)
                {
                    if (CursedArchipelagoClient.alist.hasitem(IDs.giftidtooffset(id) + Convert.ToInt32(Plugin.curse.connected.slot_data["gift_item_start"])))
                    {
                        bool pri = false;
                        foreach(var g in __instance.girls)
                        {
                            if (g.metStatus == GirlMetStatus.MET)
                            {
                                foreach (var c in g.GetGirlDefinition().collection)
                                {
                                    if (c.id == id)
                                    {
                                        if (!g.IsItemInCollection(GameManager.Data.Items.Get(id)))
                                        {
                                            p1.Add(GameManager.Data.Items.Get(id));
                                            pri = true;
                                        }
                                        break;
                                    }
                                }
                                if (pri) break;
                            }
                        }
                        if (!pri) p1.Add(GameManager.Data.Items.Get(id));
                    }
                }

                ListUtils.Shuffle<ItemDefinition>(p0);
                ListUtils.Shuffle<ItemDefinition>(p1);
                List<ItemDefinition> pend = new List<ItemDefinition>();

                if ((p0.Count + p1.Count) <= 12)
                {
                    pend.AddRange(p0);
                    pend.AddRange(p1);
                }
                else
                {
                    for (int i = 0; i<12; i++)
                    {
                        if (p0.Count > 0 && (i < 6 || p1.Count==0))
                        {
                            pend.Add(p0.Pop());
                        }
                        else
                        {
                            pend.Add(p1.Pop());
                        }
                    }
                }
                
                ListUtils.Shuffle<ItemDefinition>(pend);

                for (int l = 0; l < 12; l++)
                {
                    if (pend.Count > l)
                    {
                        storeList[l].itemDefinition = pend[l];
                        storeList[l].soldOut = false;
                    }
                    else
                    {
                        storeList[l].itemDefinition = null;
                        storeList[l].soldOut = true;
                    }
                }

                return false;
            }
            //populate the Unique Gift tab for the shop
            else if (itemType == ItemType.UNIQUE_GIFT)
            {

                List<ItemDefinition> p = new List<ItemDefinition>();

                //get all archipelago shop locations to put in the store
                int shopslots = Convert.ToInt32(Plugin.curse.connected.slot_data["number_of_shop_items"]);
                int[] pre = [293, 294, 295, 296, 297, 298];
                for (int j = 0; j < shopslots; j++)
                {
                    if (Plugin.curse.connected.checked_locations != null)
                    {
                        if (Plugin.curse.connected.checked_locations.Contains(Convert.ToInt32(Plugin.curse.connected.slot_data["shop_loc_start"]) + j + 1)) { continue; }
                    }
                    ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
                    item.type = ItemType.PRESENT;
                    item.hidden = false;
                    item.iconName = GameManager.Data.Items.Get(pre[UnityEngine.Random.Range(0, pre.Count() - 1)]).iconName;

                    item.name = "ARCH ITEM:" + (j + 1).ToString();
                    item.id = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_loc_start"]) + j + 1;
                    item.description = "LOCATION CHECK FOR ARCHIPELAGO WILL BE REMOVED FROM INVENTORY WHEN MOVING LOCATIONS";
                    p.Add(item);
                }

                //randomise and populate the store slots with items
                ListUtils.Shuffle<ItemDefinition>(p);
                if (p.Count > 12)
                {
                    p.RemoveRange(12, p.Count - 12);
                }
                for (int l = 0; l < 12; l++)
                {
                    if (p.Count > l)
                    {
                        storeList[l].itemDefinition = p[l];
                        storeList[l].soldOut = false;
                    }
                    else
                    {
                        storeList[l].itemDefinition = null;
                        storeList[l].soldOut = true;
                    }
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// set store cost for items what we want them to be
        /// </summary>
        [HarmonyPatch(typeof(ItemDefinition), "storeCost", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool itemcost(ref int __result, ItemDefinition __instance)
        {
            if (__instance.type == ItemType.PRESENT)
            {
                __result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_item_cost"]);
                return false;
            }
            else if (__instance.type == ItemType.GIFT)
            {
                __result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_gift_cost"]);
                return false;
            }
            else if (__instance.type == ItemType.UNIQUE_GIFT)
            {
                __result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_gift_cost"]);
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(StoreItem), "PopulateItem")]
        [HarmonyPostfix]
        public static void itemcost(StoreItemPlayerData storeItemPlayerData, StoreItem __instance)
        {

            if (storeItemPlayerData.soldOut) return;

            bool fav = false;
            bool curfav = false;
            bool mg = false;
            if ((storeItemPlayerData.itemDefinition.type == ItemType.GIFT || storeItemPlayerData.itemDefinition.type == ItemType.UNIQUE_GIFT))
            {
                foreach (var g in GameManager.System.Player.girls)
                {
                    if (GameManager.System.Location.currentGirl.id == g.GetGirlDefinition().id) { mg = true; }
                    else if (fav) { continue; }

                    if (g.metStatus == GirlMetStatus.MET)
                    {
                        foreach (var c in g.GetGirlDefinition().collection)
                        {
                            if (c.id == storeItemPlayerData.itemDefinition.id)
                            {
                                if (!g.IsItemInCollection(storeItemPlayerData.itemDefinition))
                                {
                                    if (GameManager.System.Location.currentGirl.id == g.GetGirlDefinition().id) curfav = true;
                                    fav = true;
                                }
                                break;
                            }
                        }
                        if (curfav || (mg && fav)) break;
                    }
                }
            }

            if (curfav)
            {
                __instance.starIcon.SetAlpha(1f, 0f);
                __instance.starIcon.sprite.SetSprite("cell_app_shop_heart");
            }
            else if (fav)
            {
                __instance.starIcon.SetAlpha(1f, 0f);
                __instance.starIcon.sprite.SetSprite("cell_app_shop_star");
            }
            else
            {
                __instance.starIcon.SetAlpha(0f, 0f);
            }
        }

    }
}
