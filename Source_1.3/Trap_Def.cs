using System;
using Verse;
using UnityEngine;
using System.Runtime;

namespace RimDungeon
{
    public class Trap_Settings : ModSettings
    {
        public float affectFriendlyBasicTrap = 0.01f;
        public float affectEnemyBasicTrap = 0.75f;
        
        public float affectFriendlyAnimalTrap = 0.01f;
        public float affectAnimalsAnimalTrap = 0.75f;
        public float affectEnemyAnimalTrap = 0.25f;
        
        public float affectFriendlyAdvancedTrap = 0;
        public float affectEnemyAdvancedTrap = 1;
        
        public bool slowDown = true;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref affectFriendlyBasicTrap, "affectFriendlyBasicTrap", 0.01f);
            Scribe_Values.Look(ref affectEnemyBasicTrap, "affectEnemyBasicTrap", 0.75f);
            
            Scribe_Values.Look(ref affectFriendlyAnimalTrap, "affectFriendlyAnimalTrap", 0.01f);
            Scribe_Values.Look(ref affectAnimalsAnimalTrap, "affectAnimalsAnimalTrap", 0.75f);
            Scribe_Values.Look(ref affectEnemyAnimalTrap, "affectEnemyAnimalTrap", 0.25f);
            
            Scribe_Values.Look(ref affectFriendlyAdvancedTrap, "affectFriendlyAdvancedTrap", 0);
            Scribe_Values.Look(ref affectEnemyAdvancedTrap, "affectEnemyAdvancedTrap", 1);
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
            listingStandard.Label("Adjust Percentage Chances of Triggering Simple Traps:\n-Caltrops\n-Oil Trap\n-Column Trap");
            listingStandard.Label(String.Format("Percentage Chance of a Friendly Colonist Triggering a Simple Trap: {0}", (int) (100*settings.affectFriendlyBasicTrap)));
            settings.affectFriendlyBasicTrap = listingStandard.Slider(settings.affectFriendlyBasicTrap, 0, 1.00f);
            listingStandard.Label(String.Format("Percentage Chance of an Enemy Triggering a Simple Trap: {0}", (int) (100*settings.affectEnemyBasicTrap)));
            settings.affectEnemyBasicTrap = listingStandard.Slider(settings.affectEnemyBasicTrap, 0, 1.00f);
            
            listingStandard.Label("Adjust Percentage Chances of Triggering Animal Traps:\n-Noose\n-Bear Trap");
            listingStandard.Label(String.Format("Percentage Chance of a Friendly Colonist Triggering an Animal Trap: {0}", (int) (100*settings.affectFriendlyAnimalTrap)));
            settings.affectFriendlyAnimalTrap = listingStandard.Slider(settings.affectFriendlyAnimalTrap, 0, 1.00f);
            listingStandard.Label(String.Format("Percentage Chance of a Neutral (Non-Manhunting) Animal Triggering an Animal Trap: {0}", (int) (100*settings.affectAnimalsAnimalTrap)));
            settings.affectAnimalsAnimalTrap = listingStandard.Slider(settings.affectAnimalsAnimalTrap, 0, 1.00f);
            listingStandard.Label(String.Format("Percentage Chance of an Enemy Triggering an Animal Trap: {0}", (int) (100*settings.affectEnemyAnimalTrap)));
            settings.affectEnemyAnimalTrap = listingStandard.Slider(settings.affectEnemyAnimalTrap, 0, 1.00f);
            
            listingStandard.Label("Adjust Percentage Chances of Triggering Advanced Traps:\n-Spikes\n-Poison Trap\n-Spike Trap");
            listingStandard.Label(String.Format("Percentage Chance of a Friendly Colonist Triggering an Advanced Trap: {0}", (int) (100*settings.affectFriendlyAdvancedTrap)));
            settings.affectFriendlyAdvancedTrap = listingStandard.Slider(settings.affectFriendlyAdvancedTrap, 0, 1.00f);
            listingStandard.Label(String.Format("Percentage Chance of an Enemy Triggering a Simple Trap: {0}", (int) (100*settings.affectEnemyAdvancedTrap)));
            settings.affectEnemyAdvancedTrap = listingStandard.Slider(settings.affectEnemyAdvancedTrap, 0, 1.00f);
            
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
        public string trapType = "Basic";

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
