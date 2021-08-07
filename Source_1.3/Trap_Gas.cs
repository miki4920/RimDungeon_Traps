using System.Collections.Generic;
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
					Trap_Hediff.ApplyHediff(hediffDef, pawn);   
                }
            }
		}
	}
	
}
