using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimDungeon
{
    class CompProperties_ArmedEmitter : CompProperties
    {
        public CompProperties_ArmedEmitter()
        {
            this.compClass = typeof(CompArmedEmitter);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (this.fleck == null)
            {
                yield return "CompFleckEmitter must have a fleck assigned.";
            }
            yield break;
        }
        public FleckDef fleck;
        public Vector3 offset;
        public int emissionInterval = -1;
        public string saveKeysPrefix;
    }
}
