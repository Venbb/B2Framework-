using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace B2Framework.UI
{
    public class MessageBox : IEnumerator
    {
        private static readonly GameObject _prefab = Resources.Load<GameObject>("MessageBox");
        private static readonly List<MessageBox> _showed = new List<MessageBox>();
        private static readonly List<MessageBox> _hidden = new List<MessageBox>();

        public static MessageBox Show(string title, string content, string sure = "Sure", string cancel = "Cancel")
        {
            if (_hidden.Count > 0)
            {
                var mb = _hidden[0];
                mb.Init(title, content, sure, cancel);
                mb.gameObject.SetActive(true);
                _hidden.RemoveAt(0);
                return mb;
            }
            else
            {
                return new MessageBox(title, content, sure, cancel);
            }
        }
        public static void CloseAll()
        {
            for (var index = 0; index < _showed.Count; index++)
            {
                var messageBox = _showed[index];
                messageBox.Hide();
                _hidden.Add(messageBox);
            }
            _showed.Clear();
        }
        public static void Dispose()
        {
            foreach (var item in _hidden)
            {
                item.Destroy();
            }

            _hidden.Clear();

            foreach (var item in _showed)
            {
                item.Destroy();
            }

            _showed.Clear();
        }
        private bool _visible = true;
        private Text _title;
        private Text _content;
        private Text _textSure;
        private Text _textCancel;
        private GameObject gameObject { get; set; }
        public Action<bool> callBack { get; set; }
        public bool isOk { get; private set; }
        private MessageBox(string title, string content, string sure, string cancel)
        {
            gameObject = UnityEngine.Object.Instantiate(_prefab);
            gameObject.name = title;

            _title = GetComponent<Text>("Title");
            _content = GetComponent<Text>("Content/Text");
            _textSure = GetComponent<Text>("Buttons/Sure/Text");
            _textCancel = GetComponent<Text>("Buttons/Cancel/Text");

            var btnSure = GetComponent<Button>("Buttons/Sure");
            var btnCancel = GetComponent<Button>("Buttons/Cancel");
            btnSure.onClick.AddListener(OnSureBtnClick);
            btnCancel.onClick.AddListener(OnCancelBtnClick);

            this.Init(title, content, sure, cancel);
        }

        private void Init(string title, string content, string sure, string cancel)
        {
            _title.text = title;
            _content.text = content;
            _textSure.text = sure;
            _textCancel.text = cancel;

            _visible = true;
            _showed.Add(this);
            this.isOk = false;
        }
        private void OnSureBtnClick()
        {
            OnBtnClick(true);
        }
        private void OnCancelBtnClick()
        {
            OnBtnClick(false);
        }
        private void OnBtnClick(bool sure)
        {
            Close();

            this.isOk = sure;

            if (callBack == null) return;
            callBack(sure);
            callBack = null;
        }
        private T GetComponent<T>(string path) where T : Component
        {
            var trans = gameObject.transform.Find(path);
            return trans.GetComponent<T>();
        }
        public void Close()
        {
            Hide();
            _hidden.Add(this);
            _showed.Remove(this);
        }
        private void Hide()
        {
            gameObject.SetActive(false);
            _visible = false;
        }
        public void Destroy()
        {
            _title = null;
            _textSure = null;
            _textCancel = null;
            _content = null;
            UnityEngine.Object.DestroyImmediate(gameObject);
            gameObject = null;
        }

        #region IEnumerator implementation
        public object Current { get { return null; } }
        public bool MoveNext()
        {
            return _visible;
        }
        public void Reset() { }
        #endregion
    }
}