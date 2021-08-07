using Verse;

namespace RimDungeon
{
    public class Trap_Def : DefModExtension
    {
        public int numberOfAttacks = 5;
        public float armorPenetration = 0.015f;

        public float sameFactionSpringChance = 0f;
        public float wildAnimalSpringChance = 0f;

        public int pathFindCost = 8000;
        public int pathWalkCost = 20;

        public bool rearmable = false;
        public bool manualSpring = false;  
    }
    public class Hediff_Trap_Def : DefModExtension
    {
        public HediffDef hediff = null;
        public float severity = 1.0f;
        public float chance = 1.0f;
        public bool toxic = false;
        public bool affectsAll = false;
        public bool affectsLiving = false;
        public bool affectsMech = false;
        public bool affectsInsect = false;
    }
    public class Gas_Trap_Def : DefModExtension
    {
        public ThingDef gas = null;
        public DamageDef damageDef = null;
        public float radius = 0f;
    }
}
