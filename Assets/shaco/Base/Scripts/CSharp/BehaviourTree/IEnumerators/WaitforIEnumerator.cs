using System.Collections.Generic;

namespace shaco.Base
{
    public class WaitforIEnumerator : shaco.Base.IBehaviourEnumerator
    {
        private IEnumerator<shaco.Base.IBehaviourEnumerator> _enumerator = null;

        static public WaitforIEnumerator Create(IEnumerator<shaco.Base.IBehaviourEnumerator> enumerator)
        {
            if (null == enumerator)
            {
                Log.Error("WaitforIEnumerator Create error: enumerator is null");
                return null;
            }

            var retValue = CreateWithPool(() => new WaitforIEnumerator());
            retValue._enumerator = enumerator;
            return retValue;
        }

        private WaitforIEnumerator()
		{
        }

        public override bool IsRunning()
		{
            if (null == _enumerator)
            {
                Log.Info("WaitforIEnumerator IsRunning error: enuerator is null");
                return false;
            }

            if (null == _enumerator.Current || !_enumerator.Current.IsRunning())
            {
                try
                {
                    _enumerator.MoveNext();
                }
                catch (System.Exception e)
                {
                    Log.Error("WaitforIEnumerator IsRunning erorr: e=" + e);
                    _enumerator = null;
                    return false;
                }
            }

            return null != _enumerator.Current && _enumerator.Current.IsRunning();
        }

        public override void Reset()
		{
            _enumerator = null;
        }

        public override void Update(float elapseSeconds)
        {
            if (null == _enumerator)
            {
                Log.Info("WaitforIEnumerator Update error: enuerator is null");
                return;
            }

            if (null == _enumerator.Current)
            {
                try
                {
                    _enumerator.MoveNext();
                }
                catch (System.Exception e)
                {
                    Log.Error("WaitforIEnumerator Update erorr: e=" + e);
                    _enumerator = null;
                    return;
                }
            }
            
            if (null != _enumerator.Current)
                _enumerator.Current.Update(elapseSeconds);
        }
    }
}

