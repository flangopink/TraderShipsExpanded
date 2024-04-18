using RimWorld;
using UnityEngine;
using Verse;

namespace TraderShipsExpanded
{
    public abstract class Designator_ShipLandingArea : Designator_Cells
    {
        private readonly DesignateMode mode;

        public override int DraggableDimensions => 2;

        public override bool DragDrawMeasurements => true;

        public Designator_ShipLandingArea(DesignateMode m)
        {
            mode = m;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (c.InNoZoneEdgeArea(Map)) return false;
            return Map.areaManager.GetLandingArea()[c] != (mode == DesignateMode.Add);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Map.areaManager.GetLandingArea()[c] = mode == DesignateMode.Add;
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            Map.areaManager.GetLandingArea().MarkForDraw();
        }
    }

    public class Designator_ShipLandingAreaClear : Designator_ShipLandingArea
    {
        public Designator_ShipLandingAreaClear() : base(DesignateMode.Remove)
        {
            defaultLabel = "TSE_ShipLandingAreaClear".Translate();
            defaultDesc = "TSE_ShipLandingAreaClearDescription".Translate();
            icon = ContentFinder<Texture2D>.Get("TSE/UI/ShipLandingAreaClear");
            soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
            soundDragChanged = null;
            soundSucceeded = SoundDefOf.Designate_ZoneDelete;
        }
    }

    public class Designator_ShipLandingAreaExpand : Designator_ShipLandingArea
    {
        public Designator_ShipLandingAreaExpand() : base(DesignateMode.Add)
        {
            defaultLabel = "TSE_ShipLandingArea".Translate();
            defaultDesc = "TSE_ShipLandingAreaDescription".Translate();
            icon = ContentFinder<Texture2D>.Get("TSE/UI/ShipLandingArea");
            soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
            soundDragChanged = null;
            soundSucceeded = SoundDefOf.Designate_ZoneAdd;
        }
    }
}
