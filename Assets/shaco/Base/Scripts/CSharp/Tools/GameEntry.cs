using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    [System.Diagnostics.DebuggerStepThrough]
    [System.Serializable]
	public class GameEntry
    {
		static private GameEntry _instance = null;
        protected Dictionary<System.Type, object> _instances = new Dictionary<System.Type, object>();

		static public T AddInstance<T>()
		{
			return (T)AddInstance(typeof(T));
		}

		static public To SetInstance<From, To>() where To : class
        {
			To retValue = null;
			object valueTmp = null;
			try
			{
				valueTmp = SetInstance(typeof(From), typeof(To));
				retValue = (To)valueTmp;
			}
			catch (System.Exception e)
			{
				throw new System.Exception(string.Format("GameEntry SetInstance exception: can't set instance from type '{0}' to type '{1}' result type '{2}' \n{3}", 
														typeof(From).FullName, typeof(To).FullName, valueTmp.GetType().FullName, e.ToString()));
			}
			return retValue;
        }

        static public object SetInstance(System.Type from, System.Type to)
        {
			if (!to.IsInherited(from))
			{
				throw new System.Exception(string.Format("GameEntry SetInstance exception: to type '{0}' not inherited from type '{1}'", to.FullName, from.FullName));
				// return null;
			}

            var retValue = to.IsInherited(typeof(IGameInstanceCreator)) ? to.InvokeMethod("Create") : to.Instantiate();
			if (null == retValue)
			{
				retValue = to.Instantiate();
			}

            GameEntry instanceTmp = GetInstance();
            System.Type keyTmp = from;
            if (HasInstance(keyTmp))
            {
                instanceTmp._instances[keyTmp] = retValue;
            }
            else
            {
                instanceTmp._instances.Add(keyTmp, retValue);
            }
            return retValue;
        }

		static public T GetInstance<T>()
		{
			System.Type typeTmp = typeof(T);
			if (!HasInstance(typeTmp))
			{
				return AddInstance<T>();
			}
			else 
			{
				return (T)GetInstance()._instances[typeTmp];
			}
		}

        static public object GetInstance(System.Type type)
        {
            GameEntry instanceTmp = GetInstance();

            if (!HasInstance(type))
            {
                return AddInstance(type);
            }
            else
            {
                return instanceTmp._instances[type];
            }
        }

		static public bool HasInstance<T>()
		{
			return HasInstance(typeof(T));
		}

		static public bool HasInstance(System.Type type)
        {
            return GetInstance()._instances.ContainsKey(type);
        }

		static public bool RemoveIntance<T>()
		{
			return RemoveIntance(typeof(T));
		}

		static public bool RemoveIntance(object obj)
		{
			return RemoveIntance(obj.GetType());
		}

        static public bool RemoveIntance(System.Type type)
		{
			if (!HasInstance(type))
			{
				Log.Error("RemoveIntance error: not find instance type=" + type.ToTypeString());
				return false;
			}
			else 
			{
				GetInstance()._instances.Remove(type);
				return true;
			}
		}

		static public void ClearIntances()
		{
			GetInstance()._instances.Clear();
		}

        static public void Foreach(System.Action<System.Type, object> callback)
        {
            var instanceTmp = GameEntry.GetInstance();
            foreach (var iter in instanceTmp._instances)
            {
                callback(iter.Key, iter.Value);
            }
        }
		
		static protected GameEntry GetInstance()
		{
			if (null == _instance)
			{
				_instance = new GameEntry();
			}
			return _instance;
		}

		static protected Dictionary<System.Type, object> GetInstances()
		{
			return GetInstance()._instances;
		}

        static private object AddInstance(System.Type type)
        {
            object retValue = null;
            GameEntry instanceTmp = GetInstance();
            if (HasInstance(type))
            {
                Log.Error("AddInstance error: has added instance type=" + type.ToTypeString());
                return null;
            }
            else
            {
                retValue = type.IsInherited(typeof(IGameInstanceCreator)) ? type.InvokeMethod("Create") : type.Instantiate();
				if (null == retValue)
				{
                    retValue = type.Instantiate();
				}

                instanceTmp._instances.Add(type, retValue);
            }
            return retValue;
        }
    }
}

