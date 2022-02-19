using System;
using Verse;
using UnityEngine;
using System.Runtime;

namespace RimDungeon
{
    public class Trap_Settings : ModSettings
    {
        public float affectFriendly = 0.01f;
        public float affectAnimals = 0.5f;
        public bool slowDown = true;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref affectFriendly, "affectFriendly", 0.01f);
            Scribe_Values.Look(ref affectAnimals, "affectAnimals", 0.5f);
            Scribe_Values.Look(ref slowDown, "slowDown", true);
            base.ExposeData();
        }
    }

    public class RimDungeon_Traps : Mod
    {
        Trap_Settings settings;
        public RimDungeon_Traps(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<Trap_Settings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label(String.Format("Percentage Chance of Your Colonist (Or Your Animal) Triggering a Trap: {0}",(int) (settings.affectFriendly*100)));
            settings.affectFriendly = listingStandard.Slider(settings.affectFriendly, 0, 1.00f);
            listingStandard.Label(String.Format("Percentage Chance of Neutral Animal Triggering a Trap: {0}", (int) (settings.affectAnimals*100)));
            settings.affectAnimals = listingStandard.Slider(settings.affectAnimals, 0, 1.00f);
            listingStandard.CheckboxLabeled("Traps slow down movement: ", ref settings.slowDown, "Change whether traps slow down movement. Warning, this makes caltrops significantly weaker.");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "RimDungeon Traps";
        }
    }

    public class Trap_Def : DefModExtension
    {
        public int numberOfAttacks = 5;
        public float armorPenetration = 0.015f;

        public float sameFactionSpringChance = 0.01f;
        public float wildAnimalSpringChance = 0.01f;

        public int pathFindCost = 8000;
        public int pathWalkCost = 20;

        public bool rearmable = false;
        public bool manualSpring = false;
        public bool animalTrap = false;
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
