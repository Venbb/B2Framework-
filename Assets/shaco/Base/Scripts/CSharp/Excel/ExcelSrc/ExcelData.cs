using System.Collections.Generic;
using System.IO;
using shaco.ExcelDataReader;

namespace shaco.Base
{
    public sealed partial class ExcelData
    {
        /// <summary>
        /// 子表数据集合类型
        /// </summary>
        public enum TabelDataType
        {
            List,
            Dictionary,
            DuplicateKeyDictionary,
        }

        /// <summary>
        /// 运行时刻数据加载类型
        /// </summary>
        public enum RuntimeLoadFileType
        {
            CSV,
            UnityAsset
        }

        /// <summary>
        /// 子表数据
        /// </summary>
        public struct TabelRowData
        {
            //是否忽略掉数据
            public bool isIgnoreData;
            //每行数据
            public string[] values;
        }

        public class TableInfo
        {
            //需要过滤数据的列
            public List<int> isIgnoreColDatasIndex = new List<int>();

            //数据类型，如果没有设置默认为List
            public TabelDataType dataType = TabelDataType.List;

            //子表名字
            public string tabelName = string.Empty;

            //表列数
            public int columnCount = 0;

            //子表横排数据
            public List<TabelRowData> rowDatas = new List<TabelRowData>();
        }

        /// <summary>
        /// 打开excel文件数据时候传入的路径
        /// </summary>
        public string excelPath { get; private set; }

        /// <summary>
        /// excel子表数量
        /// </summary>
        public int Count { get { return _dataList.Count; } }

        /// <summary>
        /// excel表中每行每列数据<行<列>>
        /// </summary>
        public IReadOnlyCollection<TableInfo> dataList { get { return _dataList.AsReadOnly(); } }
        private List<TableInfo> _dataList = null;

        /// <summary>
        /// 当前类型模板
        /// </summary>
        private StringTypePair[] _currentStringTypePair;

        /// <summary>
        /// 初始化excel表字典
        /// <param name="pathOrStringData">excel文件路径或者csv文本内容</param>
        /// <return>excel读取结果</return>
        /// </summary>
        public ExcelDefine.ExcelInitResult Init(string pathOrStringData)
        {
            var retValue = ExcelDefine.ExcelInitResult.Suceess;
            if (string.IsNullOrEmpty(pathOrStringData))
            {
                Log.Error("ExcelData Init error: path or string data is empty");
                retValue = ExcelDefine.ExcelInitResult.PathEmpty;
                return retValue;
            }

            ResetDataList();

            if (FileHelper.ExistsFile(pathOrStringData))
            {
                excelPath = pathOrStringData;
                string extensionsTmp = FileHelper.GetFilNameExtension(pathOrStringData);
                if (extensionsTmp == "xlsx")
                {
                    retValue = InitWithXlsxOrXls(pathOrStringData, false);
                }
                else if (extensionsTmp == "xls")
                {
                    retValue = InitWithXlsxOrXls(pathOrStringData, true);
                }
                else if (FileHelper.ExistsFile(pathOrStringData))
                {
                    retValue = InitWithCsv(pathOrStringData);
                }
                else
                {
                    Log.Error("ExcelData Init error: not suppot excel data, path=" + pathOrStringData);
                    retValue = ExcelDefine.ExcelInitResult.NotSupportFileFormat;
                }
            }
            else
            {
                retValue = InitWithCsvBinary(pathOrStringData.ToByteArray());
            }
            return retValue;
        }

        /// <summary>
        /// 通过二进制数据初始化表
        /// <param name="binary">csv格式的二进制数据</param>
        /// <param name="pathTag">读取路径，主要用于报错时候的日志提示，可以填空</param>
        /// <return>初始化结果</return>
        /// </summary>
        public ExcelDefine.ExcelInitResult InitWithBinary(byte[] binary, string pathTag)
        {
            excelPath = pathTag;
            return InitWithCsvBinary(binary);
        }

