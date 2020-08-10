using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LuaReddotCodeGenerater
{
    private class DataTreeItem
    {
        public GameObject go = null;
        public string luaScriptName = "";
        public string luaFunctionName = "";
        public string gen_funcname = "";
        public string gen_funcCode = "";
        public bool shuldPassParam = true;
        public List<string> paramList = new List<string>();
        public List<DataTreeItem> children = new List<DataTreeItem>();
    }
    //这个用于检测是不是有重复的名称用
    private static Dictionary<string, bool> dic_nameChecker;
    private static void CheckNodeName(GameObject rootGameObject)
    {
        if (dic_nameChecker.ContainsKey(rootGameObject.name))
        {
            Debug.LogError("如下节点名称错误，请换个名字: " + rootGameObject.name);
        }
        dic_nameChecker[rootGameObject.name] = true;
    }
    [MenuItem("B2Framework/生成红点系统(ReddotManager.lua)")]
    public static void GenerateLuaReddotCode()
    {
        dic_nameChecker = new Dictionary<string, bool>();
        Scene scene = SceneManager.GetActiveScene();
        if(!scene.name.Equals("ReddotTree"))
        {
            Debug.LogError("场景错误，必须首先打开场景：ReddotTree");
            return;
        }
        string outputFile = Application.dataPath + "/AssetBundles/Scripts/Game/ReddotManager.lua";
        string strTempPath = Application.dataPath + "/B2Framework/Scripts/Editor/Template/ReddotManagertemplate.lua";
        GameObject canvas = GameObject.FindObjectOfType<Canvas>().gameObject;
        List<DataTreeItem> list = readBranch(canvas);
        for (int i = 0; i < list.Count; ++i)
        {
            GenDataToCode(list[i]);
        }

        string str = "";
        for (int i = 0; i < list.Count; ++i)
        {
            GenerateCodeToString(ref str, list[i]);
        }
        if (File.Exists(strTempPath))
        {
            string strContent = File.ReadAllText(strTempPath);
            strContent = strContent.Replace("#Code#", str);
            File.WriteAllText(outputFile, strContent);
        }

        AssetDatabase.Refresh();
        Debug.Log("脚本生成完毕，具体结果可看：  " + outputFile);
    }

    private static List<DataTreeItem> readBranch(GameObject rootGameObject)
    {
        CheckNodeName(rootGameObject);
        List<DataTreeItem> list = new List<DataTreeItem>();
        for (int i = 0; i < rootGameObject.transform.childCount; ++i)
        {
            GameObject go = rootGameObject.transform.GetChild(i).gameObject;
            ReddotEditItem rei = go.GetComponent<ReddotEditItem>();
            DataTreeItem dti = new DataTreeItem();
            dti.go = go;
            if (!rei.luaScriptName.Equals(""))
            {
                dti.luaScriptName = rei.luaScriptName;
                dti.luaFunctionName = rei.luaFunctionName;
            }
            if(dti.go.transform.childCount > 0)
            {
                dti.children = readBranch(go);
            }
            else
            {
                CheckNodeName(dti.go);
            }
            dti.paramList = rei.paramList;
            dti.shuldPassParam = rei.shuldPassParam;
            list.Add(dti);
            if(dti.shuldPassParam)
            {
                for(int j = 0; j < dti.paramList.Count; ++j)
                {
                    if(dti.paramList[j].IndexOf(".") >= 0)
                    {
                        Debug.LogError("参数名不合法： " + dti.go.name);
                    }
                }
            }
        }
        return list;
    }

    private static void GenDataToCode(DataTreeItem dataTreeItem)
    {
        dataTreeItem.gen_funcname = "GetReddot_" + dataTreeItem.go.name;

        for (int i = 0; i < dataTreeItem.children.Count; ++i)
        {
            GenDataToCode(dataTreeItem.children[i]);
        }
        if ((!dataTreeItem.luaScriptName.Equals("")) && (!dataTreeItem.luaFunctionName.Equals("")))
        {
            dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + dataTreeItem.luaScriptName + "." + dataTreeItem.luaFunctionName + "(";
            for (int j = 0; j < dataTreeItem.paramList.Count; ++j)
            {
                if (j > 0)
                {
                    dataTreeItem.gen_funcCode = ", ";
                }
                dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + dataTreeItem.paramList[j];
            }
            dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + ")";
        }
        else
        {
            //没有子节点，但是自己却没有获得红点的方法，那么这个节点是错误的，少录入了获取红点的调用的方法
            if(dataTreeItem.children.Count == 0)
            {
                Debug.LogError("节点错误，该节点需要录入获取红点的调用的方法： " + dataTreeItem.go.name);
            }
            int childParamCount = 0;
            int childParamIndex = 0;
            for (int i = 0; i < dataTreeItem.children.Count; ++i)
            {
                if(dataTreeItem.children[i].shuldPassParam)
                {
                    childParamCount = childParamCount + dataTreeItem.children[i].paramList.Count;
                }
            }
            for (int i = 0; i < dataTreeItem.children.Count; ++i)
            {
                DataTreeItem item = dataTreeItem.children[i];
                if(i > 0)
                {
                    dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + " + ";
                }
                dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + "self." + item.gen_funcname;
                if (childParamCount > 0 && item.shuldPassParam)
                {
                    if (dataTreeItem.paramList.Count > 0)
                    {
                        if (dataTreeItem.paramList.Count != childParamCount)
                        {
                            Debug.LogError("子节点固定参数数量有误： " + dataTreeItem.go.name);
                        }
                        else
                        {
                            dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + "(";
                            for(int j = 0; j < item.paramList.Count; ++j)
                            {
                                if (j > 0)
                                {
                                    dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + ", ";
                                }
                                dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + dataTreeItem.paramList[childParamIndex];
                            }
                            dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + ")";
                        }
                    }
                    else
                    {
                        Debug.LogError("节点错误，该节点必须调用第三方函数或者为子函数指定传参(paramList)： " + dataTreeItem.go.name + " <-----> " + item.go.name);
                    }
                }
                else
                {
                    dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + "()";
                }
            }
        }
        dataTreeItem.gen_funcCode = dataTreeItem.gen_funcCode + "\n";
    }

    private static void GenerateCodeToString(ref string str, DataTreeItem dataTreeItem)
    {
        for(int i = 0; i < dataTreeItem.children.Count; ++i)
        {
            GenerateCodeToString(ref str, dataTreeItem.children[i]);
        }
        str = str + "function ReddotManager:" + dataTreeItem.gen_funcname + "(";
        if(dataTreeItem.shuldPassParam)
        {
            for (int i = 0; i < dataTreeItem.paramList.Count; ++i)
            {
                if (i > 0)
                {
                    str = str + ", ";
                }
                str = str + dataTreeItem.paramList[i];
            }
        }
        str = str + ")\n";
        str = str + "   return " + dataTreeItem.gen_funcCode;
        str = str + "end\n\n";
    }
}
