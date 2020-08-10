using UnityEngine;
using System.Collections;

namespace shaco
{
	[System.Serializable]
	public class AutoValue : shaco.Base.AutoValue
	{
		static readonly private System.Type TypeUnityObject = typeof(Object);
		static readonly private System.Type TypeUnityVector2 = typeof(Vector2);
		static readonly private System.Type TypeUnityVector3 = typeof(Vector3);
		static readonly private System.Type TypeUnityVector4 = typeof(Vector4);
		static readonly private System.Type TypeUnityRect = typeof(Rect);
		static readonly private System.Type TypeUnityColor = typeof(Color);
		static readonly private System.Type TypeUnityBounds = typeof(Bounds);
        static readonly private System.Type TypeUnityQuaternion = typeof(Quaternion);

        public static implicit operator Object(shaco.AutoValue value) { return value._value as Object; }
        public static implicit operator Vector2(shaco.AutoValue value) { return (Vector2)value._value; }
        public static implicit operator Vector3(shaco.AutoValue value) { return (Vector3)value._value; }
        public static implicit operator Vector4(shaco.AutoValue value) { return (Vector4)value._value; }
        public static implicit operator Rect(shaco.AutoValue value) { return (Rect)value._value; }
        public static implicit operator Color(shaco.AutoValue value) { return (Color)value._value; }
        public static implicit operator Bounds(shaco.AutoValue value) { return (Bounds)value._value; }
        public static implicit operator Quaternion(shaco.AutoValue value) { return (Quaternion)value._value; }

		public override bool Set(object Value)
		{
			if (null == Value)
			{
				return false;
			}

			bool retValue = base.Set(Value);
			if (!retValue)
			{
				System.Type typeTmp = Value.GetType();
                if (typeTmp.IsInherited<UnityEngine.Object>()) { _value = (Object)Value; }
                else if (typeTmp == TypeUnityVector2) { _value = (Vector2)Value; }
                else if (typeTmp == TypeUnityVector3) { _value = (Vector3)Value; }
                else if (typeTmp == TypeUnityVector4) { _value = (Vector4)Value; }
                else if (typeTmp == TypeUnityRect) { _value = (Rect)Value; }
                else if (typeTmp == TypeUnityColor) { _value = (Color)Value; }
                else if (typeTmp == TypeUnityBounds) { _value = (Bounds)Value; }
                else if (typeTmp == TypeUnityQuaternion) { _value = (Quaternion)Value; }
                else
                {
                    Log.Error("AutoValue Set error: unsupport value type=" + typeTmp.ToString());
					retValue = false;
                }
                _requestType = typeTmp;
			}
			return retValue;
		}
	}
}