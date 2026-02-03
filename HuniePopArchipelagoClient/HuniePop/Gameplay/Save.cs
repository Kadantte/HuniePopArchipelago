using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using HuniePopArchiepelagoClient.HuniePop.Girls;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Save
    {

        /// <summary>
        /// change the save file to be HuniePopArchSaveData1.game
        /// </summary>
        [HarmonyPatch(typeof(SaveUtils), "Init")]
        [HarmonyILManipulator]
        public static void talkedit(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldstr)
                {
                    if (ctx.Instrs[i].Operand.ToString() == "/HuniePopSaveData")
                    {
                        ctx.Instrs[i].Operand = "/HuniePopArchSaveData";
                    }
                    if (ctx.Instrs[i].Operand.ToString() == "HuniePopSaveData")
                    {
                        ctx.Instrs[i].Operand = "HuniePopArchSaveData";
                    }
                }
            }
        }


        /// <summary>
        /// make sure that when reseting a file that the inventory is clear of items and starting location/girl is -1
        /// </summary>
        [HarmonyPatch(typeof(SaveFile), "ResetFile")]
        [HarmonyPostfix]
        public static void savereset(SaveFile __instance)
        {
            __instance.currentGirl = -1;
            __instance.currentLocation = -1;

            __instance.inventory = new InventoryItemSaveData[30];
            for (int j = 0; j < __instance.inventory.Length; j++)
            {
                __instance.inventory[j] = new InventoryItemSaveData();
            }
        }


        /// <summary>
        /// make it so every time the same is saved that we save at archdata as well
        /// </summary>
        [HarmonyPatch(typeof(GameManager), "SaveGame")]
        [HarmonyPostfix]
        public static void saveflags()
        {
            using (StreamWriter file = File.CreateText(Application.persistentDataPath + "/archdata"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, CursedArchipelagoClient.alist);
            }
        }

    }
}
