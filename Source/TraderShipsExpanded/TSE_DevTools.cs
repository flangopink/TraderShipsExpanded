using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using LudeonTK;

namespace TraderShipsExpanded
{
    //[HotSwap.HotSwappable]
    public static class TSE_DevTools
    {
        [DebugAction("Trader Ships Expanded", "Spawn trade ship...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static List<DebugActionNode> DebugSpawnTraderShip()
        {
            List<DebugActionNode> list = [];
            foreach (TraderCompanyDef def in DefDatabase<TraderCompanyDef>.AllDefs)
            {
                DebugActionNode debugActionNode = new(def.defName);
                debugActionNode.AddChild(new DebugActionNode("Incoming", DebugActionType.ToolMap, delegate ()
                {
                    Utils.TryCallInTraderShip(Find.CurrentMap, def, null, null, false, true);
                }));
                debugActionNode.AddChild(new DebugActionNode("Stationary", DebugActionType.ToolMap, delegate ()
                {
                    Utils.TryCallInTraderShip(Find.CurrentMap, def, null, null, true);
                }));
                list.Add(debugActionNode);
            }
            return list;
        }

        [DebugAction("Trader Ships Expanded", "Call in trade ship...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static List<DebugActionNode> DebugCallInTraderShip()
        {
            List<DebugActionNode> list = [];
            foreach (TraderCompanyDef def in DefDatabase<TraderCompanyDef>.AllDefs)
            {
                list.Add(new DebugActionNode(def.defName, DebugActionType.Action, delegate ()
                {
                    Utils.TryCallInTraderShip(Find.CurrentMap, def);
                }));
            }
            return list;
        }

        [DebugAction("Trader Ships Expanded", "Call in trade ship (Random faction)...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static List<DebugActionNode> DebugCallInTraderShipRandomFaction()
        {
            List<DebugActionNode> list = [];
            foreach (TraderCompanyDef def in DefDatabase<TraderCompanyDef>.AllDefs)
            {
                list.Add(new DebugActionNode(def.defName, DebugActionType.Action, delegate ()
                {
                    var faction = Find.FactionManager.RandomNonHostileFaction();
                    //Log.Message(faction.ToString());
                    Utils.TryCallInTraderShip(Find.CurrentMap, def, null, faction);
                }));
            }
            return list;
        }

        /*[DebugAction("Trader Ships Expanded", "Log passing ships", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void LogPassingShips()
        {
            var allShips = Find.CurrentMap.passingShipManager.passingShips;
            var allTSEShips = Find.CurrentMap.GetComponent<TSEOrbitalCompanyManager>().Companies;
            var allShipsCount = allShips.Count;
            var regularShipsCount = allShips.Count(x => x.GetType() == typeof(PassingShip));
            var TSELandedShipsCount = allShips.Count(x => x.GetType() == typeof(LandedShip));
            var TSEShipsCount = allTSEShips.Count;

            StringBuilder sb = new ();
            sb.Append("All passing ships: ");
            foreach (var ship in allShips) sb.AppendInNewLine(ship.FullTitle);
            sb.Append("All TSE ships: ");
            foreach (var ship in allTSEShips) sb.AppendInNewLine(ship.FullTitle);

            Log.Message($"Total passing ships: {allShipsCount}\nRegular passing ships: {regularShipsCount}\nTSE landed ships: {TSELandedShipsCount}\nTSE company ships: {TSEShipsCount}");
            Log.Message(sb.ToString());
        }*/

        [DebugAction("Trader Ships Expanded", "Skip all cooldowns", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SkipAllCooldowns()
        {
            foreach (OrbitingCompany company in Find.CurrentMap.GetComponent<TSEOrbitalCompanyManager>().Companies)
            {
                if (company.cooldownTicks > 0) company.cooldownTicks = 1;
            }
        }

        [DebugAction("Trader Ships Expanded", "Try receive recruitment letter", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TryRecruitWithBioLetter(Pawn pawn)
        {
            Log.Message("bonk");
            Utils.SendLetterJoinerWithBio(pawn);
        }

        [DebugAction("Trader Ships Expanded", "Get building region", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GetBuildingRegion()
        {
            var r = UI.MouseCell().GetFirstBuilding(Find.CurrentMap)?.GetRegion();
            Log.Message(r);
        }
    }
}
