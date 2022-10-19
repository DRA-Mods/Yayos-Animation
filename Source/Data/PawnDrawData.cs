using UnityEngine;
using Verse;

namespace yayoAni.Data
{
    public class PawnDrawData
    {
        public float angleOffset = 0f;
        public Vector3 posOffset = Vector3.zero;
        public Rot4? fixedRot = null;
        public bool forcedShowBody = false;

        public void Reset()
        {
            angleOffset = 0f;
            posOffset = Vector3.zero;
            fixedRot = null;
            forcedShowBody = false;
        }
    }
}
