using Verse;

namespace TraderShipsExpanded
{
    public class CompProperties_TraderShip : CompProperties
    {
        public SoundDef soundThud;

        public ThingDef landAnimation;

        public ThingDef takeoffAnimation;

        public CompProperties_TraderShip()
        {
            compClass = typeof(CompTraderShip);
        }
    }
}
