using DualWield.Storage;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace yayoAni
{
    static class dualWield_etc
    {
        public static bool TryGetOffHandEquipment(this Pawn_EquipmentTracker instance, out ThingWithComps result)
        {
            result = null;
            if (instance.pawn.HasMissingArmOrHand())
            {
                return false;
            }
            
            ExtendedDataStorage store = DualWield.Base.Instance.GetExtendedDataStorage();
            foreach (ThingWithComps twc in instance.AllEquipmentListForReading)
            {
                if (store.TryGetExtendedDataFor(twc, out ExtendedThingWithCompsData ext) && ext.isOffHand)
                {
                    result = twc;
                    return true;
                }
            }
            return false;
        }

        public static bool HasMissingArmOrHand(this Pawn instance)
        {
            bool hasMissingHand = false;
            foreach (Hediff_MissingPart missingPart in instance.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (missingPart.Part.def == BodyPartDefOf.Hand || missingPart.Part.def == BodyPartDefOf.Arm)
                {
                    hasMissingHand = true;
                }
            }
            return hasMissingHand;
        }


        public static Pawn_StanceTracker GetStancesOffHand(this Pawn instance)
        {
            if (DualWield.Base.Instance.GetExtendedDataStorage() is ExtendedDataStorage store)
            {
                return store.GetExtendedDataFor(instance).stancesOffhand;
            }
            return null;
        }


    }
}