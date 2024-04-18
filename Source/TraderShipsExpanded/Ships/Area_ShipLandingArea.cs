using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TraderShipsExpanded
{
    public class Area_ShipLandingArea : Area
    {
        public override string Label => "TSE_ShipLandingArea".Translate();

        public override Color Color => new ColorInt(20, 200, 200).ToColor;

        public override int ListPriority => 6969;

        public Area_ShipLandingArea() {}

        public Area_ShipLandingArea(AreaManager areaManager) : base(areaManager) 
        { 

        }

        public override string GetUniqueLoadID()
        {
            return "Area_" + ID + "_ShipLandingArea";
        }

        public IEnumerable<IntVec3> LandingLocations()
        {
            CellIndices indices = Map.cellIndices;
            HashSet<int> visited = [];
            List<int> toVisit = [];
            foreach (IntVec3 cell in ActiveCells)
            {
                int cellIndex = indices.CellToIndex(cell);
                if (visited.Contains(cellIndex)) continue;
                toVisit.Add(cellIndex);
                int sx = 0;
                int sz = 0;
                int count = 0;
                while (toVisit.Any())
                {
                    cellIndex = toVisit[toVisit.Count - 1];
                    toVisit.RemoveAt(toVisit.Count - 1);
                    if (base[cellIndex])
                    {
                        visited.Add(cellIndex);
                        IntVec3 vec = indices.IndexToCell(cellIndex);
                        int x = vec.x;
                        int z = vec.z;
                        sx += x;
                        sz += z;
                        count++;
                        int cellSubIndex = indices.CellToIndex(x + 1, z);
                        if (!visited.Contains(cellSubIndex)) toVisit.Add(cellSubIndex);
                        cellSubIndex = indices.CellToIndex(x - 1, z);
                        if (!visited.Contains(cellSubIndex)) toVisit.Add(cellSubIndex);
                        cellSubIndex = indices.CellToIndex(x, z + 1);
                        if (!visited.Contains(cellSubIndex)) toVisit.Add(cellSubIndex);
                        cellSubIndex = indices.CellToIndex(x, z - 1);
                        if (!visited.Contains(cellSubIndex)) toVisit.Add(cellSubIndex);
                    }
                }
                if (count > 0)
                {
                    yield return new IntVec3(sx / count, 0, sz / count);
                }
            }
        }

        public bool TryFindShipLandingArea(IntVec2 size, out IntVec3 result, out Thing blocker)
        {
            blocker = null;
            foreach (IntVec3 item in LandingLocations())
            {
                blocker = null;
                foreach (IntVec3 item2 in new CellRect(item.x - (size.x + 1) / 2 + 1, item.z - (size.z + 1) / 2 + 1, size.x, size.z))
                {
                    foreach (Thing thing in item2.GetThingList(Map))
                    {
                        if (thing is Pawn || thing.def.Fillage == FillCategory.None)
                        {
                            continue;
                        }
                        blocker = thing;
                        break;
                    }
                    if (blocker != null) break;
                }
                if (blocker == null)
                {
                    result = item;
                    return true;
                }
            }
            result = IntVec3.Invalid;
            return false;
        }
    }
}
