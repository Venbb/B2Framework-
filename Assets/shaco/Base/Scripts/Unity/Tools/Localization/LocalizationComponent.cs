using UnityEngine;
using System.Collections;

namespace shaco
{
    public class LocalizationComponent : MonoBehaviour
    {
        public enum TargetType
        {
            None,
            Text,
            Image,
            Prefab
        }

        [HideInInspector]
        public TargetType type { get { return _type; } private set { _type = value; } }
        private TargetType _type = TargetType.Text;

        public string languageKey
        {
            get { return _languageKey; }
            set
            {
                if (_languageKey != value)
                {
                    _languageKey = value;
                    UpdateLocalization();
                }
            }
        }

        //是否异步加载图片或者prefab
        [HideInInspector]
        public bool isAsyncLoad = true;

        [HideInInspector]
        public string[] formatParams = new string[0];

        private bool isInited = false;

        [SerializeField]
        [HideInInspector]
        private string _languageKey = string.Empty;

        void OnEnable()
        {
            if (isInited)
                UpdateLocalization();
        }

        void Start()
        {
            if (!isInited)
            {
                isInited = true;
                UpdateLocalization();
            }
        }

        void Reset()
        {
            if (!ChangeTargetType(this.type))
            {
                this.type = TargetType.None;
            }
        }

        public bool ChangeTargetType(TargetType type)
        {
            bool isValid = true;
            switch (type)
            {
                case TargetType.Text:
                    {
                        if (null == this.GetComponent<UnityEngine.UI.Text>())
                        {
                            Log.Error(string.Format("LocalizationComponent type '{0}' require component '{1}'", type, typeof(UnityEngine.UI.Text).ToTypeString()));
                            isValid = false;
                        }
                        break;
                    }
                case TargetType.Image:
                    {
                        if (null == this.GetComponent<UnityEngine.UI.Image>())
                        {
                            Log.Error(string.Format("LocalizationComponent type '{0}' require component '{1}'", type, typeof(UnityEngine.UI.Image).ToTypeString()));
                            isValid = false;
                        }
                        break;
                    }
                case TargetType.Prefab: break;
                default: Log.Error("LocalizationComponent unsupport target type=" + type); isValid = false; break;
            }

            if (isValid)
                this.type = type;
            return isValid;
        }

        public void UpdateLocalization()
        {
            switch (type)
            {
                case TargetType.None: /*do nothing*/ break;
                case TargetType.Text: InitWithText(); break;
                case TargetType.Image: InitWithImage(); break;
                case TargetType.Prefab: InitWithPrefab(); break;
                default: shaco.Log.Error("LocalizationComponent error: unsupport type=" + type, this); break;
            }
        }

        private void InitWithText()
        {
            var textTargetTmp = this.GetComponent<UnityEngine.UI.Text>();
            if (null == textTargetTmp)
            {
                Log.Error("LocalizationComponent InitWithText error: Require UnityEngine.UI.Text Component ! target name=" + this.name, this);
                return;
            }

            textTargetTmp.text = shaco.GameHelper.localization.GetTextFormat(languageKey, formatParams);

#if UNITY_EDITOR
            //force to update text in scene
            UnityEditor.EditorUtility.SetDirty(textTargetTmp);
#endif
        }

        /// <summary>
        /// 获取合并路径，资源路径可以分为路径和版本名字，用|隔开，例如test.png|version
        /// <param name="localizationValue">本地化语言读取的值</param>
        /// <param name="path">资源路径</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        private void GetPathWithCombine(string localizationValue, out string path, out string multiVersionControlRelativePath)
        {
            var splitPaths = localizationValue.Split('|');
            if (splitPaths.IsNullOrEmpty())
            {
                path = multiVersionControlRelativePath = string.Empty;
            }

            path = splitPaths.Length > 0 ? splitPaths[0] : string.Empty;
            multiVersionControlRelativePath = splitPaths.Length > 1 ? splitPaths[1] : string.Empty;
        }

        private void InitWithImage()
        {
            var imageTargetTmp = this.GetComponent<UnityEngine.UI.Image>();
            if (null == imageTargetTmp)
            {
                Log.Error("LocalizationComponent InitWithImagePath error: Require UnityEngine.UI.Image Component ! target name=" + this.name, this);
                return;
            }

            string path, multiVersionControlRelativePath;
            GetPathWithCombine(shaco.GameHelper.localization.GetTextFormat(languageKey, formatParams), out path, out multiVersionControlRelativePath);

            if (isAsyncLoad)
            {
                shaco.GameHelper.localization.GetResourceAsync<Sprite>(path, null, (loadObj) =>
                {
                    imageTargetTmp.sprite = loadObj;
                }, multiVersionControlRelativePath);
            }
            else
            {
                imageTargetTmp.sprite = shaco.GameHelper.localization.GetResource<Sprite>(path, multiVersionControlRelativePath);
            }
        }

        private void InitWithPrefab()
        {
            var pathTmp = shaco.GameHelper.localization.GetTextFormat(languageKey, formatParams);
            var newPrefab = shaco.GameHelper.localization.GetResource<GameObject>(pathTmp);
            if (null == newPrefab)
            {
                Log.Error("LocalizationComponent InitWithPrefab error: cannot load resource key=" + languageKey + " path=" + pathTmp, this);
                return;
            }

            string path, multiVersionControlRelativePath;
            GetPathWithCombine(shaco.GameHelper.localization.GetTextFormat(languageKey, formatParams), out path, out multiVersionControlRelativePath);

            if (isAsyncLoad)
            {
                shaco.GameHelper.localization.GetResourceAsync<GameObject>(path, null, (loadObj) =>
                {
                    newPrefab = MonoBehaviour.Instantiate(loadObj) as GameObject;
                    shaco.UnityHelper.ChangeParentLocalPosition(newPrefab, this.transform.parent.gameObject);
                    MonoBehaviour.Destroy(this.gameObject);
                }, multiVersionControlRelativePath);
            }
            else
            {
                newPrefab = MonoBehaviour.Instantiate(newPrefab) as GameObject;
                shaco.UnityHelper.ChangeParentLocalPosition(newPrefab, this.transform.parent.gameObject);
                MonoBehaviour.Destroy(this.gameObject);
            }
        }
    }
}

