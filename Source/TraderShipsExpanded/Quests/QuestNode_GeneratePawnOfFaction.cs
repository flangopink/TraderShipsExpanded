using System.Linq;
using System.Security.Policy;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace TraderShipsExpanded
{
    public class QuestNode_GeneratePawnOfFaction : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;

        public SlateRef<PawnKindDef> mustBeOfKind;

        public SlateRef<FactionDef> factionDef;
        
        public SlateRef<bool> mustBeNonHostileToPlayer;

        public SlateRef<bool> eitherThisFactionOrRandom;

        protected override bool TestRunInt(Slate slate)
        {
            if (slate.TryGet<Pawn>(storeAs.GetValue(slate), out var var) && IsGoodPawn(var, slate))
            {
                return true;
            }
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            if (!QuestGen.slate.TryGet<Pawn>(storeAs.GetValue(slate), out var pawn) || !IsGoodPawn(pawn, slate))
            {
                Faction faction;
                if (factionDef.GetValue(slate) != null && !(eitherThisFactionOrRandom.GetValue(slate) && Rand.Chance(0.5f))) // i hope i didn't mess this up
                {
                    faction = Find.FactionManager.FirstFactionOfDef(factionDef.GetValue(slate));
                }
                else faction = Find.FactionManager.RandomNonHostileFaction(minTechLevel: TechLevel.Industrial);

                pawn = GeneratePawn(slate, faction);

                if (QuestGen.quest.TryGetFirstPartOfType<QuestPart_InvolvedFactions>(out _))
                {
                    QuestPart_InvolvedFactions questPart_InvolvedFactions = new();
                    questPart_InvolvedFactions.factions.Add(pawn.Faction);
                    QuestGen.quest.AddPart(questPart_InvolvedFactions);
                }
                QuestGen.slate.Set(storeAs.GetValue(slate), pawn);
            }
        }

        private Pawn GeneratePawn(Slate slate, Faction faction = null)
        {
            PawnKindDef kindDef = mustBeOfKind.GetValue(slate);

            kindDef ??= DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef kind) => kind.race.race.Humanlike).RandomElement();

            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef, faction, PawnGenerationContext.NonPlayer, -1, true));
            Find.WorldPawns.PassToWorld(pawn);

            // adds the asker pawn to hyperlinks so i can reference it later. neat!
            QuestPart_Hyperlinks questPart_Hyperlinks = new();
            questPart_Hyperlinks.pawns.Add(pawn);
            QuestGen.quest.AddPart(questPart_Hyperlinks);

            return pawn;
        }

        private bool IsGoodPawn(Pawn pawn, Slate slate)
        {
            if (factionDef.GetValue(slate) != null && (pawn.Faction == null || pawn.Faction.def != factionDef.GetValue(slate)))
            {
                return false;
            }
            if (mustBeOfKind.GetValue(slate) != null && pawn.kindDef != mustBeOfKind.GetValue(slate))
            {
                return false;
            }
            if (mustBeNonHostileToPlayer.GetValue(slate) && (pawn.HostileTo(Faction.OfPlayer) || (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.Faction.HostileTo(Faction.OfPlayer))))
            {
                return false;
            }
            return true;
        }
    }
}
