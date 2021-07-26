using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace RimDungeon
{
    class Designator_RearmTrap : Designator
    {
        public Designator_RearmTrap()
        {
            this.defaultLabel = "CommandRearm".Translate();
            this.defaultDesc = "CommandRearmDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/RearmTrap", true);
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_Claim;
        }

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            if (!this.RearmablesInCell(c).Any<Thing>())
            {
                return false;
            }
            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            foreach (Thing t in this.RearmablesInCell(c))
            {
                this.DesignateThing(t);
            }
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            Trap_Framework trap = t as Trap_Framework;
            return trap != null && !trap.armed && base.Map.designationManager.DesignationOn(trap, DesignationDefOf.RearmTrap) == null;
        }

        public override void DesignateThing(Thing t)
        {
            base.Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.RearmTrap));
        }

        private IEnumerable<Thing> RearmablesInCell(IntVec3 c)
        {
            if (c.Fogged(base.Map))
            {
                yield break;
            }
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (this.CanDesignateThing(thingList[i]).Accepted)
                {
                    yield return thingList[i];
                }
            }
            yield break;
        }
    }
}
