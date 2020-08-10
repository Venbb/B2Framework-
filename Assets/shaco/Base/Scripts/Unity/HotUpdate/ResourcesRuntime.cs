using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class ResourcesRuntime : IResources
    {
        private const string RESOURCES_FOLDER_TAG = "/resources/";

        public UnityEngine.Object Load(string path, System.Type type, string multiVersionControlRelativePath)
		{
            var retValue = Resources.Load(GetValidResourcesPath(path), type);

            if (null != retValue)
            {
                shaco.GameHelper.resCache.AddReferenceCount(path, multiVersionControlRelativePath, true);
            }
            return retValue;
		}

        public UnityEngine.Object[] LoadAll(string path, System.Type type, string multiVersionControlRelativePath)
		{
            var retValue = Resources.LoadAll(GetValidResourcesPath(path), type);
            if (!retValue.IsNullOrEmpty())
            {
                shaco.GameHelper.resCache.AddReferenceCount(path, multiVersionControlRelativePath, true);
            }
            return retValue;
        }

        public void LoadAsync(string path, System.Type type, System.Action<float> callbackProgress, System.Action<UnityEngine.Object> callbackEnd, string multiVersionControlRelativePath)
		{
            var request = Resources.LoadAsync(GetValidResourcesPath(path), type);
            shaco.Base.WaitFor.Run(() =>
			{
				if (null != callbackProgress && (request.isDone && null != request.asset))
                {
                    callbackProgress(request.progress);
                }
                return request.isDone;
			},
			() =>
			{
                if (null == request.asset)
                {
                    callbackEnd(null);
                }
                else
                {
                    shaco.GameHelper.resCache.AddReferenceCount(path, multiVersionControlRelativePath, true);
                    callbackEnd(request.asset);
                }
			});
        }

        static public string GetValidResourcesPath(string path)
        {
            var indexFind = path.IndexOf(RESOURCES_FOLDER_TAG);
            if (indexFind >= 0)
            {
                path = path.Remove(0, indexFind + RESOURCES_FOLDER_TAG.Length);
            }

            var indexFind1 = path.LastIndexOf(shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            var indexFind2 = path.LastIndexOf(shaco.Base.FileDefine.DOT_SPLIT);
            if (indexFind1 < indexFind2)
            {
                path = path.Remove(indexFind2);
            }
            return path;
        }
    }
}