        /// <summary>
        /// 通过行数据初始化表
        /// <param name="datas">行数据</param>
        /// <param name="pathTag">读取路径，主要用于报错时候的日志提示，可以填空</param>
        /// <return>初始化结果</return>
        /// </summary>
        public ExcelDefine.ExcelInitResult InitWithRowDatas(List<ExcelDefine.RowData> datas, string pathTag = GlobalParams.EmptyString)
        {
            excelPath = pathTag;
            ExcelDefine.ExcelInitResult retValue = ExcelDefine.ExcelInitResult.Suceess;
            if (datas.IsNullOrEmpty())
            {
                Log.Error("ExcelData InitWithCsvBinary error: data is empty, path=" + excelPath);
                retValue = ExcelDefine.ExcelInitResult.EmptyData;
                return retValue;
            }

            //表仅支持1个tabel读取，以后有时间再考虑优化
            ResetDataList();
            _dataList.Add(new TableInfo());

            //以第一行数据长度为准
            int dataCountInRow = datas[0].r.Length;

            if (null == _currentStringTypePair)
                _currentStringTypePair = GameHelper.dateTypesTemplate.GetTypesTemplate();

            //添加数据到表
            for (int i = 0; i < datas.Count; ++i)
            {
                AddLineData(datas[i].r, _dataList[0], dataCountInRow);
            }
            return retValue;
        }

        /// <summary>
        /// 获取excel的数据
        /// <param name="rowIndex">第几行下标</param>
        /// <param name="colIndex">第几列下标</param>
        /// <param name="tabelIndex">第几张表下标</param>
        /// </summary>
        public string GetData(int rowIndex, int colIndex, int tabelIndex = 0)
        {
            string retValue = null;
            if (tabelIndex < 0 || tabelIndex >= _dataList.Count)
            {
                Log.Exception("ExcelData GetData tabelIndex out of range, tabelIndex=" + tabelIndex + " listCount=" + _dataList.Count);
                return retValue;
            }
            var tableInfo = _dataList[tabelIndex];
            
            if (rowIndex < 0 || rowIndex >= tableInfo.rowDatas.Count)
            {
                Log.Exception("ExcelData GetData rowIndex out of range, tabelIndex=" + tabelIndex + " rowIndex=" + rowIndex + " rowCount=" + tableInfo.rowDatas.Count);
                return retValue;
            }
            var rowData = tableInfo.rowDatas[rowIndex];

            if (colIndex < 0 || colIndex >= rowData.values.Length)
            {
                Log.Exception("ExcelData GetData colIndex out of range, tabelIndex=" + tabelIndex + " rowIndex=" + rowIndex + " colIndex=" + colIndex + " valueCount=" + rowData.values.Length);
                return retValue;
            }
            retValue = rowData.values[colIndex];
            return retValue;
        }

        /// <summary>
        /// 获取一行excel的数据
        /// <param name="rowIndex">第几行下标</param>
        /// <param name="tabelIndex">第几张表下标</param>
        /// </summary>
        public string GetRowData(int rowIndex, int tabelIndex = 0)
        {
            string retValue = null;
            if (tabelIndex < 0 || tabelIndex >= _dataList.Count)
            {
                Log.Exception("ExcelData GetData tabelIndex out of range, tabelIndex=" + tabelIndex + " listCount=" + _dataList.Count);
                return retValue;
            }
            var tableInfo = _dataList[tabelIndex];

            if (rowIndex < 0 || rowIndex >= tableInfo.rowDatas.Count)
            {
                Log.Exception("ExcelData GetData rowIndex out of range, tabelIndex=" + tabelIndex + " rowIndex=" + rowIndex + " rowCount=" + tableInfo.rowDatas.Count);
                return retValue;
            }
            var rowData = tableInfo.rowDatas[rowIndex];
            var strAppend = new System.Text.StringBuilder();
            for (int i = 0; i < rowData.values.Length; ++i)
            {
                strAppend.Append(rowData.values[i]);
                strAppend.Append("\t");
            }
            retValue = strAppend.ToString();
            return retValue;
        }

