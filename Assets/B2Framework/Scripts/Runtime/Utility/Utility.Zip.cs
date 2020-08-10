using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace B2Framework
{
    public static partial class Utility
    {
        public static partial class Zip
        {
            /// <summary>
            /// 压缩字符转
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string Compress(string str)
            {
                if (string.IsNullOrEmpty(str)) return str;
                var bytes = Compress(Encoding.UTF8.GetBytes(str));
                return Convert.ToBase64String(bytes);
            }
            /// <summary>
            /// 解压字符串
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string Decompress(string str)
            {
                if (string.IsNullOrEmpty(str)) return str;
                var bytes = Decompress(Convert.FromBase64String(str));
                return Encoding.UTF8.GetString(bytes);
            }
            /// <summary>
            /// 压缩字节数组
            /// </summary>
            /// <param name="bytes"></param>
            /// <returns></returns>
            public static byte[] Compress(byte[] bytes)
            {
                if (bytes == null || bytes.Length <= 0) return bytes;
                try
                {
                    using (var stream = new MemoryStream(bytes))
                    using (var compressedStream = new MemoryStream())
                    {
                        if (Compress(stream, compressedStream))
                        {
                            return compressedStream.ToArray();
                        }
                        else return null;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            /// <summary>
            /// 解压字节数组
            /// </summary>
            /// <param name="bytes"></param>
            /// <returns></returns>
            public static byte[] Decompress(byte[] bytes)
            {
                if (bytes == null || bytes.Length <= 0) return bytes;
                using (var stream = new MemoryStream(bytes))
                using (var decompressedStream = new MemoryStream())
                {
                    if (Decompress(stream, decompressedStream))
                    {
                        return decompressedStream.ToArray();
                    }
                    else return null;
                }
            }
            /// <summary>
            /// 压缩二进制流
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="compressedStream"></param>
            /// <returns></returns>
            public static bool Compress(Stream stream, Stream compressedStream)
            {
                if (stream == null || compressedStream == null) return false;
                try
                {
                    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        return CopyStream(stream, zipStream);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            /// <summary>
            /// 解压二进制流
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="decompressedStream"></param>
            /// <returns></returns>
            public static bool Decompress(Stream stream, Stream decompressedStream)
            {
                if (stream == null || decompressedStream == null) return false;
                try
                {
                    using (var zipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        return CopyStream(zipStream, decompressedStream);
                        // zipStream.CopyTo(decompressedStream);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            /// <summary>
            /// 拷贝数据流
            /// </summary>
            /// <param name="originalStream"></param>
            /// <param name="destStream"></param>
            /// <returns></returns>
            internal static bool CopyStream(Stream originalStream, Stream destStream)
            {
                try
                {
                    var buffer = new byte[0x1000];//4K，每次读取长度
                    int length; //真实读取到的长度
                    while ((length = originalStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // 每次读取len，确保数据读取不会错误
                        destStream.Write(buffer, 0, length);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }
}