using System.IO;
using System.Text;
using System;

namespace B2Framework.Net
{
    public class NetBuffer
    {
        MemoryStream stream = null;
        BinaryWriter writer = null;
        BinaryReader reader = null;

        public NetBuffer(byte[] data = null)
        {
            if (data != null)
            {
                stream = new MemoryStream(data);
            }
            else
            {
                stream = new MemoryStream();
            }
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
        }

        public void Close()
        {
            if (writer != null) writer.Close();
            if (reader != null) reader.Close();
            if (stream != null) stream.Close();

            writer = null;
            reader = null;
            stream = null;
        }

        public void Seek(int offset, SeekOrigin loc)
        {
            stream.Seek(offset, loc);
        }

        public void SetLength(int len = 0)
        {
            stream.SetLength(len);
        }

        public int RemainLen()
        {
            return (int)(stream.Length - stream.Position);
        }

        public int GetLength()
        {
            return (int)stream.Length;
        }

        public void ResetRemain()
        {
            byte[] leftover = reader.ReadBytes(RemainLen());
            stream.SetLength(0);
            stream.Write(leftover, 0, leftover.Length);
            stream.Seek(0, SeekOrigin.Begin);
        }



        public void WriteByte(byte v)
        {
            writer.Write(v);
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public void WriteInt(int v)
        {
            writer.Write((int)v);
        }

        public int ReadInt()
        {
            return (int)reader.ReadInt32();
        }

        public void WriteUInt(uint v)
        {
            writer.Write((uint)v);
        }

        public uint ReadUInt()
        {
            return reader.ReadUInt32();
        }

        public void WriteLong(long v)
        {
            writer.Write((long)v);
        }

        public long ReadLong()
        {
            return (long)reader.ReadInt64();
        }

        public void WriteUShort(ushort v)
        {
            writer.Write((ushort)v);
        }

        public ushort ReadUShort()
        {
            return (ushort)reader.ReadUInt16();
        }

        public void WriteShort(short v)
        {
            writer.Write((short)v);
        }

        public ushort ReadShort()
        {
            return (ushort)reader.ReadInt16();
        }

        public void WriteFloat(float v)
        {
            byte[] temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            writer.Write(BitConverter.ToSingle(temp, 0));
        }

        public float ReadFloat()
        {
            byte[] temp = BitConverter.GetBytes(reader.ReadSingle());
            Array.Reverse(temp);
            return BitConverter.ToSingle(temp, 0);
        }

        public void WriteDouble(double v)
        {
            byte[] temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            writer.Write(BitConverter.ToDouble(temp, 0));
        }

        public double ReadDouble()
        {
            byte[] temp = BitConverter.GetBytes(reader.ReadDouble());
            Array.Reverse(temp);
            return BitConverter.ToDouble(temp, 0);
        }

        public void WriteString(string v)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(v);
            writer.Write((uint)bytes.Length);
            writer.Write(bytes);
        }

        public string ReadString()
        {
            int len = (int)ReadUInt();
            byte[] buffer = new byte[len];
            buffer = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(buffer);
        }

        public void WriteBytes(byte[] v)
        {
            writer.Write(v);
        }

        public void WriteBytes(byte[] v, int index, int len)
        {
            writer.Write(v, 0, len);
        }

        public byte[] ReadBytes(int len)
        {
            return reader.ReadBytes(len);
        }

        public byte[] ToBytes()
        {
            writer.Flush();
            return stream.ToArray();
        }

        public void Flush()
        {
            writer.Flush();
        }

        public void PrintBytes()
        {
            string returnStr = string.Empty;
            byte[] a = stream.ToArray();
            for (int i = 0; i < a.Length; i++)
            {
                returnStr += a[i].ToString("X2");
            }
            Debug.Log(returnStr);
        }

        public string GetString()
        {
            return reader.ReadString();
        }
    }
}