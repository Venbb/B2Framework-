using System.Collections;

namespace shaco
{
    /// <summary>
    /// 资源异步请求
    /// </summary>
    public class ResourcesRequest<T> : EnumeratorRequest<T> where T : UnityEngine.Object
    {
        public string path { get; private set; }

        /// <summary>
        /// 资源异步请求初始化
        /// <param name="path">路径</param>
        /// <param name="type">类型</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        public void Init(string path, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            this.path = path;
            
            shaco.GameHelper.res.LoadResourcesOrLocalAsync(path, type, (obj) =>
            {
                this.SetResult(obj as T);
            },
            (progress) =>
            {
                this.SetProgress(progress);
            }, multiVersionControlRelativePath);
        }
    }
}
