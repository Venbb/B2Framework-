using System;

namespace B2Framework
{
    public static partial class Utility
    {
        public static partial class Encryption
        {
            private static byte[] ENCRYPT_BYTES = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
            public static void GetBytes(byte[] bytes, int length = 0)
            {
                GetBytes(bytes, ENCRYPT_BYTES, length);
            }
            public static void GetBytes(byte[] bytes, byte[] encrypts, int length = 0)
            {
                GetBytes(bytes, 0, length, encrypts);
            }
            public static void GetBytes(byte[] bytes, int index, int length, byte[] encrypts)
            {
                if (bytes == null || encrypts == null || length <= 0) return;

                var len = encrypts.Length;
                if (len <= 0) return;

                index = Math.Max(0, index);
                length = Math.Min(length, bytes.Length);

                var idx = index % len;
                for (int i = index; i < length; i++)
                {
                    bytes[i] ^= encrypts[idx++];
                    idx %= len;
                }
            }
        }
    }
}