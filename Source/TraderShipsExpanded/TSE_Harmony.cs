using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace TraderShipsExpanded
{
    [StaticConstructorOnStartup]
    public class TSE_Harmony
    {
        static TSE_Harmony()
        {
            var h = new Harmony("flangopink.TraderShipsExpanded");

            Harmony.DEBUG = true;
            // what the actual fuck is this
            h.Patch(typeof(Building_OrbitalTradeBeacon).GetNestedTypes(AccessTools.all)
                                                       .SelectMany(AccessTools.GetDeclaredMethods)
                                                       .First(predicate: mi => mi.ReturnType == typeof(bool) 
                                                                            && mi.GetParameters().Length == 1
                                                                            && mi.GetParameters().First().ParameterType == typeof(Region))
                                                       ,
                                                       transpiler: new HarmonyMethod(typeof(Transpiler_TradeableCellsAround).GetMethod("TranspilerGetRange")));


            h.Patch(typeof(Building_OrbitalTradeBeacon).GetMethod("TradeableCellsAround"), transpiler: new HarmonyMethod(typeof(Transpiler_TradeableCellsAround).GetMethod("TranspilerMaxRegions")));
            h.Patch(typeof(Building_OrbitalTradeBeacon).GetMethod("TradeableCellsAround"), transpiler: new HarmonyMethod(typeof(Transpiler_TradeableCellsAround).GetMethod("TranspilerBeaconIgnoreWalls")));
           
            h.PatchAll();     


            // remove this later
            if(ModLister.HasActiveModWithName("Vanilla Vehicles Expanded"))
            {
                Log.Error("");
                Log.Error("<color=#FFC0CB> ==================================================== </color>");
                Log.Error("<color=#FFC0CB> ============== IMPORTANT! PLEASE READ ============== </color>");
                Log.Error("<color=#FFC0CB> ====== IF YOU ARE USING TRADER SHIPS EXPANDED ====== </color>");
                Log.Error("<color=#FFC0CB> ==================================================== </color>");
                Log.Error("<color=#FFC0CB> hi, flangopink here. if you are seeing this, this means you are playing with both Trader Ships Expanded and Vanilla Vehicles Expanded installed. TSE currently doesn't support VVE properly due to VVE not being updated to 1.5 earlier, so please report this error to the dev team or DM me on Discord @flangopink. thanks.\n\n</color>");
                Log.Error("<color=#FFC0CB> TSE still works, you can ignore errors. probably. ¯\\_(ツ)_/¯ </color>");
                Log.Error("<color=#FFC0CB> ==================================================== </color>");
                Log.Error("");
            }
        }
    }
}
