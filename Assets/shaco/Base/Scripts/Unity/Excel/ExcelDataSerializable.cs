using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    /// <summary>
    /// Unity excel数据序列化类，根据csv数据创建UnityEngine.ScriptableObject脚本
    /// 采用该方式的效率比直接读取csv文本要高出非常非常非常多～～
    /// </summary>
    public class ExcelDataSerializable
    {
        /// <summary>
        /// 将excel数据序列化并自动创建对应的c#脚本，并自动读取
        /// <param name="dataList">excel数据</param>
        /// <param name="exportCSharpPath">导出的c#脚本路径</param>
        /// <param name="typePairs">类型匹配模板</param>
        /// <param name="templatePaths">模板字符串文件路径</param>
        /// </summary>
        static public void SerializableAsCSharpScript(IReadOnlyCollection<shaco.Base.ExcelData.TableInfo> dataList, string exportCSharpPath, shaco.Base.StringTypePair[] typePairs, params string[] templatePaths)
        {
            if (null == dataList || 0 == dataList.Count)
            {
                Log.Error("ExcelData+Serializable SerializableAsCSharpScript error: path=" + exportCSharpPath);
                return;
            }

            //如果模板路径为空，则使用默认模板路径
            if (templatePaths.IsNullOrEmpty())
            {
                var basePath = shaco.Base.GlobalParams.GetShacoUnityEditorResourcesPath();
                templatePaths = new string[]
                {
                    basePath.ContactPath("ExcelDataTemplate_List.cs.txt"),
                    basePath.ContactPath("ExcelDataTemplate_Dictionary.cs.txt"),
                    basePath.ContactPath("ExcelDataTemplate_DuplicateKey_Dictionary.cs.txt"),
                };
            }

            var excelFileName = shaco.Base.FileHelper.GetLastFileName(exportCSharpPath);
            if (1 == dataList.Count)
            {
                foreach (var iter in dataList)
                {
                    SerializableAsCSharpScript(excelFileName, exportCSharpPath, iter, typePairs, templatePaths);
                    break;
                }
            }
            else
            {
                foreach (var iter in dataList)
                {
                    var exportTabelPath = shaco.Base.ExcelData.GetTabelSavePath(exportCSharpPath, iter);
                    SerializableAsCSharpScript(excelFileName, exportTabelPath, iter, typePairs, templatePaths);
                }
            }
        }

        /// <summary>
        /// 是否已经存在序列化脚本文件
        /// <param name="dataList">excel数据</param>
        /// <param name="exportCSharpPath">导出的c#脚本路径</param>
        /// </summary>
        public bool ExistCSharpScript(IReadOnlyCollection<shaco.Base.ExcelData.TableInfo> dataList, string exportCSharpPath)
        {
            if (1 == dataList.Count)
            {
                return shaco.Base.FileHelper.ExistsFile(exportCSharpPath);
            }
            else
            {
                bool existFile = true;
                foreach (var iter in dataList)
                {
                    var exportTabelPath = shaco.Base.ExcelData.GetTabelSavePath(exportCSharpPath, iter);
                    if (!shaco.Base.FileHelper.ExistsFile(exportTabelPath))
                    {
                        existFile = false;
                        break;
                    }
                }
                return existFile;
            }
        }

        /// <summary>
        /// 将excel数据序列化并自动创建对应的c#脚本，并自动读取
        /// <param name="excelFileName">excel原始名字</param>
        /// <param name="exportCSharpPath">导出的c#脚本路径</param>
        /// <param name="tableInfo">子表数据</param>
        /// <param name="typePairs">类型匹配模板</param>
        /// <param name="templatePaths">模板字符串文件路径</param>
        /// </summary>
        static private void SerializableAsCSharpScript(string excelFileName, string exportCSharpPath, shaco.Base.ExcelData.TableInfo tableInfo, shaco.Base.StringTypePair[] typePairs, params string[] templatePaths)
        {
            if (tableInfo.rowDatas.IsNullOrEmpty())
            {
                Log.Error("ExcelDataSerializable SerializableAsCSharpScript error: not init excel=" + excelFileName + " table name=" + tableInfo.tabelName + " export path=" + exportCSharpPath);
                return;
            }

            if (templatePaths.IsNullOrEmpty())
            {
                Log.Error("ExcelDataSerializable SerializableAsCSharpScript error: no template path");
                return;
            }

            int requestTemplatePathCount = shaco.Base.Utility.ToEnums<shaco.Base.ExcelData.TabelDataType>().Length;
            if (templatePaths.Length != requestTemplatePathCount)
            {
                Log.Error("ExcelDataSerializable SerializableAsCSharpScript error: not enough template path count, current=" + templatePaths.Length + " request=" + requestTemplatePathCount);
                return;
            }

            //导出c#脚本文件名字
            var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(exportCSharpPath);

            //导入原始excel文件全路径
            var fullExcelPath = shaco.Base.FileHelper.GetFolderNameByPath(exportCSharpPath).ContactPath(excelFileName);

            //使用表头自定义的脚本导出相对与excel文件的路径
            var customExportScriptPath = shaco.Base.GameHelper.excelSetting.GetCustomExportScriptPath(fullExcelPath);
            if (!string.IsNullOrEmpty(customExportScriptPath))
            {
                // var folderPathTmp = shaco.Base.FileHelper.GetFolderNameByPath(exportCSharpPath);
                var convertFileNameCSTmp = shaco.Base.FileHelper.ReplaceLastExtension(fileNameTmp, "cs");
                exportCSharpPath = customExportScriptPath.ContactPath(convertFileNameCSTmp);
            }
            else
                exportCSharpPath = shaco.Base.FileHelper.ReplaceLastExtension(exportCSharpPath, "cs");

            //获取excel导出相对路径
            if (string.IsNullOrEmpty(fileNameTmp))
            {
                Log.Error("ExcelDataSerializable SerializableAsCSharpScript error: missing file name");
                return;
            }

            //使用表头自定义的加载路径
            var loadResourceRelativePath = string.Empty;
            var convertFileNameTmp = shaco.Base.FileHelper.RemoveLastExtension(fileNameTmp);
            var customResourcePath = shaco.Base.GameHelper.excelSetting.GetCustomLoadPath(fullExcelPath);
            if (!string.IsNullOrEmpty(customResourcePath))
            {
                loadResourceRelativePath = customResourcePath.ContactPath(convertFileNameTmp);
                convertFileNameTmp = shaco.Base.FileHelper.GetLastFileName(loadResourceRelativePath);
            }
            //根据表名字自动生成加载路径
            else
            {
                loadResourceRelativePath = convertFileNameTmp;
            }

            //类名支持的符号最多只有下划线，其他符号一律转换为下划线
            var className = shaco.Base.FileHelper.RemoveLastExtension(convertFileNameTmp);
            className = shaco.Base.Utility.ConvertToValidClassName(className);

            //获取数据类型
            var rowTypePairs = GetTypeRow(exportCSharpPath, tableInfo, typePairs);
            if (rowTypePairs.IsNullOrEmpty())
            {
                Log.Error("ExcelDataSerializable SerializableAsCSharpScript error: can't get data type. path=" + exportCSharpPath);
                return;
            }

            //获取自定义命名空间
            var customNamespaceName = shaco.Base.GameHelper.excelSetting.GetCustomNamespace(fullExcelPath);

            //设置数据类型
            tableInfo.dataType = shaco.Base.GameHelper.excelSetting.GetDataType(fullExcelPath);

            //获取参数名字和数据类型(dataType)和有效的key字段下标(_dataDictionaryKeyTypeColIndex)
            int dataDictionaryKeyTypeColIndex = 0;
            var rowValueNames = GetValueNameRow(tableInfo, typePairs, rowTypePairs.Length, out dataDictionaryKeyTypeColIndex);

            var readTemplateString = string.Empty;
            var keyValueTypeName = rowTypePairs[dataDictionaryKeyTypeColIndex].fullTypeName;
            var keyParameterName = rowValueNames[dataDictionaryKeyTypeColIndex];

            //读取模板文本
            switch (tableInfo.dataType)
            {
                case shaco.Base.ExcelData.TabelDataType.List:
                    {
                        readTemplateString = shaco.Base.FileHelper.ReadAllByUserPath(templatePaths[(int)shaco.Base.ExcelData.TabelDataType.List]);

                        //当列表时候强制设定为整数类型
                        keyValueTypeName = typeof(int).ToTypeString();
                        break;
                    }
                case shaco.Base.ExcelData.TabelDataType.Dictionary:
                    {
                        readTemplateString = shaco.Base.FileHelper.ReadAllByUserPath(templatePaths[(int)shaco.Base.ExcelData.TabelDataType.Dictionary]);
                        break;
                    }
                case shaco.Base.ExcelData.TabelDataType.DuplicateKeyDictionary:
                    {
                        readTemplateString = shaco.Base.FileHelper.ReadAllByUserPath(templatePaths[(int)shaco.Base.ExcelData.TabelDataType.DuplicateKeyDictionary]);
                        break;
                    }
                default: Log.Error("ExcelDataSerializable SerializableAsCSharpScript: unsupport data type=" + tableInfo.dataType); break;
            }

            //依次替换公共模板参数
            readTemplateString = readTemplateString.Replace("__ClassName__", className);
            readTemplateString = readTemplateString.Replace("##InheritedName##", typeof(shaco.Base.IExcelData).FullName);
            readTemplateString = readTemplateString.Replace("##KeyType##", keyValueTypeName);
            readTemplateString = readTemplateString.Replace("##ConfigPath##", '"' + loadResourceRelativePath + '"');
            readTemplateString = readTemplateString.Replace("##NameSpaceName##", string.IsNullOrEmpty(customNamespaceName) ? "shaco.ExcelData" : customNamespaceName);
            readTemplateString = readTemplateString.Replace("##KeyParamName##", keyParameterName);
            readTemplateString = readTemplateString.Replace("##KeyJudgementNull##", GetKeyJudgementNullString(keyValueTypeName, keyParameterName));

            //依次替换变量定义
            ReplaceTemplateParameters(tableInfo, "##RowDataParmeters##", ref readTemplateString, rowTypePairs, rowValueNames, (index, typePair, valueName) =>
            {
                var writeTmp = new System.Text.StringBuilder();
                if (index != 0) writeTmp.Append("\t\t\t");
                writeTmp.Append(string.Format("public {0} {1} {{ get {{ return _{2}; }} }}\n", typePair.fullTypeName, valueName, valueName));
                writeTmp.Append("\t\t\t");
                writeTmp.Append(string.Format("private {0} _{1};\n", typePair.fullTypeName, valueName));
                return writeTmp.ToString();
            });

            //替换RowData的构造方法参数
            ReplaceTemplateParameters(tableInfo, "##ConstructionParameters##", ref readTemplateString, rowTypePairs, rowValueNames, (index, typePair, valueName) =>
            {
                var writeTmp = new System.Text.StringBuilder();
                writeTmp.Append(string.Format("{0} {1}", typePair.fullTypeName, valueName));

                if (haveNextValidData(rowTypePairs, index))
                    writeTmp.Append(", \n\t\t\t\t\t\t\t");
                return writeTmp.ToString();
            });

            //替换RowData的构造方法
            ReplaceTemplateParameters(tableInfo, "##ConstructionFunction##", ref readTemplateString, rowTypePairs, rowValueNames, (index, typePair, valueName) =>
            {
                var writeTmp = new System.Text.StringBuilder();
                if (index != 0) writeTmp.Append("\t\t\t\t");
                writeTmp.Append(string.Format("this._{0} = {1};", valueName, valueName));
                if (index != rowTypePairs.Length - 1)
                    writeTmp.Append("\n");
                return writeTmp.ToString();
            });

            //替换RowData中Clone的读取方法
            ReplaceTemplateParameters(tableInfo, "##RowDataCloneFunction##", ref readTemplateString, rowTypePairs, rowValueNames, (index, typePair, valueName) =>
            {
                var writeTmp = new System.Text.StringBuilder();
                if (index != 0) writeTmp.Append("\t\t\t\t");
                writeTmp.Append(string.Format("cloneData._{0} = this.{1};\n", valueName, valueName));
                return writeTmp.ToString();
            });

            ReplaceTemplateParameters(tableInfo, "##GetRowDataFunction##", ref readTemplateString, rowTypePairs, rowValueNames, (index, typePair, valueName) =>
            {
                var writeTmp = new System.Text.StringBuilder();
                if (index != 0) writeTmp.Append("\t\t\t\t\t");

                var typeTmp = rowTypePairs[index];
                writeTmp.Append(string.Format("var v{0} = excelData.GetData(i, {1}){2};", index, index, null != typePair ? typePair.convertFunction : string.Empty));

                //查看是否还有后续数据
                if (haveNextValidData(rowTypePairs, index))
                    writeTmp.Append("\n");
                return writeTmp.ToString();
            });

            //依次新增变量从配置读取方法
            ReplaceTemplateParameters(tableInfo, "##ExcelDataUpdateFunction##", ref readTemplateString, rowTypePairs, rowValueNames, (index, typePair, valueName) =>
            {
                var writeTmp = new System.Text.StringBuilder();
                var typeTmp = rowTypePairs[index];
                writeTmp.Append(string.Format("v{0}", index));

                //查看是否还有后续数据
                if (haveNextValidData(rowTypePairs, index))
                    writeTmp.Append(",");
                return writeTmp.ToString();
            });

            shaco.Base.FileHelper.WriteAllByUserPath(exportCSharpPath, readTemplateString.ToString());
        }

        /// <summary>
        /// 是否还有后续数据(自动过滤注释标记)
        /// </summary>
        static private bool haveNextValidData(shaco.Base.StringTypePair[] rowTypes, int index)
        {
            //查看是否还有后续数据
            bool retValue = false;
            for (int i = index + 1; i < rowTypes.Length; ++i)
            {
                if (null != rowTypes[i])
                {
                    retValue = true;
                    break;
                }
            }
            return retValue;
        }

        /// <summary>
        /// 写入模板字符串方法
        /// <param name="tableInfo">excel数据</param>
        /// <param name="findTemplateString">要查找的模板字符串</param>
        /// <param name="readTemplateString">从文件中读取的模板字符串</param>
        /// <param name="rowTypePairs">一行数据类型</param>
        /// <param name="rowValueNames">一行数据变量名字</param>
        /// <param name="typePairs">类型匹配表</param>
        /// <param name="callbackReplaceValue">替换参数字符串回调</param>
        /// </summary>
        static private void ReplaceTemplateParameters(shaco.Base.ExcelData.TableInfo tableInfo, string findTemplateString, ref string readTemplateString,
                                                    shaco.Base.StringTypePair[] rowTypePairs, string[] rowValueNames,
                                                    System.Func<int, shaco.Base.StringTypePair, string, string> callbackReplaceValue)
        {
            //依次替换变量定义
            int indexFind = readTemplateString.IndexOf(findTemplateString);
            if (-1 == indexFind)
            {
                Log.Error("ExcelDataSerializable WriteTemplateParameters error: not found in template string=" + findTemplateString + "\nsource=" + readTemplateString);
                return;
            }
            var writeClassRowData = new System.Text.StringBuilder();
            for (int j = 0; j < rowValueNames.Length; ++j)
            {
                //如果是空类型则表示为过滤类型
                if (null == rowTypePairs[j] || rowTypePairs.IsOutOfRange(j))
                    continue;

                //第一行数据为属性名字，如果没有设置第一行为属性名字，则以Item为默认属性名字
                var valueName = GetValueName(tableInfo, j, rowValueNames);

                //写入参数
                var writeValueStrTmp = callbackReplaceValue(j, rowTypePairs[j], valueName);
                if (!string.IsNullOrEmpty(writeValueStrTmp))
                    writeClassRowData.Append(writeValueStrTmp);
            }
            readTemplateString = readTemplateString.Replace(findTemplateString, writeClassRowData.ToString());
        }

        /// <summary>
        /// 获取每行的数据类型，以第一行有效数据类型为准，如果行与行之间的数据类型不一致，会导致读取出错
        /// <param name="exportCSharpPath">导出的c#脚本路径</param>
        /// <param name="tableInfo">子表数据</param>
        /// <param name="typePairs">类型匹配模板</param>
        /// <return>每行数据类型</return>
        /// </summary>
        static private shaco.Base.StringTypePair[] GetTypeRow(string exportCSharpPath, shaco.Base.ExcelData.TableInfo tableInfo, shaco.Base.StringTypePair[] typePairs)
        {
            List<shaco.Base.StringTypePair> retValue = new List<shaco.Base.StringTypePair>();

            if (tableInfo.rowDatas.Count == 0)
            {
                Log.Error("ExcelDataSerializable GetTypeRow: no data");
                return retValue.ToArray();
            }

            //如果上一行是注释了的，并且是可以解析的类型，则使用上一行强指定类型
            for (int i = 0; i < tableInfo.rowDatas.Count; ++i)
            {
                var rowDatasTmp = tableInfo.rowDatas[i];

                //如果第一列数据设置了强自定类型，则判断该行作为强指定类型行使用
                if (null != shaco.Base.ExcelData.TypeStringToTypePair(rowDatasTmp.values[0], typePairs))
                {
                    for (int j = 0; j < rowDatasTmp.values.Length; ++j)
                    {
                        //如果是忽略数据列，则设定为null
                        if (tableInfo.isIgnoreColDatasIndex.Contains(j))
                        {
                            retValue.Add(null);
                        }
                        else
                        {
                            var valueTmp = rowDatasTmp.values[j];
                            var typePairTmp = shaco.Base.ExcelData.TypeStringToTypePair(valueTmp, typePairs);
                            var newTypePair = new shaco.Base.StringTypePair();

                            if (null == typePairTmp)
                            {
                                Log.Error(string.Format("ExcelData+Serializable GetTypeRow error: can't find paird type, path[{0}] type[{1}] col[{2}]", exportCSharpPath, valueTmp, j));
                                Log.Error("will use default type 'System.String' instead of it");
                                newTypePair.fullTypeName = typeof(string).ToTypeString();
                            }
                            else
                            {
                                shaco.Base.Utility.CopyPropertiesAndFields(typePairTmp, newTypePair);
                                var pairTypeCheck = shaco.Base.Utility.Assembly.GetTypeWithinLoadedAssemblies(typePairTmp.fullTypeName);
                                if (null == pairTypeCheck)
                                {
                                    Log.Error("ExcelData+Serializable GetTypeRow erorr: can't create type with name=" + typePairTmp.fullTypeName);
                                    Log.Error("will use default type 'System.String' instead of it");
                                    newTypePair.fullTypeName = typeof(string).ToTypeString();
                                }
                            }
                            retValue.Add(newTypePair);
                        }
                    }
                }

                if (null != retValue)
                    break;
            }

            //如果没有强指定类型，则根据数据类型自动识别类型
            if (null == retValue)
            {
                //忽略注释的行，获取第一行有效数据在第几行
                int typeRowIndexTmp = -1;
                for (int i = 0; i < tableInfo.rowDatas.Count; ++i)
                {
                    if (!tableInfo.rowDatas[i].isIgnoreData)
                    {
                        typeRowIndexTmp = i;
                        break;
                    }
                }

                if (typeRowIndexTmp < 0)
                {
                    Log.Error("ExcelDataSerializable GetTypeRow error: not found valid type in sheet name=" + tableInfo.tabelName);
                    return retValue.ToArray();
                }

                retValue.Clear();
                int dataCount = tableInfo.rowDatas[typeRowIndexTmp].values.Length;

                //获取数据类型
                for (int i = 0; i < dataCount; ++i)
                {
                    var valueTmp = tableInfo.rowDatas[typeRowIndexTmp].values[i];
                    retValue.Add(DataStringToTypePair(valueTmp, typePairs));
                }
            }
            return retValue.ToArray();
        }



        /// <summary>
        /// 通过类型获取类型配对信息
        /// <param name="type">类型</param>
        /// <param name="typePairs">类型匹配模板</param>
        /// <return>数据类型配对信息</return>
        /// </summary>
        static private shaco.Base.StringTypePair TypeToTypePair(System.Type type, shaco.Base.StringTypePair[] typePairs)
        {
            shaco.Base.StringTypePair retValue = null;
            if (typePairs.IsNullOrEmpty())
                return retValue;

            for (int i = typePairs.Length - 1; i >= 0; --i)
            {
                if (null != type && type.FullName == typePairs[i].fullTypeName)
                {
                    retValue = typePairs[i];
                    break;
                }
            }

            if (null == retValue)
            {
                Log.Error("ExcelDataSerializable TypeToTypePair error: unsupport type=" + type);
                retValue = shaco.Base.ExcelData.TypeStringToTypePair("string", typePairs);
            }
            return retValue;
        }

        /// <summary>
        /// 通过字符串判断数据类型
        /// <param name="str">数据字符串</param>
        /// <param name="typePairs">类型匹配模板</param>
        /// <return>数据类型</return>
        /// </summary>
        static private shaco.Base.StringTypePair DataStringToTypePair(string str, shaco.Base.StringTypePair[] typePairs)
        {
            System.Type findType = typeof(string);

            if (str == "true" || str == "false")
            {
                findType = typeof(bool);
            }
            else if (shaco.Base.Utility.IsNumber(str))
            {
                if (str.Contains("."))
                    findType = typeof(float);
                else
                    findType = typeof(int);
            }
            else
            {
                findType = typeof(string);
            }
            
            return TypeToTypePair(findType, typePairs);
        }

        /// <summary>
        /// 获取所有参数名字
        /// <param name="tableInfo">子表数据</param>
        /// <param name="typePairs">类型匹配模板</param>
        /// <param name="typeCount">类型数量</param>
        /// <param name="dataDictionaryKeyTypeColIndex">数据key行下表</param>
        /// <return>返回一行的参数名字</return>
        /// </summary>
        static private string[] GetValueNameRow(shaco.Base.ExcelData.TableInfo tableInfo, shaco.Base.StringTypePair[] typePairs, int typeCount, out int dataDictionaryKeyTypeColIndex)
        {
            dataDictionaryKeyTypeColIndex = 0;
            if (tableInfo.rowDatas.Count == 0)
            {
                Log.Error("ExcelDataSerializable GetValueNameRow: no data");
                return null;
            }

            int maxColums = typeCount;
            var findIndex = -1;
            for (int i = 0; i < tableInfo.rowDatas.Count; ++i)
            {
                var dataRowTmp = tableInfo.rowDatas[i];

                //以每行第一列数据为准，找到第一个以自定义数据类型定义的行
                if (!string.IsNullOrEmpty(dataRowTmp.values[dataRowTmp.values.Length - 1]))
                {
                    //获取当前行首列是否有设置数据标记
                    if (dataRowTmp.values.Length > 0)
                    {
                        var valueTmp = dataRowTmp.values[0];

                        //判断该行是否为自定义数据行
                        if (null != shaco.Base.ExcelData.TypeStringToTypePair(valueTmp, typePairs))
                        {
                            //找到数据类型行，退出循环
                            findIndex = i;
                            break;
                        }
                    }
                }
            }

            //统一以第一列数据为key
            dataDictionaryKeyTypeColIndex = 0;

            //获取一行的参数名字
            var parametersName = new List<string>();
            if (findIndex >= 0)
            {
                for (int i = 0; i < maxColums; ++i)
                {
                    //如果是忽略数据列，则添加为空字符串
                    if (tableInfo.isIgnoreColDatasIndex.Contains(i))
                    {
                        parametersName.Add(string.Empty);
                        continue;
                    }
                    var parameterName = tableInfo.rowDatas[findIndex].values[i];

                    //删除注释标记
                    if (parameterName.StartsWith(shaco.Base.ExcelDefine.IGNORE_FLAG))
                    {
                        parameterName = parameterName.Remove(0, shaco.Base.ExcelDefine.IGNORE_FLAG.Length);
                    }

                    //删除类型标记
                    var typePairTmp = shaco.Base.ExcelData.TypeStringToTypePair(parameterName, typePairs);
                    if (null != typePairTmp)
                    {
                        int customTypeStringLen = typePairTmp.customTypeString.Length;
                        int placeholderLength = (customTypeStringLen < parameterName.Length && parameterName[customTypeStringLen] == '_') ? 1 : 0;
                        parameterName = parameterName.Remove(0, typePairTmp.customTypeString.Length + placeholderLength);
                    }
                    parametersName.Add(parameterName);
                }
            }
            //没有找到参数名字行，用默认参数名字代替
            else
            {
                maxColums = tableInfo.rowDatas[0].values.Length - tableInfo.isIgnoreColDatasIndex.Count;
                for (int i = 0; i < maxColums; ++i)
                {
                    parametersName.Add("Item" + i);
                }
            }
            return parametersName.ToArray();
        }

        /// <summary>
        /// 获取参数的名字
        /// <param name="tableInfo">子表数据</param>
        /// <param name="indexCol">纵向数据下标</param>
        /// <param name="rowValueNames">一行参数名字</param>
        /// <return>参数名字</return>
        /// </summary>
        static private string GetValueName(shaco.Base.ExcelData.TableInfo tableInfo, int indexCol, string[] rowValueNames)
        {
            var retValue = string.Empty;
            if (tableInfo.rowDatas.IsNullOrEmpty())
            {
                Log.Error("ExcelDataSerializable GetValueName error: not init data, tableName=" + tableInfo.tabelName);
                return retValue;
            }

            if (indexCol < 0 || indexCol > rowValueNames.Length - 1)
            {
                Log.Error("ExcelDataSerializable GetValueName error: out of range, indexCol=" + indexCol + " count=" + rowValueNames.Length);
                return retValue;
            }

            retValue = rowValueNames[indexCol];

            //参数明明不允许空格，默认替换空格为下划线
            retValue = retValue.Replace(" ", "_");
            return retValue;
        }

        /// <summary>
        /// 判断是否为基础数据类型
        /// </summary>
        static private string GetKeyJudgementNullString(string fullTypeName, string keyParameterName)
        {
            var retValue = string.Empty;
            bool isBaseType = typeof(byte).FullName == fullTypeName
                           || typeof(sbyte).FullName == fullTypeName
                           || typeof(int).FullName == fullTypeName
                           || typeof(uint).FullName == fullTypeName
                           || typeof(short).FullName == fullTypeName
                           || typeof(ushort).FullName == fullTypeName
                           || typeof(long).FullName == fullTypeName
                           || typeof(ulong).FullName == fullTypeName
                           || typeof(uint).FullName == fullTypeName
                           || typeof(decimal).FullName == fullTypeName
                           || typeof(float).FullName == fullTypeName
                           || typeof(double).FullName == fullTypeName
                           || typeof(bool).FullName == fullTypeName
                           || typeof(char).FullName == fullTypeName;

            if (isBaseType)
            {
                retValue = string.Empty;
            }
            else if (typeof(string).FullName == fullTypeName)
            {
                retValue = string.Format(" || string.IsNullOrEmpty(newData.{0})", keyParameterName);
            }
            else
            {
                retValue = string.Format(" || null == newData.{0}", keyParameterName);
            }
            return retValue;
        }
    }
}