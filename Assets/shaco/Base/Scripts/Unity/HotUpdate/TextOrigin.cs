using UnityEngine;
using System.Collections;

namespace shaco
{
    public class TextOrigin : Object
    {
        public string text
        {
            get { return null == _bytes || 0 == _bytes.Length ? string.Empty : _bytes.ToStringArray(); }
        }

        public byte[] bytes
        {
            get { return _bytes; }
            set { _bytes = value; }
        }

        public override string ToString()
        {
            return text;
        }

        public bool success
        {
            get { return !_bytes.IsNullOrEmpty(); }
        }

        public static bool operator ==(TextOrigin x, TextOrigin y)
        {
            var otherTmp = y as TextOrigin;
            bool isNullX = IsNullTmp(x);
            if (isNullX)
                return IsNullTmp(otherTmp);
            else
            {
                return IsNullTmp(otherTmp) ? false : (object)x == (object)otherTmp;
            }
        }

        public static bool operator !=(TextOrigin x, TextOrigin y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object other)
        {
            var otherTmp = other as TextOrigin;
            bool isNullX = IsNullTmp(this);
            if (isNullX)
                return IsNullTmp(otherTmp);
            else
            {
                return IsNullTmp(otherTmp) ? false : (object)this == (object)otherTmp;
            }
        }

        static private bool IsNullTmp(object obj)
        {
            return null == obj;
        }

        private byte[] _bytes = new byte[0];
    }
}