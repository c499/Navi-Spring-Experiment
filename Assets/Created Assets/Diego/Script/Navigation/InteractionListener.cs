using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Assets.Created_Assets.Diego.Script
{
    public interface InteractionListener
    {
        void onTriggerPressed(float time);
        void onHeadUpdate(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR);
    }
}
