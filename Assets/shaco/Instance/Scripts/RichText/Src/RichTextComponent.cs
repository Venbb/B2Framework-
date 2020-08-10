using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace shaco.Instance.RichText
{
    //仅使用在Untiy4.6+版本
    [AddComponentMenu("UI/Extensions/Rich Text")]
    [RequireComponent(typeof(RectTransform))]
    public partial class RichTextComponent : MonoBehaviour
    {
        [System.Serializable]
        public enum TextType
        {
            Text = 0,
            Image,
            Prefab,

            Count
        }

        [System.Serializable]
        public class CharacterInfo
        {
            public TextType type = TextType.Image;
            public string key = string.Empty;
            public string value = string.Empty;
            public Object asset = null;
            public Vector2 customSize = Vector2.zero;
        }

        [System.Serializable]
        public class CharacterFolderInfo
        {
            public string path = string.Empty;
            public bool isAutoUseFullName = true;
        }

        private class WillDisplayStringInfo
        {
            public string data;
            public bool isReturn;
        }

        public string text
        {
            get { return _text; }
            set { if (_text != value) SetText(value); }
        }

        //文本缓存池支持最大的缓存节点数量，超出该数量后不再回收而是直接销毁
        [SerializeField]
        [HideInInspector]
        public int maxCachedCharacterCount = 5;

        /// <summary>
        /// 动态加载的资源路径，当没有设定资源集的时候自动使用，仅仅支持后缀名为.png的图片识别
        /// </summary>
        [HideInInspector]
        public string dynamicCharacterFolder
        {
            get { return _dynamicCharacterFolder; }
            set
            {
                if (_dynamicCharacterFolder != value)
                {
                    var indexFind = value.IndexOf("/Resources");
                    if (indexFind < 0)
                    {
                        Debug.LogError("RichTextComponent set dynamicCharacterFolder error: only support 'Resources' folder value=" + value);
                        return;
                    }
                    _dynamicCharacterFolder = value;
                    int removeCount = indexFind + "/Resources".Length;
                    if (value.IndexOf("/Resources/") >= 0)
                        removeCount += 1;

                    _dynamicCharacterFolderRelativePath = value.Remove(0, removeCount);
                    SetUpdateLayoutDirty();
                }
            }
        }
        private string _dynamicCharacterFolder = "Assets/Resources/richtextpng";
        private string _dynamicCharacterFolderRelativePath = "richtextpng";

        [HideInInspector]
        public Text textModel
        {
            get { return _textModel; }
            set
            {
                if (_textModel != value)
                {
                    _textModel = value;

                    if (null != _textModel)
                    {
                        _horizontalOverflow = _textModel.horizontalOverflow;
                    }
                    OnTextModelUpdate();
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        private Text _textModel = null;

        [HideInInspector]
        public HorizontalWrapMode horizontalOverflow
        {
            get { return _horizontalOverflow; }
            set
            {
                if (_horizontalOverflow != value)
                {
                    _horizontalOverflow = value;
                    if (null != _textModel)
                    {
                        _textModel.horizontalOverflow = value;
                        OnTextModelUpdate();
                    }
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        private HorizontalWrapMode _horizontalOverflow;

        [HideInInspector]
        public TextAnchor textAnchor { get { return _textAnchor; } set { if (_textAnchor != value) { _textAnchor = value; SetUpdateLayoutDirty(); } } }
        [SerializeField]
        [HideInInspector]
        private TextAnchor _textAnchor = TextAnchor.UpperLeft;

        [HideInInspector]
        public TextAnchor contentAnchor { get { return _contentAnchor; } set { if (_contentAnchor != value) { _contentAnchor = value; SetUpdateLayoutDirty(); } } }
        [SerializeField]
        [HideInInspector]
        private TextAnchor _contentAnchor = TextAnchor.UpperLeft;

        [HideInInspector]
        public Vector2 margin { get { return _margin; } set { if (_margin != value) { _margin = value; SetUpdateLayoutDirty(); } } }
        [SerializeField]
        [HideInInspector]
        private Vector2 _margin = Vector2.zero;

        [HideInInspector]
        public List<CharacterFolderInfo> characterFolderPaths = new List<CharacterFolderInfo>();

        [SerializeField]
        [HideInInspector]
        private List<CharacterInfo> _savedCharacters = new List<CharacterInfo>();

        [SerializeField]
        [HideInInspector]
        private string _text = string.Empty;

        private Dictionary<string, CharacterInfo> _searchCharacters = new Dictionary<string, CharacterInfo>();
        private List<WillDisplayStringInfo> _willDisplayStrings = new List<WillDisplayStringInfo>();
        private List<RichTextContentComponent>[] _cachedCharacters = new List<RichTextContentComponent>[(int)TextType.Count];
        private List<RichTextContentComponent> _displayCharacters = new List<RichTextContentComponent>();
        private int _cachedCharactersCount = 0;
        private const string CUSTOM_PARAM_START_FLAG = "[[";
        private const string CUSTOM_PARAM_END_FLAG = "]]";
        private const string DEFAULT_DYNAMIC_IMAGE_EXTENSION = ".png";
        private const char SPLIT_FLAG = '#';
        private const string SPLIT_FLAG_STRING = "#";
        private const string SPLIT_FLAG_TRANSFER = "\\#";
        private const string SPLIT_FLAG_TRANSFER_CONVERT = "[@]";
        private bool _updateLayoutDirty = true;

        void Start()
        {
            if (null != textModel)
            {
                textModel.enabled = false;
            }
            UpdateListDataToDictionaryData();
        }

        void Update()
        {
            UpdateLayout();
        }

        void OnValidate()
        {
            UpdateListDataToDictionaryData();
        }

        void OnDestroy()
        {
            ClearCachedCharacters();
        }

        public void SetText(string text)
        {
            _willDisplayStrings.Clear();
            _text = text;

            if (string.IsNullOrEmpty(text))
            {
                ClearCachedCharacters();
                return;
            }

            var hasTransferSplitFlag = text.IndexOf(SPLIT_FLAG_TRANSFER) >= 0;

            if (hasTransferSplitFlag)
                text = text.Replace(SPLIT_FLAG_TRANSFER, SPLIT_FLAG_TRANSFER_CONVERT);

            var texts = text.Split(SPLIT_FLAG);
            bool needReturn = false;

            for (int i = 0; i < texts.Length; ++i)
            {
                var textTmp = texts[i];

                if (hasTransferSplitFlag && textTmp.IndexOf(SPLIT_FLAG_TRANSFER_CONVERT) >= 0)
                {
                    textTmp = textTmp.Replace(SPLIT_FLAG_TRANSFER_CONVERT, SPLIT_FLAG_STRING);
                }

                if (textTmp.Contains("\n"))
                {
                    var textsTmp = textTmp.Split('\n');
                    for (int j = 0; j < textsTmp.Length; ++j)
                    {
                        if (j == textsTmp.Length - 1 && string.IsNullOrEmpty(textsTmp[j])) continue;
                        var newInfo = new WillDisplayStringInfo()
                        {
                            data = textsTmp[j]
                        };
                        _willDisplayStrings.Add(newInfo);
                        if (needReturn)
                        {
                            needReturn = false;
                            newInfo.isReturn = true;
                        }
                        else
                            newInfo.isReturn = j > 0;
                    }
                }
                else
                {
                    var newInfo = new WillDisplayStringInfo()
                    {
                        data = textTmp
                    };
                    _willDisplayStrings.Add(newInfo);
                    if (needReturn)
                    {
                        needReturn = false;
                        newInfo.isReturn = true;
                    }
                    else
                        newInfo.isReturn = false;
                }

                if (textTmp.Length > 0 && textTmp[textTmp.Length - 1] == '\n')
                    needReturn = true;
            }

            SetUpdateLayoutDirty();
        }

        public void ForeachCharacters(System.Func<CharacterInfo, bool> callback)
        {
            foreach (var iter in _searchCharacters)
            {
                var result = true;
                try
                {
                    result = callback(iter.Value);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("RichText ForeachCharacters exception: key=" + iter.Value.key + " e=" + e);
                }

                if (!result)
                    break;
            }
        }

        public bool HasCharacter(string key)
        {
            return _searchCharacters.ContainsKey(key);
        }

        public bool AddCharacter(CharacterInfo character)
        {
            if (_searchCharacters.ContainsKey(character.key)) return false;
            _searchCharacters.Add(character.key, character);
            _savedCharacters.Add(character);
            return true;
        }

        public bool RemoveCharacter(string key)
        {
            if (!_searchCharacters.ContainsKey(key)) return false;
            _searchCharacters.Remove(key);
            for (int i = _savedCharacters.Count - 1; i >= 0; --i)
            {
                if (_savedCharacters[i].key == key)
                {
                    _savedCharacters.RemoveAt(i);
                    break;
                }
            }
            return true;
        }

        public void ClearCharacters()
        {
            _searchCharacters.Clear();
            _savedCharacters.Clear();
            _dynamicCharacterFolder = string.Empty;
            _dynamicCharacterFolderRelativePath = string.Empty;
            characterFolderPaths.Clear();
            ClearCachedCharacters();
        }

        public void ForceUpdateListDataToDictionaryData()
        {
            _searchCharacters.Clear();
            UpdateListDataToDictionaryData();
        }

        public void ForceUpdateLayout()
        {
            UpdateLayout();
        }

        //当文本模板更新时候主动调用，否则富文本因为本身缓存机制使用的部分文字还是旧的模板
        public void OnTextModelUpdate()
        {
            ClearCachedCharacters();
            SetUpdateLayoutDirty();
        }

        public int GetDisplayCharacterCount()
        {
            return _displayCharacters.Count;
        }

        private void ClearCachedCharacters()
        {
            for (int i = this.transform.childCount - 1; i >= 0; --i)
            {
                DestroySafe(this.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < _cachedCharacters.Length; ++i)
            {
                var listTmp = _cachedCharacters[i];
                if (null != listTmp && listTmp.Count > 0)
                {
                    listTmp.Clear();
                    _cachedCharacters[i] = null;
                }
            }
        }

        private void SetUpdateLayoutDirty()
        {
            _updateLayoutDirty = true;
        }

        private RichTextContentComponent SetTextWithRichText(string textIn)
        {
            RichTextContentComponent newContent = null;
            CharacterInfo characterTmp = null;
            var textKey = RemoveSubstring(textIn, CUSTOM_PARAM_START_FLAG, CUSTOM_PARAM_END_FLAG);
            if (!_searchCharacters.ContainsKey(textKey))
            {
                if (textKey.EndsWith(DEFAULT_DYNAMIC_IMAGE_EXTENSION) && !string.IsNullOrEmpty(dynamicCharacterFolder))
                {
                    characterTmp = new CharacterInfo();
                    characterTmp.type = TextType.Image;

                    var relativePath = System.IO.Path.Combine(_dynamicCharacterFolderRelativePath, textKey);

                    var indexFind1 = relativePath.LastIndexOf('.');
                    var indexFind2 = relativePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                    if (indexFind1 >= 0 && indexFind1 > indexFind2)
                        relativePath = relativePath.Remove(indexFind1);
                    characterTmp.value = relativePath;
                    characterTmp.asset = Resources.Load<Sprite>(characterTmp.value);
                    GetCustomSizeWithText(characterTmp, textIn);
                }

                if (null == characterTmp && null == newContent)
                {
                    CharacterInfo contentTmp = new CharacterInfo();
                    contentTmp.value = textKey;
                    contentTmp.type = TextType.Text;
                    GetCustomSizeWithText(contentTmp, textIn);
                    newContent = CreateText(contentTmp);
                }
            }

            if (null == newContent)
            {
                if (null == characterTmp)
                {
                    characterTmp = _searchCharacters[textKey];
                    GetCustomSizeWithText(characterTmp, textIn);
                }

                switch (characterTmp.type)
                {
                    case TextType.Text: newContent = CreateText(characterTmp); break;
                    case TextType.Image: newContent = CreateImage(characterTmp); break;
                    case TextType.Prefab: newContent = CreatePrefab(characterTmp); break;
                    default: Debug.LogError("RichText SetTextWithRichText error: unsupport type=" + characterTmp.type); break;
                }
            }

            //暂时隐藏对象，等UpdateLayout执行后再显示出来
            newContent.item.SetActive(false);
            return newContent;
        }

        private void DestroySafe(GameObject target)
        {
            if (Application.isPlaying)
                MonoBehaviour.Destroy(target);
            else
                MonoBehaviour.DestroyImmediate(target);
        }

        //自动回收所有字符
        private void CheckAutoRecycleCharacters()
        {
            _cachedCharactersCount = 0;
            for (int i = 0; i < _cachedCharacters.Length; ++i)
            {
                if (null != _cachedCharacters[i])
                    _cachedCharacters[i].Clear();
            }

            for (int i = this.transform.childCount - 1; i >= 0; --i)
            {
                var child = this.transform.GetChild(i);
                if (_cachedCharactersCount >= maxCachedCharacterCount)
                {
                    DestroySafe(child.gameObject);
                    continue;
                }

                var contentComponent = child.GetComponent<RichTextContentComponent>();
                if (null == contentComponent)
                    continue;

                var typeIndex = (int)contentComponent.type;
                if (typeIndex < 0 || typeIndex > _cachedCharacters.Length)
                {
                    Debug.LogError("RichTextComponent CheckAutoRecycleCharacters error: type index out of range, index=" + typeIndex + " count=" + _cachedCharacters.Length);
                    DestroySafe(child.gameObject);
                    continue;
                }

                ++_cachedCharactersCount;
                if (null == _cachedCharacters[typeIndex])
                    _cachedCharacters[typeIndex] = new List<RichTextContentComponent>();

                contentComponent.item.SetActive(false);
                _cachedCharacters[typeIndex].Add(contentComponent);
            }
        }

        private RichTextContentComponent GetFromCacheOrCreateContent(GameObject model, TextType type)
        {
            RichTextContentComponent retValue = null;
            var findComponents = _cachedCharacters[(int)type];
            if (null == findComponents || 0 == findComponents.Count)
            {
                var newItemGameObject = null == model ? CreateGameObject() : MonoBehaviour.Instantiate(model);
                retValue = newItemGameObject.AddComponent<RichTextContentComponent>();
                retValue.item = newItemGameObject;
                retValue.type = type;
            }
            else
            {
                retValue = findComponents[findComponents.Count - 1];
                retValue.item.SetActive(true);
                findComponents.RemoveAt(findComponents.Count - 1);
                --_cachedCharactersCount;
            }
            return retValue;
        }

        private T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            var retValue = obj.GetComponent<T>();
            if (null == retValue)
                retValue = obj.AddComponent<T>();
            return retValue;
        }

        private RichTextContentComponent CreateText(CharacterInfo character)
        {
            var retValue = GetFromCacheOrCreateContent(null != textModel ? textModel.gameObject : null, character.type);
            Text textTmp = null;
            if (null == textModel)
            {
                textTmp = GetOrAddComponent<Text>(retValue.item);
                if (null == textTmp.font)
                {
#if UNITY_5_3_OR_NEWER
                    textTmp.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
#else
                    textTmp.font = new Font("Arial");
                    textTmp.fontSize = 16;
#endif
                }
            }
            else
            {
                textTmp = retValue.item.GetComponent<Text>();
                textTmp.enabled = true;
                retValue.item = textTmp.gameObject;
                retValue.item.SetActive(true);
                retValue.item.transform.SetParent(this.transform, false);
            }

            var transTmp = GetComponent<RectTransform>();
            var transText = textTmp.GetComponent<RectTransform>();

            //获取显示行数
            textTmp.text = string.Empty;
            var heightLine = textTmp.preferredHeight;

            //如果是best fit模式，需要把空格先替换为其他字符然后再进行计算宽度
            textTmp.text = textTmp.resizeTextForBestFit ? character.value.Replace(' ', '_') : character.value;

            // float pivotOffset = 1 - transTmp.pivot.x;
            var widthLine = Mathf.Min(transTmp.rect.width /** pivotOffset*/, textTmp.preferredWidth);
            var realPreferredSize = new Vector2(widthLine, heightLine);

            if (horizontalOverflow == HorizontalWrapMode.Wrap)
            {
                var viewWidth = transTmp.rect.width;
                int lineCount = (int)(textTmp.preferredWidth / (viewWidth)) + 1;
                realPreferredSize = new Vector2(realPreferredSize.x, realPreferredSize.y * lineCount);
            }

            //先粗略设置Text一次宽高
            transText.sizeDelta = realPreferredSize;

            //再根据Text精准计算宽高
            transText.sizeDelta = new Vector2(realPreferredSize.x, textTmp.preferredHeight + 1);

            if (textTmp.resizeTextForBestFit)
                textTmp.text = character.value;

            return retValue;
        }

        private void GetCustomSizeWithText(CharacterInfo character, string text)
        {
            Vector2 retValue = Vector2.zero;
            var findValueString = RichTextComponent.Substring(text, CUSTOM_PARAM_START_FLAG, CUSTOM_PARAM_END_FLAG);
            if (!string.IsNullOrEmpty(findValueString))
            {
                var splitString = findValueString.Split(',');
                if (null != splitString && splitString.Length > 0 && splitString.Length == 2)
                {
                    float.TryParse(splitString[0], out retValue.x);
                    float.TryParse(splitString[1], out retValue.y);

                    var removeString = CUSTOM_PARAM_START_FLAG + findValueString + CUSTOM_PARAM_END_FLAG;
                    var findIndex = character.value.IndexOf(removeString);
                    if (findIndex >= 0)
                        character.value = character.value.Remove(findIndex, removeString.Length);
                }
            }
            character.customSize = retValue;
        }

        private bool SetCustomSizeInCharacter(CharacterInfo character, GameObject target)
        {
            if (null == target || 0 == character.customSize.x || 0 == character.customSize.y)
                return false;

            var rectTransformTmp = target.GetComponent<RectTransform>();
            if (null != rectTransformTmp)
            {
                rectTransformTmp.sizeDelta = character.customSize;
                return true;
            }
            else
                return false;
        }

        private RichTextContentComponent CreateImage(CharacterInfo character)
        {
            var retValue = GetFromCacheOrCreateContent(null, character.type);
            var imageTmp = GetOrAddComponent<Image>(retValue.item);
            imageTmp.sprite = GetAsset<Sprite>(character);
            if (!SetCustomSizeInCharacter(character, retValue.item.gameObject))
            {
                imageTmp.SetNativeSize();
            }

            return retValue;
        }

        private RichTextContentComponent CreatePrefab(CharacterInfo character)
        {
            var loadObj = GetAsset<GameObject>(character);
            var retValue = GetFromCacheOrCreateContent(loadObj, character.type);
            retValue.item.SetActive(true);
            SetCustomSizeInCharacter(character, retValue.item.gameObject);

            retValue.item.transform.SetParent(this.transform, false);
            var transTmp = retValue.item.GetComponent<RectTransform>();
            if (transTmp == null)
                Debug.LogError("RichText CreatePrefab error: missing 'RectTransform' prefab=" + retValue);
            return retValue;
        }

        private T GetAsset<T>(CharacterInfo character) where T : Object
        {
            if (null == character.asset)
            {
                Debug.LogError("RichTextComponent GetAsset error: not found asset path=" + character.value);
                return null;
            }

            //不要再读取文件，直接从prefab中配置的引用更好点
            var retValue = character.asset as T;
            if (null == retValue)
            {
                Debug.LogError("RichTextComponent GetAsset error: not correct cast type=" + typeof(T).FullName + " path=" + character.value);
                return retValue;
            }
            return retValue;
        }

        private GameObject CreateGameObject()
        {
            var retValue = new GameObject();
            var transTmp = GetOrAddComponent<RectTransform>(retValue);
            transTmp.pivot = RichTextComponent.ToPivot(textAnchor);

            retValue.transform.SetParent(this.transform, false);
            return retValue;
        }

        private void UpdateLayout()
        {
            if (!_updateLayoutDirty)
                return;

            _updateLayoutDirty = false;
            _displayCharacters.Clear();
            CheckAutoRecycleCharacters();

            for (int i = 0; i < _willDisplayStrings.Count; ++i)
            {
                var infoTmp = _willDisplayStrings[i];
                var newCharacterInfo = SetTextWithRichText(infoTmp.data);
                newCharacterInfo.isReturn = infoTmp.isReturn;
                _displayCharacters.Add(newCharacterInfo);
            }

            if (_displayCharacters.Count == 0)
            {
                ClearCachedCharacters();
                return;
            }

            var transItemPrev = _displayCharacters[0].item.GetComponent<RectTransform>();
            transItemPrev.localPosition = Vector2.zero;
            var transItemsPrevLine = new List<RectTransform>();
            transItemsPrevLine.Add(transItemPrev);

            transItemPrev.gameObject.SetActive(true);

            UpdateTextAnchor(_displayCharacters[0]);
            for (int i = 1; i < _displayCharacters.Count; ++i)
            {
                var contentTmp = _displayCharacters[i];
                UpdateTextAnchor(contentTmp);
                var transItemNext = contentTmp.item.GetComponent<RectTransform>();

                contentTmp.item.SetActive(true);

                SetItemLayout(contentTmp, ref transItemPrev, transItemNext, transItemsPrevLine);
                if (CheckAutoWrap(contentTmp, transItemsPrevLine))
                    SetItemLayout(contentTmp, ref transItemPrev, transItemNext, transItemsPrevLine);

                transItemPrev = transItemNext;
                transItemsPrevLine.Add(transItemNext);
            }

            UpdatePrevLineItemsLayoutWhenVerticalMiddleAnchor(transItemsPrevLine);
            UpdateItemsLayoutWhenHorizontalMiddleAnchor(_displayCharacters);
        }

        private void UpdateTextAnchor(RichTextContentComponent content)
        {
            //刷新锚点
            if (content.type == TextType.Text)
            {
                var textTmp = content.item.GetComponent<Text>();
                var transText = textTmp.GetComponent<RectTransform>();

                //设置锚点
                textTmp.alignment = textAnchor;

                //根据文本的alignment自动设置锚点
                transText.pivot = RichTextComponent.ToPivot(textTmp.alignment);
            }
        }

        private bool CheckAutoWrap(RichTextContentComponent contentInfo, List<RectTransform> transItemsPrevLine)
        {
            if (horizontalOverflow != HorizontalWrapMode.Wrap || contentInfo.isReturn) return false;

            var transTmp = contentInfo.item.GetComponent<RectTransform>();
            var transMax = this.GetComponent<RectTransform>();

            var allItemSize = transTmp.sizeDelta;
            for (int i = 0; i < transItemsPrevLine.Count; ++i)
            {
                allItemSize += transItemsPrevLine[i].GetComponent<RectTransform>().sizeDelta;
            }
            allItemSize.x += _margin.x * transItemsPrevLine.Count;

            if (allItemSize.x > transMax.sizeDelta.x)
            {
                contentInfo.isReturn = true;
                return true;
            }
            else
            {
                contentInfo.isReturn = false;
                return false;
            }
        }

        private void SetItemLayout(RichTextContentComponent contentInfo, ref RectTransform transItemPrev, RectTransform transItemNext, List<RectTransform> transItemsPrevLine)
        {
            Rect rectItemNext = new Rect(0, 0, transItemNext.sizeDelta.x, transItemNext.sizeDelta.y);
            Vector3 pivotNext = Vector3.zero;

            if (contentInfo.isReturn)
            {
                var transFirstItem = transItemsPrevLine[0];
                float offsetVertical = 0;

                Vector3 pivotPrevLine = Vector3.zero;
                bool isNegative = textAnchor == TextAnchor.LowerLeft || textAnchor == TextAnchor.LowerCenter || textAnchor == TextAnchor.LowerRight;
                if (isNegative)
                {
                    pivotPrevLine = new Vector3(transFirstItem.pivot.x, 1);
                    pivotNext = new Vector3(transFirstItem.pivot.x, 0);
                    offsetVertical = _margin.y;
                }
                else
                {
                    pivotPrevLine = new Vector3(transFirstItem.pivot.x, 0);
                    pivotNext = new Vector3(transFirstItem.pivot.x, 1);
                    offsetVertical = -_margin.y;
                }

                rectItemNext.position = RichTextComponent.GetLocalPositionByPivot(transFirstItem.gameObject, pivotPrevLine) + new Vector3(0, offsetVertical);
                UpdatePrevLineItemsLayoutWhenVerticalMiddleAnchor(transItemsPrevLine);
                transItemsPrevLine.Clear();
            }
            else
            {
                Vector3 pivotSource = Vector3.zero;
                Vector3 pivotDes = Vector3.zero;
                float offsetHorizontal = 0;
                if (transItemPrev.pivot.x == 0.5f)
                {
                    pivotSource = RichTextComponent.Pivot.MiddleRight;
                    pivotDes = RichTextComponent.Pivot.MiddleLeft;

                    //中心点的margin偏移量在UpdatePrevLineItemsLayoutWhenMiddleAnchor方法中处理
                }
                else
                {
                    pivotSource = new Vector3(1 - transItemPrev.pivot.x, 0.5f);
                    pivotDes = new Vector3(transItemPrev.pivot.x, 0.5f);
                    offsetHorizontal = transItemPrev.pivot.x < 0.5f ? _margin.x : -_margin.x;
                }

                rectItemNext.position = RichTextComponent.GetLocalPositionByPivot(transItemPrev.gameObject, pivotSource) + new Vector3(offsetHorizontal, 0);
                pivotNext = pivotDes;
            }
            RichTextComponent.SetLocalPositionByPivot(transItemNext.gameObject, rectItemNext.position, pivotNext);
        }

        private void UpdatePrevLineItemsLayoutWhenVerticalMiddleAnchor(List<RectTransform> transItemsPrevLine)
        {
            if (transItemsPrevLine.Count < 2)
                return;

            if (contentAnchor != TextAnchor.UpperCenter && contentAnchor != TextAnchor.MiddleCenter && contentAnchor != TextAnchor.LowerCenter)
                return;

            var firstItem = transItemsPrevLine[0];
            var lastItem = transItemsPrevLine[transItemsPrevLine.Count - 1];
            var leftMiddlePos = RichTextComponent.GetLocalPositionByPivot(firstItem.gameObject, RichTextComponent.Pivot.MiddleLeft);
            var rightMiddlePos = RichTextComponent.GetLocalPositionByPivot(lastItem.gameObject, RichTextComponent.Pivot.MiddleRight);

            var rectTmp = new Rect(leftMiddlePos.x, leftMiddlePos.y, rightMiddlePos.x, rightMiddlePos.y);

            var offsetPosOfContentCenter = rectTmp.center - this.GetComponent<RectTransform>().rect.center;
            rectTmp.position -= new Vector2(offsetPosOfContentCenter.x + firstItem.rect.width / 4, 0);

            RichTextComponent.SetLocalPositionByPivot(firstItem.gameObject, rectTmp.min, RichTextComponent.Pivot.MiddleLeft);
            var prevItem = firstItem;
            for (int i = 1; i < transItemsPrevLine.Count; ++i)
            {
                var itemTmp = transItemsPrevLine[i];
                var prevItemPos = RichTextComponent.GetLocalPositionByPivot(prevItem.gameObject, RichTextComponent.Pivot.MiddleRight);

                var newPosPrevItem = prevItem.transform.localPosition;
                newPosPrevItem.x -= _margin.x / 2;
                prevItem.transform.localPosition = newPosPrevItem;
                prevItemPos.x += _margin.x / 2;
                RichTextComponent.SetLocalPositionByPivot(itemTmp.gameObject, prevItemPos, RichTextComponent.Pivot.MiddleLeft);

                prevItem = itemTmp;
            }
        }

        private void UpdateItemsLayoutWhenHorizontalMiddleAnchor(IList<RichTextContentComponent> displayCharacters)
        {
            if (displayCharacters.Count <= 1)
                return;

            if (contentAnchor != TextAnchor.MiddleLeft && contentAnchor != TextAnchor.MiddleCenter && contentAnchor != TextAnchor.MiddleRight)
                return;

            var firstItem = displayCharacters[0];
            var lastItem = displayCharacters[displayCharacters.Count - 1];

            float topY = RichTextComponent.GetLocalPositionByPivot(firstItem.item, RichTextComponent.Pivot.UpperCenter).y;
            float downY = RichTextComponent.GetLocalPositionByPivot(lastItem.item, RichTextComponent.Pivot.LowerCenter).y;

            var allHeight = topY - downY;
            var maxTop = allHeight / 2;
            var offsetPositionY = maxTop - RichTextComponent.GetLocalPositionByPivot(firstItem.item, RichTextComponent.Pivot.UpperCenter).y;

            for (int i = 0; i < displayCharacters.Count; ++i)
            {
                var itemTmp = displayCharacters[i];
                itemTmp.item.transform.localPosition += new Vector3(0, offsetPositionY, 0);
            }
        }

        private string GetAssetPathWithoutResourcesFolder(string path)
        {
            //因为Resources目录比较特殊，需要去除前面的相对路径才能正常读取
            int indexFind = path.IndexOf("Resources/");
            if (indexFind >= 0)
                path = path.Remove(0, indexFind + "Resources/".Length);
            return path;
        }

        private void UpdateListDataToDictionaryData()
        {
            if (_searchCharacters.Count > 0) return;

            for (int i = _savedCharacters.Count - 1; i >= 0; --i)
            {
                var characterTmp = _savedCharacters[i];
                _searchCharacters.Add(characterTmp.key, characterTmp);
            }
            SetText(_text);
        }
    }
}
