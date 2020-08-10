using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace B2Framework.Editor
{
    public class ExToolsObject : ScriptableObject
    {
        // 工具根目录
        public string homePath = string.Empty;
        // proto 执行脚本路径
        public string protoBatchScript = string.Empty;
        // config 执行脚本路径
        public string cfgBatchScript = string.Empty;
        // 多语言批处理脚本
        public string locBatchScript = string.Empty;
        // 是否生成pb文件
        public bool genPb = true;
        // 是否生成配置文件
        public bool genCfg = true;
        // 是否生成多语言
        public bool genLoc = true;
        // proto源文件所在路径
        public string protoPath = string.Empty;
        // pb文件输出路径
        public string pbOutPath = string.Empty;
        // Excel文件所在路径
        public string xlsxPath = string.Empty;
        // lua 配置文件所在路径
        public string luaCfgPath = string.Empty;
        // 多语言 配置文件所在路径
        public string locXlsxPath = string.Empty;
        // 多语言 json 文件所在路径
        public string locCfgPath = string.Empty;

        public GameLanguage language = GameLanguage.None;
        public void OnEnable()
        {
            Reset();
        }
        public void Reset()
        {
            homePath = "ExTools";

            protoBatchScript = Utility.Path.Combine(homePath, "proto2pb.py");
            protoPath = Utility.Path.Combine(homePath, "protobuf/proto");
            pbOutPath = "Assets/AssetBundles/Protos";

            cfgBatchScript = Utility.Path.Combine(homePath, "xlsx2lua.py");
            xlsxPath = Utility.Path.Combine(homePath, "config");
            luaCfgPath = "Assets/Scripts/Lua/Game/Data/Config";
            
            locBatchScript = Utility.Path.Combine(homePath, "localization.py");
            locXlsxPath = Utility.Path.Combine(homePath, "localization");
            locCfgPath = "Assets/AssetBundles/Localization";

            language = Localization.language;
        }
        public string GetLan()
        {
            StringBuilder sb = new StringBuilder();
            var lan = string.Empty;
            var hasLan = GetLanguage(GameLanguage.English, out lan);
            if (hasLan) sb.Append(lan);

            hasLan = GetLanguage(GameLanguage.ChineseSimplified, out lan);
            if (hasLan)
            {
                if (sb.Length > 0) sb.Append("|");
                sb.Append(lan);
            }

            hasLan = GetLanguage(GameLanguage.ChineseTraditional, out lan);
            if (hasLan)
            {
                if (sb.Length > 0) sb.Append("|");
                sb.Append(lan);
            }

            hasLan = GetLanguage(GameLanguage.Japanese, out lan);
            if (hasLan)
            {
                if (sb.Length > 0) sb.Append("|");
                sb.Append(lan);
            }
            // foreach (GameLanguage suit in (GameLanguage[])Enum.GetValues(typeof(GameLanguage)))
            return sb.ToString();
        }
        public bool GetLanguage(GameLanguage lan, out string result)
        {
            bool hasLan = ((int)lan & (int)language) == (int)lan;
            result = hasLan ? Language.GetLanguage(lan) : string.Empty;
            return hasLan;
        }
    }
}