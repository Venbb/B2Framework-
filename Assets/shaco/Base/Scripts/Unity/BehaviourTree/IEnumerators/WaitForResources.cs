namespace shaco
{
    public class WaitForResources : shaco.Base.IBehaviourEnumerator
    {
        private bool _isLoading = false;

        static public WaitForResources Create<T>(string path, System.Action<T> callbackEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString) where T : UnityEngine.Object
        {
            var retValue = CreateWithPool(() => new WaitForResources());
            retValue._isLoading = true;
            shaco.GameHelper.res.LoadResourcesOrLocalAsync<T>(path, (loadObj) =>
            {
                callbackEnd((T)loadObj);
                retValue._isLoading = false;
            }, null, multiVersionControlRelativePath);
            return retValue;
        }

        static public WaitForResources CreateWithAll(string path, System.Action<UnityEngine.Object[]> callbackEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            var retValue = CreateWithPool(() => new WaitForResources());
            retValue._isLoading = true;
            shaco.GameHelper.res.LoadResourcesOrLocalAsyncAll(path, (loadObjs) =>
            {
                callbackEnd(loadObjs);
                retValue._isLoading = false;
            }, null, multiVersionControlRelativePath);
            return retValue;
        }

        static public WaitForResources CreateWithSequeue(string[] paths, System.Action<int, UnityEngine.Object> callbackEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            var retValue = CreateWithPool(() => new WaitForResources());
            retValue._isLoading = true;
            var requestSequeue = new shaco.ResourcesOrLocalSequeue();

            for (int i = 0; i < paths.Length; ++i)
            {
                int indexTmp = i;
                requestSequeue.AddRequest(paths[i], (loadObj) =>
                {
                    callbackEnd(indexTmp, loadObj);
                }, typeof(UnityEngine.Object), multiVersionControlRelativePath);
            }
            requestSequeue.Start((percent) =>
            {
                if (percent >= 1.0f)
                {
                    retValue._isLoading = false;
                }
            });
            return retValue;
        }

        private WaitForResources()
		{
        }

        public override bool IsRunning()
		{
            return _isLoading;
        }

        public override void Reset()
		{
            _isLoading = false;
        }

        public override void Update(float elapseSeconds)
        {
        }
    }
}

