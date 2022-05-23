using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimDungeon;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimDungeon
{
    class Trap_Hediff
    {
        public static bool CanApplyHediff(Hediff_Trap_Def hediff, Pawn p)
        {
            if (hediff.affectItemWieldersOnly && p?.equipment.Primary == null)
            {
                return false;
            }
            if(hediff.affectsLiving && p.RaceProps.FleshType != FleshTypeDefOf.Normal)
            {
                return false;
            }
            if (hediff.affectsInsect && !p.RaceProps.Insect)
            {
                return false;
            }
            if (hediff.affectsMech && !p.RaceProps.IsMechanoid)
            {
                return false;
            }
            return true;
        }
        public static void ApplyHediff(Hediff_Trap_Def hediff, Pawn p)
        {
            float random = Rand.Value;
            if(random > hediff.chance || hediff.severity <= 0f)
            {
                return;
            }
            if (!CanApplyHediff(hediff, p))
            {
                return;
            }
            float severity = hediff.severity;
            if(hediff.toxic && (hediff.affectsLiving || hediff.affectsAll) && (p.RaceProps.FleshType == FleshTypeDefOf.Normal))
            {
                severity *= p.GetStatValue(StatDefOf.ToxicSensitivity);
            }
            Hediff hediffToAdd = HediffMaker.MakeHediff(hediff.hediff, p);
            if (p.health.hediffSet.HasHediff(hediff.hediff))
            {
                p.health.hediffSet.GetFirstHediffOfDef(hediff.hediff).Severity += severity;
                return;
            }
            hediffToAdd.Severity = severity;
            p.health.AddHediff(hediffToAdd);

        }
    }
}
