using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TraderShipsExpanded
{
    public class ModExt_TSEIncidentExtension : DefModExtension
    {
        public FactionDef factionDef;
        public IntRange itemCountRange;
        public IntRange pawnCountRange;
        public int podOpenDelay = 60;
        public bool allowNonStackableDuplicates;
        public IntRange countRange;
        public FloatRange totalMarketValueRange;
    }

    public class IncidentWorker_CargoShipCrash : IncidentWorker_ShipChunkDrop
    {
        private ModExt_TSEIncidentExtension ext;
        private ModExt_TSEIncidentExtension Ext => ext ??= def.GetModExtension<ModExt_TSEIncidentExtension>();

        private Faction faction;
        private Faction TargetFaction => faction ??= Find.FactionManager.FirstFactionOfDef(Ext.factionDef);

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            bool result = base.TryExecuteWorker(parms); // ship chunks

            if (result) // cargo pods
            {
                Map map = (Map)parms.target;

                var trader = DefDatabase<TraderKindDef>.AllDefsListForReading.Where(x => x.faction == TargetFaction.def)?.RandomElement();

                List<TargetInfo> targets = [];
                IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
                targets.Add(new TargetInfo(intVec, map));

                ThingSetMakerParams itemPodParams = new()
                {
                    traderDef = trader,
                    allowNonStackableDuplicates = Ext.allowNonStackableDuplicates,
                    countRange = Ext.countRange,
                    totalMarketValueRange = Ext.totalMarketValueRange
                };

                ThingSetMakerParams pawnPodParams = new()
                {
                    makingFaction = TargetFaction
                };

                List<Thing> dropPodThings = ThingSetMakerDefOf.ResourcePod.root.Generate(itemPodParams);

                for (int i = 1; i <= Ext.itemCountRange.RandomInRange; i++)
                {
                    DropPodUtility.DropThingsNear(intVec, map, dropPodThings, Ext.podOpenDelay, leaveSlag: true);
                }
                for (int i = 1; i <= Ext.pawnCountRange.RandomInRange; i++)
                {
                    DropPawn(map);
                }

                SendStandardLetter("TSE_CargoShipCrash".Translate(), "TSE_CargoShipCrashDescription".Translate(), LetterDefOf.PositiveEvent, parms, targets);

                return true;
            }
            return false;
        }

        private void DropPawn(Map map)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(TargetFaction.RandomPawnKind());
            pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);
            HealthUtility.DamageUntilDowned(pawn);
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            ActiveDropPodInfo activeDropPodInfo = new()
            {
                openDelay = Ext.podOpenDelay,
                leaveSlag = true
            };
            activeDropPodInfo.innerContainer.TryAddOrTransfer(pawn, canMergeWithExistingStacks: false);
            DropPodUtility.MakeDropPodAt(intVec, map, activeDropPodInfo, TargetFaction);
        }
    }
}
