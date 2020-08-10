using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
	/// <summary>
	/// 资源加载接口
	/// </summary>
    public interface IResources
    {
		UnityEngine.Object Load(string path, System.Type type, string multiVersionControlRelativePath);
        UnityEngine.Object[] LoadAll(string path, System.Type type, string multiVersionControlRelativePath);
        void LoadAsync(string path, System.Type type, System.Action<float> callbackProgress, System.Action<UnityEngine.Object> callbackEnd, string multiVersionControlRelativePath);
    }
}