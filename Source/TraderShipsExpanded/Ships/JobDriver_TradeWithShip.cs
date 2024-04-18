using System.Collections.Generic;
using Verse.AI;
using Verse;
using RimWorld;

namespace TraderShipsExpanded
{
    public class JobDriver_TradeWithShip : JobDriver
    {
        private ThingWithComps Trader => TargetThingA as ThingWithComps;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Trader, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompTraderShip comp = Trader.TryGetComp<CompTraderShip>();
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOn(() => comp == null || !comp.tradeShip.CanTradeNow);
            Toil trade = new Toil();
            trade.initAction = delegate
            {
                Pawn actor = trade.actor;
                if (comp.tradeShip.CanTradeNow)
                {
                    Find.WindowStack.Add(new Dialog_Trade(actor, comp.tradeShip));
                }
            };
            yield return trade;
        }
    }
}
