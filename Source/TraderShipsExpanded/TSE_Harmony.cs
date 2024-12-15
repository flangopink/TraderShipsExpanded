using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TraderShipsExpanded
{
    [StaticConstructorOnStartup]
    public class TSE_Harmony
    {
        static TSE_Harmony()
        {
            List<ThingDef> allDefs = DefDatabase<ThingDef>.AllDefsListForReading;
            int count = allDefs.Count;
            for (int i = 0; i < count; i++)
            {
                ThingDef thingDef = allDefs[i];
                if (thingDef.thingClass == typeof(Building_Storage) && thingDef.HasComp<CompHideStoredItems>())//thingDef.HasModExtension<ModExt_HideStoredItems>())
                {
                    TSE_Cache.hiddenStorageDefs.Add(thingDef);
                }
            }

            var h = new Harmony("flangopink.TraderShipsExpanded");

            //Harmony.DEBUG = true;
            // what the actual fuck is this
            h.Patch(typeof(Building_OrbitalTradeBeacon).GetNestedTypes(AccessTools.all)
                                                       .SelectMany(AccessTools.GetDeclaredMethods)
                                                       .First(predicate: mi => mi.ReturnType == typeof(bool) 
                                                                            && mi.GetParameters().Length == 1
                                                                            && mi.GetParameters().First().ParameterType == typeof(Region)),
                                                       transpiler: new HarmonyMethod(typeof(Transpiler_TradeableCellsAround).GetMethod("TranspilerGetRange")));


            h.Patch(typeof(Building_OrbitalTradeBeacon).GetMethod("TradeableCellsAround"), transpiler: new HarmonyMethod(typeof(Transpiler_TradeableCellsAround).GetMethod("TranspilerMaxRegions")));
            h.Patch(typeof(Building_OrbitalTradeBeacon).GetMethod("TradeableCellsAround"), transpiler: new HarmonyMethod(typeof(Transpiler_TradeableCellsAround).GetMethod("TranspilerBeaconIgnoreWalls")));
           
            h.PatchAll();
        }
    }
}
