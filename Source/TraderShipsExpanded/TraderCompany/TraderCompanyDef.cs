using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;

namespace TraderShipsExpanded
{
    public class CompanyTraderKind
    {
        public TraderKindDef traderKindDef;
        public int requestCost;

        public CompanyTraderKind() { }
        public CompanyTraderKind(TraderKindDef def, int cost = 0)
        { 
            traderKindDef = def; 
            requestCost = cost; 
        }

        public override string ToString()
        {
            return "CTK: (traderKindDef = " + traderKindDef + ", cost = " + requestCost + ")";
        }
    }

    public class TraderCompanyDef : Def
    {
        public List<ThingDef> shipThingDefs;

        public Color iconColor = Color.white;

        public int requestCostForAll = 0;

        [NoTranslate]
        public string iconPath;

        public FactionDef factionDef;

        public IntRange shipArrivalRangeTicks = new(5000,30000); // 2 - 12 hours

        public IntRange cooldownRangeTicks = new(30000,60000); // 12 - 24 hours

        public int shipDepartureTicks = 30000; // 12 hours

        public List<CompanyTraderKind> traderKinds = [];
        public List<TraderKindDef> excludedTraderKinds = [];
        public List<CompanyTraderKind> TraderKinds
        {
            get
            {
                List<CompanyTraderKind> filteredKinds = [];
                if (traderKinds.NullOrEmpty())
                {
                    foreach (var def in DefDatabase<TraderKindDef>.AllDefsListForReading.Where(x => x.requestable && !x.label.NullOrEmpty()))
                    {
                        if (excludedTraderKinds.Contains(def)) continue;

                        filteredKinds.Add(new(def, requestCostForAll));
                    }
                    return filteredKinds;
                }
                if (!excludedTraderKinds.NullOrEmpty()) 
                {
                    foreach (var kind in traderKinds)
                    {
                        if (excludedTraderKinds.Contains(kind.traderKindDef)) continue;

                        filteredKinds.Add(new(kind.traderKindDef, requestCostForAll));
                    }
                    return filteredKinds;
                }
                return traderKinds;
            }
        }

        private Texture2D iconTex;
        public Texture2D Icon
        {
            get
            {
                if (iconTex != null) return iconTex;
                if (iconPath == null) return BaseContent.ClearTex;
                return iconTex = ContentFinder<Texture2D>.Get(iconPath);
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string str in base.ConfigErrors())
            {
                yield return str;
            }

            if (shipThingDefs.NullOrEmpty())
            {
                yield return "defName " + defName + " has empty shipThingDefs.";
            }

            if (!traderKinds.NullOrEmpty())
            {
                foreach (var kindcost in traderKinds)
                {
                    if (kindcost.traderKindDef == null)
                        yield return $"defName {defName} has a null traderKind. ({kindcost})";
                    if (kindcost.requestCost < 0)
                        yield return $"defName {defName} has a requestCost < 0. ({kindcost})";
                }
            }
        }
    }
}
