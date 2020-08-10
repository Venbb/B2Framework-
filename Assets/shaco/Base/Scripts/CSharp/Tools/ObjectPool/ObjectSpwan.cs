using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class ObjectSpwan : IObjectSpawn
    {
        public T CreateNewObject<T>(System.Func<T> callbackCreate)
        {
            return callbackCreate();
        }

        public void CreateNewObjectAsync<T>(System.Action<System.Action<T>> callbackCreate, System.Action<T> callbackEnd)
        {
            callbackCreate((valueValue) =>
            {
                try
                {
                    callbackEnd(valueValue);
                }
                catch (System.Exception e)
                {
                    Log.Error("ObjectSpwan CreateNewObjectAsync error: callbackCreate=" + callbackCreate + " callbackEnd=" + callbackEnd + " e=" + e);
                }
            });
        }

        public void ActiveObject<T>(T obj)
		{
			///...do nothing
		}

        public void RecyclingObject<T>(T obj)
        {
            obj = default(T);
        }

        public void DestroyObject<T>(T obj)
		{
            obj = default(T);
        }
    }
}
