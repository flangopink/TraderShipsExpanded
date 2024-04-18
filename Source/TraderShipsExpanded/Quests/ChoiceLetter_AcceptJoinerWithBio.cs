using RimWorld;
using System.Collections.Generic;
using Verse;
using static System.Net.Mime.MediaTypeNames;

namespace TraderShipsExpanded
{
    public class ChoiceLetter_AcceptJoinerWithBio : ChoiceLetter_AcceptJoiner
    {
        public Pawn pawn;

        public override bool CanShowInLetterStack => true; // why the fuck would i need a quest for that?

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly || pawn == null || pawn.Map == null) 
                {
                    yield return Option_Close;
                    yield break;
                }

                DiaOption bio = new("TabCharacter".Translate());
                DiaOption accept = new("AcceptButton".Translate());
                DiaOption reject = new("RejectLetter".Translate());

                bio.action = delegate
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(pawn));
                };
                bio.resolveTree = false;

                accept.action = delegate
                {
                    pawn.SetFaction(Faction.OfPlayer);
                    Find.LetterStack.RemoveLetter(this);
                };
                accept.resolveTree = true;

                reject.action = delegate
                {
                    Find.LetterStack.RemoveLetter(this);
                };
                reject.resolveTree = true;

                yield return bio;
                yield return accept;
                yield return reject;

                if (pawn != null && pawn.Map != null)
                {
                    if (lookTargets.IsValid())
                        yield return Option_JumpToLocationAndPostpone; // postponing WILL cause issues and i refuse to solve them.
                    yield return Option_Postpone;
                }
            }
        }

        public override void OpenLetter()
        {
            if (pawn == null || pawn.Map == null)
            {
                Text = "TSE_RecruitOpportunityExpired".Translate();
            }
            DiaNode diaNode = new(Text);
            diaNode.options.AddRange(Choices);
            Dialog_NodeTreeWithFactionInfo window = new(diaNode, relatedFaction, false, radioMode, title);
            Find.WindowStack.Add(window);
        }

        public override void Received()
        {
            Log.Message("Recruited " + pawn.Label);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
        }
    }
}
