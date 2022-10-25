using UnityEngine;
using Verse;

namespace yayoAni
{
    public class YayoAniSettings : ModSettings
    {
        public bool walkEnabled = true;
        public float walkSpeed = 0.8f;
        public float walkAngle = 0.6f;

        public bool combatEnabled = true;
        public bool combatTwirlEnabled = true;

        public bool anyJobEnabled = true;

        public bool sleepEnabled = true;
        public bool lovinEnabled = true;

        public bool mechanoidWalkEnabled = true;
        public bool mechanoidJobEnabled = true;
        public bool mechanoidCombatEnabled = true;

        public bool debugMode = false;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref walkEnabled, "WalkAnim", true);
            Scribe_Values.Look(ref walkSpeed, "WalkSpeed", 0.8f);
            Scribe_Values.Look(ref walkAngle, "WalkAngle", 0.6f);
            walkSpeed = Mathf.Clamp(walkSpeed, 0.1f, 10f);
            walkAngle = Mathf.Clamp(walkAngle, 0.1f, 10f);

            Scribe_Values.Look(ref combatEnabled, "CombatAnim", true);
            Scribe_Values.Look(ref combatTwirlEnabled, "CombatTwirl", true);

            Scribe_Values.Look(ref anyJobEnabled, "AnyJobAnim", true);

            Scribe_Values.Look(ref sleepEnabled, "SleepAnim", true);
            Scribe_Values.Look(ref lovinEnabled, "LovinAnim", true);

            Scribe_Values.Look(ref mechanoidWalkEnabled, "MechanoidWalk", true);
            Scribe_Values.Look(ref mechanoidJobEnabled, "MechanoidJob", true);
            Scribe_Values.Look(ref mechanoidCombatEnabled, "MechanoidCombat", true);

            Scribe_Values.Look(ref debugMode, "Debug", false);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.ColumnWidth = 400f;

            listing.CheckboxLabeled("YayoAnim_Walk".Translate(), ref walkEnabled);
            string buffer = null;
            listing.TextFieldNumericLabeled("YayoAnim_WalkAnimSpeed".Translate(), ref walkSpeed, ref buffer, 0.1f, 10f);
            buffer = null;
            listing.TextFieldNumericLabeled("YayoAnim_WalkAnimAngle".Translate(), ref walkAngle, ref buffer, 0.1f, 10f);
            walkSpeed = Mathf.Clamp(walkSpeed, 0.1f, 10f);
            walkAngle = Mathf.Clamp(walkAngle, 0.1f, 10f);

            listing.Gap();

            listing.CheckboxLabeled("YayoAnim_Combat".Translate(), ref combatEnabled);
            listing.CheckboxLabeled("YayoAnim_TwirlWeapon".Translate(), ref combatTwirlEnabled);

            listing.Gap();

            listing.CheckboxLabeled("YayoAnim_AllJobs".Translate(), ref anyJobEnabled);

            listing.Gap();

            listing.CheckboxLabeled("YayoAnim_Sleep".Translate(), ref sleepEnabled);
            listing.CheckboxLabeled("YayoAnim_LovinBed".Translate(), ref lovinEnabled);

            listing.Gap();

            listing.CheckboxLabeled("YayoAnim_MechanoidWalk".Translate(), ref mechanoidWalkEnabled, "YayoAnim_MechanoidWalkTooltip".Translate());
            listing.CheckboxLabeled("YayoAnim_MechanoidJob".Translate(), ref mechanoidJobEnabled, "YayoAnim_MechanoidJobTooltip".Translate());
            listing.CheckboxLabeled("YayoAnim_MechanoidCombat".Translate(), ref mechanoidCombatEnabled, "YayoAnim_MechanoidCombatTooltip".Translate());

            listing.Gap();

            if (listing.ButtonText("YayoAnim_ResetToDefault".Translate()))
                ResetToDefault();

            listing.Gap();

            listing.CheckboxLabeled("YayoAnim_DebugMode".Translate(), ref debugMode);

            listing.End();
        }

        private void ResetToDefault()
        {
            walkEnabled = true;
            walkSpeed = 0.8f;
            walkAngle = 0.6f;

            combatEnabled = true;
            combatTwirlEnabled = true;

            anyJobEnabled = true;

            sleepEnabled = true;
            lovinEnabled = true;

            debugMode = false;
        }
    }
}