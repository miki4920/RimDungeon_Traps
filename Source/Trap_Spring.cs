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
			CompExplosive explosive = base.GetComp<CompExplosive>();
			if(explosive != null)
            {
				SpringExplosiveTrap();
			}
			else if(TrapDef.gas != null)
            {
				SpringGasTrap();
            }
			else
            {
				if (p == null)
				{
					return;
				}
				SpringTrap(p);
			}
			
		}
		protected void SpringTrap(Pawn p)
        {
			float num = this.GetStatValue(StatDefOf.TrapMeleeDamage, true) * DamageRandomFactorRange.RandomInRange / this.TrapDef.numberOfAttacks;
			float armorPenetration = num * TrapDef.armorPenetration;
			int num2 = 0;
			if (TrapDef.hediff.hediffToAdd != null)
			{
				float rand = Rand.Value;
				if (rand <= TrapDef.hediff.hediffChance)
				{
					Hediff effectOnPawn = p.health?.hediffSet?.GetFirstHediffOfDef(TrapDef.hediff.hediffToAdd);
					float severity = TrapDef.hediff.hediffSeverity;
					if (effectOnPawn != null)
					{
						effectOnPawn.Severity += severity;
					}
					else
					{
						Hediff hediff = HediffMaker.MakeHediff(TrapDef.hediff.hediffToAdd, p);
						hediff.Severity = severity;
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

		protected void SpringExplosiveTrap()
        {
			base.GetComp<CompExplosive>().StartWick(null);
		}
		protected void SpringGasTrap()
        {
			GenExplosion.DoExplosion(base.Position, base.Map, TrapDef.gas.radius, TrapDef.gas.damageDef, this, 0, 0f, TrapDef.gas.damageDef.soundExplosion, null, null, null, TrapDef.gas.gasDef, 1f, 1, false, null, 0f, 1, 0f, false, null, null);
		}
		private static readonly FloatRange DamageRandomFactorRange = new FloatRange(0.8f, 1.2f);
	}
}
