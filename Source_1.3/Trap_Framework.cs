﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimDungeon
{
    public abstract class Trap_Framework : Building
    {
        public Trap_Def TrapDef => base.def.GetModExtension<Trap_Def>();
        public bool armed;
        public bool autoRebuild;
        public bool autoRearm;
        public List<Pawn> touchingPawns = new List<Pawn>();
        public Trap_Settings settings = LoadedModManager.GetMod<RimDungeon_Traps>().GetSettings<Trap_Settings>();

        public bool CanSetAutoRebuild
        {
            get
            {
                return base.Faction == Faction.OfPlayer && this.def.blueprintDef != null && this.def.building.trapDestroyOnSpring;
            }
        }

        public bool CanSetAutoRearm
        {
            get
            {
                return base.Faction == Faction.OfPlayer && TrapDef.rearmable;
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

        public override void PostMake()
        {
            base.PostMake();
            armed = !base.def.HasComp(typeof(CompChangeableProjectile));
        }

        public bool KnowsOfTrap(Pawn p)
        {
            if (p.Faction == null && (p.RaceProps.Animal || p.RaceProps.Insect) && TrapDef.trapType == "Animal")
            {
                return false;
            }
            if(TrapDef.rearmable && !this.armed)
            {
                return true;
            }
            return (p.Faction != null && !p.Faction.HostileTo(base.Faction)) || (p.Faction == null && p.RaceProps.Animal && !p.InAggroMentalState) || (p.guest != null && p.guest.Released) || (!p.IsPrisoner && base.Faction != null && p.HostFaction == base.Faction) || (p.RaceProps.Humanlike && p.IsFormingCaravan()) || (p.IsPrisoner && p.guest.ShouldWaitInsteadOfEscaping && base.Faction == p.HostFaction) || (p.Faction == null && p.RaceProps.Humanlike);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.armed, "armed", true, false);
            Scribe_Values.Look<bool>(ref this.autoRebuild, "autoRebuild", false, false);
            Scribe_Values.Look<bool>(ref this.autoRearm, "autoRearm", false, false);
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
                    if (thingList[i] is Pawn pawn && !this.touchingPawns.Contains(pawn))
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
        public float SpringChance(Pawn p)
        {
            float num = 0;
            if (!armed && this.TrapDef.rearmable) {
                return 0f;
            }

            if (this.def.HasModExtension<Hediff_Trap_Def>())
            {
                Hediff_Trap_Def hediff = def.GetModExtension<Hediff_Trap_Def>();
                if (!Trap_Hediff.CanApplyHediff(hediff, p))
                {
                    return 0f;
                }
            }
            if(this.def.HasComp(typeof(CompChangeableProjectile)))
            {
                CompChangeableProjectile comp = this.GetComp<CompChangeableProjectile>();
                ThingWithComps projectile = (ThingWithComps) ThingMaker.MakeThing(comp.LoadedShell, null);
                Hediff_Trap_Def hediff = projectile.def.GetModExtension<Hediff_Trap_Def>();
                if (!projectile.def.HasModExtension<Gas_Trap_Def>() && !Trap_Hediff.CanApplyHediff(hediff, p))
                {
                    return 0f;
                }
            }
            if (p.HostileTo(base.Faction) || p.RaceProps.Insect)
            {
                switch (this.TrapDef.trapType)
                {
                    case("Basic"):
                        num = this.settings.affectEnemyBasicTrap * p.GetStatValue(StatDefOf.PawnTrapSpringChance, true);
                        break;
                    case("Animal"):
                        num = this.settings.affectEnemyAnimalTrap * p.GetStatValue(StatDefOf.PawnTrapSpringChance, true);
                        break;
                    case("Advanced"):
                        num = this.settings.affectEnemyAdvancedTrap * p.GetStatValue(StatDefOf.PawnTrapSpringChance, true);
                        break;
                    default:
                        num = 0;
                        break;
                }
            }
            else if (p.RaceProps.Animal && !p.HostileTo(this.Faction) && this.TrapDef.trapType == "Animal")
            {
                num = this.settings.affectAnimalsAnimalTrap;
            }
            else if (p.Faction == base.Faction || !p.HostileTo(base.Faction) && p.Faction != null)
            {
                switch (this.TrapDef.trapType)
                {
                    case("Basic"):
                        num = this.settings.affectFriendlyBasicTrap;
                        break;
                    case("Animal"):
                        num = this.settings.affectFriendlyAnimalTrap;
                        break;
                    case("Advanced"):
                        num = this.settings.affectFriendlyAdvancedTrap;
                        break;
                    default:
                        num = 0;
                        break;
                }
            }
            
            return Mathf.Clamp01(num);
        }
        public override ushort PathFindCostFor(Pawn p)
        {
            if (!this.KnowsOfTrap(p))
            {
                return 0;
            }
            return (ushort)TrapDef.pathFindCost;
        }
        public override ushort PathWalkCostFor(Pawn p)
        {
            return (ushort)((ushort)TrapDef.pathWalkCost * (settings.slowDown ? 1 : 0) + 1);
        }
        public void Spring(Pawn p)
        {
            bool spawned = base.Spawned;
            Map map = base.Map;
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
            else
            {
                Disarm();
                this.CheckAutoRearm(this, map);
            }
        }

        protected abstract void SpringSub(Pawn p);

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
            if (this.autoRebuild && this.CanSetAutoRebuild && map != null && GenConstruct.CanPlaceBlueprintAt(this.def, base.Position, base.Rotation, map, false, null, null, base.Stuff).Accepted)
            {
                GenConstruct.PlaceBlueprintForBuild_NewTemp(this.def, base.Position, map, base.Rotation, Faction.OfPlayer, base.Stuff);
            }
        }

        public void CheckAutoRearm(Thing trap, Map map)
        {
            if (this.autoRearm && this.CanSetAutoRearm)
            {
                map.designationManager.AddDesignation(new Designation(trap, DesignationDefOf.RearmTrap));
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
            CompChangeableProjectile compChangeableProjectile = this.TryGetComp<CompChangeableProjectile>();
            if (compChangeableProjectile != null)
            {
                text += "\n";
                if (compChangeableProjectile.Loaded)
                {
                    text += "ShellLoaded".Translate(compChangeableProjectile.LoadedShell.LabelCap, compChangeableProjectile.LoadedShell);
                }
                else
                {
                    text += "ShellNotLoaded".Translate();
                }
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
            if (TrapDef.rearmable)
            {
                if (this.CanSetAutoRearm)
                {
                    yield return new Command_Toggle
                    {
                        defaultLabel = "CommandAutoRearm".Translate(),
                        defaultDesc = "CommandAutoRearmDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc3,
                        icon = TexCommand.RearmTrap,
                        isActive = (() => this.autoRearm),
                        toggleAction = delegate ()
                        {
                            this.autoRearm = !this.autoRearm;
                        }
                    };
                }
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
                    isActive = (() => this.autoRebuild),
                    toggleAction = delegate ()
                    {
                        this.autoRebuild = !this.autoRebuild;
                    }
                };
            }
            if (TrapDef.manualSpring && (!TrapDef.rearmable || (TrapDef.rearmable && armed)))
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
            if (this.CanExtractShell && this.armed)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandExtractShell".Translate(),
                    defaultDesc = "CommandExtractShellDesc".Translate(),
                    icon = this.GetComp<CompChangeableProjectile>().LoadedShell.uiIcon,
                    iconDrawScale = GenUI.IconDrawScale(this.GetComp<CompChangeableProjectile>().LoadedShell),
                    action = delegate ()
                    {
                        this.ExtractShell();
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
        public bool CanExtractShell
        {
            get
            {
                CompChangeableProjectile compChangeableProjectile = this.TryGetComp<CompChangeableProjectile>();
                return compChangeableProjectile != null && compChangeableProjectile.Loaded;
            }
        }
        public void ExtractShell()
        {
            GenPlace.TryPlaceThing(this.TryGetComp<CompChangeableProjectile>().RemoveShell(), base.Position, base.Map, ThingPlaceMode.Near, null, null, default(Rot4));
            armed = false;
        }

        public override void DrawExtraSelectionOverlays()
        {
            if(this.def.HasComp(typeof(CompChangeableProjectile)))
            {
                CompChangeableProjectile compChangeableProjectile = this.GetComp<CompChangeableProjectile>();
                if(compChangeableProjectile.Loaded && compChangeableProjectile.LoadedShell.HasModExtension<Gas_Trap_Def>()) {
                    GenDraw.DrawRadiusRing(this.Position, compChangeableProjectile.LoadedShell.GetModExtension<Gas_Trap_Def>().radius);
                }
            }
        }
    }
}