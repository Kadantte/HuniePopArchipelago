using HarmonyLib;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Traits
    {
        /// <summary>
        /// disable buying talent upgrades
        /// </summary>
        [HarmonyPatch(typeof(TraitsCellApp), "OnStoreItemSlotPressed")]
        [HarmonyPrefix]
        public static bool talentoveride()
        {
            return false;
        }

        [HarmonyPatch(typeof(TraitsItem), "Refresh")]
        [HarmonyPostfix]
        public static void traitcostoverride(TraitsItem __instance, PlayerTraitType ____playerTraitType)
        {
            if (GameManager.System.Player.GetTraitLevel(____playerTraitType) < 6)
            {
                __instance.itemCostLabel.SetText($"{GameManager.System.Player.GetTraitLevel(____playerTraitType)}/6");
            }
            __instance.itemSlot.button.Disable();
        }
    }
}
