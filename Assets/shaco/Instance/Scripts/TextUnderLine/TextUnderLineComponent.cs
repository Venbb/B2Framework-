using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace shaco.Instance.TextUnderLine
{
    //下划线组件，支持自定义颜色、位置、粗细、点击事件等
    [AddComponentMenu("UI/Effects/Text UnderLine")]
    public class TextUnderLineComponent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private class UnderlineProperty
        {
            public Vector3 _position;
            public float _width;
            public float _height;
            public Vector2 _privot;
        }

        //是否根据下划线粗细动态调整lineSpacing
        [SerializeField]
        private bool _autoResizeLineSpacing = true;

        //下划线颜色
        [SerializeField]
        private Color _underlineColor = new Color(0, 0, 0, 1);

        //下划线粗细
        [SerializeField]
        private float _underlineHeight = 1;

        //下划线偏移量
        [SerializeField]
        private Vector3 _offsetPosition = Vector3.zero;

        //最大缓存数量
        [SerializeField]
        [Range(0, 100)]
        private int _maxCachedLineCount = 0;

        //下划线点击事件
        public System.Action clickEvent
        {
            get
            {
                return _clickEvent;
            }

            set
            {
                _clickEvent = value;
            }
        }

        private Text _text;
        private List<Image> _lines = new List<Image>();
        private List<Image> _linesCached = new List<Image>();
        private System.Action _clickEvent = null;
        private bool _isInitUnderline = false;
        private string _prevText;
        private int _prevCharecterCount;
        private float _fixedEmptyFlagWidth = 0;
        private bool _isForceUpdateWhenTextChanged = false;
        private Color _prevUnderlineColor;
        private Vector3 _prevOffsetPosition;
        private float _prevUnderlineHeight;

        //实现下划线同步点击效果
        public void OnPointerDown(PointerEventData eventData)
        {
            if (null == _clickEvent)
                return;

            for (int i = 0; i < _lines.Count; ++i)
            {
                Color[] colors = _lines[i].sprite.texture.GetPixels();
                for (int j = 0; j < colors.Length; ++j)
                    colors[j] = new Color(colors[j].r, colors[j].g, colors[j].b, colors[j].a * 0.70f);
                _lines[i].sprite.texture.SetPixels(colors);
                _lines[i].sprite.texture.Apply();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (null == _clickEvent)
                return;

            for (int i = 0; i < _lines.Count; ++i)
            {
                Color[] colors = _lines[i].sprite.texture.GetPixels();
                for (int j = 0; j < colors.Length; ++j)
                    colors[j] = new Color(colors[j].r, colors[j].g, colors[j].b, colors[j].a / 0.70f);
                _lines[i].sprite.texture.SetPixels(colors);
                _lines[i].sprite.texture.Apply();
            }
        }

        public void UpdateUnderLine()
        {
            CheckInit();
            Update();
        }

        private void Start()
        {
            CheckInit();
        }

        private void OnEnable()
        {
            for (int i = _lines.Count - 1; i >= 0; --i)
            {
                _lines[i].enabled = true;
            }

            for (int i = this.transform.childCount - 1; i >= 0; --i)
            {
                var child = this.transform.GetChild(i);
                var childImage = child.GetComponent<Image>();
                if (null == childImage || childImage.enabled)
                    continue;

                if (child.name.StartsWith("underline"))
                    childImage.enabled = true;
            }
        }

        private void OnDisable()
        {
            for (int i = _lines.Count - 1; i >= 0; --i)
            {
                _lines[i].enabled = false;
            }

            for (int i = this.transform.childCount - 1; i >= 0; --i)
            {
                var child = this.transform.GetChild(i);
                var childImage = child.GetComponent<Image>();
                if (null == childImage || !childImage.enabled)
                    continue;

                if (child.name.StartsWith("underline"))
                    childImage.enabled = false;
            }
        }

        private void Update()
        {
            if (!_isInitUnderline || !this.enabled)
                return;

            //当二者文本长度相同但是内容发生变化的时候需要特殊处理
            if (null != _prevText && _prevText.Length == _text.text.Length && _prevText != _text.text)
            {
                RequestForceUpdateTextLayout();
            }

            //刷新字符
            if (_prevCharecterCount != _text.cachedTextGenerator.characterCount)
            {
                UpdateCharacters();
            }

            //刷新偏移位置
            if (_prevOffsetPosition != _offsetPosition && _text.cachedTextGenerator.lines.Count > 0)
            {
                UpdateOffsetPosition();
            }

            //刷新下划线高度
            if (_prevUnderlineHeight != _underlineHeight)
            {
                _prevUnderlineHeight = _underlineHeight;
                for (int i = _lines.Count - 1; i >= 0; --i)
                {
                    var transTmp = _lines[i].rectTransform;
                    transTmp.sizeDelta = new Vector2(transTmp.sizeDelta.x, _underlineHeight);
                }

                //重新设置文本 lineSpacing
                if (_autoResizeLineSpacing && _text.cachedTextGenerator.lines.Count > 0)
                {
                    var lineHeight = _text.cachedTextGenerator.lines[0].height;
                    if (lineHeight > 0)
                    {
                        if (_underlineHeight == 1)
                        {
                            _text.lineSpacing = 1;
                            RequestForceUpdateTextLayout();
                        }
                        else
                        {
                            _text.lineSpacing = (_underlineHeight - 1) / lineHeight + 1.0f;
                            UpdateCharacters();
                        }

                    }
                }
            }

            //刷新颜色
            if (_prevUnderlineColor != _underlineColor)
            {
                _prevUnderlineColor = _underlineColor;
                for (int i = _lines.Count - 1; i >= 0; --i)
                {
                    _lines[i].color = _underlineColor;
                }
            }
        }

        private void RequestForceUpdateTextLayout()
        {
            //强制修改文本，让Text才可以重新刷新内部布局
            _text.text += " ";
            _isForceUpdateWhenTextChanged = true;
        }

        private void UpdateCharacters()
        {
            _prevCharecterCount = _text.cachedTextGenerator.characterCount;
            _prevText = _text.text;

            RecyclingTextCache();

            List<UnderlineProperty> list = GetUnderlinePropertys();
            CreateUnderLines(list);

            //回滚强制修改的文本
            if (_isForceUpdateWhenTextChanged)
            {
                _text.text = _text.text.Remove(_text.text.Length - 1);
                _isForceUpdateWhenTextChanged = false;
            }
        }

        private void UpdateOffsetPosition()
        {
            _prevOffsetPosition = _offsetPosition;
            var charactersTmp = _text.cachedTextGenerator.characters;
            var linesTmp = _text.cachedTextGenerator.lines;
            for (int i = _lines.Count - 1; i >= 0; --i)
            {
                var curPos = charactersTmp[linesTmp[i].startCharIdx].cursorPos;
                var width = GetWidth(_text.cachedTextGenerator.lines[i].startCharIdx, _text.cachedTextGenerator.characters);
                curPos.x = GetPositionXWithTextAlignment(curPos.x, width);
                _lines[i].transform.localPosition = new Vector3(curPos.x + _offsetPosition.x, curPos.y - linesTmp[i].height + _offsetPosition.y, _offsetPosition.z);
            }
        }

        private List<UnderlineProperty> GetUnderlinePropertys()
        {
            List<UnderlineProperty> list = new List<UnderlineProperty>();
            for (int i = 0; i < _text.cachedTextGenerator.lineCount; ++i)
            {
                var curPos = _text.cachedTextGenerator.characters[_text.cachedTextGenerator.lines[i].startCharIdx].cursorPos;
                UnderlineProperty property = new UnderlineProperty
                {
                    _height = _text.cachedTextGenerator.lines[i].height,
                    _width = GetWidth(_text.cachedTextGenerator.lines[i].startCharIdx, _text.cachedTextGenerator.characters),
                    _position = new Vector3(curPos.x, curPos.y - _text.cachedTextGenerator.lines[i].height, 0) + _offsetPosition,
                    _privot = GetTextAnchorPivot(_text.alignment)
                };

                list.Add(property);
            }

            //临时回滚修改文本的长度，以防止下划线会跳一下的bug
            if (_isForceUpdateWhenTextChanged && list.Count > 0)
            {
                var lastProperty = list[list.Count - 1];
                lastProperty._width -= _text.cachedTextGenerator.GetPreferredWidth(" ", _text.GetGenerationSettings(Vector2.zero));
            }

            return list;
        }

        private float GetWidth(int idx, IList<UICharInfo> info)
        {
            float width = 0;
            float start = info[idx].cursorPos.x;
            for (int i = idx; i < info.Count - 1; ++i)
            {
                if (info[i].cursorPos.x > info[i + 1].cursorPos.x)
                {
                    width = info[i].cursorPos.x - start + info[i].charWidth;
                    break;
                }

                if (info.Count - 1 == i + 1)
                    width = info[i + 1].cursorPos.x - start;
            }
            return width;
        }

        private Vector2 GetTextAnchorPivot(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.LowerLeft: return new Vector2(0, 0);
                case TextAnchor.LowerCenter: return new Vector2(0.5f, 0);
                case TextAnchor.LowerRight: return new Vector2(1, 0);
                case TextAnchor.MiddleLeft: return new Vector2(0, 0.5f);
                case TextAnchor.MiddleCenter: return new Vector2(0.5f, 0.5f);
                case TextAnchor.MiddleRight: return new Vector2(1, 0.5f);
                case TextAnchor.UpperLeft: return new Vector2(0, 1);
                case TextAnchor.UpperCenter: return new Vector2(0.5f, 1);
                case TextAnchor.UpperRight: return new Vector2(1, 1);
                default: return Vector2.zero;
            }
        }

        private void RecyclingTextCache()
        {
            _linesCached.Clear();
            for (int i = this.transform.childCount - 1; i >= 0; --i)
            {
                bool isValidChild = false;
                var child = this.transform.GetChild(i);
                if (_linesCached.Count < _maxCachedLineCount && child.name.StartsWith("underline"))
                {
                    var imageLine = child.GetComponent<Image>();
                    if (null != imageLine)
                    {
                        _linesCached.Add(imageLine);
                        imageLine.enabled = false;
                        isValidChild = true;
                    }
                }

                if (!isValidChild)
                {
                    if (Application.isPlaying)
                        MonoBehaviour.Destroy(child.gameObject);
                    else
                    {
                        child.gameObject.SetActive(false);
                        MonoBehaviour.DestroyImmediate(child.gameObject);
                    }
                }
            }
        }

        private void CreateUnderLines(List<UnderlineProperty> list)
        {
            _lines.Clear();

            for (int i = 0; i < list.Count; ++i)
            {
                //初始化
                var property = list[i];
                var underlineTmp = GetOrCreateUnderline(i, property);
                _lines.Add(underlineTmp);
                underlineTmp.rectTransform.pivot = property._privot;

                if (property._width == 0 || property._height == 0)
                {
                    underlineTmp.enabled = false;
                    continue;
                }

                //大小
                if (_underlineHeight < 0)
                    underlineTmp.rectTransform.sizeDelta = new Vector2(property._width, -_underlineHeight - _text.rectTransform.sizeDelta.y);
                else
                    underlineTmp.rectTransform.sizeDelta = new Vector2(property._width, _underlineHeight);

                //颜色
                underlineTmp.color = _underlineColor;

                //坐标
                float x = GetPositionXWithTextAlignment(property._position.x, property._width);
                underlineTmp.rectTransform.anchoredPosition = new Vector2(x, property._position.y);
            }
        }

        private float GetPositionXWithTextAlignment(float sourcePosX, float width)
        {
            float x = sourcePosX;
            if (_text.alignment == TextAnchor.MiddleCenter || _text.alignment == TextAnchor.UpperCenter || _text.alignment == TextAnchor.LowerCenter)
                x = 0;
            if (_text.alignment == TextAnchor.MiddleRight || _text.alignment == TextAnchor.UpperRight || _text.alignment == TextAnchor.LowerRight)
                x += width;
            return x;
        }

        private void CheckInit()
        {
            if (_isInitUnderline)
                return;
            _isInitUnderline = true;

            _text = transform.GetComponent<Text>();
            _prevText = _text.text;
            _prevUnderlineHeight = _underlineHeight;

            _fixedEmptyFlagWidth = _text.cachedTextGenerator.GetPreferredWidth(" ", _text.GetGenerationSettings(Vector2.zero));

            if (null != _clickEvent)
            {
                _text.gameObject.AddComponent<Button>().onClick.AddListener(() =>
                {
                    if (_clickEvent != null)
                        _clickEvent();
                });
            }
        }

        private Image GetOrCreateUnderline(int index, UnderlineProperty property)
        {
            Image retValue = null;

            //新创建
            if (_linesCached.Count == 0)
            {
                retValue = new GameObject("underline" + index).AddComponent<Image>();
                retValue.transform.SetParent(this.transform, false);
                retValue.raycastTarget = false;
                var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                retValue.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }

            //从缓存获取
            if (null == retValue)
            {
                retValue = _linesCached[_linesCached.Count - 1];
                _linesCached.RemoveAt(_linesCached.Count - 1);
                retValue.enabled = true;
            }

            return retValue;
        }
    }
}