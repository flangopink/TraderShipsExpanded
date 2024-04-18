using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TraderShipsExpanded
{
    ////[HotSwap.HotSwappable]
    public sealed class TSEOrbitalCompanyManager(Map map) : MapComponent(map)
    {
        private List<OrbitingCompany> companies = [];
        public List<OrbitingCompany> Companies => companies;

        public Dictionary<QueuedTradeShip, int> shipQueue;
        private List<QueuedTradeShip> shipQueueKeys;

        private int nextCompanyID;

        public int GetNextCompanyID()
        {
            return Utils.GetNextID(ref nextCompanyID);
        }

        public override void ExposeData()
        {
            shipQueue ??= [];

            Scribe_Values.Look(ref nextCompanyID, "nextCompanyID", 0);
            Scribe_Collections.Look(ref companies, "companies", LookMode.Deep);
            Scribe_Collections.Look(ref shipQueue, "shipQueue", LookMode.Deep, LookMode.Value);
            Scribe_Collections.Look(ref shipQueueKeys, "shipQueueKeys", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                for (int i = 0; i < companies.Count; i++)
                {
                    companies[i].manager = this;
                }
            }
        }

        public OrbitingCompany GetFirstCompanyOfDef(TraderCompanyDef def) => companies.FirstOrDefault(x => x.companyDef == def);

        public void AddShipToQueue(Map map, TraderCompanyDef def, TraderKindDef kindDef = null, Faction faction = null, int ticksToArrival = 0)
        {
            if (shipQueue.NullOrEmpty()) shipQueue = [];
            shipQueue.Add(new QueuedTradeShip(map, def, kindDef, faction), ticksToArrival);
            shipQueueKeys = new List<QueuedTradeShip>(shipQueue.Keys);
            GetFirstCompanyOfDef(def).cooldownTicks = def.cooldownRangeTicks.RandomInRange;
        }

        public void RemoveShipFromQueue(QueuedTradeShip key)
        {
            shipQueue.Remove(key);
            shipQueueKeys = new List<QueuedTradeShip>(shipQueue.Keys);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (shipQueue.NullOrEmpty()) return;

            foreach (var key in shipQueueKeys)
            {
                if (shipQueue[key] <= 0)
                {
                    Utils.TryCallInTraderShip(key.map, key.companyDef, key.kindDef, key.faction);
                    RemoveShipFromQueue(key);
                    continue;
                }
                shipQueue[key]--;
            }

            foreach (var company in companies)
            {
                if (company.IsOnCooldown) 
                { 
                    company.cooldownTicks--;
                    if (company.cooldownTicks <= 0) Messages.Message("TSE_TraderIsNowAvailable".Translate(company.FullTitle), MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            var companyDefs = Utils.AllTraderCompanyDefs;
            // this should update the company manager to account for new or deleted companies. i hope this doesn't break anything
            companies.Clear();
            foreach (var def in companyDefs)
            {
                AddShip(new OrbitingCompany(def)); 
            }
        }

        public void AddShip(OrbitingCompany company)
        {
            companies.Add(company);
            company.manager = this;
        }

        public void RemoveShip(OrbitingCompany company)
        {
            companies.Remove(company);
            company.manager = null;
        }
    }
}
