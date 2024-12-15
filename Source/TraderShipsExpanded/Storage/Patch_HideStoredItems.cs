using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace TraderShipsExpanded
{
    [HarmonyPatch(typeof(Graphic), "Print")]
    public class Patch_HideStoredItems
    {
        [HarmonyPrefix]
        public static bool Graphic_Print_Prefix(SectionLayer layer, Thing thing)
        {
            return Utils.DrawStoredItems(thing, layer.section.map);
        }
    }

    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.DrawGUIOverlay))]
    static class Patch_ThingWithComps_DrawGUIOverlay
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var label = generator.DefineLabel();
            foreach (var instruction in instructions) if (instruction.opcode == OpCodes.Ret) { instruction.labels.Add(label); break; }

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_ThingWithComps_DrawGUIOverlay), nameof(CheckDrawGUIOverlay)));
            yield return new CodeInstruction(OpCodes.Brfalse, label);

            foreach (var instruction in instructions) yield return instruction;
        }
        static bool CheckDrawGUIOverlay(Thing __instance)
        {
            return Utils.DrawStoredItems(__instance, __instance.MapHeld);
        }
    }
}
