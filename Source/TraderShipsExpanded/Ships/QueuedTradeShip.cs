using RimWorld;
using Verse;

namespace TraderShipsExpanded
{
    public class QueuedTradeShip : IExposable
    {
        public Map map;
        public TraderCompanyDef companyDef;
        public TraderKindDef kindDef;
        public Faction faction;

        public QueuedTradeShip() { }
        public QueuedTradeShip(Map map, TraderCompanyDef companyDef, TraderKindDef kindDef, Faction faction) 
        {
            this.map = map;
            this.companyDef = companyDef;
            this.kindDef = kindDef;
            this.faction = faction;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref companyDef, "companyDef");
            Scribe_Defs.Look(ref kindDef, "kindDef");
            Scribe_References.Look(ref map, "map");
            Scribe_References.Look(ref faction, "faction");
        }
    }
}