        /// <summary>
        /// 该行excel数据是为注释忽略的
        /// <param name="rowIndex">第几行</param>
        /// <param name="tabelIndex">第几张表下标</param>
        /// <return>该行excel数据为注释过滤数据</return>
        /// </summary>
        public bool IsIgnoreRowData(int rowIndex, int tabelIndex = 0)
        {
            bool retValue = true;

            if (_dataList.IsOutOfRange(tabelIndex))
            {
                Log.Error("ExcelData IsIgnoreRowData rowIndex error: tabelIndex=" + tabelIndex);
                return retValue;
            }

            var tableInfo = _dataList[tabelIndex];
            if (tableInfo.rowDatas.IsOutOfRange(rowIndex))
            {
                Log.Error("ExcelData IsIgnoreRowData rowIndex error: tabelIndex=" + tabelIndex + " rowIndex=" + rowIndex);
                return retValue;
            }

            retValue = tableInfo.rowDatas[rowIndex].isIgnoreData;
            return retValue;
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            _dataList.Clear();
            _currentStringTypePair = null;
        }

        /// <summary>
        /// 将excel以txt制表符的文本形式保存，以方便快速读取和修改
        /// *注意*：仅能导出第一张工作表(tabel)的内容，如果一个excel存在多张工作表，请手动拆分开来使用
        /// <param name="savePath">保存路径</param>
        /// <return>保存的子表数据路径</return>
        /// </summary>
        public string[] SaveAsTxt(string savePath)
        {
            string[] retValue = null;
            if (_dataList.IsNullOrEmpty())
            {
                Log.Error("ExcelData SaveAsTxt error: not init save path=" + savePath);
                return retValue;
            }

            retValue = new string[_dataList.Count];
            var excelFileName = shaco.Base.FileHelper.GetLastFileName(savePath);

            if (_dataList.Count == 1)
            {
                SaveTabelAsTxt(excelFileName, savePath, _dataList[0]);
                retValue[0] = savePath;
            }
            else
            {
                for (int i = 0; i < _dataList.Count; ++i)
                {
                    var saveTabelPath = GetTabelSavePath(savePath, _dataList[i]);
                    SaveTabelAsTxt(excelFileName, saveTabelPath, _dataList[i]);
                    retValue[i] = saveTabelPath;
                }
            }
            return retValue;
        }

        /// <summary>
        /// 获取tabel表的csv格式字符串
        /// <param name="tableInfo">子表数据</param>
        /// <return>不重复的子表路径</return>
        /// </summary>
        public string GetCSVString(TableInfo tableInfo)
        {
            var writeString = new System.Text.StringBuilder();
            for (int i = 0; i < tableInfo.rowDatas.Count; ++i)
            {
                int ColCount = tableInfo.rowDatas[i].values.Length;
                for (int j = 0; j < ColCount; ++j)
                {
                    string valueTmp = tableInfo.rowDatas[i].values[j].ToString();
                    writeString.Append(valueTmp);

                    if (j < ColCount - 1)
                    {
                        writeString.Append('\t');
                    }
                }
                writeString.Append('\n');
            }

            if (writeString.Length > 0)
            {
                writeString.Remove(writeString.Length - 1, 1);
            }
            return writeString.ToString();
        }

        /// <summary>
        /// 获取子表数据数量
        /// <param name="tabelIndex">子表下标</param>
        /// <return>子表数据数量</return>
        /// </summary>
        public int GetTabelDataCount(int tabelIndex)
        {
            int retValue = 0;
            if (_dataList.IsOutOfRange(tabelIndex))
            {
                Log.Error("ExcelData GetTabelDataCount error: tabelIndex=" + tabelIndex);
                return retValue;
            }
            else
            {
                retValue = _dataList[tabelIndex].rowDatas.Count;
                return retValue;
            }
        }

        /// <summary>
        /// 获取子表数据行数
        /// <param name="tabelIndex">子表下标</param>
        /// <return>行数</return>
        /// </summary>
        public int GetTabelDataRowCount(int tabelIndex = 0)
        {
            int retValue = 0;
            if (_dataList.IsOutOfRange(tabelIndex))
            {
                Log.Error("ExcelData GetTabelDataRowCount error: tabelIndex=" + tabelIndex);
                return retValue;
            }

            retValue = _dataList[tabelIndex].rowDatas.Count;
            return retValue;
        }

