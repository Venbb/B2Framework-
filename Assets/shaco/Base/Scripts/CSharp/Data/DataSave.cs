using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class DataSave : shaco.Base.IDataSave
    {
        override public string SPLIT_FLAG { get { return "#,"; } }

        /// <summary>
        /// 文件存读路径
        /// </summary>
        override public string savePath
        {
            get
            {
                if (string.IsNullOrEmpty(this._savePath))
                {
                    this._savePath = FileDefine.persistentDataPath + FileDefine.PATH_FLAG_SPLIT + "DataSave2_1." + GlobalParams.DATA_SAVE_EXTENSIONS;
                }
                return _savePath;
            }
            set { _savePath = value; }
        }

        static private readonly string FORMAT_BEGIN = "#[";
        static private readonly string FORMAT_END = "#]";
        static private readonly string SPLIT_DICTIONARY_FLAG = "#:";

        private bool autoEncrypt = true;
        private Dictionary<string, string> _mapDatas = new Dictionary<string, string>();

        private System.Threading.Mutex _mutex = new System.Threading.Mutex();
        private string _savePath = string.Empty;

        //是否正在保存数据到文件，用于降低同一帧同时保存多个文件导致的性能瓶颈
        private bool _isSavingToFile = false;

        //有数据发生变化需要保存
        private bool _isDataChangedDirty = false;

        //是否初始化过了
        private bool _isInited = false;

        //Write functions ------ basic type
        override public void WriteChar(string key, char value) { WriteString(key, value.ToString(), false); }
        override public void WriteBool(string key, bool value) { WriteString(key, value.ToString(), false); }
        override public void WriteShort(string key, short value) { WriteString(key, value.ToString(), false); }
        override public void WriteInt(string key, int value) { WriteString(key, value.ToString(), false); }
        override public void WriteFloat(string key, float value) { WriteString(key, value.ToString(), false); }
        override public void WriteDouble(string key, double value) { WriteString(key, value.ToString(), false); }
        override public void WriteLong(string key, long value) { WriteString(key, value.ToString(), false); }
        override public void WriteString(string key, string value) { WriteString(key, value.ToString(), true); }
        override public void WriteEnum<T>(string key, T value)
        {
            WriteString(key, value.ToString());
        }

        override public void WriteArray<T>(string key, T[] args)
        {
            WriteArrayCustom<T>(key, args, (v) => v.ToString());
        }

        override public void WriteList<T>(string key, List<T> args)
        {
            WriteListCustom<T>(key, args, (v) => v.ToString());
        }

        override public void WriteList(string key, List<object> args)
        {
            WriteList<object>(key, args);
        }

        override public void WriteArrayCustom<T>(string key, T[] args, System.Func<T, string> convert)
        {
            int writeLength = args.Length;
            var writeObjs = new object[writeLength];
            for (int i = 0; i < writeLength; ++i)
            {
                writeObjs[i] = convert(args[i]);
            }
            WriteArguments(key, writeObjs);
        }

        override public void WriteListCustom<T>(string key, List<T> args, System.Func<T, string> convert)
        {
            int writeLength = args.Count;
            var writeObjs = new object[writeLength];
            for (int i = 0; i < writeLength; ++i)
            {
                writeObjs[i] = convert(args[i]);
            }
            WriteArguments(key, writeObjs);
        }

        override public void WriteDictionary<KEY, VALUE>(string key, Dictionary<KEY, VALUE> args)
        {
            var writeObjs = new object[args.Count];
            int index = 0;
            foreach (var iter in args)
            {
                writeObjs[index++] = iter.Key + SPLIT_DICTIONARY_FLAG + iter.Value;
            }
            WriteArguments(key, writeObjs);
        }

        //Read functions ------ basic type
        override public char ReadChar(string key, char defaultValue = ' ') { return char.Parse(ReadString(key, defaultValue.ToString())); }
        override public bool ReadBool(string key, bool defaultValue = false) { return bool.Parse(ReadString(key, defaultValue.ToString())); }
        override public int ReadInt(string key, int defaultValue = 0) { return int.Parse(ReadString(key, defaultValue.ToString())); }
        override public float ReadFloat(string key, float defaultValue = 0) { return float.Parse(ReadString(key, defaultValue.ToString())); }
        override public double ReadDouble(string key, double defaultValue = 0) { return double.Parse(ReadString(key, defaultValue.ToString())); }
        override public long ReadLong(string key, long defaultValue = 0) { return long.Parse(ReadString(key, defaultValue.ToString())); }
        override public T ReadEnum<T>(string key, T defaultValue = default(T)) { return shaco.Base.Utility.ToEnum<T>(ReadString(key, defaultValue.ToString())); }
        override public T[] ReadArray<T>(string key, System.Func<string, T> convert, T[] defaultValue = null)
        {
            var readString = ReadString(key);
            if (string.IsNullOrEmpty(readString))
                return defaultValue;
            var readStrings = readString.Split(SPLIT_FLAG);

            var retValue = new T[readStrings.Length];
            for (int i = 0; i < readStrings.Length; ++i)
            {
                retValue[i] = convert(readStrings[i]);
            }
            return retValue;
        }

        override public List<T> ReadList<T>(string key, System.Func<string, T> convert, List<T> defaultValue = null)
        {
            var readString = ReadString(key);
            if (string.IsNullOrEmpty(readString))
                return defaultValue;

            var readStrings = readString.Split(SPLIT_FLAG);
            var retValue = new List<T>();

            for (int i = 0; i < readStrings.Length; ++i)
            {
                retValue.Add(convert(readStrings[i]));
            }
            return retValue;
        }
        
        override public List<object> ReadList(string key, System.Func<string, object> convert, List<object> defaultValue = null)
        {
            return ReadList<object>(key, convert, defaultValue);
        }

        override public string[] ReadArrayString(string key, string[] defaultValue = null)
        {
            var readString = ReadString(key);
            if (string.IsNullOrEmpty(readString))
                return defaultValue;
            else
                return readString.Split(SPLIT_FLAG);
        }

        override public List<string> ReadListString(string key, List<string> defaultValue = null)
        {
            var readString = ReadString(key);
            if (string.IsNullOrEmpty(readString))
                return defaultValue;
            else
                return readString.Split(SPLIT_FLAG).ToList();
        }

        override public Dictionary<KEY, VALUE> ReadDictionary<KEY, VALUE>(string key, System.Func<string, KEY> convertKey, System.Func<string, VALUE> convertValue, Dictionary<KEY, VALUE> defaultValue = null)
        {
            var readString = ReadString(key);
            if (string.IsNullOrEmpty(readString))
                return defaultValue;
            else
            {
                var readStrings = readString.Split(SPLIT_FLAG);
                var retValue = new Dictionary<KEY, VALUE>();
                for (int i = 0; i < readStrings.Length; ++i)
                {
                    var splitStrTmp = readStrings[i].Split(SPLIT_DICTIONARY_FLAG);
                    if (splitStrTmp.Length != 2)
                    {
                        Log.Error("DataSave ReadDictionary error: not paired element: index=" + i + " sub split string=" + readStrings[i] + "\nread string=" + readString);
                        return defaultValue;
                    }

                    var keyString = splitStrTmp[0];
                    var valueString = splitStrTmp[1];
                    retValue.Add(convertKey(keyString), convertValue(valueString));
                }
                return retValue;
            }
        }

        override public string ReadString(string key, string defaultValue = shaco.Base.GlobalParams.EmptyString)
        {
            this.CheckInit();
            if (!_mapDatas.ContainsKey(key))
                return defaultValue;
            else
                return _mapDatas[key];
        }

        override public T ReadUserType<T>(string key, System.Func<string[], T> callbackConvert, T defaultValue)
        {
            var strTmp = ReadString(key);
            if (string.IsNullOrEmpty(strTmp))
            {
                return defaultValue;
            }
            else
            {
                var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, SPLIT_FLAG);
                return (T)callbackConvert(splitTmp);
            }
        }

        override public void Remove(string key)
        {
            if (ContainsKey(key))
            {
                _mapDatas.Remove(key);
                this.CheckSaveMapData(this.savePath);
            }
            else
            {
                Log.Error("DataSave Remove error: not found key=" + key);
            }
        }

        override public void RemoveStartWith(string keyPrefix)
        {
            var listRemoveKeys = new List<string>();
            foreach (var iter in _mapDatas)
            {
                if (iter.Key.Contains(keyPrefix))
                {
                    listRemoveKeys.Add(iter.Key);
                }
            }

            for (int i = listRemoveKeys.Count - 1; i >= 0; --i)
            {
                _mapDatas.Remove(listRemoveKeys[i]);
            }

            if (listRemoveKeys.Count > 0)
            {
                listRemoveKeys.Clear();
                this.CheckSaveMapData(this.savePath);
            }
        }

        override public bool ContainsKey(string key)
        {
            CheckInit();
            return _mapDatas.ContainsKey(key);
        }

        //other functions 
        override public void Clear()
        {
            CheckInit();

            Log.Info("DataSave Clear all datas and rewrite path=" + this.savePath);
            _mapDatas.Clear();
            SaveMapData(this.savePath);
            this.savePath = string.Empty;
        }

        override public string GetFormatString()
        {
            string strPrint = string.Empty;
            foreach (var key in _mapDatas.Keys)
            {
                var value = _mapDatas[key];

                strPrint += "key:[" + key + "]" + "  value:[" + value + "]" + "\n";
            }

            if (strPrint.Length > 0)
            {
                strPrint = strPrint.Remove(strPrint.Length - 1, 1);
            }
            return strPrint;
        }

        /// <summary>
        /// 保存数据到指定路径中
        /// <param name="path">保存文件路径</param>
        /// </summary>
        override public void SaveAsFile(string path)
        {
            this.SaveMapData(path);
        }

        /// <summary>
        /// realoda all save config from path
        /// </summary>
        /// <param name="path">if path is null or empty, will auto set it as default path</param>
        /// <param name="autoEncrypt">if true, will encrypt all config data</param>
        override public void ReloadFromFile(string path = null, bool autoEncrypt = true)
        {
            CheckSaveModifyData();

            this.autoEncrypt = autoEncrypt;
            this.savePath = path;
            _mapDatas.Clear();
            this.InitMapDataWithFile();
        }

        override public void ReloadFromBytes(byte[] bytes, bool autoEncrypt = true)
        {
            CheckSaveModifyData();

            this.autoEncrypt = autoEncrypt;
            _mapDatas.Clear();
            this.InitMapDataWithBytes(bytes);
        }

        override public void WriteArguments(string key, params object[] args)
        {
            string strWrite = GetStringBySplitFlag(args);
            WriteString(key, strWrite, false);
        }

        override public void CheckSaveModifyData()
        {
            if (!_isDataChangedDirty || string.IsNullOrEmpty(this.savePath))
                return;
            SaveMapData(this.savePath);
        }

        private string GetStringBySplitFlag(params object[] args)
        {
            if (args.Length == 0)
            {
                return string.Empty;
            }
            var retValue = new System.Text.StringBuilder();
            int lenTmp = args.Length - 1;
            for (int i = 0; i < lenTmp; ++i)
            {
                retValue.Append(args[i]);
                retValue.Append(SPLIT_FLAG);
            }

            retValue.Append(args[lenTmp]);
            return retValue.ToString();
        }

        private void Format(string source, System.Text.StringBuilder builder)
        {
            if (source.Contains(FORMAT_BEGIN))
                source = source.Replace(FORMAT_BEGIN, "{");
            if (source.Contains(FORMAT_END))
                source = source.Replace(FORMAT_END, "}");

            builder.Append(FORMAT_BEGIN);
            builder.Append(source);
            builder.Append(FORMAT_END);
        }

        private List<string> GetSourceByLine(string readString)
        {
            List<string> ret = new List<string>();
            int startIndex = 0;

            while (ret.Count < 2)
            {
                var strSub = readString.Substring(FORMAT_BEGIN, FORMAT_END, startIndex);
                ret.Add(strSub);
                startIndex += strSub.Length + FORMAT_BEGIN.Length + FORMAT_END.Length;

                if (startIndex >= readString.Length)
                {
                    break;
                }
            }
            return ret;
        }

        private void CheckInit()
        {
            if (string.IsNullOrEmpty(FileDefine.persistentDataPath))
            {
                Log.Error("FileDefine.persistentDataPath is empty");
            }

            InitMapDataWithFile();
        }

        private void InitMapDataWithFile()
        {
            if (_isInited)
                return;

            lock (_mutex)
            {
                var path = this.savePath;
                if (!FileHelper.ExistsFile(path))
                    return;

                var readBytes = FileHelper.ReadAllByteByUserPath(path);
                InitMapDataWithBytes(readBytes);
            }
        }

        private void InitMapDataWithBytes(byte[] bytes)
        {
            if (_isInited)
                return;

            bool isForceSaveWhenDuplicateKey = false;
            _isInited = true;
            lock (_mutex)
            {
                _isSavingToFile = true;
                var readBytes = bytes;

                if (null == readBytes || readBytes.Length == 0)
                {
                    _isSavingToFile = false;
                    return;
                }

                var decryptBytes = EncryptDecrypt.Decrypt(readBytes);
                var decryptStream = new System.IO.MemoryStream(decryptBytes);
                var readerStream = new System.IO.StreamReader(decryptStream);
                var splitFlag = FORMAT_END + FORMAT_BEGIN;
                var readIndex = 0;

                _mapDatas.Clear();
                while (true)
                {
                    var readLineString = readerStream.ReadLine();
                    if (string.IsNullOrEmpty(readLineString))
                        break;

                    var listStr = GetSourceByLine(readLineString);
                    if (listStr.Count < 2)
                    {
                        Log.Error("Data Save initMapData error: file length=" + listStr.Count + " readLineString=" + readLineString + " index=" + readIndex);
                        break;
                    }

                    //key 
                    var strKey = listStr[0];

                    //value
                    var strValue = listStr[1];

                    if (_mapDatas.ContainsKey(strKey))
                    {
                        Log.Error("DataSave InitMapData error: has duplicate key=" + strKey + " value=" + strValue + " readLineString=" + readLineString + " index=" + readIndex);
                        isForceSaveWhenDuplicateKey = true;
                    }
                    else
                    {
                        _mapDatas.Add(strKey, strValue);
                    }

                    ++readIndex;
                }
                _isSavingToFile = false;
            }

            if (isForceSaveWhenDuplicateKey)
            {
                SaveMapData(this.savePath);
            }
        }

        private void CheckMapDataModify(string key, string value)
        {
            lock (_mutex)
            {
                bool isChanged = true;
                CheckInit();

                if (_mapDatas.ContainsKey(key))
                {
                    if (_mapDatas[key] != value)
                    {
                        _mapDatas[key] = value;
                    }
                    else
                    {
                        isChanged = false;
                    }
                }
                else
                {
                    _mapDatas.Add(key, value);
                }

                //reWrite all file
                if (isChanged || !FileHelper.ExistsFile(this.savePath))
                {
                    _isDataChangedDirty = true;
                    CheckSaveMapData(this.savePath);
                }
            }
        }

        private void CheckSaveMapData(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            lock (_mutex)
            {
                //正在保存数据中
                if (_isSavingToFile)
                    return;

                _isSavingToFile = true;
                shaco.Base.WaitFor.Run(() =>
                {
                    return true;
                }, () =>
                {
                    SaveMapData(path);
                });
            }
        }

        private void SaveMapData(string path)
        {
            var writeContent = new System.Text.StringBuilder();
            foreach (var key in _mapDatas.Keys)
            {
                var key2 = key;
                var value = _mapDatas[key2];

                Format(key2, writeContent);
                Format(value, writeContent);
                writeContent.Append('\n');
            }

            if (writeContent.Length > 0)
            {
                writeContent = writeContent.Remove(writeContent.Length - 1, 1);
            }
            var writeStringTmp = writeContent.ToString();

            var folderPath = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            if (autoEncrypt)
            {
                var writeBytes = EncryptDecrypt.Encrypt(writeStringTmp.ToByteArray(), 3);
                FileHelper.WriteAllByteByUserPathWithoutLog(path, writeBytes);
            }
            else
            {
                FileHelper.WriteAllByUserPathWithoutLog(path, writeStringTmp);
            }

            _isSavingToFile = false;
            _isDataChangedDirty = false;
        }

        private void WriteString(string key, string value, bool checkValueValid)
        {
            if (checkValueValid)
            {
                // if (value.Contains(SPLIT_FLAG))
                // {
                //     Log.Exception("DataSave Write string error: value can't contain flag " + SPLIT_FLAG + " \nkey=" + key + " value=" + value);
                // }
            }
            this.CheckMapDataModify(key, value);
        }
    }
}
