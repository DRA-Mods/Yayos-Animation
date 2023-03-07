using RimWorld;
using UnityEngine;
using Verse;
using yayoAni.Compat;

namespace yayoAni;

public class YayoAniSettings : ModSettings
{
    public bool onlyPlayerPawns = false;
    public int updateFrequencyTicks = 1;
    public CameraZoomRange maximumZoomLevel = CameraZoomRange.Furthest;

    public bool walkEnabled = true;
    public float walkSpeed = 0.8f;
    public float walkAngle = 0.6f;

    public bool combatEnabled = true;
    public bool combatTwirlEnabled = true;
    public bool combatTwirlMaxMassEnabled = true;
    public float combatTwirlMaxMass = 5f;
    public bool combatTwirlMaxSizeEnabled = true;
    public float combatTwirlMaxSize = 1.25f;

    public bool anyJobEnabled = true;

    public bool sleepEnabled = true;
    public bool lovinEnabled = true;

    public bool mechanoidWalkEnabled = true;
    public bool mechanoidJobEnabled = true;
    public bool mechanoidCombatEnabled = true;

    public bool animalWalkEnabled = true;
    public bool animalJobEnabled = true;
    public bool animalCombatEnabled = true;

#if IDEOLOGY
    public bool applyHarPatch = true;
#endif
    public bool applyOversizedChanges = true;

    public bool debugMode = false;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref onlyPlayerPawns, "OnlyPlayer", false);
        Scribe_Values.Look(ref updateFrequencyTicks, "UpdateFrequencyTicks", 1);
        Scribe_Values.Look(ref maximumZoomLevel, "MaximumZoomLevel", CameraZoomRange.Furthest);
        updateFrequencyTicks = Mathf.Clamp(updateFrequencyTicks, 1, 10);

        Scribe_Values.Look(ref walkEnabled, "WalkAnim", true);
        Scribe_Values.Look(ref walkSpeed, "WalkSpeed", 0.8f);
        Scribe_Values.Look(ref walkAngle, "WalkAngle", 0.6f);
        walkSpeed = Mathf.Clamp(walkSpeed, 0.1f, 10f);
        walkAngle = Mathf.Clamp(walkAngle, 0.1f, 10f);

        Scribe_Values.Look(ref combatEnabled, "CombatAnim", true);
        Scribe_Values.Look(ref combatTwirlEnabled, "CombatTwirl", true);
        Scribe_Values.Look(ref combatTwirlMaxMassEnabled, "CombatTwirlMaxMassEnabled", true);
        Scribe_Values.Look(ref combatTwirlMaxMass, "CombatTwirlMaxMass", 5f);
        Scribe_Values.Look(ref combatTwirlMaxSizeEnabled, "CombatTwirlMaxSizeEnabled", true);
        Scribe_Values.Look(ref combatTwirlMaxSize, "CombatTwirlMaxSize", 1.25f);

        Scribe_Values.Look(ref anyJobEnabled, "AnyJobAnim", true);

        Scribe_Values.Look(ref sleepEnabled, "SleepAnim", true);
        Scribe_Values.Look(ref lovinEnabled, "LovinAnim", true);

        Scribe_Values.Look(ref mechanoidWalkEnabled, "MechanoidWalk", true);
        Scribe_Values.Look(ref mechanoidJobEnabled, "MechanoidJob", true);
        Scribe_Values.Look(ref mechanoidCombatEnabled, "MechanoidCombat", true);

        Scribe_Values.Look(ref animalWalkEnabled, "AnimalWalk", true);
        Scribe_Values.Look(ref animalJobEnabled, "AnimalJob", true);
        Scribe_Values.Look(ref animalWalkEnabled, "AnimalCombat", true);

#if IDEOLOGY
        Scribe_Values.Look(ref applyHarPatch, "ApplyHarPatch", true);
#endif
        Scribe_Values.Look(ref applyOversizedChanges, "applyOversizedChanges", true);

        Scribe_Values.Look(ref debugMode, "Debug", false);
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        string buffer;
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.ColumnWidth = 400f;

        listing.CheckboxLabeled("YayoAnim_OnlyPlayerPawns".Translate(), ref onlyPlayerPawns, "YayoAnim_OnlyPlayerPawnsTooltip".Translate());
        buffer = null;
        listing.TextFieldNumericLabeled($"{"YayoAnim_UpdateFrequency".Translate()}  ", ref updateFrequencyTicks, ref buffer, 1, 10);
        if (listing.ButtonTextTooltip("YayoAnim_MaximumZoomLevel".Translate($"YayoAnim_MaximumZoomLevel_{maximumZoomLevel}".Translate()), "YayoAnim_MaximumZoomLevelTooltip".Translate()))
        {
            FloatMenuUtility.MakeMenu(
                (CameraZoomRange[])typeof(CameraZoomRange).GetEnumValues(),
                range => $"YayoAnim_MaximumZoomLevel_{range}".Translate(),
                range => () => maximumZoomLevel = range);
        }

        listing.Gap();

        listing.CheckboxLabeled("YayoAnim_Walk".Translate(), ref walkEnabled);
        buffer = null;
        listing.TextFieldNumericLabeled($"{"YayoAnim_WalkAnimSpeed".Translate()}  ", ref walkSpeed, ref buffer, 0.1f, 10f);
        buffer = null;
        listing.TextFieldNumericLabeled($"{"YayoAnim_WalkAnimAngle".Translate()}  ", ref walkAngle, ref buffer, 0.1f, 10f);
        walkSpeed = Mathf.Clamp(walkSpeed, 0.1f, 10f);
        walkAngle = Mathf.Clamp(walkAngle, 0.1f, 10f);

        listing.Gap();

        listing.CheckboxLabeled("YayoAnim_Combat".Translate(), ref combatEnabled);
