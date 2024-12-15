using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TraderShipsExpanded
{
    public class ModExt_TSEIncidentExtension : DefModExtension
    {
        public FactionDef factionDef;
        public IntRange shipChunkCountRange;
        public IntRange itemPodsCountRange;
        public IntRange pawnPodsCountRange;
        public int podOpenDelay = 60;
        public float stackValueLimit = 800;
        public string letterString = "TSE_CargoShipCrash";
    }

    public class IncidentWorker_CargoShipCrash : IncidentWorker
    {
        private ModExt_TSEIncidentExtension ext;
        private ModExt_TSEIncidentExtension Ext => ext ??= def.GetModExtension<ModExt_TSEIncidentExtension>();

        private Faction faction;
        private Faction TargetFaction => faction ??= Find.FactionManager.FirstFactionOfDef(Ext.factionDef);

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (Ext == null)
            {
                Log.Error(def.defName + " is missing ModExt_TSEIncidentExtension. Incident aborted.");
                return false;
            }

            Map map = (Map)parms.target;
            if (!TryFindShipChunkDropCell(map.Center, map, 999999, out IntVec3 intVec))
            {
                return false;
            }

            SpawnShipChunks(intVec, map, Ext.shipChunkCountRange.RandomInRange);
            Messages.Message("MessageShipChunkDrop".Translate(), new TargetInfo(intVec, map), MessageTypeDefOf.NeutralEvent);

            var trader = DefDatabase<TraderKindDef>.AllDefsListForReading.Where(x => x.faction == TargetFaction.def)?.RandomElement();

            List<TargetInfo> targets = [];
            //IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            targets.Add(new TargetInfo(intVec, map));

            List<Thing> stock = Utils.GetRandomTraderStock(TargetFaction.def);
            List<Thing> thingsToDrop = [];

            for (int i = 1; i <= Ext.itemPodsCountRange.RandomInRange; i++)
            {
                var thing = stock.RandomElement();
                thing.stackCount = Rand.RangeInclusive(1, thing.def.stackLimit / 2); // avoid over-limit errors
                var totalStackValue = thing.MarketValue * thing.stackCount;
                while (totalStackValue > Ext.stackValueLimit) // no 22 adv.components from the sky for you.
                {
                    thing.stackCount /= 2;
                    totalStackValue = thing.MarketValue * thing.stackCount;
                }
                thingsToDrop.Add(thing);
            }
            DropPodUtility.DropThingsNear(intVec, map, thingsToDrop, Ext.podOpenDelay, leaveSlag: true);

            for (int i = 1; i <= Ext.pawnPodsCountRange.RandomInRange; i++)
            {
                DropPawnNear(intVec, map);
            }
            SendStandardLetter(Ext.letterString.Translate(), "TSE_CargoShipCrashDesc".Translate(), LetterDefOf.PositiveEvent, parms, targets);
            return true;
        }

        private void DropPawnNear(IntVec3 intVec, Map map)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(Utils.GetRandomShipPersonnelPawnKind(Utils.GetTraderPawnGroupMaker(TargetFaction.def)), TargetFaction);
            if (pawn.equipment?.Primary != null) pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
            HealthUtility.DamageUntilDowned(pawn);
            ActiveDropPodInfo activeDropPodInfo = new()
            {
                openDelay = Ext.podOpenDelay,
                leaveSlag = true,
            };
            activeDropPodInfo.innerContainer.TryAddOrTransfer(pawn, canMergeWithExistingStacks: false);
            if (!DropCellFinder.TryFindDropSpotNear(intVec, map, out IntVec3 resultCell, true, true, true)) // tweak this later if anything gets weird
            {
                resultCell = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(map), map);
            }
            DropPodUtility.MakeDropPodAt(resultCell, map, activeDropPodInfo, TargetFaction);
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            Map map = (Map)parms.target;
            return TryFindShipChunkDropCell(map.Center, map, 999999, out _);
        }

        private void SpawnShipChunks(IntVec3 firstChunkPos, Map map, int count)
        {
            SpawnChunk(firstChunkPos, map);
            for (int i = 0; i < count - 1; i++)
            {
                if (TryFindShipChunkDropCell(firstChunkPos, map, 10, out var pos))
                {
                    SpawnChunk(pos, map);
                }
            }
        }

        private void SpawnChunk(IntVec3 pos, Map map)
        {
            SkyfallerMaker.SpawnSkyfaller(ThingDefOf.ShipChunkIncoming, ThingDefOf.ShipChunk, pos, map);
        }

        private bool TryFindShipChunkDropCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
        {
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.ShipChunkIncoming, map, out pos, 10, nearLoc, maxDist);
        }
    }
}
