using RimWorld;
using System.Linq;
using Verse.AI;
using Verse;
using Verse.AI.Group;

namespace TraderShipsExpanded
{
    public class LordToil_DefendPointThenLeave : LordToil_DefendPoint
    {
        private readonly int updateTicks = 120; // 2 sec
        private readonly int leaveAfterTicks = 600; // 10 sec
        private bool isOver;

        public LordToil_DefendPointThenLeave(bool canSatisfyLongNeeds = true) : base(canSatisfyLongNeeds)
        {
        }

        public LordToil_DefendPointThenLeave(IntVec3 defendPoint, float? wanderRadius = null, float? defendRadius = 28f) : base(defendPoint, wanderRadius, defendRadius)
        {
        }

        public override void LordToilTick()
        {
            if (isOver && Find.TickManager.TicksGame % updateTicks == leaveAfterTicks)
            {
                if (Map.mapPawns.AllPawnsSpawned.Any((Pawn p) => p.Faction.def == TSE_DefOf.TSE_Faction_GTC && !p.DeadOrDowned))
                    Messages.Message("TSE_CrashSurvivorsAreLeaving", MessageTypeDefOf.NeutralEvent);
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
                    if (pawn.CanJoinColony() && Rand.Value < 0.25) // 25% chance to join player
                    {
                        // pawn.SetFaction(Faction.OfPlayer);
                        // Find.LetterStack.ReceiveLetter("TSE_ShipCrashRecruitSuccess".Translate(), "TSE_ShipCrashRecruitSuccessDesc".Translate(pawn.Label), LetterDefOf.PositiveEvent, new LookTargets(p));
                        // Log.Message("Recruitment roll for " + pawn.Label + "... Success!"); 
                        Utils.SendLetterJoinerWithBio(pawn, true);
                    }
                    //else
                    //{
                        // Log.Warning("Recruitment roll for " + pawn.Label + "... Failed!");
                    pawn.mindState.exitMapAfterTick = leaveAfterTicks;
                    //}
                }
                Signal signal = new("Quest" + thisQuest.id + ".TradeShipDefenseOver");
                Find.SignalManager.SendSignal(signal);

                lord.ownedPawns.Clear();
                isOver = true;
            }
        }
    }
}
