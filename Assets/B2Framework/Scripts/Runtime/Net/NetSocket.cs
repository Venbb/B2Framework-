using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace B2Framework.Net
{
    public class NetPoint
    {
        public string host;
        public int port;
    }
    public class NetEvent
    {
        public bool sucess;
        public string error;
        public byte[] data;
        public int cmd;

        public NetEvent(bool sucess, int cmd = -1, byte[] data = null, string error = "")
        {
            this.sucess = sucess;
            this.cmd = cmd;
            this.data = data;
            this.error = error;
        }
    }
    public class NetSocket
    {
        private const int STREAM_TIMEOUT = 1000;
        private const int BUFFER_LEN = 65535;
        private byte[] _recvBuffer = new byte[BUFFER_LEN];
        public delegate void NetHandler(NetEvent evt);
        private Thread _conThread;
        private Thread _recvThread;
        public NetHandler onConnect { get; set; }
        public NetHandler onRecv { get; set; }
        public Action onReconnect { get; set; }
        private List<NetEvent> _events = new List<NetEvent>(2);

        private NetPoint _point;
        private TcpClient _tcpClient;
        private NetworkStream _socketStream = null;
        private NetBuffer _dataBuffer;

        private int connectId = 0;

        public void Connect(string host, int port)
        {
            Connect(new NetPoint() { host = host, port = port });
        }
        public void Connect(NetPoint point)
        {
            _point = point;

            _dataBuffer = new NetBuffer();
            //_recvThread = new Thread(new ThreadStart(DoRecv));
            _conThread = new Thread(new ThreadStart(DoConnect));
            _conThread.Start();
        }
        private void DoConnect()
        {
            try
            {
                IPAddress[] address = Dns.GetHostAddresses(_point.host);
                Log.Debug("try to connect host :" + _point.host);
                if (address.Length == 0)
                {
                    AddEvent(new NetEvent(false, -1, null, "host err!"));
                    return;
                }
                if (address[0].AddressFamily == AddressFamily.InterNetworkV6)
                {
                    _tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
                }
                else
                {
                    _tcpClient = new TcpClient(AddressFamily.InterNetwork);
                }

                _tcpClient.SendTimeout = STREAM_TIMEOUT;
                _tcpClient.ReceiveTimeout = STREAM_TIMEOUT;
                _tcpClient.NoDelay = true;
                _tcpClient.Connect(_point.host, _point.port);
                connectId++;

                if (_tcpClient.Connected)
                {
                    AddEvent(new NetEvent(true, -1));
                    _socketStream = _tcpClient.GetStream();
                }
                else
                {
                    Close();
                    AddEvent(new NetEvent(false, -1, null, "connect err!"));
                }
            }
            catch (Exception e)
            {
                Close();
                AddEvent(new NetEvent(false, -1, null, "connect err! " + e.ToString()));
                Log.Debug("DoConnect Err : " + e.ToString());
            }
        }
        public bool Send(int cmd, byte[] data)
        {
            int conId = connectId;
            NetBuffer buf = new NetBuffer();
            data = NetEncoder.OnSendData(data);

            NetEncoder.OnSendHead(buf, cmd, data.Length);
            buf.WriteBytes(data);

            try
            {
                if (_tcpClient != null && _tcpClient.Connected)
                {
                    _socketStream.Write(buf.ToBytes(), 0, buf.GetLength());
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Debug("SendCmd error :" + cmd + "," + e.ToString());
                Log.Debug("conid:" + conId + "conecntId:" + connectId);
                if (onReconnect != null && conId == connectId)
                {
                    onReconnect();
                }
                return false;
            }
        }
        protected void DoRecv()
        {
            if (_tcpClient != null && _tcpClient.Connected && _socketStream.DataAvailable)
            { //&& _socketStream.DataAvailable
                try
                {
                    int len = _socketStream.Read(_recvBuffer, 0, BUFFER_LEN);
                    if (len < 1)
                    {
                        //Close();
                        if (onReconnect != null)
                        {
                            onReconnect();
                        }
                        AddEvent(new NetEvent(false, -1, null, "connect disconnect!"));
                        return;
                    }
                    _dataBuffer.Seek(0, SeekOrigin.End);
                    _dataBuffer.WriteBytes(_recvBuffer, 0, len);
                    _dataBuffer.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception e)
                {
                    // Close();
                    if (onReconnect != null)
                    {
                        onReconnect();
                    }
                    AddEvent(new NetEvent(false, -1, null, "connect err!"));
                    Log.Debug("Read data from socket err :" + e.ToString());
                }
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

                    if (_recvThread != null)
                    {
                        AddEvent(new NetEvent(true, cmd, data)); //Encoding.UTF8.GetString( ) msgbuf.ReadBytes(len)
                    }
                    else
                    {
                        onRecv(new NetEvent(true, cmd, data));
                    }
                }
                else
                {
                    _dataBuffer.Seek(0, SeekOrigin.Begin);
                    // _dataBuffer.ResetRemain();
                }
            }
        }
        public void ProcessEvents()
        {
            if (_tcpClient == null || _socketStream == null)
            {
                return;
            }
            if (_tcpClient.Connected)
                DoRecv();
            lock (_events)
            {
                if (_events.Count > 0)
                {
                    for (int i = 0; i < _events.Count; i++)
                    {
                        if (_events[i].cmd > 0)
                        {
                            onRecv(_events[i]);
                        }
                        else
                        {
                            onConnect(_events[i]);
                        }
                    }
                    _events.Clear();
                }
            }
        }
        public void Disconnect()
        {
            Close();
        }
        public bool IsConnect()
        {
            if (_tcpClient != null)
            {
                return _tcpClient.Connected;
            }
            return false;
        }
        public void AddEvent(NetEvent evt)
        {
            lock (_events)
            {
                _events.Add(evt);
            }
        }
        public void Close()
        {
            if (_tcpClient != null)
            {
                if (_tcpClient.Connected) _tcpClient.Close();
                _tcpClient = null;
            }

            if (_conThread != null && _conThread.IsAlive)
            {
                _conThread.Abort();
                _conThread = null;
            }
            if (_recvThread != null && _recvThread.IsAlive)
            {
                _recvThread.Abort();
                _recvThread = null;
            }

            if (_dataBuffer != null)
            {
                _dataBuffer.Close();
                _dataBuffer = null;
            }
            if (_socketStream != null)
            {
                _socketStream.Close();
                _socketStream = null;
            }

            // _cfg = null;
            // _onConnect = null;
            // _onRecv = null;
        }
    }
}