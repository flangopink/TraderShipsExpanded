using RimWorld;
using System;
using Verse;
using Verse.Sound;

namespace TraderShipsExpanded
{
    //[HotSwap.HotSwappable]
    public static class TraderDialogMaker
    {
        private static SoundDef PurchaseSound => SoundDefOf.ExecuteTrade;
        public static DiaNode TraderDialogFor(TraderCompanyDef companyDef, Pawn negotiator)
        {
            Map map = negotiator.Map;

            var traderLabelColored = companyDef.LabelCap.Colorize(ColoredText.NameColor);

            // ABC greets XYZ and blah blah blah. 
            DiaNode root = new("TSE_TraderShipGreeting".Translate(traderLabelColored, negotiator.LabelShort));
            if (map != null && map.IsPlayerHome)
            {
                AddAndDecorateOption(RequestTraderOption(map, companyDef, negotiator), needsSocial: true);
            }

            AddAndDecorateOption(new DiaOption("(" + "Disconnect".Translate() + ")") { resolveTree = true }, needsSocial: false);
            
            return root;

            void AddAndDecorateOption(DiaOption opt, bool needsSocial)
            {
                if (needsSocial && negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
                {
                    opt.Disable("WorkTypeDisablesOption".Translate(SkillDefOf.Social.label));
                }
                root.options.Add(opt);
            }
        }

        private static DiaOption RequestTraderOption(Map map, TraderCompanyDef companyDef, Pawn negotiator)
        {
            var manager = Find.CurrentMap.GetComponent<TSEOrbitalCompanyManager>();
            var company = manager.GetFirstCompanyOfDef(companyDef);

            var traderLabelColored = companyDef.LabelCap.Colorize(ColoredText.NameColor);

            DiaOption diaReqShip = new("TSE_RequestTraderShip".Translate());

            if (company.IsOnCooldown)
            {
                diaReqShip.disabled = true;
                diaReqShip.disabledReason = "TSE_RequestOnCooldown".Translate(GenDate.ToStringTicksToPeriod(company.cooldownTicks));
                return diaReqShip;
            }

            DiaNode diaTraderSent = new("");
            diaTraderSent.options.Add(OKToRoot(companyDef, negotiator));

            DiaNode diaChooseTraderKind = new("TSE_ChooseTraderShipKind".Translate(traderLabelColored));

            var kinds = companyDef.TraderKinds;

            foreach (CompanyTraderKind kindDefWithCost in kinds)
            {
                var kind = kindDefWithCost.traderKindDef;
                var cost = kindDefWithCost.requestCost;
                if (cost == 0) cost = companyDef.requestCostForAll;

                DiaOption diaCallTrader = new("TSE_RequestSpecificTraderShip".Translate(kind.LabelCap, cost))
                {
                    action = delegate
                    {
                        //if (Utils.TryCallInTraderShip(map, companyDef, kind, Find.FactionManager.FirstFactionOfDef(companyDef.factionDef)))
                        //{
                        int arrivalTicks = companyDef.shipArrivalRangeTicks.RandomInRange;
                        //Log.Message(arrivalTicks.ToString());
                        Find.CurrentMap.GetComponent<TSEOrbitalCompanyManager>().AddShipToQueue(map, companyDef, kind, Find.FactionManager.FirstFactionOfDef(companyDef.factionDef), arrivalTicks);
                        TradeUtility.LaunchSilver(map, cost); // pay silver
                        PurchaseSound.PlayOneShotOnCamera();
                        diaTraderSent.text = $"{"TSE_TraderShipSent".Translate(traderLabelColored)}\n\n{"TSE_ShipWillArriveInHours".Translate(GenDate.ToStringTicksToPeriod(arrivalTicks))}";
                        //}
                    },
                    link = diaTraderSent
                };

                if (!TradeUtility.ColonyHasEnoughSilver(map, cost))
                {
                    diaCallTrader.disabled = true;
                    diaCallTrader.disabledReason = "NeedSilverLaunchable".Translate(cost);
                }
                diaChooseTraderKind.options.Add(diaCallTrader);
            }

            diaChooseTraderKind.options.Add(new DiaOption("(" + "Disconnect".Translate() + ")") { resolveTree = true });
            diaReqShip.link = diaChooseTraderKind;
            return diaReqShip;
        }

        private static DiaOption OKToRoot(TraderCompanyDef companyDef, Pawn negotiator)
        {
            return new DiaOption("OK".Translate())
            {
                linkLateBind = ResetToRoot(companyDef, negotiator)
            };
        }
        public static Func<DiaNode> ResetToRoot(TraderCompanyDef companyDef, Pawn negotiator)
        {
            return () => TraderDialogFor(companyDef, negotiator);
        }
    }
}