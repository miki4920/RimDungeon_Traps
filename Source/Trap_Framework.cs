using System;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;

namespace RimDungeon
{
		public abstract class Trap_Framework : Building_Trap
		{
			public Trap_Def TrapDef => base.def.GetModExtension<Trap_Def>();
			private bool autoRearm;
			public bool armed = true;

			private bool CanSetAutoRebuild
			{
				get
				{
					return base.Faction == Faction.OfPlayer && this.def.blueprintDef != null && this.def.IsResearchFinished && TrapDef.rebuildable;
				}
			}
			public override Graphic Graphic
			{
				get
				{
					if (!this.armed && TrapDef.rearmable)
					{
						return this.def.building.trapUnarmedGraphicData.GraphicColoredFor(this);
					}
					return base.Graphic;
				}
			}
			public override void ExposeData()
			{
				base.ExposeData();
				Scribe_Values.Look<bool>(ref this.autoRearm, "autoRearm", false, false);
				Scribe_Values.Look<bool>(ref this.armed, "armed", true, false);
			}
			protected override float SpringChance(Pawn p)
			{
				float num = 1f;
				if (!armed && this.TrapDef.rearmable)
				{
					return 0f;
				}
				if (this.KnowsOfTrap(p))
				{
					if (p.Faction == null)
					{
						if (p.RaceProps.Animal)
						{
							num = TrapDef.wildAnimalSpringChance;
							num *= this.def.building.trapPeacefulWildAnimalsSpringChanceFactor;
						}
						else
						{
							num = TrapDef.noFactionSpringChance;
						}
					}
					else if (p.Faction == base.Faction)
					{
						num = TrapDef.sameFactionSpringChance;
					}
					else
					{
						num = 0f;
					}
				}
				num *= this.GetStatValue(StatDefOf.TrapSpringChance, true) * p.GetStatValue(StatDefOf.PawnTrapSpringChance, true);
				return Mathf.Clamp01(num);
			}
			public override ushort PathFindCostFor(Pawn p)
			{
				if (!this.KnowsOfTrap(p) && !TrapDef.slows)
				{
					return 0;
				}
				return (ushort)TrapDef.pathFindCost;
			}
			public override ushort PathWalkCostFor(Pawn p)
			{
				if (!this.KnowsOfTrap(p) && !TrapDef.slows)
				{
					return 0;
				}
				return (ushort)TrapDef.pathWalkCost;
			}
			public new void Spring(Pawn p)
			{
				bool spawned = base.Spawned;
				Map map = base.Map;
				this.SpringSub(p);
				armed = false;
				if (this.def.building.trapDestroyOnSpring)
				{
					if (!base.Destroyed)
					{
						this.Destroy(DestroyMode.Vanish);
					}
					if (spawned)
					{
						this.CheckAutoRebuild(map);
					}
				}
			}
			public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
			{
				bool spawned = base.Spawned;
				Map map = base.Map;
				base.Kill(dinfo, exactCulprit);
				if (spawned)
				{
					this.CheckAutoRebuild(map);
				}
			}
			private void CheckAutoRebuild(Map map)
			{
				if (this.autoRearm && this.CanSetAutoRebuild && map != null && GenConstruct.CanPlaceBlueprintAt(this.def, base.Position, base.Rotation, map, false, null, null, base.Stuff).Accepted)
				{
					GenConstruct.PlaceBlueprintForBuild(this.def, base.Position, map, base.Rotation, Faction.OfPlayer, base.Stuff);
				}
			}

			public override string GetInspectString()
			{
				string text = base.GetInspectString();
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				if (armed)
				{
					text += "TrapArmed".Translate();
				}
				else
				{
					text += "TrapNotArmed".Translate();
				}
				return text;
			}

			public override IEnumerable<Gizmo> GetGizmos()
			{
				foreach (Gizmo gizmo in base.GetGizmos())
				{
					yield return gizmo;
				}
				IEnumerator<Gizmo> enumerator = null;
				if (this.CanSetAutoRebuild)
				{
					yield return new Command_Toggle
					{
						defaultLabel = "CommandAutoRebuild".Translate(),
						defaultDesc = "CommandAutoRebuildDesc".Translate(),
						hotKey = KeyBindingDefOf.Misc3,
						icon = TexCommand.RearmTrap,
						isActive = (() => this.autoRearm),
						toggleAction = delegate ()
						{
							this.autoRearm = !this.autoRearm;
						}
					};
				}
				if (!armed && TrapDef.rearmable && CanBeDesignatedRearm())
				{
					yield return new Command_Action
					{
						defaultLabel = "CommandRearm".Translate(),
						defaultDesc = "CommandRearmDesc".Translate(),
						hotKey = KeyBindingDefOf.Misc4,
						icon = TexCommand.RearmTrap,
						action = AddRearmDesignation
					};
				}
				yield break;
			}
			public void AddRearmDesignation()
			{
				base.Map.designationManager.AddDesignation(new Designation(this, DesignationDefOf.RearmTrap));
			}

			private bool CanBeDesignatedRearm()
			{
				return !armed && Map.designationManager.AllDesignationsOn(this).Where(i => i.def == DesignationDefOf.RearmTrap).FirstOrDefault() == null;
			}

			public void Rearm()
			{
				armed = true;
			}
		}
	}