using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TraderShipsExpanded
{
    public class GenStep_ShipCrashSite : GenStep
    {
        public override int SeedPart => 69696901;

        public override void Generate(Map map, GenStepParams parms)
        {
            IntVec3 shipPos = IntVec3.Invalid;
            List<Thing> things = Utils.GetRandomTraderStock(TSE_DefOf.TSE_Faction_GTC);

            int num = 2;
            while (shipPos == IntVec3.Invalid)
            {
                CellFinder.TryFindRandomCellNear(map.Center, map, num, (IntVec3 c) => c.Walkable(map) && !c.Roofed(map) && !map.fogGrid.IsFogged(c), out shipPos);
                num++;
            }

            Thing wreck = ThingMaker.MakeThing(Utils.GetRandomShipWreckDef());
            wreck.SetFactionDirect(map.ParentFaction);
            wreck.HitPoints = (int)(wreck.MaxHitPoints * Rand.Range(0.25f, 0.75f));
            GenSpawn.Spawn(wreck, shipPos, map);

            var cellsRadius = GenRadial.RadialCellsAround(shipPos, 8f, true).Concat(GenRadial.RadialCellsAround(new IntVec3(shipPos.x - 14, 0, shipPos.z), 8f, true)).Where(c => !c.Impassable(map));
            IntVec3 shipPosShiftedX = new(shipPos.x - 8, 0, shipPos.z);

            for (int j = 0; j < 150; j++)
            {
                SpawnRandomFilth(map, cellsRadius.RandomElement());
            }
            for (int j = 0; j < 40; j++)
            {
                StartRandomFire(map, cellsRadius.RandomElement(), wreck);
            }

            for (int i = 0; i < Rand.RangeInclusive(12, 25); i++)
            {
                CellFinder.TryFindRandomCellNear(shipPosShiftedX, map, 12, (IntVec3 c) => c.Walkable(map) && !c.Impassable(map) && !map.fogGrid.IsFogged(c), out var spawnCell);

                Thing thing;
                if (Rand.Value < 0.33) // 33% random loot, 67% slag
                {
                    thing = things.RandomElement();
                    thing.stackCount = Rand.RangeInclusive(1, thing.def.stackLimit / 2); // avoid over-limit errors
                    thing.SetForbidden(true, false);
                }
                else thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);

                //Log.Warning("Tried to spawn " + thing.Label + " at " + spawnCell);
                if (!thing.Spawned) GenSpawn.Spawn(thing, spawnCell, map, WipeMode.VanishOrMoveAside);
            }
        }

        private void SpawnRandomFilth(Map map, IntVec3 cell)
        {
            if (!cell.InBounds(map)) return;
            var def = Rand.RangeInclusive(0, 3) switch
            {
                0 => ThingDefOf.Filth_Dirt,
                1 => ThingDefOf.Filth_MachineBits,
                2 => ThingDefOf.Filth_RubbleRock,
                3 => ThingDefOf.Filth_Trash,
                _ => null
            };
            GenSpawn.Spawn(ThingMaker.MakeThing(def), cell, map);
        }

        private void StartRandomFire(Map map, IntVec3 cell, Thing instigator)
        {
            if (!cell.InBounds(map)) return;

            float fireSize = Rand.Range(0.1f, 0.8f);

            bool cellHasThing = cell.TryGetFirstThing(map, out Thing thing);

            if (cellHasThing && Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(thing)))
		    {
                thing.TryAttachFire(fireSize, instigator);
                return;
            }
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Filth_Fuel), cell, map);

            FireUtility.TryStartFireIn(cell, map, fireSize, instigator);

            //if (!success) Log.Warning("Failed to start fire in " + cell);
        }
    }
}