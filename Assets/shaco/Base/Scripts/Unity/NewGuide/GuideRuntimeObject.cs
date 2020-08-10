using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class GuideRuntimeObject<T> : GuideResourceObject<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// 当前对象
        /// </summary>
        new public T value
        {
            get { return GetValue(); }
            set { SetObject(value); }
        }

        /// <summary>
        /// 默认支持的对象的隐式转换，不在此支持的类型(例如自定义类型)，则需要显示类型转换了
        /// </summary>
        public static implicit operator UnityEngine.Object(GuideRuntimeObject<T> value) { return value.ConvertToUnityType<UnityEngine.Object>(); }
        public static implicit operator UnityEngine.GameObject(GuideRuntimeObject<T> value) { return value.ConvertToUnityType<UnityEngine.GameObject>(); }
        public static implicit operator UnityEngine.Animator(GuideRuntimeObject<T> value) { return value.ConvertToUnityType<UnityEngine.Animator>(); }
        public static implicit operator UnityEngine.Animation(GuideRuntimeObject<T> value) { return value.ConvertToUnityType<UnityEngine.Animation>(); }
        public static implicit operator UnityEngine.ScriptableObject(GuideRuntimeObject<T> value) { return value.ConvertToUnityType<UnityEngine.ScriptableObject>(); }

        public static implicit operator GuideRuntimeObject<T>(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var retValue = new GuideRuntimeObject<T>();
            retValue.SetPath(path);
            return retValue;
        }

        public static implicit operator GuideRuntimeObject<T>(shaco.LitJson.JsonData jsonData)
        {
            if (null == jsonData)
                return null;

            var retValue = new GuideRuntimeObject<T>();
            retValue.SetPath(jsonData.ToString());
            return retValue;
        }

        /// <summary>
        /// 刷新路径
        /// <param name="saveString">保存过的配置字符串(通过GetSaveString获取)</param>
        /// </summary>
		override public void SetPath(string saveString)
		{
            UnityEngine.Object newObj = null;

            string relativePath = null;
            string guid = null;

            GetPathAndGUID(saveString, out relativePath, out guid);
            if (string.IsNullOrEmpty(relativePath))
                return;
            
            if (!string.IsNullOrEmpty(relativePath))
			{
                newObj = FindObjectInHierachy(relativePath);
			}
			SetObject(newObj);
            this._path = relativePath;
            this._name = shaco.Base.FileHelper.GetLastFileName(_path, false);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        public T GetValue()
        {
            if (null == _value && Application.isPlaying && !string.IsNullOrEmpty(_path))
            {
                _value = FindObjectInHierachy(_path);
                if (null == _value)
                {
                    Log.Error("GuideRuntimeObject FindObjectInHierachy error: not found object by path=" + path);
                }
            }
            return _value;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        override public void GetValue(System.Action<T> callblack)
        {
            callblack(GetValue());
        }

		override public void SetObject(Object newValue)
		{
			if (null == newValue)
			{
				this._path = string.Empty;
				this._value = null;
			}
			else
			{
#if UNITY_EDITOR
                var isContainsInEditor = UnityEditor.AssetDatabase.Contains(newValue.GetInstanceID());
                if (isContainsInEditor)
                {
                    Log.Error("GuideRuntimeObject SetObject error: select must in 'Hierachy' window, object=" + newValue);
                    return;
                }
#endif
                this._path = shaco.UnityHelper.GetObjectPathInHierarchy(newValue);
                this._value = (T)newValue;
                this._name = newValue.name;
            }
		}

        private T FindObjectInHierachy(string path)
        {
            T retValue = null;
            if (typeof(T).IsInherited<UnityEngine.Component>())
            {
                var obj = GameObject.Find(path);
                retValue = null == obj ? null : obj.GetComponent<T>();
            }
            else if (typeof(T) == typeof(GameObject))
                retValue = (T)(Object)GameObject.Find(path);
            else
            {
                Log.Error("GuideRuntimeObject FindObjectInHierachy error: unsupport type=" + typeof(T).FullName);
            }
            return retValue;
        }
    }
}	