using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace B2Framework
{
    public static partial class Utility
    {
        public static partial class Verifier
        {
            private static readonly MD5 md5 = MD5.Create();
            private static readonly Crc32 crc32 = new Crc32();
            /// <summary>
            /// 获取MD5 Hash值
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string GetMD5(string str)
            {
                return GetMD5(Encoding.UTF8.GetBytes(str));
            }
            /// <summary>
            /// 获取MD5 Hash值
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string GetMD5(Stream input)
            {
                var data = md5.ComputeHash(input);
                return ToHash(data);
            }
            /// <summary>
            /// 获取MD5 Hash值
            /// </summary>
            /// <param name="buffer"></param>
            /// <returns></returns>
            public static string GetMD5(byte[] buffer)
            {
                // MD5 md5 = new MD5CryptoServiceProvider();
                // 计算字节数组哈希数据  
                var data = md5.ComputeHash(buffer);
                // md5.Clear();
                return ToHash(data);
            }
            /// <summary>
            /// 获取CRC32 Hash值
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string GetCRC32(string str)
            {
                return GetCRC32(Encoding.UTF8.GetBytes(str));
            }
            /// <summary>
            /// 获取CRC32 Hash值
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string GetCRC32(Stream input)
            {
                var data = crc32.ComputeHash(input);
                return ToHash(data);
            }
            /// <summary>
            /// 获取CRC32 Hash值
            /// </summary>
            /// <param name="buffer"></param>
            /// <returns></returns>
            public static string GetCRC32(byte[] buffer)
            {
                var data = crc32.ComputeHash(buffer);
                return ToHash(data);
            }
            /// <summary>
            /// 将字节数组转换成16进制字符串
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public static string ToHash(byte[] data)
            {
                // 创建一个 Stringbuilder 来收集字节并创建字符串  
                var sb = new StringBuilder();
                // 循环遍历哈希数据的每一个字节并格式化为十六进制字符串  
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));//（小写）加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位
                }
                // 返回十六进制字符串  
                return sb.ToString();
            }
        }
    }
}