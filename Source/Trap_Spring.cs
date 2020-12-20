using RimWorld;
using System;
using Verse;
using Verse.Sound;


namespace RimDungeon
{
    class Trap_Spring : Trap_Framework
    {
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				SoundDefOf.TrapArm.PlayOneShot(new TargetInfo(base.Position, map, false));
			}
		}
		protected override void SpringSub(Pawn p)
		{
			SoundDefOf.TrapSpring.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			if (p == null)
			{
				return;
			}
			float num = this.GetStatValue(StatDefOf.TrapMeleeDamage, true) * DamageRandomFactorRange.RandomInRange / this.TrapDef.numberOfAttacks;
			float armorPenetration = num * TrapDef.armorPenetration;
			int num2 = 0;
			if (TrapDef.hediffToAdd != null) {
				float rand = Rand.Value;
				if(rand <= TrapDef.addHediffChance)
                {
					Hediff effectOnPawn = p.health?.hediffSet?.GetFirstHediffOfDef(TrapDef.hediffToAdd);
					float randomSeverity = Rand.Range(0.15f, 0.30f);
					if (effectOnPawn != null)
					{
						effectOnPawn.Severity += randomSeverity;
					}
					else
					{
						Hediff hediff = HediffMaker.MakeHediff(TrapDef.hediffToAdd, p);
						hediff.Severity = randomSeverity;
						p.health.AddHediff(hediff);
					}
				}
			}
			while ((float)num2 < this.TrapDef.numberOfAttacks)
			{
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Stab, num, armorPenetration, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
				DamageWorker.DamageResult damageResult = p.TakeDamage(dinfo);
				if (num2 == 0)
				{
					BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(p, RulePackDefOf.DamageEvent_TrapSpike, null);
					Find.BattleLog.Add(battleLogEntry_DamageTaken);
					damageResult.AssociateWithLog(battleLogEntry_DamageTaken);
				}
				num2++;
			}
		}
		private static readonly FloatRange DamageRandomFactorRange = new FloatRange(0.8f, 1.2f);
	}
}
