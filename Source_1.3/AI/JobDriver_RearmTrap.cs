using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace RimDungeon
{
    public class JobDriver_RearmTrap : JobDriver
    {
        private const int RearmTicks = 1125;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        private static bool TrapNeedsLoading(Building b)
        {
            Trap_Framework trap = b as Trap_Framework;
            if (trap == null)
            {
                return false;
            }
            CompChangeableProjectile compChangeableProjectile = trap.TryGetComp<CompChangeableProjectile>();
            return compChangeableProjectile != null && !compChangeableProjectile.Loaded;
        }

        public static Thing FindShellForTrap(Pawn pawn, Trap_Framework trap)
        {
            StorageSettings allowedShellsSettings = pawn.IsColonist ? trap.TryGetComp<CompChangeableProjectile>().allowedShellsSettings : null;
            Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1, null, false) && (allowedShellsSettings == null || allowedShellsSettings.AllowedToAccept(t));
            return GenClosest.ClosestThingReachable(trap.Position, trap.Map, ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), 40f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
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
            Toil loadIfNeeded = new Toil();
            loadIfNeeded.initAction = delegate ()
            {
                Pawn actor = loadIfNeeded.actor;
                Building building = (Building)actor.CurJob.targetA.Thing;
                Trap_Framework trap = building as Trap_Framework;
                if (!JobDriver_RearmTrap.TrapNeedsLoading(building))
                {
                    this.JumpToToil(gotoThing);
                    return;
                }
                Thing thing = JobDriver_RearmTrap.FindShellForTrap(this.pawn, trap);
                if (thing == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                }
                actor.CurJob.targetB = thing;
                actor.CurJob.count = 1;
            };
            yield return loadIfNeeded;
            yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil loadShell = new Toil();
            loadShell.initAction = delegate ()
            {
                Pawn actor = loadShell.actor;
                Trap_Framework trap = ((Building)actor.CurJob.targetA.Thing) as Trap_Framework;
                trap.TryGetComp<CompChangeableProjectile>().LoadShell(actor.CurJob.targetB.Thing.def, 1);
                actor.carryTracker.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            };
            yield return loadShell;
            yield return gotoThing;
            yield return Toils_General.Wait(RearmTicks).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            Toil rearmTrap = new Toil();
            rearmTrap.initAction = delegate ()
               {
                   Pawn actor = loadIfNeeded.actor;
                   Building building = (Building)actor.CurJob.targetA.Thing;
                   Trap_Framework trap = building as Trap_Framework;
                   Thing thing = this.job.targetA.Thing;
                   Designation designation = base.Map.designationManager.DesignationOn(thing, DesignationDefOf.RearmTrap);
                   if (designation != null)
                   {
                       designation.Delete();
                   }
                   trap.Rearm();
               };
            yield return rearmTrap;
            yield break;
        }

    }
}
