using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimDungeon
{
    public class JobDriver_RearmTrap : JobDriver
    {
        private const int RearmTicks = 1125;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.RearmTrap);
            Toil gotoThing = new Toil();
            gotoThing.initAction = delegate ()
            {
                this.pawn.pather.StartPath(base.TargetThingA, PathEndMode.Touch);
            };
            gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return gotoThing;
            yield return Toils_General.Wait(RearmTicks).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Thing thing = this.job.targetA.Thing;
                    Designation designation = base.Map.designationManager.DesignationOn(thing, DesignationDefOf.RearmTrap);
                    if (designation != null)
                    {
                        designation.Delete();
                    }
                    Trap_Framework trap = thing as Trap_Framework;
                    trap.Rearm();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }

    }
}
