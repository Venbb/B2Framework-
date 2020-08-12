using UnityEngine;

namespace B2Framework.Unity
{
    [System.Serializable]
    public class LuaScriptBinder : ISerializationCallbackReceiver
    {
        [SerializeField]
        protected TextAsset text;

        [SerializeField]
        protected string filename;

        [SerializeField]
        protected LuaScriptBindEnum type = LuaScriptBindEnum.Filename;
        public virtual LuaScriptBindEnum Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public virtual TextAsset Text
        {
            get { return this.text; }
            set { this.text = value; }
        }

        public virtual string Filename
        {
            get { return this.filename; }
            set { this.filename = value; }
        }
        public void OnAfterDeserialize()
        {
            if (Application.isEditor) return;
            switch (type)
            {
                case LuaScriptBindEnum.TextAsset:
                    this.filename = null;
                    break;
                case LuaScriptBindEnum.Filename:
                    this.text = null;
                    break;
            }
        }

        public void OnBeforeSerialize()
        {
            if (Application.isEditor) return;
            switch (type)
            {
                case LuaScriptBindEnum.TextAsset:
                    this.filename = null;
                    break;
                case LuaScriptBindEnum.Filename:
                    this.text = null;
                    break;
            }
        }
    }
}