using System;

namespace B2Framework.Unity.Net
{
    public class NetManager : MonoSingleton<NetManager>, IDisposable
    {
        public NetClient client { get; private set; }
        public NetManager Initialize()
        {
            if (Game.platform != "WebGL")
                client = gameObject.AddComponent<NetSocketClient>();
            else
                client = gameObject.AddComponent<WebSocketClient>();
            return this;
        }
        public void Connect(string host, int port, int timeout, Action<bool, string> onConnect, Action<int, byte[]> onReceive, Action onFailed)
        {
            client?.Connect(host, port, timeout, onConnect, onReceive, onFailed);
        }
        public bool SendCmd(int cmd, byte[] data)
        {
            if (client != null) return client.SendCmd(cmd, data);
            return false;
        }
        public void Reconnect()
        {
            client?.Reconnect();
        }
        public void Disconnect()
        {
            client?.Disconnect();
        }
        public bool IsConnect()
        {
            if (client != null) return client.IsConnect();
            return false;
        }
        public void Close()
        {
            if (client != null)
                client.Close();
            client = null;
        }
        public void Dispose()
        {
            Close();
        }
    }
}