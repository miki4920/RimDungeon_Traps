using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimDungeon;
using Verse;
using RimWorld;

namespace RimDungeon
{
    class Trap_Hediff
    {
        public static void ApplyHediff(Hediff_Trap_Def hediff, Pawn p)
        {
            float random = Rand.Value;
            if(random > hediff.chance || hediff.severity <= 0f)
            {
                return;
            }

            if(hediff.affectsLiving && !(p.RaceProps.FleshType == FleshTypeDefOf.Normal) || hediff.affectsInsect && !p.RaceProps.Insect || hediff.affectsMech && !p.RaceProps.IsMechanoid)
            {
                return;
            }
            float severity = hediff.severity;
            if(hediff.toxic && (hediff.affectsLiving || hediff.affectsAll) && (p.RaceProps.FleshType == FleshTypeDefOf.Normal))
            {
                severity *= p.GetStatValue(StatDefOf.ToxicSensitivity, true);
            }
            Hediff hediffToAdd = HediffMaker.MakeHediff(hediff.hediff, p, null);
            if (p.health.hediffSet.HasHediff(hediff.hediff, false))
            {
                p.health.hediffSet.GetFirstHediffOfDef(hediff.hediff, false).Severity += severity;
                return;
            }
            p.health.AddHediff(hediffToAdd, null, null, null);

        }
    }
}
