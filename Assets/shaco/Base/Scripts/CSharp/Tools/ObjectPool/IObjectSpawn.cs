using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
	public interface IObjectSpawn : IGameInstance
	{
        T CreateNewObject<T>(System.Func<T> callbackCreate);

        void CreateNewObjectAsync<T>(System.Action<System.Action<T>> callbackCreate, System.Action<T> callbackEnd);

		void ActiveObject<T>(T obj);

        void RecyclingObject<T>(T obj);

		void DestroyObject<T>(T obj);
	}
}

