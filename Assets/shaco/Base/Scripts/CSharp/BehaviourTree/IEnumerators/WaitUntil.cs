namespace shaco.Base
{
    public class WaitUntil : shaco.Base.IBehaviourEnumerator
    {
        private System.Func<bool> _predicate = null;

        static public WaitUntil Create(System.Func<bool> predicate)
        {
            if (null == predicate)
            {
                Log.Error("WaitUntil Create error: callback is null");
                return null;
            }

            var retValue = CreateWithPool(() => new WaitUntil());
            retValue._predicate = predicate;
            return retValue;
        }

        public override bool IsRunning()
        {
            return !_predicate();
        }

        public override void Reset()
        {
            _predicate = null;
        }

        private WaitUntil() {}
    }
}

