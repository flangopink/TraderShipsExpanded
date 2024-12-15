using RimWorld;
using Verse;

namespace TraderShipsExpanded
{
    public class CompProperties_ForcedStyle : CompProperties
    {
        public ThingStyleDef styleDef;

        public CompProperties_ForcedStyle()
        {
            compClass = typeof(CompForcedStyle);
        }
    }

    public class CompForcedStyle : ThingComp
    {
        public CompProperties_ForcedStyle Props => (CompProperties_ForcedStyle)props;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            parent.SetStyleDef(Props.styleDef);
        }
    }
}
