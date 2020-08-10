namespace shaco.Base
{
    public class WaitForFrame : shaco.Base.IBehaviourEnumerator
    {
        private float _frame = 1;
        private float _currentFrame = 0;

        static public WaitForFrame Create(int frame = 1)
        {
            var retValue = CreateWithPool(() => new WaitForFrame());
            retValue._frame = frame;
            return retValue;
        }

        private WaitForFrame()
        {
        }

        public override bool IsRunning()
        {
            return this._currentFrame++ < this._frame;
        }

        public override void Reset()
        {
            _frame = 1;
            _currentFrame = 0;
        }
    }
}

