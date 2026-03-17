using Boomlagoon.JSON;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using System;
using System.Collections;
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
            if (storeItemSlot.itemDefinition.id > Convert.ToInt32(Plugin.curse.connected.slot_data["shop_loc_start"]))
            {
                ArchipelagoConsole.LogMessage($"PURCHASED ITEM:{storeItemSlot.itemDefinition.name}");
                ArchipelagoConsole.LogMessage($"SENDING LOCATION {storeItemSlot.itemDefinition.id}");
                Plugin.curse.sendLoc(storeItemSlot.itemDefinition.id);
                return true;
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
                List<ItemDefinition> list = GameManager.Data.Items.GetAllOfType(ItemType.DATE_GIFT, false);

                ListUtils.Shuffle<ItemDefinition>(list);

                if (list.Count > 12)
                {
                    list.RemoveRange(12, list.Count - 12);
                }
                for (int l = 0; l < 12; l++)
                {
                    if (list.Count > l)
                    {
                        storeList[l].itemDefinition = list[l];
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

                List<ItemDefinition> arch = new List<ItemDefinition>();

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
                    arch.Add(item);
                }

                //get all gift items that have been recieved by the client
                List<ItemDefinition> gifts = new List<ItemDefinition>();
                List<ItemDefinition> prigifts = new List<ItemDefinition>();
                foreach (int id in IDs.giftids)
                {
                    if (CursedArchipelagoClient.alist.hasitem(IDs.giftidtooffset(id) + Convert.ToInt32(Plugin.curse.connected.slot_data["gift_item_start"])))
                    {
                        bool pri = false;
                        foreach (var g in __instance.girls)
                        {
                            if (g.metStatus == GirlMetStatus.MET)
                            {
                                foreach (var c in g.GetGirlDefinition().collection)
                                {
                                    if (c.id == id)
                                    {
                                        if (!g.IsItemInCollection(GameManager.Data.Items.Get(id)))
                                        {
                                            prigifts.Add(GameManager.Data.Items.Get(id));
                                            pri = true;
                                        }
                                        break;
                                    }
                                }
                                if (pri) break;
                            }
                        }
                        if (!pri) prigifts.Add(GameManager.Data.Items.Get(id));
                    }
                }

                ListUtils.Shuffle<ItemDefinition>(arch);
                ListUtils.Shuffle<ItemDefinition>(gifts);
                ListUtils.Shuffle<ItemDefinition>(prigifts);
                List<ItemDefinition> storelist = new List<ItemDefinition>();

                if ((gifts.Count + prigifts.Count + arch.Count) <= 12)
                {
                    storelist.AddRange(gifts);
                    storelist.AddRange(prigifts);
                    storelist.AddRange(arch);
                }
                else
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (prigifts.Count > 0 && (i < 6 || (arch.Count == 0 && gifts.Count == 0)))
                        {
                            storelist.Add(prigifts.Pop());
                        }
                        else if (gifts.Count > 0 && (i < 6 || (arch.Count == 0)))
                        {
                            storelist.Add(gifts.Pop());
                        }
                        else if (arch.Count > 0)
                        {
                            storelist.Add(arch.Pop());
                        }
                    }
                }

                ListUtils.Shuffle<ItemDefinition>(storelist);

                for (int l = 0; l < 12; l++)
                {
                    if (storelist.Count > l)
                    {
                        storeList[l].itemDefinition = storelist[l];
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
                //__result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_item_cost"]);
                int i = __instance.id - Convert.ToInt32(Plugin.curse.connected.slot_data["shop_loc_start"]);
                if (CursedArchipelagoClient.archshopcost.Keys.Contains($"shop{i}")) { __result = CursedArchipelagoClient.archshopcost[$"shop{i}"]; }
                else { __result = 50000; }
                return false;
            }
            else if (__instance.type == ItemType.GIFT)
            {
                //__result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_gift_cost"]);
                if (CursedArchipelagoClient.giftshopcost.Keys.Contains(__instance.id)) { __result = CursedArchipelagoClient.giftshopcost[__instance.id]; }
                else { __result = 50000; }
                return false;
            }
            else if (__instance.type == ItemType.UNIQUE_GIFT)
            {
                //__result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_gift_cost"]);
                if (CursedArchipelagoClient.giftshopcost.Keys.Contains(__instance.id)) { __result = CursedArchipelagoClient.giftshopcost[__instance.id]; }
                else { __result = 50000; }
                return false;
            }
            else if (__instance.type == ItemType.DATE_GIFT)
            {
                //__result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_date_gift_cost"]);
                if (CursedArchipelagoClient.dateshopcost.Keys.Contains(__instance.id)) { __result = CursedArchipelagoClient.dateshopcost[__instance.id]; }
                else { __result = 50000; }
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


        [HarmonyPatch(typeof(ActionMenuButton), "Refresh")]
        [HarmonyPrefix]
        public static bool stroelabel(ActionMenuButton __instance)
        {
            if (__instance.definition.subtitleType == ActionMenuSubtitleType.SHOP_FOR_HER)
            {
                int num3 = 0;
                for (int k = 0; k < GameManager.System.Player.storeUnique.Length; k++)
                {
                    if (!GameManager.System.Player.storeUnique[k].soldOut)
                    {
                        num3++;
                    }
                }
                if (num3 > 0)
                {
                    __instance.actionSubLabel.SetText(num3.ToString() + " in Stock");
                }
                else
                {
                    __instance.actionSubLabel.SetText("Out of Stock");
                }
                return false;
            }
            return true;
        }
    }
}
