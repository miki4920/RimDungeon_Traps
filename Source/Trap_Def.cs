﻿using Verse;

namespace RimDungeon
{
    public class Trap_Def : DefModExtension
    {
        public int numberOfAttacks = 5;
        public float armorPenetration = 0.015f;

        public float sameFactionSpringChance = 0.005f;
        public float wildAnimalSpringChance = 0.2f;

        public int pathFindCost = 8000;
        public int pathWalkCost = 50;

        public bool rearmable = false;
        public bool manualSpring = false;
        public GraphicData trapUnarmedGraphicData;

        public bool slows = false;

        public Hediff_Trap_Def hediff = null;
        public Gas_Trap_Def gas = null;

    }
    public class Hediff_Trap_Def : Def
    {
        public HediffDef hediffToAdd = null;
        public float hediffSeverity = 1.0f;
        public float hediffChance = 1.0f;
    }
    public class Gas_Trap_Def : Def
    {
        public ThingDef gasDef= null;
        public DamageDef damageDef = null;
        public float radius = 0f;
    }
}
