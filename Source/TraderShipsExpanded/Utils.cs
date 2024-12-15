using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TraderShipsExpanded
{
    //[HotSwap.HotSwappable]
    [StaticConstructorOnStartup]
    public static class Utils
    {
        public static List<TraderCompanyDef> AllTraderCompanyDefs;
        public static List<ThingDef> AllShipWrecks = [];
        public static List<ThingDef> AllShipDefs = [];
        public static IEnumerable<FloatMenuOption> FloatMenuOptions;
        public static Texture2D QuestionMarkIcon = ContentFinder<Texture2D>.Get("UI/Overlays/QuestionMark");

        static Utils()
        {
            AllTraderCompanyDefs = DefDatabase<TraderCompanyDef>.AllDefsListForReading;
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.defName.Contains("TSE_TraderShip_")) AllShipDefs.Add(def);
                else if (def.defName.Contains("TSE_Wreck_")) AllShipWrecks.Add(def);
            }
        }

        public static int GetNextID(ref int nextID) // why the fuck would you make this one private but not the others
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Log.Warning("Getting next unique ID during LoadingVars before UniqueIDsManager was loaded. Assigning a random value.");
                return Rand.Int;
            }
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Log.Warning("Getting next unique ID during saving This may cause bugs.");
            }
            int result = nextID;
            nextID++;
            if (nextID == int.MaxValue)
            {
                Log.Warning("Next ID is at max value. Resetting to 0. This may cause bugs.");
                nextID = 0;
            }
            return result;
        }
        public static ThingDef GetRandomShipDef() => AllShipDefs.RandomElement();

        public static ThingDef GetRandomShipWreckDef() => AllShipWrecks.RandomElement();

        public static List<Thing> GetRandomTraderStock(FactionDef factionDef)
        {
            var makerParams = default(ThingSetMakerParams);
            makerParams.traderDef = DefDatabase<TraderKindDef>.AllDefsListForReading.Where(x => x.faction == factionDef).RandomElement(); ;
            return ThingSetMakerDefOf.TraderStock.root.Generate(makerParams);
        }

        public static List<Thing> GetTraderStock(TraderKindDef traderKindDef)
        {
            var makerParams = default(ThingSetMakerParams);
            makerParams.traderDef = traderKindDef;
            return ThingSetMakerDefOf.TraderStock.root.Generate(makerParams);
        }

        public static PawnGroupMaker GetTraderPawnGroupMaker(FactionDef factionDef = null)
        {
            factionDef ??= TSE_DefOf.TSE_Faction_GTC;
            return factionDef.pawnGroupMakers.Where(x => x.kindDef == PawnGroupKindDefOf.Trader).FirstOrDefault();
        }

        public static Quest GetQuest(Map map) => Find.QuestManager.QuestsListForReading.Find((Quest q) => q.QuestLookTargets.Any((GlobalTargetInfo look) => look.HasWorldObject && Find.World.worldObjects.ObjectsAt(map.Tile).Contains(look.WorldObject)));

        public static PawnKindDef GetRandomShipPersonnelPawnKind(PawnGroupMaker personnel, bool combatOnly = false)
        {
            if (personnel == null) 
            {
                Log.Error("Personnel is null. This shouldn't have happened.");
                return null; 
            }

            if (combatOnly)
            {
                return personnel.guards.RandomElement().kind;
            }
            else
            {
                var rand = Rand.Value;
                return rand switch
                {
                    // 20% for slave
                    var _ when rand < 0.2 => personnel.carriers.RandomElement().kind,

                    // 30% for manager
                    var _ when rand < 0.5 => personnel.traders.RandomElement().kind,

                    // 50% for marine, because someone's gotta keep the trader secure, right?
                    var _ when rand < 1 => personnel.guards.RandomElement().kind,

                    _ => null,
                };
            }
        }

        public static DamageDef RandomViolenceDamageTypeNoBullet()
        {
            return Rand.RangeInclusive(0, 3) switch
            {
                0 => DamageDefOf.Cut,
                1 => DamageDefOf.Blunt,
                2 => DamageDefOf.Stab,
                3 => DamageDefOf.Scratch,
                _ => null,
            };
        }

        public static bool CanJoinColony(this Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(TSE_DefOf.DeathAcidifier)) return false;
            return true;
        }

        public static void SendLetterJoinerWithBio(Pawn pawn, bool afterDefense = false)
        {
            //Log.Message("Receiving letter...");
            TaggedString title = "LetterJoinOfferLabel".Translate();
            TaggedString letterText = afterDefense ? "TSE_WantsToJoinDesc".Translate(pawn.Named("PAWN"), Find.FactionManager.OfPlayer.Named("FACTION")).AdjustedFor(pawn) 
                                                   : "TSE_WantsToJoinAfterDefenseDesc".Translate(pawn.Named("PAWN"), Find.FactionManager.OfPlayer.Named("FACTION")).AdjustedFor(pawn);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref title, pawn);
            var letter = (ChoiceLetter_AcceptJoinerWithBio)LetterMaker.MakeLetter(title, letterText, TSE_DefOf.TSE_AcceptJoinerWithBio, new LookTargets(pawn));
            letter.pawn = pawn;
            Find.LetterStack.ReceiveLetter(letter);
            //Log.Message("...Done! " + letter.ToString() + letter.Label.ToString() + letter.Text.ToString());
        }

        public static bool TryCallInTraderShip(Map map, TraderCompanyDef def, TraderKindDef kindDef = null, Faction faction = null, bool stationary = false, bool onMouse = false, IntVec3? atCell = null, bool questShip = false)
        {
            ThingDef shipThingDef = def.shipThingDefs.RandomElement();
            Thing thing = ThingMaker.MakeThing(shipThingDef, null);
            thing.SetFaction(faction);

            //Messages.Message("Def: " + shipThingDef + " Selected texture: " + thing.Graphic, MessageTypeDefOf.SilentInput, false);

            CompTraderShip comp = thing.TryGetComp<CompTraderShip>();
            comp.GenerateInternalTradeShip(map, kindDef ?? def.TraderKinds.RandomElement().traderKindDef, def.shipDepartureTicks);
            if (questShip)
            {
                comp.isQuestShip = true;
            }
            Skyfaller skyfaller = stationary ? null : SkyfallerMaker.MakeSkyfaller(comp.Props.landAnimation, thing);

            if (skyfaller != null)
            {
                skyfaller.CopyThingGraphicOntoSkyfaller(thing);
                skyfaller.DrawColor = thing.DrawColor;
            }

            IntVec3 landingCell = (stationary || onMouse) ? (atCell != null ? (IntVec3)atCell : UI.MouseCell()) : GetBestShipLandingSpot(map, thing);

            GenSpawn.CheckMoveItemsAside(landingCell, default, shipThingDef, map);

            return GenPlace.TryPlaceThing(stationary ? thing : skyfaller, landingCell, map, ThingPlaceMode.Near);
        }

        public static void CopyThingGraphicOntoSkyfaller(this Skyfaller skyfaller, Thing thing)
        {
            GraphicData thingGD = thing.Graphic.data;
            skyfaller.Graphic.Init(new GraphicRequest(typeof(Graphic_Single), thingGD.texPath, thing.Graphic.Shader, thingGD.drawSize, thingGD.color, thingGD.colorTwo, thingGD, 0, thingGD.shaderParameters, thingGD.maskPath));
        }

        public static Area_ShipLandingArea GetLandingArea(this AreaManager areaManager)
        {
            Area_ShipLandingArea area = areaManager.Get<Area_ShipLandingArea>();
            if (area == null)
            {
                areaManager.AllAreas.Add(area = new Area_ShipLandingArea(areaManager));
            }
            return area;
        }

        public static IntVec3 GetBestShipLandingSpot(Map map, Thing ship)
        {
            IntVec2 shuttleSize = ship.def.Size;
            IntVec3 result = IntVec3.Invalid;
            Thing blocker = null;

            var area_LandingZone = map.areaManager.GetLandingArea();
            bool noAreaDesignated = area_LandingZone.ActiveCells.EnumerableNullOrEmpty(); // idk if it will work

            if (ModsConfig.RoyaltyActive && DropCellFinder.TryFindShipLandingArea(map, out result, out blocker))
            {
                if (result.IsValid) return result; // Landing beacons should work with this.
            }

            if (noAreaDesignated)
            {
                Messages.Message("TSE_NoShipLandingAreaFound".Translate(), MessageTypeDefOf.NeutralEvent);
            }
            else area_LandingZone.TryFindShipLandingArea(shuttleSize, out result, out blocker);

            if (!result.IsValid)
            {
                if (blocker != null)
                {
                    Messages.Message("TSE_ShipLandingAreaBlocked".Translate(blocker), blocker, MessageTypeDefOf.NeutralEvent);
                }
                FindCloseLandingSpot(ship.Faction, map, shuttleSize, out result);
            }
            return result;
        }

        public static void FindCloseLandingSpot(Faction faction, Map map, IntVec2? size, out IntVec3 spot)
        {
            IntVec3 intVec = default;
            int num = 0;
            foreach (Building item in map.listerBuildings.allBuildingsColonist.Where((Building x) => x.def.size.x > 1 || x.def.size.z > 1))
            {
                intVec += item.Position;
                num++;
            }
            if (num == 0)
            {
                FindAnyLandingSpot(faction, map, size, out spot);
                return;
            }
            intVec.x /= num;
            intVec.z /= num;
            int num2 = 20;
            float num3 = 999999f;
            spot = default;
            for (int i = 0; i < num2; i++)
            {
                FindAnyLandingSpot(faction, map, size, out var spot2);
                if ((spot2 - intVec).LengthManhattan < num3)
                {
                    num3 = (spot2 - intVec).LengthManhattan;
                    spot = spot2;
                }
            }
        }

        public static void FindAnyLandingSpot(Faction faction, Map map, IntVec2? size, out IntVec3 spot)
        {
            if (!DropCellFinder.FindSafeLandingSpot(out spot, faction, map, 0, 15, 25, size))
            {
                IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
                if (!DropCellFinder.TryFindDropSpotNear(intVec, map, out spot, allowFogged: false, canRoofPunch: false, allowIndoors: false, size))
                {
                    spot = intVec;
                }
                if (!intVec.IsValid) Messages.Message("TSE_ShipLandingFailed".Translate(), MessageTypeDefOf.NegativeEvent);
            }
        }

        public static bool DrawStoredItems(Thing thing, Map map)
        {
            if (thing.def.category == ThingCategory.Item)
            {
                Building edifice = thing.positionInt.GetEdifice(map);
                if (edifice != null)
                {
                    if (TSE_Cache.hiddenStorageDefs.Contains(edifice.def))
                    {
                        return TSE_Cache.storageCache[edifice];
                    }
                }
            }
            return true;
        }
    }
}