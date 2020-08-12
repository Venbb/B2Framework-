using UnityEngine;

namespace B2Framework.Unity.Net
{
    public interface INetworkListener
    {
        void OnReachablityChanged(NetworkReachability reachability);
    }

    public class NetworkObserver : MonoBehaviour
    {
        private NetworkReachability _reachability;
        public INetworkListener listener { get; set; }
        [SerializeField] private float sampleTime = 0.5f;
        private float _time;
        private bool _running;

        void Start()
        {
            _reachability = Application.internetReachability;
            ReStart();
        }
        public void ReStart()
        {
            _time = Time.timeSinceLevelLoad;
            _running = true;
        }
        public void Stop()
        {
            _running = false;
        }
        void Update()
        {
            if (!_running) return;
            if (Time.timeSinceLevelLoad - _time >= sampleTime)
            {
                var state = Application.internetReachability;
                if (_reachability != state)
                {
                    listener?.OnReachablityChanged(state);
                    _reachability = state;
                }
                _time = Time.timeSinceLevelLoad;
            }
        }
    }
}
