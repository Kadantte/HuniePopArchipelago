using HarmonyLib;

namespace HuniePopArchiepelagoClient.HuniePop.Girls
{
    [HarmonyPatch]
    public class Details
    {
        /// <summary>
        /// send an archipelago location for learning a girls detail
        /// </summary>
        [HarmonyPatch(typeof(GirlPlayerData), "KnowDetail")]
        [HarmonyPrefix]
        public static void favlocation(GirlDetailType type, GirlPlayerData __instance)
        {
            Plugin.curse.sendLoc($"{__instance.GetGirlDefinition().firstName.ToLower()}_question_loc_start", (int)type +1);
        }
    }
}
