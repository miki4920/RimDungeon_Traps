using Verse;

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
        public GraphicData trapUnarmedGraphicData;

        public bool slows = false;

        public float addHediffChance = 1.0f;
        public float hediffSeverity = 1.0f;
        public HediffDef hediffToAdd = null;

    }
}
