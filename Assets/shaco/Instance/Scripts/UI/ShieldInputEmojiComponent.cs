using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屏蔽输入emoj内容组件
/// </summary>
namespace shaco.Instance.UI
{
    [RequireComponent(typeof(InputField))]
    public class ShieldInputEmojiComponent : MonoBehaviour
    {
        private InputField _inputField;
        private string _prevInputText = string.Empty;

        //emoji表情匹配
        private readonly string[] _pattenEmoj = new string[] { @"\p{Cs}", @"\p{Co}", @"\p{Cn}", @"[\u2702-\u27B0]" };

        void Awake()
        {
            _inputField = GetComponent<InputField>();
            if (null == _inputField)
            {
                Debug.LogError(string.Format("ShieldInputEmojiComponent Awake error: not found component {0}", typeof(InputField).FullName));
                return;
            }
            _prevInputText = _inputField.text;
        }

        void Update()
        {
            OnInputTextChanged();
        }

        void OnInputTextChanged()
        {
            if (_prevInputText != _inputField.text)
            {
                string checkResult = _inputField.text;
                bool hasEmoji = false;
                for (int i = _pattenEmoj.Length - 1; i >= 0; --i)
                {
                    //屏蔽emoji
                    if (System.Text.RegularExpressions.Regex.IsMatch(checkResult, _pattenEmoj[i]))
                    {
                        checkResult = System.Text.RegularExpressions.Regex.Replace(checkResult, _pattenEmoj[i], string.Empty);
                        hasEmoji = true;
                    }
                }

                if (hasEmoji)
                    _inputField.text = checkResult;
                _prevInputText = _inputField.text;
            }
        }
    }
}