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
        public IntRange defaultPointsRange = new(250, 600);

        public override int SeedPart => 69696903;

        public override void Generate(Map map, GenStepParams parms)
        {
            var pirates = Find.FactionManager.OfPirates;
            var ship = map.spawnedThings.FirstOrDefault(x => Utils.AllShipWrecks.Contains(x.def));
            IntVec3 shipCrashSpot = ship != null ? ship.Position : map.Center;

            ResolveParams resolveParams = default;
            TryFindRaidWalkInPosition(map, shipCrashSpot, out var walkIntSpot);
            resolveParams.rect = CellRect.CenteredOn(walkIntSpot, 3); // pls don't spawn them off the map thanks
            resolveParams.faction = pirates;
            resolveParams.spawnPawnsOnEdge = true;

            Lord singlePawnLord = LordMaker.MakeNewLord(resolveParams.faction, new LordJob_DefendPoint(shipCrashSpot), map);
            resolveParams.singlePawnLord = singlePawnLord;
            resolveParams.pawnGroupKindDef = PawnGroupKindDefOf.Combat ?? PawnGroupKindDefOf.Settlement;
            resolveParams.pawnGroupMakerParams ??= new()
            {
                tile = map.Tile,
                faction = resolveParams.faction,
                points = defaultPointsRange.RandomInRange //(int)((double)StorytellerUtility.DefaultSiteThreatPointsNow() * 1.25) // causes nonsensical errors
            };
            BaseGen.symbolStack.Push("pawnGroup", resolveParams);
            BaseGen.globalSettings.map = map;
            BaseGen.Generate();

            //LetterMaker.MakeLetter("TSE_ShipDefenseRaid", "TSE_ShipDefenseRaidDesc", LetterDefOf.ThreatSmall, new LookTargets(new TargetInfo(walkIntSpot, map)));
        }

        private bool TryFindRaidWalkInPosition(Map map, IntVec3 shipCrashSpot, out IntVec3 spawnSpot)
        {
            bool Predicate(IntVec3 p) => !map.roofGrid.Roofed(p) && p.Walkable(map) && map.reachability.CanReach(p, shipCrashSpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some);
            if (RCellFinder.TryFindEdgeCellFromPositionAvoidingColony(shipCrashSpot, map, Predicate, out spawnSpot))
            {
                return true;
            }
            if (CellFinder.TryFindRandomEdgeCellWith(Predicate, map, CellFinder.EdgeRoadChance_Hostile, out spawnSpot))
            {
                return true;
            }
            spawnSpot = IntVec3.Invalid;
            return false;
        }
    }
}
