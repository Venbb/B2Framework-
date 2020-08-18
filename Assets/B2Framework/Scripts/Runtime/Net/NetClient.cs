using System;
using UnityEngine;

namespace B2Framework.Net
{
    public abstract class NetClient : MonoBehaviour
    {
        protected NetPoint netPoint;
        protected int timeout;
        protected Action<bool, string> onConnect = null;
        protected Action<int, byte[]> onReceive = null;
        protected Action onFailed = null;
        public abstract void Connect(string host, int port, int timeout, Action<bool, string> onConnect, Action<int, byte[]> onReceive, Action onFailed);
        public abstract bool SendCmd(int cmd, byte[] data);
        public abstract void Reconnect();
        public abstract void Disconnect();
        public abstract bool IsConnect();
        public abstract void Close();
    }
}