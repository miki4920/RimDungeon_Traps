using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimDungeon
{
    public class WorkGiver_RearmTraps : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (Designation des in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.RearmTrap))
            {
                yield return des.target.Thing;
            }
            yield break;
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.ClosestTouch;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing trap, bool forced = false)
        {
            if (pawn.Map.designationManager.DesignationOn(trap, DesignationDefOf.RearmTrap) == null)
            {
                return false;
            }
            LocalTargetInfo target = trap;
            if (!pawn.CanReserve(target, 1, -1, null, forced))
            {
                return false;
            }
            List<Thing> thingList = trap.Position.GetThingList(trap.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                IntVec3 intVec;
                if (thingList[i] != trap && thingList[i].def.category == ThingCategory.Item && (thingList[i].IsForbidden(pawn) || thingList[i].IsInValidStorage() || !HaulAIUtility.CanHaulAside(pawn, thingList[i], out intVec)))
                {
                    return false;
                }
            }
            CompChangeableProjectile compChangeableProjectile = trap.TryGetComp<CompChangeableProjectile>();
            if (compChangeableProjectile != null && !compChangeableProjectile.Loaded)
            {
                StorageSettings allowedShellsSettings = pawn.IsColonist ? trap.TryGetComp<CompChangeableProjectile>().allowedShellsSettings : null;
                ThingFilter thingFilter = allowedShellsSettings.filter;
                Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1, null, false) && (allowedShellsSettings == null || allowedShellsSettings.AllowedToAccept(t));
                Thing shell = GenClosest.ClosestThingReachable(trap.Position, trap.Map, thingFilter.BestThingRequest, PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), 40f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                if (shell == null)
                {
                    return false;
                }
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing trap, bool forced = false)
        {
            List<Thing> thingList = trap.Position.GetThingList(trap.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i] != trap && thingList[i].def.category == ThingCategory.Item)
                {
                    Job job = HaulAIUtility.HaulAsideJobFor(pawn, thingList[i]);
                    if (job != null)
                    {
                        return job;
                    }
                }
            }
            return new Job(JobDefOf.RearmTrapJob, trap);
        }
    }
}