        /// <summary>
        /// 获取子表数据列数
        /// <param name="tabelIndex">子表下标</param>
        /// <return>列数</return>
        /// </summary>
        public int GetTabelDataColCount(int tabelIndex = 0)
        {
            int retValue = 0;
            if (_dataList.IsOutOfRange(tabelIndex))
            {
                Log.Error("ExcelData GetTabelDataColCount error: tabelIndex=" + tabelIndex);
                return retValue;
            }

            if (_dataList[tabelIndex].rowDatas.IsNullOrEmpty())
            {
                Log.Error("ExcelData GetTabelDataColCount error: data is empty tabelIndex=" + tabelIndex);
                return retValue;
            }

            retValue = _dataList[tabelIndex].rowDatas[0].values.Length;
            return retValue;
        }

        /// <summary>
        /// 通过字符串类型获取类型配对信息，不区分大小写
        /// <param name="str">数据字符串</param>
        /// <param name="typePairs">类型匹配模板</param>
        /// <return>数据类型配对信息</return>
        /// </summary>
        static public StringTypePair TypeStringToTypePair(string typeString, StringTypePair[] typePairs)
        {
            StringTypePair retValue = null;
            if (typePairs.IsNullOrEmpty())
                return retValue;

            if (typeString.StartsWith(ExcelDefine.IGNORE_FLAG))
                return retValue;

            var strLower = typeString.ToLower();
            var tmpstr = strLower.Split("_");
            if (tmpstr.Length < 1)
                return retValue;

            for (int i = typePairs.Length - 1; i >= 0; --i)
            {
                if (tmpstr[0].Equals(typePairs[i].customTypeString))
                {
                    retValue = typePairs[i];
                    break;
                }
            }
            return retValue;
        }

        /// <summary>
        /// 获取tabel表的保存路径
        /// <param name="savePath">原保存路径</param>
        /// <param name="tableInfo">子表数据</param>
        /// <return>不重复的子表路径</return>
        /// </summary>
        static public string GetTabelSavePath(string savePath, TableInfo tableInfo)
        {
            var retValue = string.Empty;

            if (FileHelper.HasFileNameExtension(savePath))
            {
                retValue = FileHelper.AddFileNameTag(savePath, "_" + tableInfo.tabelName);
            }
            else
            {
                retValue = savePath + "_" + tableInfo.tabelName;
            }
            return retValue;
        }

