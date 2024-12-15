using RimWorld;
using System.Linq;
using Verse.AI;
using Verse;
using Verse.AI.Group;

namespace TraderShipsExpanded
{
    public class LordToil_DefendShip : LordToil_DefendPoint
    {
        private readonly int updateTicks = 120; // 2 sec
        private readonly int leaveAfterTicks = 600; // 10 sec
        private bool isOver;

        public LordToil_DefendShip(bool canSatisfyLongNeeds = true) : base(canSatisfyLongNeeds)
        {
        }

        public LordToil_DefendShip(IntVec3 defendPoint, float? wanderRadius = null, float? defendRadius = 28f) : base(defendPoint, wanderRadius, defendRadius)
        {
        }

        public override void LordToilTick()
        {
            if (isOver && Find.TickManager.TicksGame % updateTicks == leaveAfterTicks)
            {
                if (Map.mapPawns.AllPawnsSpawned.Any((Pawn p) => p.Faction.def == TSE_DefOf.TSE_Faction_GTC && !p.DeadOrDowned))
                {
                    foreach (Pawn pawn in lord.ownedPawns) pawn.DeSpawn();
                    lord.ownedPawns.Clear();
                    var shipComp = Map.spawnedThings.FirstOrFallback(x => Utils.AllShipDefs.Contains(x.def))?.TryGetComp<CompTraderShip>();
                    if (shipComp != null) shipComp.shouldDepart = true;
                    else Log.Error("Failed to find ship comp");
                }
            }
            if (Find.TickManager.TicksGame % updateTicks == 0)
            {
                UpdateAllDuties();
            }
            base.LordToilTick();
        }

        public override void UpdateAllDuties()
        {
            if (isOver) return;

            LordToilData_DefendPoint lordToilData_DefendPoint = Data;
            if (Map.mapPawns.AllPawnsSpawned.Any((Pawn p) => p.Faction.HostileTo(lord.ownedPawns.Count > 0 ? lord.ownedPawns[0].Faction : Faction.OfPlayer)))
            {
                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    Pawn pawn = lord.ownedPawns[i];
                    if (pawn?.mindState != null)
                    {
                        pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, lordToilData_DefendPoint.defendPoint)
                        {
                            focusSecond = lordToilData_DefendPoint.defendPoint,
                            radius = (pawn.kindDef.defendPointRadius >= 0f) ? pawn.kindDef.defendPointRadius : lordToilData_DefendPoint.defendRadius,
                            wanderRadius = lordToilData_DefendPoint.wanderRadius
                        };
                    }
                }
                return;
            }
            // Defense over
            if (!isOver)
            {
                Quest thisQuest = Utils.GetQuest(Map);

                for (int j = 0; j < lord.ownedPawns.Count; j++)
                {
                    Pawn pawn = lord.ownedPawns[j];
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.WanderClose, lordToilData_DefendPoint.defendPoint)
                    {
                        focusSecond = lordToilData_DefendPoint.defendPoint,
                        radius = (pawn.kindDef.defendPointRadius >= 0f) ? pawn.kindDef.defendPointRadius : lordToilData_DefendPoint.defendRadius,
                        wanderRadius = 6
                    }; 
                    Messages.Message("TSE_ShipDefendersAreLeaving", MessageTypeDefOf.NeutralEvent);
                }
                Signal signal = new("Quest" + thisQuest.id + ".TradeShipDefenseOver");
                Find.SignalManager.SendSignal(signal);
                isOver = true;
            }
        }
    }
}
