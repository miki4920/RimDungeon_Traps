using RimWorld;
using Verse;

namespace RimDungeon
{
	public class Trap_Explosive : Trap_Framework
	{
		protected override void SpringSub(Pawn p)
		{
			base.GetComp<CompExplosive>().StartWick(null);
		}
	}
}

