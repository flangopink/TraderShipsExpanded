using RimWorld;
using System;
using Verse;

namespace TraderShipsExpanded
{
    public class OrbitingCompany : IExposable, ICommunicable, ILoadReferenceable
    {
        public TSEOrbitalCompanyManager manager;

        public TraderCompanyDef companyDef;

        private Faction faction;
        public Faction Faction => faction;

        public string name = "Nameless";
        protected int loadID = -1;

        public virtual string FullTitle => name;// + " (" + shipDef.label + ")";

        public bool IsOnCooldown => cooldownTicks > 0;
        public int cooldownTicks;

        public OrbitingCompany() 
        { 
        }

        public OrbitingCompany(TraderCompanyDef def, FactionDef factionDef = null)
        { 
            companyDef = def;
            name = def.label;
            faction = Find.FactionManager.FirstFactionOfDef(factionDef);
            loadID = Find.UniqueIDsManager.GetNextPassingShipID();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref loadID, "loadID", 0);
            Scribe_Values.Look(ref cooldownTicks, "cooldownTicks", 0);
            Scribe_References.Look(ref faction, "faction");
            Scribe_Defs.Look(ref companyDef, "companyDef");
        }
        public virtual string GetCallLabel() => name;// + " (" + shipDef.label + ")";

        public string GetInfoText() => FullTitle;

        Faction ICommunicable.GetFaction() => null;

        protected virtual AcceptanceReport CanCommunicateWith(Pawn negotiator)
        {
            if (negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
            {
                return new AcceptanceReport("WorkTypeDisablesOption".Translate(SkillDefOf.Social.label));
            }
            else return negotiator.CanTradeWith(Faction).Accepted;
        }

        public void TryOpenComms(Pawn negotiator)
        {
            {
                Dialog_Negotiation dialog_Negotiation = new(negotiator, this, TraderDialogMaker.TraderDialogFor(companyDef, negotiator), true)
                {
                    soundAmbient = SoundDefOf.RadioComms_Ambience
                };
                Find.WindowStack.Add(dialog_Negotiation);
            }
        }

        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
        {
            string label = "CallOnRadio".Translate(GetCallLabel());
            if (faction != null) label += " (" + faction.PlayerRelationKind.GetLabelCap() + ", " + faction.PlayerGoodwill.ToStringWithSign() + ")";

            Action action = null;
            AcceptanceReport canCommunicate = CanCommunicateWith(negotiator);
            if (!canCommunicate.Accepted)
            {
                if (!canCommunicate.Reason.NullOrEmpty())
                {
                    action = delegate
                    {
                        Messages.Message(canCommunicate.Reason, console, MessageTypeDefOf.RejectInput, historical: false);
                    };
                }
            }
            else
            {
                action = delegate
                {
                    console.GiveUseCommsJob(negotiator, this);
                };
            }
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, companyDef.Icon, companyDef.iconColor, MenuOptionPriority.InitiateSocial), negotiator, console);
        }

        public string GetUniqueLoadID() => "TSEShip_" + loadID;
    }
}
