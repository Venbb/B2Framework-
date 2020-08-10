using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace B2Framework.Net
{
    public class WebSocketClient : NetClient
    {
        private WebSocket _socket;
        private NetBuffer _dataBuffer;

        void Awake()
        {
            _dataBuffer = new NetBuffer();
        }
        public override void Connect(string host, int port, int timeout, Action<bool, string> onConnect, Action<int, byte[]> onReceive, Action onFailed)
        {
            netPoint = new NetPoint() { host = host, port = port };
            this.timeout = timeout;

            this.onConnect = onConnect;
            this.onReceive = onReceive;
            this.onFailed = onFailed;

            // string _uri = Utility.Text.Format("ws://{0}:{1}/", host, port);
            var url = Utility.Text.Format("{0}:{1}/", host, port);
            _socket = new WebSocket(new Uri(url));
            _socket.m_onConnect = () =>
            {
                CancelInvoke("CheckConnectTimeOut");
                onConnect(true, "");
            };
            _socket.m_onReconnect = () =>
            {
                if (IsConnect())
                    onConnect(true, "");
                else
                    onFailed();
            };

            StartCoroutine(_doConnect());

            Invoke("CheckConnectTimeOut", timeout);
        }
        public override bool SendCmd(int cmd, byte[] data)
        {
            NetBuffer buf = new NetBuffer();
            data = NetEncoder.OnSendData(data);

            NetEncoder.OnSendHead(buf, cmd, data.Length);
            buf.WriteBytes(data);

            _socket.Send(buf.ToBytes());
            return true;
        }
        private void CheckConnectTimeOut()
        {
            if (!IsConnect())
            {
                Disconnect();
                if (onConnect != null)
                {
                    onConnect(false, "time out");
                }
            }
        }

        void Update()
        {
            if (IsConnect())
            {
                byte[] bts = _socket.Recv();

                if (bts != null && bts.Length > 0)
                {
                    _dataBuffer.Seek(0, SeekOrigin.End);
                    _dataBuffer.WriteBytes(bts, 0, bts.Length);
                    _dataBuffer.Seek(0, SeekOrigin.Begin);
                }

                if (_dataBuffer.RemainLen() > NetEncoder.HEAD_LEN)
                {
                    int cmd, datalen;
                    NetEncoder.OnRecvHead(_dataBuffer, out cmd, out datalen);

                    if (_dataBuffer.RemainLen() >= datalen)
                    {
                        NetBuffer msgbuf = new NetBuffer();
                        msgbuf.WriteBytes(_dataBuffer.ReadBytes(datalen));
                        msgbuf.Seek(0, SeekOrigin.Begin);
                        _dataBuffer.ResetRemain();
                        int len = msgbuf.GetLength();

                        byte[] data;
                        data = NetEncoder.OnRecvData(msgbuf.ReadBytes(len));

                        if (cmd == 0)
                        {
                            onConnect?.Invoke(true, "");
                        }
                        onReceive?.Invoke(cmd, data);
                    }
                    else
                    {
                        _dataBuffer.Seek(0, SeekOrigin.Begin);
                        // _dataBuffer.ResetRemain();
                    }
                }
            }
        }

        IEnumerator _doConnect()
        {
            yield return StartCoroutine(_socket.Connect());
        }

        public override void Disconnect()
        {
            if (_socket != null)
            {
                _socket.Close();
            }
        }

        public override bool IsConnect()
        {
            return _socket != null && _socket.IsConnected();
        }

        void OnDestroy()
        {
            Close();
        }

        public override void Reconnect()
        {
            if (_socket != null)
            {
                _doConnect();
                Invoke("CheckConnectTimeOut", timeout);
            }
            else
            {
                Connect(netPoint.host, netPoint.port, timeout, onConnect, onReceive, onFailed);
            }

        }

        public override void Close()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
        }
    }
}