using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace TraderShipsExpanded
{
    //[HotSwap.HotSwappable]
    [StaticConstructorOnStartup]
    public class CompTraderShip : ThingComp
    {
        public CompProperties_TraderShip Props => (CompProperties_TraderShip)props;
        public LandedShip tradeShip;
        public Color col;

        public bool isQuestShip;
        public bool shouldDepart;

        private static readonly Texture2D SendAwayTexture = ContentFinder<Texture2D>.Get("TSE/UI/SendAway", true);

        private int HitPointsThreshold => isQuestShip ? (int)(parent.MaxHitPoints * 0.25) : (int)(parent.MaxHitPoints * 0.8);

        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref tradeShip, "ship");
            Scribe_Values.Look(ref col, "col");
            Scribe_Values.Look(ref isQuestShip, "isQuestShip");
            //Scribe_Deep.Look(ref tradeRequest, "tradeRequest");
        }

        public override void CompTick()
        {
            if (tradeShip == null) return;
            if (isQuestShip)
            {
                if (shouldDepart) SendAway();
                return; 
            }

            if (tradeShip.Departed)
            {
                if (parent.Spawned) SendAway();
            }
            else tradeShip.PassingShipTick();
        }


        public string TraderKindLabel
        {
            get
            {
                string label = tradeShip.def.label;
                if (label.NullOrEmpty() || label.ToLower() == "trader") return "TSE_Goods".Translate();
                return label;
            }
        }


        public override string CompInspectStringExtra()
        {
            var faction = parent.Faction;
            string factionString = (faction != null) ? $"\n{"Faction_Label".Translate()}: {faction.NameColored}" : $"\n{"TSE_TraderNameInQuotes".Translate(tradeShip.TraderName)}";
            string res = "TSE_TraderShip".Translate(TraderKindLabel).CapitalizeFirst()
                + factionString
                + "\n----------------------------------------\n"
                + (isQuestShip ? "TSE_TraderShipCannotLeaveQuest".Translate() : "TSE_TraderShipLeavingIn".Translate(GenDate.ToStringTicksToPeriod(tradeShip.ticksUntilDeparture)))
                + (DebugSettings.godMode ? $"\n ({tradeShip.ticksUntilDeparture} ticks)" : "");
            return res;
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (respawningAfterLoad) return;
            if (isQuestShip) return;

            Props.soundThud?.PlayOneShot(parent);

            var faction = parent.Faction;
            bool isFactionShip = faction != null;
            string letterDesc = isFactionShip ? "TSE_TraderShipArrival_LetterDescFaction" : "TSE_TraderShipArrival_LetterDesc";
            TaggedString info = letterDesc.Translate(isFactionShip ? faction.NameColored : tradeShip.name, TraderKindLabel, isFactionShip ? "TraderArrivalFromFaction".Translate(faction.Named("FACTION")) : "TraderArrivalNoFaction".Translate());
            Find.LetterStack.ReceiveLetter("TSE_TraderShipArrival_LetterHeader".Translate(TraderKindLabel), info, LetterDefOf.PositiveEvent, parent, faction);
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            bool leaving = false;

            if (totalDamageDealt > 25) leaving = true;
            else if (parent.HitPoints <= HitPointsThreshold) leaving = true;

            if (leaving)
            {
                SendAway(true);
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn negotiator)
        {
            string label = "TradeWith".Translate(tradeShip.GetCallLabel());

            if (negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
            {
                yield return new FloatMenuOption("TSE_CannotTradeReason".Translate(negotiator.LabelShort, "WorkTypeDisablesOption".Translate(SkillDefOf.Social.label)), null);
            }
            else yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, delegate ()
            {
                Job job = JobMaker.MakeJob(TSE_DefOf.TSE_TradeWithShip, parent);
                negotiator.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }, MenuOptionPriority.InitiateSocial), negotiator, parent);

            yield break;
        }

        Faction GetFaction(TraderKindDef trader)
        {
            if (trader.faction == null) return null;
            return (from f in Find.FactionManager.AllFactions where f.def == trader.faction select f).RandomElementWithFallback();
        }

        public void GenerateInternalTradeShip(Map map, TraderKindDef traderKindDef = null, int departureTicks = 30000)
        {
            traderKindDef ??= DefDatabase<TraderKindDef>.AllDefs.RandomElementByWeightWithFallback(x => x.CalculatedCommonality);

            tradeShip = new LandedShip(map, traderKindDef, GetFaction(traderKindDef))
            {
                passingShipManager = map.passingShipManager,
                ticksUntilDeparture = departureTicks
            };
            tradeShip.GenerateThings();
        }


        /*
        private void FulfillTradeRequest()
        {
            tradeRequest.FulfillRequest(tradeShip);

            QuestUtility.SendQuestTargetSignals(parent.questTags, "TradeRequestFulfilled", parent.Named("SUBJECT"));
            tradeRequest = null;
        }
        */

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (isQuestShip) yield break;

            yield return new Command_Action
            {
                defaultLabel = "TSE_TraderShipSendAway".Translate(),
                defaultDesc = "TSE_TraderShipSendAwayDesc".Translate(),
                action = new Action(()=>SendAway()), // what is this black magic?
                icon = SendAwayTexture,
            };
            yield break;
        }

        private void SendAway(bool damaged = false)
        {
            Map map = parent.Map;
            IntVec3 position = parent.Position;

            if (damaged) Messages.Message("TSE_TraderShipLeavingDueToDamage".Translate(tradeShip.FullTitle), parent, MessageTypeDefOf.NegativeEvent);
            else Messages.Message("TSE_TraderShipLeaving".Translate(tradeShip.FullTitle), MessageTypeDefOf.NeutralEvent);

            map.passingShipManager.RemoveShip(tradeShip);
            tradeShip.things.ClearAndDestroyContentsOrPassToWorld();
            tradeShip.soldPrisoners.Clear();

            parent.DeSpawn();

            Skyfaller skyfaller = ThingMaker.MakeThing(Props.takeoffAnimation, null) as Skyfaller;
            skyfaller.CopyThingGraphicOntoSkyfaller(parent);
            skyfaller.DrawColor = parent.DrawColor;

            if (!skyfaller.innerContainer.TryAdd(parent, true))
            {
                Log.Error("Could not add " + parent.ToStringSafe<Thing>() + " to a skyfaller.");
                parent.Destroy(DestroyMode.QuestLogic);
            }

            GenSpawn.Spawn(skyfaller, position, map, WipeMode.Vanish);
            //map.wealthWatcher.ForceRecount(); // so ship value doesn't stick to total wealth.
        }
    }
}
