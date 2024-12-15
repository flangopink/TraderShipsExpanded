﻿using Verse;
using Verse.AI.Group;

namespace TraderShipsExpanded
{
    public class LordJob_DefendShip : LordJob_DefendPoint
    {
        public LordJob_DefendShip() : base()
        {
        }

        public LordJob_DefendShip(IntVec3 point, float? wanderRadius = null, float? defendRadius = null, bool isCaravanSendable = false, bool addFleeToil = true) : base(point, wanderRadius, defendRadius, isCaravanSendable, addFleeToil)
        {
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new();
            stateGraph.AddToil(new LordToil_DefendShip(point, defendRadius, wanderRadius));
            return stateGraph;
        }
    }
}
