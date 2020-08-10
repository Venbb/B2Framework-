using UnityEngine;

namespace B2Framework
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