        /// <summary>
        /// 通过.xlsx或者.xls方式初始化excel数据
        /// <param name="path">路径</param>
        /// <return>excel读取结果</return>
        /// </summary>
        private ExcelDefine.ExcelInitResult InitWithXlsxOrXls(string path, bool isXls)
        {
            ExcelDefine.ExcelInitResult retValue = ExcelDefine.ExcelInitResult.Suceess;
            FileStream stream = null;
            try
            {
                stream = File.Open(path, FileMode.Open, FileAccess.Read);
                var readTables = new List<TableInfo>();

                IExcelDataReader excelReader = null;
                if (isXls)
                {
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else
                {
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                do
                {
                    var newTableTmp = new TableInfo();
                    readTables.Add(newTableTmp);
                    newTableTmp.tabelName = excelReader.Name;
                    while (excelReader.Read())
                    {
                        newTableTmp.columnCount = excelReader.FieldCount;
                        var rowDataTmp = new TabelRowData();
                        rowDataTmp.values = new string[excelReader.FieldCount];
                        for (int i = 0; i < excelReader.FieldCount; ++i)
                        {
                            string value = excelReader.IsDBNull(i) ? "" : excelReader.GetString(i);
                            rowDataTmp.values[i] = value;
                        }
                        newTableTmp.rowDatas.Add(rowDataTmp);
                    }

                } while (excelReader.NextResult());

                // if (!string.IsNullOrEmpty(excelReader.ExceptionMessage))
                // {
                //     Log.Error("ExcelData Init exception: path=" + path + "\n" + excelReader.ExceptionMessage);
                //     retValue = ExcelDefine.ExcelInitResult.ReadError;
                //     return retValue;
                // }

                if (readTables.IsNullOrEmpty() || readTables[0].rowDatas.IsNullOrEmpty())
                {
                    Log.Error("ExcelData Init exception: nothing can read,  path=" + path);
                    retValue = ExcelDefine.ExcelInitResult.NothingCanRead;
                    return retValue;
                }

                for (int i = 0; i < readTables.Count; ++i)
                {
                    AddtableInfo(readTables[i], i);
                }
            }
            catch (System.Exception e)
            {
                var errorString = e.ToString();

                //文件已经被打开的时候，再File.Open会出现文件占用的错误
                if (errorString.Contains("Sharing violation on path"))
                {
                    retValue = ExcelDefine.ExcelInitResult.SharingViolation;
                }
                else
                {
                    Log.Error("ExcelData Init erorr: path=" + path + "\n" + errorString);
                    retValue = ExcelDefine.ExcelInitResult.ReadError;
                }
            }
            finally
            {
                if (null != stream)
                {
                    stream.Close();
                    stream = null;
                }
            }
            return retValue;
        }

        /// <summary>
        /// 添加一条子表数据
        /// <param name="dataTabel">子表源数据</param>
        /// <param name="tabelIndex">子表下标</param>
        /// </summary>
        private void AddtableInfo(TableInfo dataTabel, int tabelIndex)
        {
            int minColums = 0;
            int maxColums = 0;
            var tableInfoNew = new TableInfo();
            int rows = dataTabel.rowDatas.Count;

            GetParameterColumsRange(dataTabel, out minColums, out maxColums);

            _dataList.Add(tableInfoNew);

            tableInfoNew.tabelName = dataTabel.tabelName;
            tableInfoNew.columnCount = dataTabel.columnCount;

            for (int i = 0; i < rows; ++i)
            {
                var lineStrings = new string[maxColums + 1];

                //拷贝数据到数组
                for (int j = minColums; j <= maxColums; ++j)
                {
                    var newData = dataTabel.rowDatas[i].values[j].ToString();
                    lineStrings[j - minColums] = newData;
                }

                //添加一行excel数据
                AddLineData(lineStrings, tableInfoNew, maxColums - minColums + 1);
            }
        }

        /// <summary>
        /// 获取一行最小、最大参数，不足参数都自动补齐
        /// </summary>
        private void GetParameterColumsRange(TableInfo dataTabel, out int minIndex, out int maxIndex)
        {
            minIndex = maxIndex = 0;
            int columns = dataTabel.columnCount;
            if (columns <= 0)
                return;

            int findLastValidRowIndex = -1;
            int rows = dataTabel.rowDatas.Count;
            int colums = dataTabel.columnCount;

            //以表中有效数据第一行开始为数据有效长度检查行
            for (int i = 0; i < rows; ++i)
            {
                var rowData = dataTabel.rowDatas[i];
                var validValue = string.Empty;
                bool isValidRowData = false;
                for (int j = 0; j < colums; ++j)
                {
                    validValue = dataTabel.rowDatas[i].values[j].ToString();
                    if (!string.IsNullOrEmpty(validValue))
                    {
                        isValidRowData = true;
                        break;
                    }
                }
                if (isValidRowData)
                {
                    findLastValidRowIndex = i;
                    break;
                }
            }
            if (findLastValidRowIndex < 0)
            {
                findLastValidRowIndex = 0;
            }
            else if (findLastValidRowIndex > dataTabel.rowDatas.Count - 1)
            {
                findLastValidRowIndex = dataTabel.rowDatas.Count - 1;
            }

            var rowDataTmp = dataTabel.rowDatas[findLastValidRowIndex].values;
            for (int i = 0; i < colums; ++i)
            {
                var valueTmp = rowDataTmp[i].ToString();
                if (!string.IsNullOrEmpty(valueTmp))
                {
                    minIndex = i;
                    break;
                }
            }
            for (int i = colums - 1; i >= 0; --i)
            {
                var valueTmp = rowDataTmp[i].ToString();
                if (!string.IsNullOrEmpty(valueTmp))
                {
                    maxIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// 从csv中初始化excel数据
        /// *注意* csv表仅支持一个子表
        /// <param name="path">路径</param>
        /// <return>excel读取结果</return>
        /// </summary>
        private ExcelDefine.ExcelInitResult InitWithCsv(string path)
        {
            ExcelDefine.ExcelInitResult retValue = ExcelDefine.ExcelInitResult.Suceess;

            if (!FileHelper.ExistsFile(path))
            {
                Log.Error("ExcelData InitWithCsv error: missing path=" + path);
                retValue = ExcelDefine.ExcelInitResult.PathEmpty;
                return retValue;
            }

            var readString = FileHelper.ReadAllByUserPath(path);
            if (string.IsNullOrEmpty(readString))
            {
                Log.Error("ExcelData InitWithCsv error: no data, path=" + path);
                retValue = ExcelDefine.ExcelInitResult.EmptyData;
                return retValue;
            }

            retValue = InitWithCsvBinary(readString.ToByteArray());
            return retValue;
        }

        /// <summary>
        /// 重制表数据
        /// </summary>
        private void ResetDataList()
        {
            if (null == _dataList)
            {
                _dataList = new List<TableInfo>();
            }
            else
            {
                _dataList.Clear();
            }
        }

        /// <summary>
        /// 写入一个子表的数据
        /// <param name="excelFileName">excel原始名字</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="tabelDatas">子表数据</param>
        /// </summary>
        private void SaveTabelAsTxt(string excelFileName, string savePath, TableInfo tableInfo)
        {
            if (_dataList.IsNullOrEmpty())
            {
                Log.Error("ExcelData SaveTabelAsTxt error: not tabel data");
                return;
            }

            var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(savePath);
            var customExportTextPath = shaco.Base.GameHelper.excelSetting.GetCustomExportTextPath(savePath);

            //使用表头自定义的脚本导出相对与excel文件的路径
            if (!string.IsNullOrEmpty(customExportTextPath))
            {
                // var folderPathTmp = shaco.Base.FileHelper.GetFolderNameByPath(savePath);
                var convertFileNameTmp = shaco.Base.FileHelper.ReplaceLastExtension(fileNameTmp, ExcelDefine.EXTENSION_TXT);
                savePath = customExportTextPath.ContactPath(convertFileNameTmp);
            }
            else
                savePath = shaco.Base.FileHelper.ReplaceLastExtension(savePath, ExcelDefine.EXTENSION_TXT);

            var writeString = GetCSVString(tableInfo);
            FileHelper.WriteAllByUserPath(savePath, writeString);
        }

        /// <summary>
        /// 从csv中初始化excel数据
        /// <param name="value">csv字符串数据</param>
        /// <return>excel读取结果</return>
        /// </summary>
        private ExcelDefine.ExcelInitResult InitWithCsvBinary(byte[] bytes)
        {
            ExcelDefine.ExcelInitResult retValue = ExcelDefine.ExcelInitResult.Suceess;
            if (bytes.IsNullOrEmpty())
            {
                Log.Error("ExcelData InitWithCsvBinary error: data is empty, path=" + excelPath);
                retValue = ExcelDefine.ExcelInitResult.EmptyData;
                return retValue;
            }

            var streamReader = new System.IO.StreamReader(new System.IO.MemoryStream(bytes));

            //csv表仅支持1个tabel读取，以后有时间再考虑优化
            ResetDataList();
            _dataList.Add(new TableInfo());

            //每行数据数量，以第一行数据数量为准
            var firstLineStr = streamReader.ReadLine();
            var firstLineSplit = firstLineStr.Split('\t');

            if (firstLineSplit.IsNullOrEmpty())
            {
                Log.Error("ExcelData InitWithCsvBinary error: first line data is empty, path=" + excelPath);
                retValue = ExcelDefine.ExcelInitResult.EmptyData;
                return retValue;
            }

            int dataCountInRow = firstLineSplit.Length;

            if (null == _currentStringTypePair)
                _currentStringTypePair = GameHelper.dateTypesTemplate.GetTypesTemplate();

            if (null == _currentStringTypePair)
                _currentStringTypePair = GameHelper.dateTypesTemplate.GetTypesTemplate();

            //第一行数据
            AddLineData(firstLineSplit, _dataList[0], dataCountInRow);

            //后续行数据
            while (true)
            {
                var lineStr = streamReader.ReadLine();
                if (string.IsNullOrEmpty(lineStr))
                    break;
                var lineStrSplit = lineStr.Split('\t');
                AddLineData(lineStrSplit, _dataList[0], dataCountInRow);
            }
            return retValue;
        }

        /// <summary>
        /// 添加一行数据
        /// <param name="lineReadDatas">读取到的一行excel的数据</param>
        /// <param name="tabelIndex">第几张表下标</param>
        /// <param name="dataCountInRow">默认一行的excel数据量</param>
        /// <param name="defaultValue">默认填充的数据</param>
        /// </summary>
        private void AddLineData(string[] lineReadDatas, TableInfo tableInfo, int dataCountInRow, string defaultValue = shaco.Base.GlobalParams.EmptyString)
        {
            TabelRowData rowInfo;

            //超出首行数据时候默认丢弃，并报错提示
            if (lineReadDatas.Length > dataCountInRow)
            {
                Log.ErrorFormat("ExcelData AddLineData error: line:{0} data count:{1} over than default count:{2}\n{3}", tableInfo.rowDatas.Count, lineReadDatas.Length, dataCountInRow, excelPath);
                lineReadDatas = lineReadDatas.FixSize(dataCountInRow, string.Empty);
            }

            //行数据不足时候自动用空字符串补齐，并报错提示
            if (lineReadDatas.Length < dataCountInRow)
            {
                Log.ErrorFormat("ExcelData AddLineData error: line:{0} data count:{1} less than default count:{2}\n{3}", tableInfo.rowDatas.Count, lineReadDatas.Length, dataCountInRow, excelPath);
                lineReadDatas = lineReadDatas.FixSize(dataCountInRow, string.Empty);
            }

            //如果当前行没有数据，则忽略该行其他设置了
            if (lineReadDatas.Length > 0)
            {   
                rowInfo = new TabelRowData();
            }
            else
                return;

            //如果第一列就存在//符号则视为注释，在读取数据的时候会自动过滤掉该行
            bool shouldCheckIgnore = true;
            if (tableInfo.rowDatas.Count > 0)
            {
                //如果上个数据已经不是忽略数据了，那么之后的所有数据默认都不能忽略
                shouldCheckIgnore = tableInfo.rowDatas[tableInfo.rowDatas.Count - 1].isIgnoreData;
            }

            if (shouldCheckIgnore)
            {
                rowInfo.isIgnoreData = lineReadDatas[0].StartsWith(ExcelDefine.IGNORE_FLAG)
                                        || IsEmptyRowData(lineReadDatas)
                                        || null != TypeStringToTypePair(lineReadDatas[0], _currentStringTypePair);
            }
            else
            {
                rowInfo.isIgnoreData = lineReadDatas[0].StartsWith(ExcelDefine.IGNORE_FLAG)
                                        || IsEmptyRowData(lineReadDatas);
            }

            //如果不是第一列出现//符号，但是在后面列出现了//符号，则过滤后面的列，自动设置为defaultValue，但是也会被当做数据读取
            if (!rowInfo.isIgnoreData)
            {
                if (!tableInfo.isIgnoreColDatasIndex.IsNullOrEmpty())
                {
                    for (int i = 0; i < lineReadDatas.Length; ++i)
                    {
                        //设置当前列是否有数据需要过滤
                        if (tableInfo.isIgnoreColDatasIndex.Contains(i))
                        {
                            lineReadDatas[i] = defaultValue;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < lineReadDatas.Length; ++i)
                {
                    //第一列数据不允许过滤，一般都是为id列
                    //判断该列数据是否需要过滤
                    if (0 != i && lineReadDatas[i].StartsWith(ExcelDefine.IGNORE_FLAG) && !tableInfo.isIgnoreColDatasIndex.Contains(i))
                    {
                        tableInfo.isIgnoreColDatasIndex.Add(i);
                    }
                }
            }

            //添加筛选完毕后的数据到缓存中
            rowInfo.values = lineReadDatas;
            tableInfo.rowDatas.Add(rowInfo);

            var lastItem = rowInfo.values[rowInfo.values.Length - 1];
            if (lastItem.Contains("\r"))
                rowInfo.values[rowInfo.values.Length - 1] = lastItem.RemoveBehind("\r");
        }

        /// <summary>
        /// 当前行数据是否全部为空
        /// </summary>
        private bool IsEmptyRowData(string[] lineReadDatas)
        {
            bool retValue = true;
            for (int i = 0; i < lineReadDatas.Length; ++i)
            {
                if (!string.IsNullOrEmpty(lineReadDatas[i]))
                {
                    retValue = false;
                    break;
                }
            }
            return retValue;
        }
    }
}