using UnityEngine;
using Verse;

namespace yayoAni
{
    public class YayoAniSettings : ModSettings
    {
        public bool val_walk;
        public float val_walkSpeed;
        public float val_walkAngle;

        public bool val_combat;
        public bool val_combatTwirl;

        public bool val_anyJob;

        public bool val_sleep;
        public bool val_lovin;

        public bool val_debug;

        public override void ExposeData()
        {
            base.ExposeData();

            val_walkSpeed = Mathf.Clamp(val_walkSpeed, 0.1f, 10f);
            val_walkAngle = Mathf.Clamp(val_walkAngle, 0.1f, 10f);

            Scribe_Values.Look(ref val_walk, "WalkAnim", true);
            Scribe_Values.Look(ref val_walkSpeed, "WalkSpeed", 0.8f);
            Scribe_Values.Look(ref val_walkAngle, "WalkAngle", 0.6f);

            Scribe_Values.Look(ref val_combat, "CombatAnim", true);
            Scribe_Values.Look(ref val_combatTwirl, "CombatTwirl", true);

            Scribe_Values.Look(ref val_anyJob, "AnyJobAnim", true);

            Scribe_Values.Look(ref val_sleep, "SleepAnim", true);
            Scribe_Values.Look(ref val_lovin, "LovinAnim", true);

            Scribe_Values.Look(ref val_debug, "Debug", false);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.ColumnWidth = 270f;

            listing.CheckboxLabeled("YayoAnim_Walk".Translate(), ref val_walk);
            string buffer = null;
            listing.TextFieldNumericLabeled("YayoAnim_WalkAnimSpeed".Translate(), ref val_walkSpeed, ref buffer, 0.1f, 10f);
            buffer = null;
            listing.TextFieldNumericLabeled("YayoAnim_WalkAnimAngle".Translate(), ref val_walkAngle, ref buffer, 0.1f, 10f);

            listing.Gap();

            listing.CheckboxLabeled("YayoAnim_Combat".Translate(), ref val_combat);
            listing.CheckboxLabeled("YayoAnim_TwirlWeapon".Translate(), ref val_combatTwirl);

            listing.Gap();

            listing.CheckboxLabeled("YayoAnim_AllJobs".Translate(), ref val_anyJob);

            listing.Gap();

            listing.CheckboxLabeled("YayoAnim_Sleep".Translate(), ref val_sleep);
            listing.CheckboxLabeled("YayoAnim_LovinBed".Translate(), ref val_lovin);

            listing.Gap();

            if (listing.ButtonText("YayoAnim_ResetToDefault".Translate()))
                ResetToDefault();

            listing.Gap();

            listing.CheckboxLabeled("YayoAnim_DebugMode".Translate(), ref val_debug);

            listing.End();
        }

        private void ResetToDefault()
        {
            val_walk = true;
            val_walkSpeed = 0.8f;
            val_walkAngle = 0.6f;
            
            val_combat = true;
            val_combatTwirl = true;
            
            val_anyJob = true;
            
            val_sleep = true;
            val_lovin = true;
            
            val_debug = false;
        }
    }
}