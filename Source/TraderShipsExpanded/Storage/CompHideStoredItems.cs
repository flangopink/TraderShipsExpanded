using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TraderShipsExpanded
{
    public class CompHideStoredItems : ThingComp
    {
        public bool showItems;
        //private SectionLayer_ThingsGeneral layer = new

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!TSE_Cache.storageCache.ContainsKey(parent)) 
                TSE_Cache.storageCache.Add(parent, showItems);
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            TSE_Cache.storageCache.Remove(parent);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            yield return new Command_Action()
            {
                defaultLabel = "Show/Hide Items",
                defaultDesc = "Toggle visibility of stored items.",
                icon = Utils.QuestionMarkIcon,
                action = () => 
                { 
                    showItems = !showItems;
                    if (TSE_Cache.storageCache.ContainsKey(parent))
                        TSE_Cache.storageCache[parent] = showItems;

                    /*foreach (var item in ((Building_Storage)parent).slotGroup.HeldThings)
                    {
                        item.Print(item.);
                    }*/
                }
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref showItems, "showItems", false);
        }
    }
}
