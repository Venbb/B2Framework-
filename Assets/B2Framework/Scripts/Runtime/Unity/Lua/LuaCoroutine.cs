using UnityEngine;

namespace B2Framework.Unity
{
    public class LuaCoroutine : MonoBehaviour
    {
        public void Dispose()
        {
            StopAllCoroutines();
        }
        void OnDestroy()
        {
            Dispose();
        }
    }
}