namespace shaco.Base
{
    public class WaitforSeconds : shaco.Base.IBehaviourEnumerator
    {
        private float _seconds = 1;
        private float _currentSeconds = 0;

        static public WaitforSeconds Create(float seconds)
        {
            var retValue = CreateWithPool(() => new WaitforSeconds());
            retValue._seconds = seconds;
            return retValue;
        }

        private WaitforSeconds()
		{
        }

        public override bool IsRunning()
		{
            return this._currentSeconds < this._seconds;
        }

        public override void Reset()
		{
            _currentSeconds = 0;
            _seconds = 1;
        }

        public override void Update(float elapseSeconds)
        {
            _currentSeconds += elapseSeconds;
        }
    }
}

