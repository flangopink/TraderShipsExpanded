﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Verse;

namespace TraderShipsExpanded
{
    //[HotSwap.HotSwappable]
    public class LandedShip : TradeShip, ITrader
    {
        private Map map;

        public LandedShip() {}

        public LandedShip(Map map, TraderKindDef def, Faction faction = null) : base(def, faction)
        {
            this.map = map;
            passingShipManager = map.passingShipManager;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, "map");
        }

        public override void PassingShipTick()
        {
            base.PassingShipTick();

            passingShipManager ??= map.passingShipManager;
        }

        public override void Depart()
        {

        }

        public new void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);

            if (!GenPlace.TryPlaceThing(thing, playerNegotiator.Position, Map, ThingPlaceMode.Near))
            {
                Log.Error(string.Concat(new object[] { "Could not place bought thing ", thing, " at ", playerNegotiator.Position }));
                thing.Destroy(DestroyMode.Vanish);
            }
        }

        public new IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
        {
            HashSet<Thing> alreadyListed = [];

            // technically, this is pawn selling things to itself, but the code doesn't seem to care, and
            // using this method instead of just listing all things allows for compatibility with storage
            // mods that patch Pawn_TraderTracker.ColonyThingsWillingToBuy.                                                 // thank you, very cool
            foreach (Thing thing in new Pawn_TraderTracker(playerNegotiator).ColonyThingsWillingToBuy(playerNegotiator))
            {
                alreadyListed.Add(thing);
                yield return thing;
            }

            foreach (Pawn pawn in TradeUtility.AllSellableColonyPawns(playerNegotiator.Map).Where(x => !x.Downed && ReachableForTrade(playerNegotiator, x)))
            {
                if (!alreadyListed.Contains(pawn))
                    yield return pawn;
            }

            yield break;
        }

        private bool ReachableForTrade(Pawn pawn, Thing thing)
        {
            return pawn.Map == thing.Map && pawn.Map.reachability.CanReach(pawn.Position, thing, PathEndMode.Touch, TraverseMode.PassDoors, Danger.Some);
        }
    }
}
