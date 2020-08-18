using UnityEngine;

namespace B2Framework
{
    public sealed class DontDestroyOnLoad : MonoBehaviour
    {
        private static bool _created;
        void Awake()
        {
            if (!_created)
            {
                DontDestroyOnLoad(gameObject);
                _created = true;

            }
            else DestroyImmediate(gameObject);
        }
    }
}