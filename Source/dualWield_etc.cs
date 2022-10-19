using DualWield;
using DualWield.Storage;
using RimWorld;
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

            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
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
            if (Base.Instance.GetExtendedDataStorage() is { } store)
            {
                return store.GetExtendedDataFor(instance).stancesOffhand;
            }

            return null;
        }
    }
}