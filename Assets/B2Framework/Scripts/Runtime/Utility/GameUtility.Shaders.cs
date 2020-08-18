using System.Collections.Generic;
using UnityEngine;

namespace B2Framework
{
    public static partial class GameUtility
    {
        public static partial class Shaders
        {
            // Shader.Find是一个非常消耗的函数，因此尽量缓存起来
            private static readonly Dictionary<string, Shader> CacheShaders = new Dictionary<string, Shader>();
            public static Shader Find(string shaderName)
            {
                Shader shader;
                if (!CacheShaders.TryGetValue(shaderName, out shader))
                {
                    shader = Shader.Find(shaderName);
                    CacheShaders[shaderName] = shader;
                    if (shader == null)
                        Log.Error("缺少Shader：{0}  ， 检查Graphics Settings的预置shader", shaderName);
                }

                return shader;
            }
        }
    }
}