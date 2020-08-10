using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace shaco
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class CantBeNullAttribute : PropertyAttribute
    {
        public CantBeNullAttribute()
        {
            
        }
    }
}