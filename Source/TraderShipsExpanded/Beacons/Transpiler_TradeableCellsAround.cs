using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace TraderShipsExpanded
{
    //[HotSwap.HotSwappable]
    public static class Transpiler_TradeableCellsAround
    {
        public const float DefaultRange = 7.9f;
        public const int MaxRegions = 64; // DO NOT SET THIS ABOVE 64. IT WILL BREAK THE CELL SCANNING FOR NO REASON AT ALL!

        public static IEnumerable<CodeInstruction> TranspilerGetRange(IEnumerable<CodeInstruction> instructions)
        {
            var posField = typeof(Building_OrbitalTradeBeacon).GetNestedTypes(AccessTools.all)
                           .SelectMany(AccessTools.GetDeclaredFields)
                           .First(predicate: f => f.FieldType == typeof(IntVec3) && f.Name == "pos");

            
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4)
                {
                    var injected = new List<CodeInstruction>()
                    {
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldfld, posField),
                        new(OpCodes.Ldarg_1),
                        new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Region), "Map")),
                        new(OpCodes.Call, AccessTools.Method(typeof(Transpiler_TradeableCellsAround), "GetBeaconRange"))
                    };
                    foreach (CodeInstruction c in injected)
                    {
                        yield return c;
                    }
                    continue; // don't yield 7.9f
                }
                yield return codes[i];
            }
        }        

        public static IEnumerable<CodeInstruction> TranspilerBeaconIgnoreWalls(IEnumerable<CodeInstruction> instructions, ILGenerator ilgen)
        {
            var pos = typeof(Building_OrbitalTradeBeacon).GetNestedTypes(AccessTools.all)
                           .SelectMany(AccessTools.GetDeclaredFields)
                           .First(predicate: f => f.FieldType == typeof(IntVec3) && f.Name == "pos");
            var map = typeof(Building_OrbitalTradeBeacon).GetNestedTypes(AccessTools.all)
                           .SelectMany(AccessTools.GetDeclaredFields)
                           .First(predicate: f => f.FieldType == typeof(Map) && f.Name == "map");

            var codes = new List<CodeInstruction>(instructions);

            var modExt = ilgen.DeclareLocal(typeof(ModExt_CustomTradeBeacon));
            var ignoreWallsBool = ilgen.DeclareLocal(typeof(bool));
            var listOfCells = ilgen.DeclareLocal(typeof(List<IntVec3>));

            Label il_0074 = ilgen.DefineLabel();
            Label il_0075 = ilgen.DefineLabel();
            Label il_008a = ilgen.DefineLabel();
            Label il_00c0 = ilgen.DefineLabel();
            Label il_00c9 = ilgen.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call 
                 && codes[i+1].opcode == OpCodes.Ldsfld 
                 && codes[i+2].opcode == OpCodes.Ret)
                {
                    var injected = new List<CodeInstruction>()
                    {
                        codes[i], // call
                        new(OpCodes.Nop),
                        codes[i+1].WithLabels(il_00c0), // ldsfld
                        new(OpCodes.Stloc, listOfCells.LocalIndex),
                        new(OpCodes.Br_S, il_00c9),
                        new CodeInstruction(OpCodes.Ldloc_S, listOfCells.LocalIndex).WithLabels(il_00c9),
                        codes[i+2] // ret
                    };
                    foreach (CodeInstruction c in injected)
                    {
                        //Log.Warning(c.ToString());
                        yield return c;
                    }
                    if (i + 3 >= codes.Count) break;
                }
                //Log.Message(codes[i]);
                yield return codes[i];
            }
            //Log.Error("-----------------------------------------------");

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_1 && codes[i+1].opcode == OpCodes.Ldsfld && codes[i+2].opcode == OpCodes.Dup)
                {
                    var Ldloc_1_il_008a = new CodeInstruction(OpCodes.Ldloc_1);
                    Ldloc_1_il_008a.labels.Add(il_008a);

                    var injected = new List<CodeInstruction>()
                    {
                        new(OpCodes.Ldloc_0),
                        new(OpCodes.Ldfld, pos),
                        new(OpCodes.Ldarg_1),
                        new(OpCodes.Call, AccessTools.Method(typeof(Transpiler_TradeableCellsAround), "GetModExt")),
                        new(OpCodes.Stloc, modExt.LocalIndex),
                        new(OpCodes.Ldloc, modExt.LocalIndex),
                        new(OpCodes.Brfalse_S, il_0074),

                        new(OpCodes.Ldloc, modExt.LocalIndex),
                        new(OpCodes.Ldfld, AccessTools.Field(typeof(ModExt_CustomTradeBeacon), "ignoreWalls")),
                        new(OpCodes.Br_S, il_0075),
                        
                        new CodeInstruction
                           (OpCodes.Ldc_I4_0).WithLabels(il_0074),
                        
                        new CodeInstruction
                           (OpCodes.Stloc, ignoreWallsBool.LocalIndex).WithLabels(il_0075),
                        new(OpCodes.Ldloc, ignoreWallsBool.LocalIndex),
                        new(OpCodes.Brfalse_S, il_008a),

                        new(OpCodes.Nop),
                        new(OpCodes.Ldloc_0),
                        new(OpCodes.Ldfld, pos),
                        new(OpCodes.Ldarg_1),
                        new(OpCodes.Call, AccessTools.Method(typeof(Transpiler_TradeableCellsAround), "CellsIfIgnoringWalls")),
                        new(OpCodes.Stsfld, AccessTools.Field(typeof(Building_OrbitalTradeBeacon), "tradeableCells")),
                        new(OpCodes.Nop),
                       
                        new(OpCodes.Br_S, il_00c0),
                        Ldloc_1_il_008a
                    };
                    foreach (CodeInstruction c in injected)
                    {
                        //Log.Warning(c.ToString());
                        yield return c;
                    }
                    continue;
                }
                //Log.Message(codes[i]);
                yield return codes[i];
            }
            //Log.Error("-----------------------DONE-----------------------");
        }

        public static IEnumerable<CodeInstruction> TranspilerMaxRegions(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand.ToString().Contains("16"))
                {
                    //Log.Message("MaxRegions: 16 => 64");
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, MaxRegions);
                    continue; // don't yield 16
                }
                yield return codes[i];
            }
        }

        public static float GetBeaconRange(IntVec3 pos, Map map)
        {
            return GetModExt(pos, map)?.range ?? 7.9f;
        }

        public static List<IntVec3> CellsIfIgnoringWalls(IntVec3 pos, Map map)
        {
            List<IntVec3> cells = [];
            float range = GetBeaconRange(pos, map);
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(pos, range, true))
            {
                if (cell.GetEdifice(map)?.def.passability == Traversability.Impassable) continue;
                cells.Add(cell);
                //Log.Message(cell.GetFirstItem(map));
            }
            return cells;
        }

        public static ModExt_CustomTradeBeacon GetModExt(IntVec3 pos, Map map) => pos.GetEdifice(map)?.def.GetModExtension<ModExt_CustomTradeBeacon>();
    }
}
