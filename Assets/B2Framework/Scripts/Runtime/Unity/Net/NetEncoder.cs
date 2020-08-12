using System;
using Xxtea;
using System.IO;

namespace B2Framework.Unity.Net
{
    static class NetEncoder
    {
        public static int HEAD_LEN = 8;
        private static bool _isXorEncrypt = false;
        private static byte[] _xorKey;

        private static bool _isXXTEAEncrypt = false;
        private static byte[] _XXTEAKey;

        static void _doInit(bool isXorEncrypt, byte[] xorKey, bool isXXTEAEncrypt, byte[] XXTEAKey)
        {
            _isXorEncrypt = isXorEncrypt;
            _xorKey = xorKey;
            _isXXTEAEncrypt = isXXTEAEncrypt;
            _XXTEAKey = XXTEAKey;
        }

        public static void Init()
        {
            byte[] _headKey = { 0x01, 0x02, 0x03, 0x04, 0x05 };
            byte[] _dataKey = { 0x01, 0x02, 0x03, 0x04, 0x05 };
            _doInit(true, _headKey, true, _dataKey);
        }

        public static void OnRecvHead(NetBuffer buffer, out int cmd, out int datalen)
        {
            if (_isXorEncrypt)
            {
                NetBuffer buf = new NetBuffer();
                byte[] tmp = buffer.ReadBytes(HEAD_LEN);
                buf.WriteBytes(NetEncoder.XOR(tmp, HEAD_LEN, _xorKey, _xorKey.Length));
                buf.Seek(0, SeekOrigin.Begin);
                cmd = (int)buf.ReadUInt();
                datalen = (int)buf.ReadUInt();
                if (datalen < 0)
                {
                    buffer.PrintBytes();
                    buf.PrintBytes();
                    UnityEngine.Debug.LogError("error datalen " + datalen);
                }
            }
            else
            {
                cmd = (int)buffer.ReadUInt();
                datalen = (int)buffer.ReadUInt();
                if (datalen < 0)
                {
                    UnityEngine.Debug.LogError("error datalen " + datalen);
                }
            }
        }

        public static byte[] OnRecvData(byte[] bts)
        {
            byte[] ret;
            if (_isXXTEAEncrypt)
            {
                ret = NetEncoder.XXTEADecrypt(bts, _XXTEAKey);
            }
            else
            {
                ret = bts;
            }
            return ret;
        }

        public static void OnSendHead(NetBuffer buf, int cmd, int datalen)
        {
            if (_isXorEncrypt)
            {
                NetBuffer tmp = new NetBuffer();
                tmp.WriteUInt((uint)cmd);
                tmp.WriteUInt((uint)(datalen));
                buf.WriteBytes(NetEncoder.XOR(tmp.ToBytes(), HEAD_LEN, _xorKey, _xorKey.Length));
            }
            else
            {
                buf.WriteUInt((uint)cmd);
                buf.WriteUInt((uint)(datalen));
            }
        }

        public static byte[] OnSendData(byte[] bts)
        {
            byte[] ret;
            if (_isXXTEAEncrypt)
            {
                ret = NetEncoder.XXTEAEncrypt(bts, _XXTEAKey);
            }
            else
            {
                ret = bts;
            }
            return ret;
        }

        static byte[] XOR(byte[] data, int dataLen, byte[] key, int keyLen)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ key[i % keyLen]);
            }

            return data;
        }

        static byte[] XXTEAEncrypt(byte[] Data, byte[] Key)
        {
            return XXTEA.Encrypt(Data, Key);
        }

        static byte[] XXTEADecrypt(byte[] Data, byte[] Key)
        {
            return XXTEA.Decrypt(Data, Key);
        }
    }
}