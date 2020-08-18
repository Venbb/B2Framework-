using UnityEngine.UI;

namespace B2Framework
{
    public class UIText : Text
    {
        /// <summary>
        /// 多语言key
        /// </summary>
        public string lc_key;
        bool localize = false;
        protected override void Awake()
        {
            base.Awake();
            localize = !string.IsNullOrEmpty(lc_key);
            OnLocalize();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (localize)
            {
                Localization.OnLocalize += OnLocalize;
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (localize)
            {
                if (Localization.OnLocalize != null) Localization.OnLocalize -= OnLocalize;
            }
        }
        public void OnLocalize()
        {
            if (localize)
            {
                text = Localization.Get(lc_key);
            }
        }
    }
}