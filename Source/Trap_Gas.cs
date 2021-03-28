using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimDungeon;

namespace Rimdungeon_Traps
{
    public class Trap_Gas : Gas
    {
		/*public Hediff_Trap_Def Def
		{
			get
			{
				return this.def as Hediff_Trap_Def;
			}
		}

		public override void Tick()
		{
			if (base.Destroyed)
			{
				return;
			}
			base.Tick();
			this.tickCounter--;
			if (this.tickCounter <= 0)
			{
				this.ApplyHediff();
				this.tickCounter = this.ticksPerApplication;
			}
		}

		public void ApplyHediff()
		{
			if (this.Def.hediffToAdd == null)
			{
				return;
			}
			List<Thing> thingList = base.Position.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Pawn pawn = thingList[i] as Pawn;
				if (pawn != null && !this.touchingPawns.Contains(pawn))
				{
					this.touchingPawns.Add(pawn);
					this.AddHediffToPawn(pawn, this.Def.hediffToAdd, this.Def.hediffChance, this.Def.hediffSeverity);
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

		// Token: 0x06000040 RID: 64 RVA: 0x00003320 File Offset: 0x00001520
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
		private int tickCounter = 250;
		private int ticksPerApplication = 250;*/
	}
}
