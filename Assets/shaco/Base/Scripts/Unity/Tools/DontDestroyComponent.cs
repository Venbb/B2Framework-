
using UnityEngine;
using System.Collections;

namespace shaco
{
    public class DontDestroyComponent : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}

