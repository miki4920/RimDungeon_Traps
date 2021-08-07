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
			else if(this.def.HasModExtension<Gas_Trap_Def>())
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
			if (this.def.HasModExtension<Hediff_Trap_Def>())
			{
				Hediff_Trap_Def hediff = this.def.GetModExtension<Hediff_Trap_Def>();
				Trap_Hediff.ApplyHediff(hediff, p);
				
			}

			float num = this.GetStatValue(StatDefOf.TrapMeleeDamage, true) * DamageRandomFactorRange.RandomInRange / this.TrapDef.numberOfAttacks;
			float armorPenetration = num * TrapDef.armorPenetration;
			int num2 = 0;
			while ((float)num2 < this.TrapDef.numberOfAttacks)
			{
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Stab, num, armorPenetration, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true);
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
			Gas_Trap_Def gas_Trap = this.def.GetModExtension<Gas_Trap_Def>();
			GenExplosion.DoExplosion(base.Position, base.Map, gas_Trap.radius, gas_Trap.damageDef, this, 0, 0f, gas_Trap.damageDef.soundExplosion, null, null, null, gas_Trap.gas, 1f, 1, false, null, 0f, 1, 0f, false, null, null);
		}
		private static readonly FloatRange DamageRandomFactorRange = new FloatRange(0.8f, 1.2f);
	}
}
