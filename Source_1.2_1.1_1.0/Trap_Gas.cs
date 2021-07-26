using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimDungeon;

namespace RimDungeon
{
    class Trap_Gas : Gas
	{
		public Hediff_Trap_Def HediffDef => base.def.GetModExtension<Hediff_Trap_Def>();
		public override void Tick()
		{
			base.Tick();
			ApplyHediff();

		}
		public void ApplyHediff()
		{
			if (HediffDef == null || !this.Spawned)
			{
				return;
			}
			List<Thing> thingList = base.Position.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
                if (thingList[i] is Pawn pawn && !this.touchingPawns.Contains(pawn))
                {
                    this.touchingPawns.Add(pawn);
                    this.AddHediffToPawn(pawn, HediffDef.hediff, HediffDef.hediffChance, HediffDef.hediffSeverity);
                }
            }
			for (int j = 0; j < this.touchingPawns.Count; j++)
			{
				Pawn pawn2 = this.touchingPawns[j];
				if (!pawn2.Spawned || pawn2.Position != base.Position)
				{
					this.touchingPawns.Remove(pawn2);
				}
			}
		}

		public void AddHediffToPawn(Pawn pawn, HediffDef hediffToAdd, float chanceToAddHediff, float severityToAdd)
		{
			if (!Rand.Chance(chanceToAddHediff) || severityToAdd <= 0f)
			{
				return;
			}
			float statValue = pawn.GetStatValue(StatDefOf.ToxicSensitivity, true);
			Hediff hediff = HediffMaker.MakeHediff(hediffToAdd, pawn, null);
			hediff.Severity = severityToAdd * statValue;
			if (pawn.health.hediffSet.HasHediff(hediffToAdd, false))
			{
				pawn.health.hediffSet.GetFirstHediffOfDef(hediffToAdd, false).Severity += severityToAdd * statValue;
				return;
			}
			pawn.health.AddHediff(hediff, null, null, null);
		}
        private List<Pawn> touchingPawns = new List<Pawn>();
	}
	
}
