using RimWorld;
using Verse;

namespace RimDungeon_Traps
{
	public class Building_Trap_Explosive : Dungeon_Trap_Framework
	{
		protected override void SpringSub(Pawn p)
		{
			base.GetComp<CompExplosive>().StartWick(null);
		}
	}
}

