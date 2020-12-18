using System;

namespace RimDungeon_Traps
{
	public class RimDungeon_Trap_Framework
	{
		public abstract class Dungeon_Trap_Framework : Building
		{
			public RimDungeon_Trap_Def TrapDef => base.def.GetModExtension<RimDungeon_Trap_Def>();
			private bool autoRearm;
			public bool armed = true;
			private List<Pawn> touchingPawns = new List<Pawn>();

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
				Scribe_Collections.Look<Pawn>(ref this.touchingPawns, "testees", LookMode.Reference, Array.Empty<object>());
			}
			public override void SpawnSetup(Map map, bool respawningAfterLoad)
			{
				base.SpawnSetup(map, respawningAfterLoad);
				if (!respawningAfterLoad)
				{
					this.autoRearm = (this.CanSetAutoRebuild && map.areaManager.Home[base.Position]);
				}
			}
			public override void Tick()
			{
				if (base.Spawned)
				{
					List<Thing> thingList = base.Position.GetThingList(base.Map);
					for (int i = 0; i < thingList.Count; i++)
					{
						Pawn pawn = thingList[i] as Pawn;
						if (pawn != null && !this.touchingPawns.Contains(pawn))
						{
							this.touchingPawns.Add(pawn);
							this.CheckSpring(pawn);
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
				base.Tick();
			}
			private void CheckSpring(Pawn p)
			{
				if (Rand.Chance(this.SpringChance(p)))
				{
					Map map = base.Map;
					this.Spring(p);
					if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
					{
						Find.LetterStack.ReceiveLetter("LetterFriendlyTrapSprungLabel".Translate(p.LabelShort, p).CapitalizeFirst(), "LetterFriendlyTrapSprung".Translate(p.LabelShort, p).CapitalizeFirst(), LetterDefOf.NegativeEvent, new TargetInfo(base.Position, map, false), null, null, null, null);
					}
				}
			}
			protected virtual float SpringChance(Pawn p)
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
			public bool KnowsOfTrap(Pawn p)
			{
				return (p.Faction != null && !p.Faction.HostileTo(base.Faction)) || (p.Faction == null && p.RaceProps.Animal && !p.InAggroMentalState) || (p.guest != null && p.guest.Released) || (!p.IsPrisoner && base.Faction != null && p.HostFaction == base.Faction) || (p.RaceProps.Humanlike && p.IsFormingCaravan()) || (p.IsPrisoner && p.guest.ShouldWaitInsteadOfEscaping && base.Faction == p.HostFaction) || (p.Faction == null && p.RaceProps.Humanlike);
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
			public override bool IsDangerousFor(Pawn p)
			{
				return this.KnowsOfTrap(p);
			}
			public void Spring(Pawn p)
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
			protected abstract void SpringSub(Pawn p);
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
				base.Map.designationManager.AddDesignation(new Designation(this, DefsOf.DesignationDefOf.RearmTrap));
			}

			private bool CanBeDesignatedRearm()
			{
				return !armed && Map.designationManager.AllDesignationsOn(this).Where(i => i.def == DefsOf.DesignationDefOf.RearmTrap).FirstOrDefault() == null;
			}

			public void Rearm()
			{
				armed = true;
			}
		}
	}
}