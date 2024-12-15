using RimWorld;
using Verse;
using RimWorld.BaseGen;
using Verse.AI.Group;
using Verse.AI;
using System.Linq;

namespace TraderShipsExpanded
{
    public class GenStep_TraderShipPirateRaid : GenStep
    {
        public IntRange defaultPointsRange = new(100, 400);
        public Thing ship;

        public override int SeedPart => 69696903;

        public override void Generate(Map map, GenStepParams parms)
        {
            var pirates = Find.FactionManager.OfPirates;
            ship = map.spawnedThings.FirstOrDefault(x => Utils.AllShipWrecks.Contains(x.def) || Utils.AllShipDefs.Contains(x.def));
            IntVec3 shipSpot = map.Center;//ship != null ? ship.Position : map.Center;

            ResolveParams resolveParams = default;
            TryFindRaidWalkInPosition(map, out var walkIntSpot);
            resolveParams.rect = CellRect.CenteredOn(walkIntSpot, 3); // pls don't spawn them off the map thanks
            resolveParams.faction = pirates;
            resolveParams.spawnPawnsOnEdge = true;

            Lord singlePawnLord = LordMaker.MakeNewLord(resolveParams.faction, new LordJob_DefendPoint(shipSpot), map);
            resolveParams.singlePawnLord = singlePawnLord;
            resolveParams.pawnGroupKindDef = PawnGroupKindDefOf.Combat ?? PawnGroupKindDefOf.Settlement;
            resolveParams.pawnGroupMakerParams ??= new()
            {
                tile = map.Tile,
                faction = resolveParams.faction,
                points = defaultPointsRange.RandomInRange //(int)((double)StorytellerUtility.DefaultSiteThreatPointsNow() * 1.25) // causes nonsensical errors
            };
            Log.Message($"{shipSpot} {resolveParams.rect} {resolveParams.faction} {resolveParams.singlePawnLord} {resolveParams.pawnGroupKindDef} {resolveParams.pawnGroupMakerParams.tile} {resolveParams.pawnGroupMakerParams.faction} {resolveParams.pawnGroupMakerParams.points} ");
            BaseGen.symbolStack.Push("pawnGroup", resolveParams);
            BaseGen.globalSettings.map = map;
            BaseGen.Generate();

            //LetterMaker.MakeLetter("TSE_ShipDefenseRaid", "TSE_ShipDefenseRaidDesc", LetterDefOf.ThreatSmall, new LookTargets(new TargetInfo(walkIntSpot, map)));
        }

        private bool TryFindRaidWalkInPosition(Map map, out IntVec3 spawnSpot)
        {
            bool Predicate(IntVec3 from, IntVec3 to)
            {
                if (!map.roofGrid.Roofed(from) && from.Walkable(map))
                {
                    return map.reachability.CanReach(from, to, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some);
                }
                return false;
            }

            //bool Predicate(IntVec3 p) => !map.roofGrid.Roofed(p) && p.Walkable(map) && map.reachability.CanReach(p, shipCrashSpot, PathEndMode.ClosestTouch, TraverseMode.NoPassClosedDoors, Danger.Some);
            /*if (RCellFinder.TryFindEdgeCellFromPositionAvoidingColony(shipCrashSpot, map, Predicate, out spawnSpot))
            {
                return true;
            }*/
            if (!RCellFinder.TryFindEdgeCellFromThingAvoidingColony(ship, map, Predicate, out spawnSpot))
            {
                CellFinder.TryFindRandomEdgeCellWith((IntVec3 p) => !map.roofGrid.Roofed(p) && p.Walkable(map), map, CellFinder.EdgeRoadChance_Hostile, out spawnSpot);
                
            }
            if (!spawnSpot.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out spawnSpot, map, CellFinder.EdgeRoadChance_Hostile))
            {
                return false;
            }
            return true;
            /*if (CellFinder.TryFindRandomEdgeCellWith(Predicate, map, CellFinder.EdgeRoadChance_Hostile, out spawnSpot))
            {
                return true;
            }*/
            //spawnSpot = IntVec3.Invalid;
            //return false;
        }
    }
}
