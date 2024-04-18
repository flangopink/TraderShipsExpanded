using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TraderShipsExpanded
{
    //[HotSwap.HotSwappable]
    public class PlaceWorker_ShowCustomTradeBeaconRadius : PlaceWorker
    {
        private readonly static List<IntVec3> cells = [];

        private static ModExt_CustomTradeBeacon modExt;

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            GenDraw.DrawFieldEdges(GetCells(center, currentMap, def));
        }

        public static List<IntVec3> GetCells(IntVec3 pos, Map map, ThingDef def)
        {
            cells.Clear();
            if (!pos.InBounds(map)) return cells;

            Region region = pos.GetRegion(map);
            if (region == null) return cells;

            modExt = def.GetModExtension<ModExt_CustomTradeBeacon>();
            bool ignoreWalls = modExt?.ignoreWalls ?? false;
            float range = modExt?.range ?? Transpiler_TradeableCellsAround.DefaultRange;

            if (ignoreWalls)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(pos, range, true))
                {
                    if (cell.GetEdifice(map)?.def.passability == Traversability.Impassable) continue;
                    cells.Add(cell);
                }
            }
            else RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null, delegate (Region r)
            {
                foreach (IntVec3 cell in r.Cells)
                {
                    if (cell.InHorDistOf(pos, range))
                    {
                        cells.Add(cell);
                    }
                }
                return false;
            }, Transpiler_TradeableCellsAround.MaxRegions, RegionType.Set_Passable);
            return cells;
        }
    }
}
