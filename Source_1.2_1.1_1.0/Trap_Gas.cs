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
		public Hediff_Trap_Def hediffDef => base.def.GetModExtension<Hediff_Trap_Def>();
		public override void Tick()
		{
			base.Tick();
			ApplyHediff();

		}
		public void ApplyHediff()
		{
			if (hediffDef == null || !this.Spawned)
			{
				return;
			}
			List<Thing> thingList = base.Position.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
                if (thingList[i] is Pawn pawn)
                {
					if (!hediffDef.insectOnly) {
						this.AddHediffToPawn(pawn, hediffDef.hediff, hediffDef.hediffChance, hediffDef.hediffSeverity);
					}
					else if(hediffDef.insectOnly && pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid)
                    {
						this.AddHediffToPawn(pawn, hediffDef.hediff, hediffDef.hediffChance, hediffDef.hediffSeverity);
					}
                    
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
			if (hediffDef.insectOnly)
            {
				hediff.Severity = severityToAdd;
			}
			else
            {
				hediff.Severity = severityToAdd * statValue;
            }
			if (pawn.health.hediffSet.HasHediff(hediffToAdd, false))
			{
				pawn.health.hediffSet.GetFirstHediffOfDef(hediffToAdd, false).Severity += hediff.Severity;
				return;
			}
			pawn.health.AddHediff(hediff, null, null, null);
		}
	}
	
}
