using System.Collections.Generic;
using UnityEngine;

namespace B2Framework.Unity
{
    public static class Localization
    {
        public static System.Action OnLocalize;
        static GameLanguage _language = GameLanguage.ChineseSimplified;
        /// <summary>
        /// 当前语言码
        /// </summary>
        /// <value></value>
        public static GameLanguage language
        {
            get { return _language; }
        }
        static string _lanCode;
        public static string lanCode
        {
            get { return _lanCode; }
        }
        static Dictionary<string, string> maps = new Dictionary<string, string>();
        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defualtVal"></param>
        /// <returns></returns>
        public static string Get(string key, string defualtVal = "")
        {
            string val;
            if (!maps.TryGetValue(key, out val))
            {
                Log.Warning("Localization key not found: '" + key + "'");
                return string.IsNullOrEmpty(defualtVal) ? defualtVal : key;
            }
            return val;
        }
        /// <summary>
        /// 设置当前语言
        /// </summary>
        /// <param name="lan"></param>
        /// <returns></returns>
        public static void SwitchLanguage(GameLanguage lan, bool auto = true)
        {
            var code = Language.GetLanguage(lan);
            if (code != _lanCode)
            {
                _lanCode = code;

                if (auto) Load();
            }
        }
        /// <summary>
        /// 加载多语言配置
        /// </summary>
        /// <param name="completed"></param>
        public static void Load(System.Action completed = null)
        {
            if (string.IsNullOrEmpty(_lanCode)) _lanCode = Language.current;
            // var request = Resources.LoadAsync<TextAsset>("lc_cn_ERROR");
            // request.completed += (async) =>
            // {
            //     TextAsset textAsset = request.asset as TextAsset;
            //     Parse(textAsset.text);
            //     if (completed != null) completed();
            // };
            var path = "Assets/AssetBundles/Localization/lc_zh_LOGIN.json";
            var request = Assets.LoadAssetAsync(path, typeof(TextAsset));
            request.completed = (re) =>
            {
                if (string.IsNullOrEmpty(request.error))
                {
                    TextAsset textAsset = re.asset as TextAsset;
                    Parse(textAsset.text);
                }
                else
                    Log.Error(Utility.Text.Format("Load Localization error:{0}", request.error));
                re.Release();
                if (completed != null) completed();
                if (OnLocalize != null) OnLocalize.Invoke();
            };
        }
        /// <summary>
        /// 解析配置
        /// </summary>
        /// <param name="text"></param>
        static void Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            maps.Clear();

            maps = LitJson.JsonMapper.ToObject<Dictionary<string, string>>(text);
            // foreach (var m in maps) Debug.Log(m.Key + ":" + m.Value);
        }
        /// <summary>
        /// 释放
        /// </summary>
        public static void Dispose()
        {
            maps.Clear();
        }
    }
}