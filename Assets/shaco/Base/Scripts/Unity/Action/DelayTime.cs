using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace shaco
{
    public class DelayTime : ActionBase
    {
        static public DelayTime Create(float duration)
        {
            DelayTime ret = new DelayTime();
            ret.duration = duration;

            return ret;
        }

        public override ActionBase Reverse(GameObject target)
        {
            return DelayTime.Create(duration);
        }

        public override ActionBase Clone()
        {
            return DelayTime.Create(duration);
        }

        private DelayTime() { }
    }
}