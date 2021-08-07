using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimDungeon
{
    public abstract class Trap_Framework : Building_Trap
    {
        public Trap_Def TrapDef => base.def.GetModExtension<Trap_Def>();
        public bool autoRearm;
        public bool armed = true;
        public List<Pawn> touchingPawns = new List<Pawn>();

        public bool CanSetAutoRebuild
        {
            get
            {
                return base.Faction == Faction.OfPlayer && this.def.blueprintDef != null && this.def.IsResearchFinished;
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
        public new bool KnowsOfTrap(Pawn p)
        {
            return (p.Faction != null && !p.Faction.HostileTo(base.Faction)) ||  (p.guest != null && p.guest.Released) || (!p.IsPrisoner && base.Faction != null && p.HostFaction == base.Faction) || (p.RaceProps.Humanlike && p.IsFormingCaravan()) || (p.IsPrisoner && p.guest.ShouldWaitInsteadOfEscaping && base.Faction == p.HostFaction) || (p.Faction == null && p.RaceProps.Humanlike) || (p.HostFaction == base.Faction);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.autoRearm, "autoRebuild", false, false);
            Scribe_Values.Look<bool>(ref this.armed, "armed", true, false);
            Scribe_Collections.Look<Pawn>(ref this.touchingPawns, "touchingPawns", LookMode.Reference, Array.Empty<object>());
        }
        public void CheckSpring(Pawn p)
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

        protected override float SpringChance(Pawn p)
        {
            float num = 0f;
            if (!armed && this.TrapDef.rearmable) {}
            else if (this.KnowsOfTrap(p))
            {
                if (p.Faction == null)
                {

                    num = TrapDef.wildAnimalSpringChance;

                }
                else if (p.Faction == base.Faction)
                {
                    num = TrapDef.sameFactionSpringChance;
                }
            }
            else
            {
                num = this.GetStatValue(StatDefOf.TrapSpringChance, true) * p.GetStatValue(StatDefOf.PawnTrapSpringChance, true);
            }
            return Mathf.Clamp01(num);
        }
        public override ushort PathFindCostFor(Pawn p)
        {
            return (ushort)TrapDef.pathFindCost;
        }
        public override ushort PathWalkCostFor(Pawn p)
        {
            return (ushort)TrapDef.pathWalkCost;
        }
        public new void Spring(Pawn p)
        {
            bool spawned = base.Spawned;
            Map map = base.Map;
            Disarm();
            this.SpringSub(p);
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
        public void CheckAutoRebuild(Map map)
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
                if(!gizmo.ToString().Contains("Rearm"))
                {
                    yield return gizmo;
                }              
            }
            IEnumerator<Gizmo> enumerator = null;
            if (TrapDef.rearmable)
            {
                if (!armed && CanBeDesignatedRearm())
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
                if (DebugSettings.godMode)
                {
                    if (CanBeDesignatedRearm())
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "CommandRearmDebug".Translate(),
                            defaultDesc = "CommandRearmDescDebug".Translate(),
                            hotKey = KeyBindingDefOf.Misc4,
                            icon = TexCommand.RearmTrap,
                            action = Rearm
                        };
                    }
                    if (armed)
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "CommandDisarmDebug".Translate(),
                            defaultDesc = "CommandDisarmDescDebug".Translate(),
                            hotKey = KeyBindingDefOf.Misc4,
                            icon = TexCommand.RearmTrap,
                            action = Disarm
                        };
                    }
                }
            }
            else if (this.CanSetAutoRebuild)
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
            if (TrapDef.manualSpring)
            {
                yield return new Command_Action
                {

                    defaultLabel = "CommandManualSpring".Translate(),
                    defaultDesc = "CommandManualSpringDesc".Translate(),
                    hotKey = KeyBindingDefOf.Misc4,
                    icon = TexCommand.RearmTrap,
                    action = delegate()
                    {
                        Spring(null);
                    }
                };
            }

            yield break;
        }
        public void AddRearmDesignation()
        {
            base.Map.designationManager.AddDesignation(new Designation(this, DesignationDefOf.RearmTrap));
        }

        public bool CanBeDesignatedRearm()
        {
            return !armed && Map.designationManager.AllDesignationsOn(this).Where(i => i.def == DesignationDefOf.RearmTrap).FirstOrDefault() == null;
        }

        public void Rearm()
        {
            armed = true;
        }

        public void Disarm()
        {
            armed = false;
        }
    }
}