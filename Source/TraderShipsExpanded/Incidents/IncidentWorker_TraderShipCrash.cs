using RimWorld;
using Verse;

namespace TraderShipsExpanded
{
    public class IncidentWorker_TraderShipCrash : IncidentWorker_ShipChunkDrop
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            bool result = base.TryExecuteWorker(parms); // ship chunks

            if (result) // cargo pods
            {
                Map map = (Map)parms.target;

                IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
                DropPodUtility.DropThingsNear(intVec, map, ThingSetMakerDefOf.TraderStock.root.Generate(), 60, canInstaDropDuringInit: false, leaveSlag: true);
                
                SendStandardLetter("LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.PositiveEvent, parms, new TargetInfo(intVec, map));
               
                return true;
            }
            return false;
        }
    }
}
