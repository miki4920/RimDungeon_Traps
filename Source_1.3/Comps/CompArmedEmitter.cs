using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace RimDungeon
{
    class CompArmedEmitter : ThingComp
    {
		private CompProperties_ArmedEmitter Props
		{
			get
			{
				return (CompProperties_ArmedEmitter) this.props;
			}
		}
		public override void CompTick()
		{
			Trap_Framework trap = (Trap_Framework) this.parent;
			if(trap.TrapDef.rearmable && !trap.armed)
            {
				return;
            }
			if (this.Props.emissionInterval != -1)
			{
				if (this.ticksSinceLastEmitted >= this.Props.emissionInterval)
				{
					this.Emit();
					this.ticksSinceLastEmitted = 0;
					return;
				}
				this.ticksSinceLastEmitted++;
			}
		}
		protected void Emit()
		{
			FleckMaker.Static(this.parent.DrawPos + this.Props.offset, this.parent.MapHeld, this.Props.fleck, 1f);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.ticksSinceLastEmitted, ((this.Props.saveKeysPrefix != null) ? (this.Props.saveKeysPrefix + "_") : "") + "ticksSinceLastEmitted", 0, false);
		}
		public int ticksSinceLastEmitted;

	}
}