#if BIOTECH_PLUS
        if (Core.usingReinforcedMechanoids)
            ReinforcedMechanoids2.SetReinforcedMechanoidsPatch(combatEnabled);
#endif
        listing.CheckboxLabeled("YayoAnim_TwirlWeapon".Translate(), ref combatTwirlEnabled);
        listing.CheckboxLabeled("YayoAnim_TwirlWeaponMaxMassEnabled".Translate(), ref combatTwirlMaxMassEnabled);
        buffer = null;
        listing.TextFieldNumericLabeled($"{"YayoAnim_TwirlWeaponMaxMass".Translate()}  ", ref combatTwirlMaxMass, ref buffer);
        listing.CheckboxLabeled("YayoAnim_TwirlWeaponMaxSizeEnabled".Translate(), ref combatTwirlMaxSizeEnabled);
        buffer = null;
        listing.TextFieldNumericLabeled($"{"YayoAnim_TwirlWeaponMaxSize".Translate()}  ", ref combatTwirlMaxSize, ref buffer);

        listing.Gap();

        listing.CheckboxLabeled("YayoAnim_AllJobs".Translate(), ref anyJobEnabled);

        listing.Gap();

        listing.CheckboxLabeled("YayoAnim_Sleep".Translate(), ref sleepEnabled);
        listing.CheckboxLabeled("YayoAnim_LovinBed".Translate(), ref lovinEnabled);

        listing.Gap();

        listing.CheckboxLabeled("YayoAnim_MechanoidWalk".Translate(), ref mechanoidWalkEnabled, "YayoAnim_RequiresWalkTooltip".Translate());
        listing.CheckboxLabeled("YayoAnim_MechanoidJob".Translate(), ref mechanoidJobEnabled, "YayoAnim_RequiresJobTooltip".Translate());
        listing.CheckboxLabeled("YayoAnim_MechanoidCombat".Translate(), ref mechanoidCombatEnabled, "YayoAnim_RequiresCombatTooltip".Translate());

        listing.Gap();

        listing.CheckboxLabeled("YayoAnim_AnimalWalk".Translate(), ref animalWalkEnabled, "YayoAnim_RequiresWalkTooltip".Translate());
        listing.CheckboxLabeled("YayoAnim_AnimalJob".Translate(), ref animalJobEnabled, "YayoAnim_RequiresJobTooltip".Translate());
        listing.CheckboxLabeled("YayoAnim_AnimalCombat".Translate(), ref animalCombatEnabled, "YayoAnim_RequiresCombatTooltip".Translate());

        listing.Gap();

#if IDEOLOGY
        listing.CheckboxLabeled("YayoAnim_ApplyHarPatch".Translate(), ref applyHarPatch, "YayoAnim_ApplyHarPatchTooltip".Translate());
        HumanoidAlienRaces.SetHarPatch(applyHarPatch);
#endif
        listing.CheckboxLabeled("YayoAnim_ApplyOversizedChanges".Translate(), ref applyOversizedChanges, "YayoAnim_ApplyOversizedChangesTooltip".Translate());

        listing.Gap();

        if (listing.ButtonText("YayoAnim_ResetToDefault".Translate()))
            ResetToDefault();

        listing.Gap();

        listing.CheckboxLabeled("YayoAnim_DebugMode".Translate(), ref debugMode);

        listing.End();
    }

    private void ResetToDefault()
    {
        onlyPlayerPawns = false;
        updateFrequencyTicks = 1;
        maximumZoomLevel = CameraZoomRange.Furthest;

        walkEnabled = true;
        walkSpeed = 0.8f;
        walkAngle = 0.6f;

        combatEnabled = true;
#if BIOTECH_PLUS
        if (Core.usingReinforcedMechanoids)
            ReinforcedMechanoids2.SetReinforcedMechanoidsPatch(combatEnabled);
#endif
        combatTwirlEnabled = true;
        combatTwirlMaxMassEnabled = true;
        combatTwirlMaxMass = 5f;
        combatTwirlMaxSizeEnabled = true;
        combatTwirlMaxSize = 1.25f;

        anyJobEnabled = true;

        sleepEnabled = true;
        lovinEnabled = true;

        mechanoidWalkEnabled = true;
        mechanoidJobEnabled = true;
        mechanoidCombatEnabled = true;

        animalWalkEnabled = true;
        animalJobEnabled = true;
        animalCombatEnabled = true;

#if IDEOLOGY
        applyHarPatch = true;
        HumanoidAlienRaces.SetHarPatch(applyHarPatch);
#endif
        applyOversizedChanges = true;

        debugMode = false;
    }
}