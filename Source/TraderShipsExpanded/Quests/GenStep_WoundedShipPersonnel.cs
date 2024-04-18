using System.Collections.Generic;
using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace TraderShipsExpanded
{
    public class GenStep_WoundedShipPersonnel : GenStep
    {
        public override int SeedPart => 69696902;

        public override void Generate(Map map, GenStepParams parms)
        {
            Quest thisQuest = Find.QuestManager.QuestsListForReading.Find((Quest q) => q.QuestLookTargets.Any((GlobalTargetInfo look) => look.HasWorldObject && Find.World.worldObjects.ObjectsAt(map.Tile).Contains(look.WorldObject)));
            Log.Message(thisQuest.ToString() + " id = " + thisQuest.id);

            thisQuest.TryGetFirstPartOfType(out QuestPart_Hyperlinks part);
            if (part == null) Log.Error("what the fuck");

            Pawn askerPawn = part.pawns.FirstOrDefault();

            Faction faction = Find.FactionManager.FirstFactionOfDef(TSE_DefOf.TSE_Faction_GTC);

            // Assuming the hostiles have already spawned, because this should run after the pirates GenStep. Not sure though.
            Lord lord = null;
            if (map.mapPawns.AllPawnsSpawned.Any((Pawn p) => p.Faction.HostileTo(faction)))
            {
                lord = LordMaker.MakeNewLord(faction, new LordJob_DefendPointThenLeave(map.Center, 10f, 28f), map);
            }

            var personnel = Utils.GetShipPersonnel();
            bool askerPawnGenerated = false;

            for (int i = 0; i < Rand.RangeInclusive(3, 5); i++)
            {
                CellFinder.TryFindRandomCellNear(map.Center, map, 8, (IntVec3 c) => c.Walkable(map) && !map.fogGrid.IsFogged(c), out var result);

                Pawn pawn;
                if (!askerPawnGenerated)
                {
                    pawn = askerPawn;
                    askerPawnGenerated = true;
                }
                else pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Utils.GetRandomShipPersonnelPawnKind(personnel), faction));
                GenSpawn.Spawn(pawn, result, map);

                switch (Rand.RangeInclusive(1, 5)) // 20% Dead, 20% Downed, 60% Wounded (likely will end up downed anyway)
                {
                    case 1:
                        HealthUtility.DamageUntilDead(pawn);
                        break;
                    case 2:
                        HealthUtility.DamageUntilDowned(pawn);
                        break;
                    default:
                        AddRandomDamageTo(pawn, Rand.RangeInclusive(20, 50));
                        break;
                }

                if (!pawn.Dead)
                {
                    if (!pawn.Downed && pawn.CanJoinColony() && Rand.Value < 0.20) // 20% for a non-downed non-marine pawn to join upon arrival (which is rare on its own)
                    {
                        //pawn.SetFaction(Faction.OfPlayer);
                        //Find.LetterStack.ReceiveLetter("TSE_ShipCrashRecruitSuccess".Translate(), "TSE_ShipCrashRecruitSuccessDesc".Translate(pawn.Label), LetterDefOf.PositiveEvent, new LookTargets(pawn));

                        //Log.Message("Recruitment roll for " + pawn.Label + "... Success!");
                        Utils.SendLetterJoinerWithBio(pawn);
                        continue;
                    }
                    //else Log.Warning("Recruitment roll for " + pawn.Label + "... Failed!");
                    lord?.AddPawn(pawn);
                }
            }
        }

        private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
        {
            return from x in bodyModel.GetNotMissingParts()
                   where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
                   select x;
        }

        private void AddRandomDamageTo(Pawn p, int damageAmount) // not even gonna question what the hell is going on in here.
        {
            HediffSet hediffSet = p.health.hediffSet;
            p.health.forceDowned = true;
            IEnumerable<BodyPartRecord> source = from x in HittablePartsViolence(hediffSet)
                                                 where !p.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
                                                 select x;
            int totalDamage = 0;
            while (totalDamage < damageAmount && source.Any())
            {
                BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
                int partHealth = Mathf.RoundToInt(hediffSet.GetPartHealth(bodyPartRecord)) - 3;

                if (partHealth >= 8)
                {
                    DamageDef damageDef = (bodyPartRecord.depth != BodyPartDepth.Outside) ? DamageDefOf.Blunt : Utils.RandomViolenceDamageTypeNoBullet();

                    int dealtDamage = Rand.RangeInclusive(Mathf.RoundToInt(partHealth * 0.65f), partHealth);

                    HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);

                    if (!p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, dealtDamage))
                    {
                        DamageInfo dinfo = new(damageDef, dealtDamage, 999f);
                        dinfo.SetAllowDamagePropagation(false);
                        p.TakeDamage(dinfo);
                        totalDamage += dealtDamage;
                    }
                }
            }
        }
    }
}