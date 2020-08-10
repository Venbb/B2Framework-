using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class LocalizationReplacePrefabFindText : shacoEditor.LocalizationReplacePrefab
    {
        /// <summary>
        /// 查找文本条件
        /// </summary>
        private enum SearchCondition
        {
            //包含
            Contains,
            //不包含
            NotContains,
            //相等
            Equal,
            //不相等
            NotEqual
        }

        private class SearchInfo
        {
            public SearchCondition searchCondition = SearchCondition.Equal;
            public string searchText = string.Empty;
        }

        //需要查找的文本信息
        private List<SearchInfo> _searchTexts = new List<SearchInfo>() { new SearchInfo() };

        //是否忽略大小写 
        private bool _isIgnoreCaseSensitive = false;

        /// <summary>
        /// 获取所有语言文本
        /// <param name="importPath">导入路径</param>
        /// <param name="callbackCollectInfo">加载完毕后语言文本收集信息</param>
        /// <param name="collectExtensions">需要收集的文件后缀名</param>
        /// </summary>
        override public void GetAllLanguageString(string importPath, System.Action<List<shaco.Base.Utility.LocalizationCollectnfo>> callbackCollectInfo, params string[] collectExtensions)
        {
            base.GetAllLanguageString(importPath, (List<shaco.Base.Utility.LocalizationCollectnfo> collectInfos)=>
            {
                //从筛选过的prefab导出信息中，再筛选出只包含查找文本内容的prefab
                if (null != callbackCollectInfo)
                {
                    for (int i = collectInfos.Count - 1; i >= 0; --i)
                    {
                        for (int j = _searchTexts.Count - 1; j >= 0; --j)
                        {
                            bool needRemoveInfo = false;
                            var serachText = _isIgnoreCaseSensitive ? _searchTexts[j].searchText.ToLower() : _searchTexts[j].searchText;
                            var infoText = _isIgnoreCaseSensitive ? collectInfos[i].languageString.ToLower() : collectInfos[i].languageString;
                            switch (_searchTexts[j].searchCondition)
                            {
                                case SearchCondition.Contains:
                                    {
                                        if (infoText.Contains(serachText))
                                            needRemoveInfo = true;
                                        break;
                                    }
                                case SearchCondition.Equal:
                                    {
                                        if (infoText == serachText)
                                            needRemoveInfo = true;
                                        break;
                                    }
                                case SearchCondition.NotContains:
                                    {
                                        if (!infoText.Contains(serachText))
                                            needRemoveInfo = true;
                                        break;
                                    }
                                case SearchCondition.NotEqual:
                                    {
                                        if (infoText != serachText)
                                            needRemoveInfo = true;
                                        break;
                                    }
                                default: Debug.LogError("LocalizationReplacePrefabFindText GetAllLanguageString error: unsupport serach condition type=" + _searchTexts[j].searchCondition); break;
                            }

                            if (needRemoveInfo)
                            {
                                collectInfos.RemoveAt(i);
                                break;
                            }
                        }
                    }

                    callbackCollectInfo(collectInfos);
                }
            }, collectExtensions);
        }

        /// <summary>
        /// 替换语言文本信息
        /// <param name="path">路径</param>
        /// <param name="exportInfo">导出的语言包信息</param>
        /// </summary>
        override public void RepalceLanguageString(string path, List<shaco.Base.Utility.LocalizationExportInfo> exportInfos)
        {
            base.RepalceLanguageString(path, exportInfos);
        }

        /// <summary>
        /// 绘制编辑器
        /// </summary>
        override public void DrawInspector()
        {
            base.DrawInspector();
            _isIgnoreCaseSensitive = EditorGUILayout.Toggle("IgnoreCaseSensitive", _isIgnoreCaseSensitive);
            GUILayoutHelper.DrawList(_searchTexts, "SearchText", null, null, (int index, SearchInfo searchInfo, System.Action<SearchInfo> callback) =>
            {
                searchInfo.searchCondition = (SearchCondition)EditorGUILayout.EnumPopup(searchInfo.searchCondition, GUILayout.Width(74));
                searchInfo.searchText = GUILayout.TextField(searchInfo.searchText);

                callback(searchInfo);
                return true;
            });
        }
    }
}