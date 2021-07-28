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
        public bool manualSpring = false;   
        public GraphicData trapUnarmedGraphicData;

        public bool slows = false;

    }
    public class Hediff_Trap_Def : DefModExtension
    {
        public HediffDef hediff = null;
        public float hediffSeverity = 1.0f;
        public float hediffChance = 1.0f;
        public bool insectOnly = false;
    }
    public class Gas_Trap_Def : DefModExtension
    {
        public ThingDef gas = null;
        public DamageDef damageDef = null;
        public float radius = 0f;
    }
}
