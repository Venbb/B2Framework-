using UnityEngine;
using System.Collections;

namespace shaco
{
	public class GameEntry : shaco.Base.GameEntry
	{
		static public T AddComponentInstance<T>(bool isDontDestroyOnLoad = true) where T : UnityEngine.Component
        {
            return AddComponentInstance(typeof(T), isDontDestroyOnLoad) as T;
        }

        static public object AddComponentInstance(System.Type type, bool isDontDestroyOnLoad = true)
        {
            object retValue = null;
            if (HasInstance(type))
            {
                Log.Error("AddComponentInstance error: has added instance type=" + type.FullName);
                return retValue;
            }
            else
            {
                retValue = CreateComponentInstance(type, isDontDestroyOnLoad);
                GetInstances().Add(type, retValue);
            }
            return retValue;
        }

        static public T GetComponentInstance<T>(bool isDontDestroyOnLoad = true) where T : UnityEngine.Component
        {
            return GetComponentInstance(typeof(T), isDontDestroyOnLoad) as T;
        }

        static public object GetComponentInstance(System.Type type, bool isDontDestroyOnLoad = true)
        {
            if (!HasInstance(type))
            {
                return AddComponentInstance(type, isDontDestroyOnLoad);
            }
            else
            {
                return GetInstances()[type];
            }
        }

		static public object CreateComponentInstance(System.Type type, bool isDontDestroyOnLoad)
        {
            object retValue = null;
            var listFind = GameObject.FindObjectsOfType(type);

            if (listFind.Length > 1)
            {
                for (int i = 1; i < listFind.Length; ++i)
                {
                    MonoBehaviour.DestroyImmediate(listFind[i]);
                }
            }

            if (listFind.Length > 0)
            {
                retValue = listFind[0];
            }

            if (retValue == null)
            {
                GameObject objTmp = new GameObject();
                retValue = objTmp.AddComponent(type);
                objTmp.transform.name = retValue.GetType().FullName;
            }

            if (isDontDestroyOnLoad && retValue is UnityEngine.Component)
            {
                var componentTmp = retValue as UnityEngine.Component;
                UnityHelper.SafeDontDestroyOnLoad(componentTmp.gameObject);
            }
            return retValue;
        }

        static public T CreateComponentInstance<T>(bool isDontDestroyOnLoad) where T : UnityEngine.Component
        {
            return CreateComponentInstance(typeof(T), isDontDestroyOnLoad) as T;
        }
	}
}
