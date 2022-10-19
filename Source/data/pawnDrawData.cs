using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using RimWorld.Planet;

namespace yayoAni
{
    public class pawnDrawData
    {

        public float offset_angle = 0f;
        public Vector3 offset_pos = Vector3.zero;
        public Rot4? fixed_rot = null;
        public bool forcedShowBody = false;

        public void reset()
        {
            offset_angle = 0f;
            offset_pos = Vector3.zero;
            fixed_rot = null;
            forcedShowBody = false;
        }


    }
}
