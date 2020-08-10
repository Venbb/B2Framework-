using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    /// <summary>
    /// 资源打包设定配置，当没有该设置情况下默认一个资源打一个资源包 或者 相同名字不同后缀名资源打一个资源包
    /// </summary> 
    public class ResourcePackSetting : ScriptableObject
    {
        //资源格式
        public HotUpdateDefine.ExportFileFormat exportFormat = HotUpdateDefine.ExportFileFormat.AssetBundle;

        //资源现对路径
        public List<string> assetsGUID = new List<string>();
    }
}