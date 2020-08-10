using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace B2Framework.Net
{
    public class NetSocketClient : NetClient
    {
        protected NetSocket socket;

        public override void Connect(string host, int port, int timeout, Action<bool, string> onConnect, Action<int, byte[]> onReceive, Action onFailed)
        {
            netPoint = new NetPoint() { host = host, port = port };
            this.timeout = timeout;

            socket = new NetSocket();
            socket.onConnect = OnNetConnect;
            socket.onRecv = OnRecvCmd;
            socket.onReconnect = OnReconnect;
            socket.Connect(netPoint);

            Invoke("CheckConnectTimeOut", timeout);
        }
        public override bool SendCmd(int cmd, byte[] data)
        {
            Debug.LogFormat("CS SendCmd :" + cmd + " | " + data.Length, socket);
            if (socket != null)
                return socket.Send(cmd, data);
            else
                return false;
        }
        public override void Reconnect()
        {
            if (socket != null)
            {
                socket.Connect(netPoint);
            }
            else
            {
                socket = new NetSocket();
                socket.onConnect = OnNetConnect;
                socket.onRecv = OnRecvCmd;
                socket.onReconnect = OnReconnect;
                socket.Connect(netPoint);
            }
            Invoke("CheckConnectTimeOut", timeout);
        }
        protected virtual void CheckConnectTimeOut()
        {
            if (!IsConnect())
            {
                Disconnect();
                OnNetConnect(new NetEvent(false, -1, null, "Connect Timeout!"));
            }
        }
        public override void Disconnect()
        {
            if (socket != null)
            {
                socket.Disconnect();
            }
        }

        public override bool IsConnect()
        {
            if (socket != null)
                return socket.IsConnect();
            else
                return false;
        }
        protected virtual void OnNetConnect(NetEvent evt)
        {
            Debug.Log("CS OnNetConnect : " + evt.sucess + " ; data : " + evt.error.ToString());
            if (!string.IsNullOrEmpty(evt.error))
            {
                if (null != onFailed) onFailed();
            }
            else
            {
                if (onConnect != null) onConnect(evt.sucess, evt.error);
            }
        }
        protected virtual void OnRecvCmd(NetEvent evt)
        {
            onReceive?.Invoke(evt.cmd, evt.data);
        }
        protected virtual void OnReconnect()
        {
            if (onFailed != null) onFailed();
        }
        void Update()
        {
            if (socket != null)
                socket.ProcessEvents();
        }
        void OnDestroy()
        {
            Close();
        }
        public override void Close()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
    }
}