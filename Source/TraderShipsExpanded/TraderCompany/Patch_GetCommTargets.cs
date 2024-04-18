using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TraderShipsExpanded
{
    [HarmonyPatch(typeof(Building_CommsConsole), "GetCommTargets")]
    public static class Building_CommsConsole_Patch
    {
        [HarmonyPostfix]
        public static void GetCommTargetsPatch(ref IEnumerable<ICommunicable> __result)
        {
            foreach (var ship in Find.CurrentMap.GetComponent<TSEOrbitalCompanyManager>().Companies)
            {
                __result = __result.AddItem(ship);
            }
        }
    }
}
