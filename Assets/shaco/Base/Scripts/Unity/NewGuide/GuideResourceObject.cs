using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    /// <summary>
    /// 新手引导的Unity对象，可以更方便在引导编辑器中使用
    /// </summary>
    public class GuideResourceObject<T> : shaco.Base.IGuideJsonDataConvert where T : UnityEngine.Object
    {
        protected T value
        {
            set
            {
                if (_value == value)
                    return;

                _value = value;
                CheckUpdateObject(_value);
            }
        }

        protected T _value;

        /// <summary>
        /// 对象类型
        /// </summary>
        public System.Type valueType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// 资源对象的guid
        /// </summary>
        public string guid
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(_value));
#else
                Debug.LogError("GuideResourceObject guid erorr: only support on Unity Editor");
                return string.Empty;
#endif
            }
        }

        /// <summary>
        /// 对象名字
        /// </summary>
        public string name
        {
            get { return _name; }
        }
        protected string _name;

        /// <summary>
        /// 对象路径，用于在配置中保存与读取
        /// </summary>
        public string path
        {
            get
            {
                return _path;
            }
            set
            {
                SetPath(value);
            }
        }
        protected string _path = string.Empty;

        /// <summary>
        /// 路径与gui的连接符号
        /// </summary>
        private readonly string PATH_GUID_CONTACT_FALG = ",";

        /// <summary>
        /// 设置对象的隐式转换
        /// </summary>
        public static implicit operator GuideResourceObject<T>(UnityEngine.Object obj)
        {
            if (null == obj)
                return null;

            var retValue = new GuideResourceObject<T>();
            retValue.SetObject(obj);
            return retValue;
        }

        public static implicit operator GuideResourceObject<T>(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var retValue = new GuideResourceObject<T>();
            retValue.SetPath(path);
            return retValue;
        }

        public static implicit operator GuideResourceObject<T>(shaco.LitJson.JsonData jsonData)
        {
            if (null == jsonData)
                return null;

            var retValue = new GuideResourceObject<T>();
            retValue.SetPath(jsonData.ToString());
            return retValue;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        virtual public void GetValue(System.Action<T> callback)
        {
            if (null == _value && !string.IsNullOrEmpty(_path))
            {
                shaco.GameHelper.res.LoadResourcesOrLocalAsync<T>(_path, (obj) =>
                {
                    _value = obj;

                    if (null != callback)
                        callback(obj);
                });
            }
            else
            {
                if (null != callback)
                    callback(_value);
            }
        }

        /// <summary>
        /// 设置对象
        /// <param name="newValue">新对象</param>
        /// </summary>
        virtual public void SetObject(UnityEngine.Object newValue)
        {
#if UNITY_EDITOR
            this.CheckUpdateObject(newValue);
#else
            _value = (T)newValue;
#endif
            this._name = newValue.name;
        }

        /// <summary>
        /// 刷新路径
        /// <param name="saveString">保存过的配置字符串(通过GetSaveString获取)</param>
        /// </summary>
        virtual public void SetPath(string saveString)
        {
            string relativePath = null;
            string guid = null;

            GetPathAndGUID(saveString, out relativePath, out guid);

            if (string.IsNullOrEmpty(relativePath))
                return;

#if UNITY_EDITOR
            //在编辑器模式下，如果guid可以使用，则优先使用guid来获取对应的路径更加保险一点，这样可以防止对象被修改路径后无法读取的bug
            if (!string.IsNullOrEmpty(guid))
            {
                var newRelativePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(newRelativePath))
                {
                    shaco.Log.Error("GuideResourceObject SetPath error: not found asset by path=" + relativePath + "\nguid=" + guid);
                    return;
                }

                //如果存在二者路径不一致的情况，可能是有手动修改过资源路径
                if (relativePath != newRelativePath)
                {
                    relativePath = newRelativePath;
                    shaco.GameHelper.newguide.isReSaveFileDirty = true;
                }

                //编辑器模式下使用AssetDatabase读取资源，这样资源才可以ping到
                if (!Application.isPlaying)
                    _value = (T)UnityEditor.AssetDatabase.LoadAssetAtPath(newRelativePath, typeof(T));
                else
                {
                    _path = relativePath;
                    GetValue(null);
                }
            }
#endif
            _path = relativePath;
            this._name = shaco.Base.FileHelper.GetLastFileName(_path, false);
        }

        /// <summary>
        /// 获取储存字符串
        /// </summary>
        override public string ToString()
        {
            return this.path + this.PATH_GUID_CONTACT_FALG + this.guid;
        }

        /// <summary>
        /// 字符串转引导数据
        /// </summary>
        public void LoadFromString(string json)
        {
            this.SetPath(json);
        }

        /// <summary>
        /// 从储存的字符串中获取路径和gui
        /// <param name="saveString">保存过的配置字符串(通过GetSaveString获取)</param>
        /// <param name="relativePath">文件相对路径</param>
        /// <param name="guid">文件uuid</param>
        /// </summary>
        protected void GetPathAndGUID(string saveString, out string relativePath, out string guid)
        {
            relativePath = null;
            guid = null;

            var splitString = saveString.Split(this.PATH_GUID_CONTACT_FALG);
            if (splitString.IsNullOrEmpty())
            {
                shaco.Log.Warning("GuideResourceObject GetPathAndGUID error: invalid save string=" + saveString);
                return;
            }

            if (splitString.Length > 0)
            {
                relativePath = splitString[0];
            }

            if (splitString.Length > 1)
            {
                guid = splitString[1];
            }
        }

        /// <summary>
        /// 刷新对象
        /// </summary>
        private void CheckUpdateObject(UnityEngine.Object newValue)
        {
#if !UNITY_EDITOR
			Log.Error("GuideObject CheckUpdateObject error: only support on Unity Editor");
			return;
#else
            var relativePath = UnityEditor.AssetDatabase.GetAssetPath(newValue);

            //非Unity路径或者Unity支持的对象
            if (string.IsNullOrEmpty(relativePath))
            {
                _path = string.Empty;
                _value = null;
                return;
            }

            _value = (T)newValue;
            _path = relativePath;
#endif
        }

        /// <summary>
        /// 隐式转换类型
        /// <param name="requestType">要求转换到的类型</param>
        /// <return>转换后的类型，如果为null则转换失败</return>
        /// </summary>
        protected TValue ConvertToUnityType<TValue>() where TValue : UnityEngine.Object
        {
            var retValue = default(TValue);
            retValue = (TValue)(object)this._value;
            return retValue;
        }
    }
}