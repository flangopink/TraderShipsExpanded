using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld;
using System.Collections.Generic;
using Verse.Grammar;

namespace TraderShipsExpanded
{
    public class SitePartWorker_Generic : SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
        }
    }
}
