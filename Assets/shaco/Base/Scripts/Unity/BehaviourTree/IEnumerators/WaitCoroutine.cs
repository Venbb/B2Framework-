namespace shaco
{
    public class WaitCoroutine : shaco.Base.IBehaviourEnumerator
    {
        private bool _isEnd = false;

        static public WaitCoroutine Create(System.Collections.IEnumerator coroutine)
        {
            if (null == coroutine)
            {
                shaco.Log.Error("WaitCoroutine Create error: coroutine is null");
                return null;
            }

            var retValue = CreateWithPool(() => new WaitCoroutine());
            shaco.GameHelper.StartCoroutine(retValue.WaitFunction(coroutine));
            retValue._isEnd = false;
            return retValue;
        }

        public override bool IsRunning()
        {
            return !_isEnd;
        }

        public override void Reset()
        {
            _isEnd = false;
        }

        private System.Collections.IEnumerator WaitFunction(System.Collections.IEnumerator coroutine)
        {
            yield return coroutine;
            _isEnd = true;
        }

        private WaitCoroutine() {}
    }
}

