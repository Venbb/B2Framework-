using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class UnityObjectSpawn : shaco.Base.IObjectSpawn
    {
        public T CreateNewObject<T>(System.Func<T> callbackCreate)
        {
            var retValue = callbackCreate();
            return CheckInstantiate(retValue);
        }

        public void CreateNewObjectAsync<T>(System.Action<System.Action<T>> callbackCreate, System.Action<T> callbackEnd)
        {
            callbackCreate((newValue) =>
            {
                newValue = CheckInstantiate(newValue);

                try
                {
                    callbackEnd(newValue);
                }
                catch (System.Exception e)
                {
                    Log.Error("UnityObjectSpawn CreateNewObjectAsync exception: callbackCreate=" + callbackCreate + " callbackEnd=" + callbackEnd + " e=" + e);
                }
            });
        }

        public void ActiveObject<T>(T obj)
        {
            if (null == obj)
                return;

            if (obj is UnityEngine.GameObject)
            {
                ((GameObject)(object)obj).SetActive(true);
            }
            else if (obj is UnityEngine.Behaviour)
            {
                ((UnityEngine.Behaviour)(object)obj).gameObject.SetActive(true);
            }
            else if (obj is UnityEngine.Component)
            {
                ((UnityEngine.Component)(object)obj).gameObject.SetActive(true);
            }
        }

        public void RecyclingObject<T>(T obj)
        {
            if (null == obj)
                return;

            var gameObjectTmp = ConvertAsGameObject(obj);
            if (null != gameObjectTmp)
            {
                gameObjectTmp.SetActive(false);
                shaco.GameEntry.GetComponentInstance<UnityObjectPoolCompnnet>().ChangeParentToUnityObjectPoolComponent(gameObjectTmp);
            }
            else
            {
                obj = default(T);
            }
        }

        public void DestroyObject<T>(T obj)
        {
            if (null == obj)
                return;

            var gameObjectTmp = ConvertAsGameObject(obj);
            if (null != gameObjectTmp)
            {
                MonoBehaviour.Destroy(gameObjectTmp);
            }
            else
            {
                obj = default(T);
            }
        }

        private GameObject ConvertAsGameObject<T>(T obj)
        {
            GameObject retValue = null;
            if (obj is UnityEngine.GameObject)
            {
                retValue = ((UnityEngine.GameObject)(object)obj);
            }
            else if (obj is UnityEngine.Behaviour)
            {
                retValue = ((UnityEngine.Behaviour)(object)obj).gameObject;
            }
            else if (obj is UnityEngine.Component)
            {
                retValue = ((UnityEngine.Component)(object)obj).gameObject;
            }
            return retValue;
        }

        private T CheckInstantiate<T>(T newValue)
        {
            if (null == newValue)
                return default(T);

            if (newValue is UnityEngine.Object)
            {
                newValue = (T)(object)CheckInstantiateGameObject((UnityEngine.Object)(object)newValue);
                return newValue;
            }
            else
            {
                return newValue;
            }
        }

        private T CheckInstantiateGameObject<T>(T obj) where T : UnityEngine.Object
        {
            var retValue = obj;
            var gameObjectTmp = ConvertAsGameObject(obj);
            if (null != gameObjectTmp && string.IsNullOrEmpty(gameObjectTmp.scene.path))
            {
                retValue = MonoBehaviour.Instantiate(retValue);
            }
            return retValue;
        }
    }
}