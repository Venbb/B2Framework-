using System;
using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public static class EncryptDecrypt
    {
        static private readonly string FLAG_ENCRYPTION = "[shaco_secret_flag]";
        static private readonly string FLAG_ENCRYPTION_PARAM_BEGIN = "[shaco_secret_param_begin]";
        static private readonly string FLAG_ENCRYPTION_PARAM_END = "[shaco_secret_param_end]";

        //跳转速度下标
        static private readonly int JUMP_SPEED_PARAMETER_INDEX = 0;

        //加密码下标
        static private readonly int SECRET_CODE_PARAMETER_INDEX = 1;

        //自定义字段开始下标
        static private readonly int CUSTOM_PARAMETER_START_INDEX = 2;

        //参数分割符
        static private readonly char PARAMETER_SPLIT_FLAG = ',';

        //默认的加密码
        static readonly public char ENCRYPT_SECRET_CODE = (char)14;

        /// <summary>
        /// encrypt and overwrite
        /// </summary>
        /// <param name ="path">文件路径</param>
        /// <param name ="jumpSpeed">当范围(0 ~ 1)按百分比进行跳转速度加密。当范围(大于等于1)按固定整数数量跳转速度加密</param>
        /// <param name ="secretCode">加密字符</param>
        static public void EncryptPath(string path, float jumpSpeed = 0.5f, long secretCode = 36, params object[] customParameters)
        {
            if (!System.IO.File.Exists(path))
            {
                EncryptDecrypt.PrintError("EncryptPath error: not found path=" + path);
                return;
            }
            
            var readBytesTmp = System.IO.File.ReadAllBytes(path);

            if (null == readBytesTmp || 0 == readBytesTmp.Length)
            {
                return;
            }

            var encryptBytes = Encrypt(readBytesTmp, jumpSpeed, secretCode, customParameters);
            if (null != encryptBytes)
            {
                try
                {
                    //overwrite
                    // if (System.IO.File.Exists(path))
                    //     System.IO.File.Delete(path);
                    FileHelper.WriteAllByteByUserPath(path, encryptBytes);
                }
                catch (System.Exception e)
                {
                    EncryptDecrypt.PrintError("EncryptPath error: overwrite \nexception=" + e);
                }
            }
        }

        static public byte[] Encrypt(byte[] bytes, float jumpSpeed = 0.5f, long secretCode = 36, params object[] customParameters)
        {
            byte[] retValue = null;

            if (null == bytes || 0 == bytes.Length)
                return bytes;

            if (IsEncryption(bytes))
            {
                EncryptDecrypt.PrintError("Encrypt error: has been encrypted, byte length=" + bytes.Length);
                return retValue;
            }

            var encryptionString = GetEncryptString(jumpSpeed, secretCode, customParameters);
            retValue = new byte[bytes.Length + encryptionString.Length];

            //------------------------------------------------------------
            //encrypting
            //------------------------------------------------------------

            try
            {
                //set encryption flag
                for (int i = 0; i < encryptionString.Length; ++i)
                {
                    retValue[i] = (byte)encryptionString[i];
                }
            }
            catch (System.Exception e)
            {
                EncryptDecrypt.PrintError("Encrypt error: set encryption flag \nexception=" + e);
                return retValue;
            }

            try
            {
                //set encryption bytes
                bytes = EncryptOrDecrypt(bytes, jumpSpeed, secretCode);
            }
            catch (System.Exception e)
            {
                EncryptDecrypt.PrintError("Encrypt error: set encryption bytes \nexception=" + e);
                return retValue;
            }

            try
            {
                System.Array.Copy(bytes, 0, retValue, encryptionString.Length, bytes.Length);
            }
            catch (System.Exception e)
            {
                EncryptDecrypt.PrintError("Encrypt error: copy encrypt string \nexception=" + e);
                return retValue;
            }
            return retValue;
        }

        /// <summary>
        /// 根据文件路径解密，采用流式读取，避免了二次加载文件效率更高
        /// </summary>
        /// <param name ="path">file path</param>
        static public byte[] DecryptPath(string path)
        {
            byte[] retValue = null;
            if (!System.IO.File.Exists(path))
            {
                return retValue;
            }
            
            using (var fileStream = System.IO.File.Open(path, System.IO.FileMode.OpenOrCreate))
            {
                retValue = DecryptStream(fileStream);
                fileStream.Close();
            }
            return null == retValue ? System.IO.File.ReadAllBytes(path) : retValue;
        }

        static public byte[] DecryptStream(System.IO.FileStream stream)
        {
            if (!IsEncryptionStream(stream))
            {
                return null;
            }

            //------------------------------------------------------------
            //decrypting
            //------------------------------------------------------------

            var paramsTmp = GetEncryptParametersStream(stream);
            float? jumpSpeed = null;
            long? secretCode = null;

            //jump length
            if (paramsTmp.Count > JUMP_SPEED_PARAMETER_INDEX && !string.IsNullOrEmpty(paramsTmp[JUMP_SPEED_PARAMETER_INDEX]))
            {
                jumpSpeed = float.Parse(paramsTmp[JUMP_SPEED_PARAMETER_INDEX]);
            }
            //encrypt or decrypt code
            if (paramsTmp.Count > SECRET_CODE_PARAMETER_INDEX && !string.IsNullOrEmpty(paramsTmp[SECRET_CODE_PARAMETER_INDEX]))
            {
                secretCode = long.Parse(paramsTmp[SECRET_CODE_PARAMETER_INDEX]);
            }

            int encryptionStringLength = 0;

            //Compatibility of old versions of decryption
            if (jumpSpeed == null && secretCode == null)
            {
                jumpSpeed = 14;
                secretCode = 36;
            }
            else
            {
                encryptionStringLength = GetEncryptStringLengthStream(stream);
            }

            int newBytesCount = (int)(stream.Length - encryptionStringLength);
            if (newBytesCount <= 0)
            {
                EncryptDecrypt.PrintError("Decrypt error: invalid bytes count=" + newBytesCount);
                return null;
            }

            var decryptBytes = new byte[newBytesCount];
            try
            {
                //remove encryption flag
                stream.Seek(encryptionStringLength, System.IO.SeekOrigin.Begin);
                stream.Read(decryptBytes, 0, (int)newBytesCount);
            }
            catch (System.Exception e)
            {
                EncryptDecrypt.PrintError("Decrypt error: remove encryption flag \nexception=" + e);
                decryptBytes = null;
                return decryptBytes;
            }

            try
            {
                //set decryption bytes
                decryptBytes = EncryptOrDecrypt(decryptBytes, jumpSpeed, secretCode);
            }
            catch (System.Exception e)
            {
                EncryptDecrypt.PrintError("Decrypt error: set decryption bytes \nexception=" + e);
                decryptBytes = null;
                return decryptBytes;
            }
            return decryptBytes;
        }

        static public byte[] Decrypt(byte[] bytes)
        {
            if (!IsEncryption(bytes))
            {
                return bytes;
            }

            //------------------------------------------------------------
            //decrypting
            //------------------------------------------------------------

            var paramsTmp = GetEncryptParameters(bytes);
            float? jumpSpeed = null;
            long? secretCode = null;

            //jump length
            if (paramsTmp.Count > JUMP_SPEED_PARAMETER_INDEX && !string.IsNullOrEmpty(paramsTmp[JUMP_SPEED_PARAMETER_INDEX]))
            {
                jumpSpeed = float.Parse(paramsTmp[JUMP_SPEED_PARAMETER_INDEX]);
            }
            //encrypt or decrypt code
            if (paramsTmp.Count > SECRET_CODE_PARAMETER_INDEX && !string.IsNullOrEmpty(paramsTmp[SECRET_CODE_PARAMETER_INDEX]))
            {
                secretCode = long.Parse(paramsTmp[SECRET_CODE_PARAMETER_INDEX]);
            }

            int encryptionStringLength = 0;

            //Compatibility of old versions of decryption
            if (jumpSpeed == null && secretCode == null)
            {
                jumpSpeed = 14;
                secretCode = 36;
            }
            else
            {
                encryptionStringLength = GetEncryptStringLength(bytes);
            }

            int newBytesCount = bytes.Length - encryptionStringLength;
            if (newBytesCount <= 0)
            {
                EncryptDecrypt.PrintError("Decrypt error: invalid bytes count=" + newBytesCount);
                bytes = null;
                return bytes;
            }

            var decryptBytes = new byte[newBytesCount];
            try
            {
                //remove encryption flag
                System.Array.Copy(bytes, encryptionStringLength, decryptBytes, 0, newBytesCount);
            }
            catch (System.Exception e)
            {
                EncryptDecrypt.PrintError("Decrypt error: remove encryption flag \nexception=" + e);
                decryptBytes = null;
                return decryptBytes;
            }

            try
            {
                //set decryption bytes
                decryptBytes = EncryptOrDecrypt(decryptBytes, jumpSpeed, secretCode);
            }
            catch (System.Exception e)
            {
                EncryptDecrypt.PrintError("Decrypt error: set decryption bytes \nexception=" + e);
                decryptBytes = null;
                return decryptBytes;
            }
            return decryptBytes;
        }

        /// <summary>
        /// encrypt the and overwrite with Asynchronous
        /// </summary>
        /// <param name ="path">file path</param>
        /// <param name ="callbackEnd">completed callback function</param>
        /// <param name ="jumpSpeed">encryt and decrypt speed, range(0 ~ 1)</param>
        static public void EncryptAsyncPath(string path, System.Action callbackEnd, float jumpSpeed = 0.5f, long secretCode = 36, params object[] customParameters)
        {
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                EncryptPath(path, jumpSpeed, secretCode, customParameters);
            }, () =>
            {
                if (null != callbackEnd) 
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Base.Log.Info("EncryptDecrypt EncryptAsyncPath exception: e=" + e);
                    }
                }
            });
        }

        static public void EncryptAsync(byte[] bytes, System.Action callbackEnd, float jumpSpeed = 0.5f, long secretCode = 36, params object[] customParameters)
        {
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                Encrypt(bytes, jumpSpeed, secretCode, customParameters);
            }, () =>
            {
                if (null != callbackEnd) 
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Base.Log.Info("EncryptDecrypt EncryptAsync exception: e=" + e);
                    }
                }
            });
        }

        /// <summary>
        /// encrypt the and overwrite with Asynchronous
        /// </summary>
        /// <param name ="path">file path</param>
        /// <param name ="callbackEnd">completed callback function</param>
        static public void DecryptAsyncPath(string path, System.Action<byte[]> callbackEnd)
        {
            DecryptAsyncBase(() =>
            {
                return DecryptPath(path);
            }, callbackEnd);
        }

        static public void DecryptAsync(byte[] bytes, System.Action<byte[]> callbackEnd)
        {
            DecryptAsyncBase(() =>
            {
                return Decrypt(bytes);
            }, callbackEnd);
        }

        static public bool IsEncryptionPath(string path)
        {
            if (System.IO.File.Exists(path))
            {
                return IsEncryption(GetEncryptHeaderPath(path));
            }
            else
            {
                return false;
            }
        }

        static public bool IsEncryptionStream(System.IO.FileStream stream)
        {
            bool retValue = false;

            if (null == stream || stream.Length == 0)
            {
                return retValue;
            }

            if (stream.Length < EncryptDecrypt.FLAG_ENCRYPTION.Length)
            {
                // EncryptDecrypt.PrintError("IsEncryptiond error: There is not enough length to verify");
                return retValue;
            }

            retValue = BeginContainsStream(stream, EncryptDecrypt.FLAG_ENCRYPTION);
            return retValue;
        }

        static public bool IsEncryption(byte[] bytes)
        {
            bool retValue = false;

            if (null == bytes || bytes.Length == 0)
            {
                return retValue;
            }

            if (bytes.Length < EncryptDecrypt.FLAG_ENCRYPTION.Length)
            {
                // EncryptDecrypt.PrintError("IsEncryptiond error: There is not enough length to verify");
                return retValue;
            }

            retValue = BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION);
            return retValue;
        }

        /// <summary>
        /// 获取用户自定义字段，如果没有则返回null
        /// <param name="bytes">二进制数组</param>
        /// <return>自定义字段数组</return>
        /// </summary>
        static public string[] GetEncryptCustomParameters(byte[] bytes)
        {
            string[] retValue = null;
            if (null == bytes || bytes.Length == 0)
            {
                return retValue;
            }

            var parameters = GetEncryptParameters(bytes);
            if (null != parameters && parameters.Count > CUSTOM_PARAMETER_START_INDEX)
            {
                retValue = new string[parameters.Count - CUSTOM_PARAMETER_START_INDEX];
                for (int i = CUSTOM_PARAMETER_START_INDEX; i < parameters.Count; ++i)
                {
                    retValue[i - CUSTOM_PARAMETER_START_INDEX] = parameters[i];
                }
            }

            return retValue;
        }

        /// <summary>
        /// 获取用户自定义字段，如果没有则返回null
        /// <param name="path">文件绝对路径</param>
        /// <return>自定义字段数组</return>
        /// </summary>
        static public string[] GetEncryptCustomParametersPath(string path)
        {
            return GetEncryptCustomParameters(GetEncryptHeaderPath(path));
        }

        /// <summary>
        /// 获取文件加密跳转速度
        /// <param name="path">文件绝对路径</param>
        /// <return>加密跳转速度，如果获取失败返回0</return>
        /// </summary>
        static public float GetJumpSpeedPath(string path)
        {
            var parameters = GetEncryptParameters(GetEncryptHeaderPath(path));
            if (null != parameters && parameters.Count > 0 && parameters.Count > JUMP_SPEED_PARAMETER_INDEX)
            {
                return float.Parse(parameters[JUMP_SPEED_PARAMETER_INDEX]);
            }
            else
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// 获取文件加密码
        /// <param name="path">文件绝对路径</param>
        /// <return>加密码，如果获取失败返回0</return>
        /// </summary>
        static public long GetSecretCodePath(string path)
        {
            var parameters = GetEncryptParameters(GetEncryptHeaderPath(path));
            if (null != parameters && parameters.Count > 0 && parameters.Count > SECRET_CODE_PARAMETER_INDEX)
            {
                return long.Parse(parameters[SECRET_CODE_PARAMETER_INDEX]);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取加密开头内容
        /// <param name="path">文件绝对路径</param>
        /// <return>加密二进制文件</return>
        /// </summary>
        static private byte[] GetEncryptHeaderPath(string path)
        {
            var retValue = new List<byte>();
            System.IO.FileStream fileStream = null;

            try
            {
                if (!System.IO.File.Exists(path))
                {
                    // EncryptDecrypt.PrintError("GetEncryptHeaderPath error: not found path=" + path);
                    return retValue.ToArray();
                }

                fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                var readOffset = FLAG_ENCRYPTION.Length;
                int currentReadIndex = 0;
                
                var currentReadBuffer = new byte[readOffset];

                currentReadIndex = fileStream.Read(currentReadBuffer, 0, currentReadBuffer.Length);

                //判断是否为加密文件
                if (!BeginContains(currentReadBuffer, FLAG_ENCRYPTION))
                {
                    return retValue.ToArray();
                }

                retValue.AddRange(currentReadBuffer);

                while (true)
                {
                    currentReadIndex = fileStream.Read(currentReadBuffer, 0, currentReadBuffer.Length);

                    //没有可读文件，退出循环
                    if (currentReadIndex <= 0)
                        break;

                    //添加本次读取到的内容
                    retValue.AddRange(currentReadBuffer);

                    if (Contains(retValue, FLAG_ENCRYPTION_PARAM_END))
                    {
                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                EncryptDecrypt.PrintError("GetEncryptHeader error: " + e);
            }
            finally
            {
                if (null != fileStream)
                {
                    fileStream.Close();
                }
            }
            return retValue.ToArray();
        }

        static private void DecryptAsyncBase(System.Func<byte[]> callbackDecrypt, System.Action<byte[]> callbackEnd)
        {
            byte[] bytesDecrypt = null;
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                bytesDecrypt = callbackDecrypt();
            }, 
            ()=>
            {
                if (null != callbackEnd)
                    callbackEnd(bytesDecrypt);
            });
        }

        /// <summary>
        /// 拆分字符串
        /// <param name="str">字符串对象</param>
        /// <param name="pattern">拆分符号</param>
        /// <return>拆分后的字符串数组</return>
        /// </summary>
        static public string[] SplitEx(string str, string pattern)
        {
            if (pattern.Length == 1)
                return str.Split(pattern[0]);
            else
                return str.Split(new string[] { pattern }, System.StringSplitOptions.None);
        }

        /// <summary>
        /// 获取所有加密参数
        /// <return>加密参数</return>
        /// </summary>
        static public List<string> GetEncryptParametersStream(System.IO.FileStream stream)
        {
            var retValue = new List<string>();
            if (!IsEncryptionStream(stream))
            {
                EncryptDecrypt.PrintError("GetEncryptParametersStream error: not encrypted binary data, count=" + stream.Length);
                return retValue;
            }

            if (BeginContainsStream(stream, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN, EncryptDecrypt.FLAG_ENCRYPTION.Length))
            {
                var paramsString = new System.Text.StringBuilder();
                var offsetIndex = EncryptDecrypt.FLAG_ENCRYPTION.Length + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN.Length;

                for (int i = offsetIndex; i < stream.Length; ++i)
                {
                    if (BeginContainsStream(stream, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END, i))
                    {
                        if (paramsString.Length > 0)
                            retValue.Add(paramsString.ToString());
                        break;
                    }
                    else
                    {
                        stream.Seek(i, System.IO.SeekOrigin.Begin);
                        byte readByte = (byte)stream.ReadByte();
                        var currentChar = (char)(readByte ^ EncryptDecrypt.ENCRYPT_SECRET_CODE);
                        if (currentChar == PARAMETER_SPLIT_FLAG)
                        {
                            retValue.Add(paramsString.ToString());
                            paramsString.Length = 0;
                        }
                        else
                            paramsString.Append(currentChar);
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// 获取所有加密参数
        /// <param name="bytes">二进制数组</param>
        /// <return>加密参数</return>
        /// </summary>
        static public List<string> GetEncryptParameters(byte[] bytes)
        {
            var retValue = new List<string>();
            if (!IsEncryption(bytes))
            {
                EncryptDecrypt.PrintError("GetEncryptParameters error: not encrypted binary data, count=" + bytes.Length);
                return retValue;
            }

            if (BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN, EncryptDecrypt.FLAG_ENCRYPTION.Length))
            {
                var paramsString = new System.Text.StringBuilder();
                var offsetIndex = EncryptDecrypt.FLAG_ENCRYPTION.Length + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN.Length;

                for (int i = offsetIndex; i < bytes.Length; ++i)
                {
                    if (BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END, i))
                    {
                        if (paramsString.Length > 0)
                            retValue.Add(paramsString.ToString());
                        break;
                    }
                    else
                    {
                        var currentChar = (char)(bytes[i] ^ EncryptDecrypt.ENCRYPT_SECRET_CODE);
                        if (currentChar == PARAMETER_SPLIT_FLAG)
                        {
                            retValue.Add(paramsString.ToString());
                            paramsString.Length = 0;
                        }
                        else
                            paramsString.Append(currentChar);
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// 获取加密配置长度
        /// <return>加密配置长度</return>
        /// </summary>
        static private int GetEncryptStringLengthStream(System.IO.FileStream stream)
        {
            int retValue = 0;
            if (BeginContainsStream(stream, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN, EncryptDecrypt.FLAG_ENCRYPTION.Length))
            {
                var offsetIndex = EncryptDecrypt.FLAG_ENCRYPTION.Length + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN.Length;
                for (int i = offsetIndex; i < stream.Length; ++i)
                {
                    if (BeginContainsStream(stream, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END, i))
                    {
                        retValue = i + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END.Length;
                        break;
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// 获取加密配置长度
        /// <param name="bytes">二进制内容</param>
        /// <return>加密配置长度</return>
        /// </summary>
        static private int GetEncryptStringLength(byte[] bytes)
        {
            int retValue = 0;
            if (BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN, EncryptDecrypt.FLAG_ENCRYPTION.Length))
            {
                var offsetIndex = EncryptDecrypt.FLAG_ENCRYPTION.Length + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN.Length;
                for (int i = offsetIndex; i < bytes.Length; ++i)
                {
                    if (BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END, i))
                    {
                        retValue = i + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END.Length;
                        break;
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// 删除加密标记
        /// <param name="path">文件路径</param>
        /// <param name="parameters">加密参数</param>
        /// <return>被移除的加密参数数量，如果为0则没有加密参数被移除</return>
        /// </summary>
        static public void RemoveEntryptParametersPath(string path, params string[] parameters)
        {
            if (!System.IO.File.Exists(path))
            {
                EncryptDecrypt.PrintError("RemoveEntryptParameters erorr: not found path=" + path);
                return;
            }

            var readBytes = System.IO.File.ReadAllBytes(path);
            var newBytes = RemoveEntryptParameters(readBytes, parameters);

            //重新写入文件
            if (null != newBytes && newBytes.Length > 0)
            {
                // if (System.IO.File.Exists(path))
                //     System.IO.File.Delete(path);
                FileHelper.WriteAllByteByUserPath(path, newBytes);
            }
        }

        // static private List<T> ToListEx<T>(ICollection<T> list)
        // {
        //     var ret = new List<T>();
        //     foreach (var iter in list)
        //     {
        //         ret.Add(iter);
        //     }
        //     return ret;
        // }

        /// <summary>
        /// 删除加密标记
        /// <param name="bytes">加密二进制内容</param>
        /// <param name="parameters">加密参数</param>
        /// <return>被移除加密参数后的二进制内容，如果没有被移除参数则返回null</return>
        /// </summary>
        static public byte[] RemoveEntryptParameters(byte[] bytes, params string[] parameters)
        {
            byte[] retValue = null;

            if (!IsEncryption(bytes))
            {
                EncryptDecrypt.PrintError("RemoveEntryptParameters error: not encrypted binary data, count=" + bytes.Length);
                return retValue;
            }

            //获取原有加密参数
            var oldParameters = GetEncryptParameters(bytes);
            if (null == oldParameters || 0 == oldParameters.Count)
            {
                EncryptDecrypt.PrintError("RemoveEntryptParameters error: not found custom parameter in binary data, count=" + bytes.Length);
                return retValue;
            }

            //删除参数
            bool isRemovedParameter = false;
            var oldParametersList = oldParameters;
            for (int i = parameters.Length - 1; i >= 0; --i)
            {
                int findIndex = oldParametersList.IndexOf(parameters[i]);
                if (findIndex >= 0)
                {
                    isRemovedParameter = true;
                    oldParametersList.RemoveAt(findIndex);
                    if (0 == oldParametersList.Count)
                        break;
                }
            }

            //有参数被删除，获取新的加密二进制内容
            if (isRemovedParameter)
            {
                retValue = Decrypt(bytes);
                float jumpSpeed = 0;
                long secretCode = 0;
                var newParameters = oldParametersList.ToArray();
                GetDefaultEncryptParameters(newParameters, out jumpSpeed, out secretCode);
                retValue = Encrypt(bytes, jumpSpeed, secretCode, newParameters);
            }

            return retValue;
        }

        /// <summary>
        /// 获取加密默认参数
        /// <param name="parameters">加密参数</param>
        /// <param name="jumpSpeed">跳转速度</param>
        /// <param name="secretCode">加密码</param>
        /// <return></return>
        /// </summary>
        static private void GetDefaultEncryptParameters(string[] parameters, out float jumpSpeed, out long secretCode)
        {
            jumpSpeed = 14;
            secretCode = 36;

            if (parameters.Length > JUMP_SPEED_PARAMETER_INDEX && !string.IsNullOrEmpty(parameters[JUMP_SPEED_PARAMETER_INDEX]))
                jumpSpeed = float.Parse(parameters[JUMP_SPEED_PARAMETER_INDEX]);
            if (parameters.Length > SECRET_CODE_PARAMETER_INDEX && !string.IsNullOrEmpty(parameters[SECRET_CODE_PARAMETER_INDEX]))
                secretCode = long.Parse(parameters[SECRET_CODE_PARAMETER_INDEX]);
        }

        static private byte[] EncryptOrDecrypt(byte[] bytes, float? jumpSpeed, long? secretCode)
        {
            if (null == jumpSpeed || null == secretCode)
            {
                EncryptDecrypt.PrintError("EncryptOrDecrypt error: invalid param, jumpSpeed=" + jumpSpeed + " secretCode=" + secretCode);
                return bytes;
            }

            int addCount = 1;

            //Compatibility of old versions of decryption
            if (jumpSpeed == 14)
            {
                addCount = bytes.Length / (int)jumpSpeed;
            }
            else
            {
                //为1表示文件内容不做加密，保留加密信息头
                if (jumpSpeed == 1)
                {
                    return bytes;
                }
                else if (jumpSpeed > 1)
                {
                    addCount = (int)jumpSpeed;
                }
                else
                {
                    addCount = (int)(bytes.Length * jumpSpeed);
                }
            }

            if (addCount <= 0)
                addCount = 1;

            for (int i = 0; i < bytes.Length; i += addCount)
            {
                if (i >= bytes.Length)
                    break;

                bytes[i] = (byte)((char)bytes[i] ^ secretCode);
            }
            return bytes;
        }

        static private string GetEncryptString(float? jumpSpeed, long? secretCode, object[] customParameters)
        {
            var encryptionString = new System.Text.StringBuilder();
            encryptionString.Append(EncryptDecrypt.FLAG_ENCRYPTION + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN);

            //add custom encrypt parameters 
            if (null != jumpSpeed)
            {
                encryptionString.Append(jumpSpeed.ToString());
                encryptionString.Append(PARAMETER_SPLIT_FLAG);
            }
            if (null != secretCode)
            {
                encryptionString.Append(secretCode.ToString());
                encryptionString.Append(PARAMETER_SPLIT_FLAG);
            }
            if (null != customParameters && customParameters.Length > 0)
            {
                for (int i = 0; i < customParameters.Length; ++i)
                {
                    if (null != customParameters[i] && customParameters[i].ToString().Length > 0)
                    {
                        encryptionString.Append(customParameters[i].ToString());
                        encryptionString.Append(PARAMETER_SPLIT_FLAG);
                    }
                }
            }

            //删除最后一个分隔符
            if (encryptionString.Length > 0)
            {
                encryptionString.Remove(encryptionString.Length - 1, 1);
            }

            encryptionString.Append(EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END);
            var convertBytesTmp = System.Text.Encoding.UTF8.GetBytes(encryptionString.ToString());
            var convertStringTmp = System.Text.Encoding.UTF8.GetString(EncryptOrDecrypt(convertBytesTmp, 0, EncryptDecrypt.ENCRYPT_SECRET_CODE));
            return convertStringTmp;
        }

        static private bool BeginContainsStream(System.IO.FileStream stream, string find, int startIndex = 0)
        {
            stream.Seek(startIndex, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < find.Length; ++i)
            {
                if (i + startIndex < stream.Length)
                {
                    var char1 = (char)stream.ReadByte();
                    var char2 = (char)find[i];
                    var char3 = (char)(char2 ^ EncryptDecrypt.ENCRYPT_SECRET_CODE);
                    if (char1 != char2 && char1 != char3)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        static private bool BeginContains(byte[] bytes, string find, int startIndex = 0)
        {
            for (int i = 0; i < find.Length; ++i)
            {
                if (i + startIndex < bytes.Length)
                {
                    var char1 = (char)bytes[i + startIndex];
                    var char2 = (char)find[i];
                    var char3 = (char)(char2 ^ EncryptDecrypt.ENCRYPT_SECRET_CODE);
                    if (char1 != char2 && char1 != char3)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        static private bool Contains(List<byte> bytes, string find, int startIndex = 0)
        {
            var convertString = new System.Text.StringBuilder();
            for (int i = startIndex; i < bytes.Count; ++i)
            {
                convertString.Append((char)(bytes[i] ^ EncryptDecrypt.ENCRYPT_SECRET_CODE));
            }

            //虽然这么写可以做到0GC但是速度比字符串比对慢太多了
            //以后有时间优化下算法或许考虑重新使用该方法吧
            // bool isContains = true;
            // char charTmp;
            // int indexOffset = 0;
            // int findLength = find.Length - 1;
            // for (int i = convertString.Length - 1; i >= 0; --i)
            // {
            //     isContains = true;
            //     for (int j = findLength; j >= 0; --j)
            //     {
            //         indexOffset = i + j;
            //         if (indexOffset > convertString.Length - 1)
            //         {
            //             isContains = false;
            //             break;
            //         }
            //         charTmp = convertString[indexOffset];
            //         if (find[j] != charTmp)
            //         {
            //             isContains = false;
            //             break;
            //         }
            //     }

            //     if (isContains)
            //         break;
            // }

            return convertString.ToString().Contains(find);
        }

        static private void PrintError(string message)
        {
            Log.Error(message);
        }
    }
}