using UnityEngine;

namespace B2Framework
{
    [System.Serializable]
    public class Variable
    {
        [SerializeField]
        protected string name = "";

        [SerializeField]
        protected UnityEngine.Object objectValue;

        [SerializeField]
        protected string dataValue;

        [SerializeField]
        protected VariableEnum variableType;

        public virtual string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public virtual VariableEnum VariableType
        {
            get { return this.variableType; }
        }

        public virtual System.Type ValueType
        {
            get
            {
                switch (this.variableType)
                {
                    case VariableEnum.Boolean:
                        return typeof(bool);
                    case VariableEnum.Float:
                        return typeof(float);
                    case VariableEnum.Integer:
                        return typeof(int);
                    case VariableEnum.String:
                        return typeof(string);
                    case VariableEnum.Color:
                        return typeof(Color);
                    case VariableEnum.Vector2:
                        return typeof(Vector2);
                    case VariableEnum.Vector3:
                        return typeof(Vector3);
                    case VariableEnum.Vector4:
                        return typeof(Vector4);
                    case VariableEnum.Object:
                        return this.objectValue == null ? typeof(UnityEngine.Object) : this.objectValue.GetType();
                    case VariableEnum.GameObject:
                        return this.objectValue == null ? typeof(GameObject) : this.objectValue.GetType();
                    case VariableEnum.Component:
                        return this.objectValue == null ? typeof(Component) : this.objectValue.GetType();
                    default:
                        throw new System.NotSupportedException();
                }
            }
        }

        public virtual void SetValue<T>(T value)
        {
            this.SetValue(value);
        }

        public virtual T GetValue<T>()
        {
            return (T)GetValue();
        }

        public virtual void SetValue(object value)
        {
            switch (this.variableType)
            {
                case VariableEnum.Boolean:
                    this.dataValue = Utility.Converter.GetString((bool)value);
                    break;
                case VariableEnum.Float:
                    this.dataValue = Utility.Converter.GetString((float)value);
                    break;
                case VariableEnum.Integer:
                    this.dataValue = Utility.Converter.GetString((int)value);
                    break;
                case VariableEnum.String:
                    this.dataValue = Utility.Converter.GetString((string)value);
                    break;
                case VariableEnum.Color:
                    this.dataValue = Utility.Converter.GetString((Color)value);
                    break;
                case VariableEnum.Vector2:
                    this.dataValue = Utility.Converter.GetString((Vector2)value);
                    break;
                case VariableEnum.Vector3:
                    this.dataValue = Utility.Converter.GetString((Vector3)value);
                    break;
                case VariableEnum.Vector4:
                    this.dataValue = Utility.Converter.GetString((Vector4)value);
                    break;
                case VariableEnum.Object:
                    this.objectValue = (UnityEngine.Object)value;
                    break;
                case VariableEnum.GameObject:
                    this.objectValue = (GameObject)value;
                    break;
                case VariableEnum.Component:
                    this.objectValue = (Component)value;
                    break;
                default:
                    throw new System.NotSupportedException();
            }
        }
        public virtual object GetValue()
        {
            switch (this.variableType)
            {
                case VariableEnum.Boolean:
                    return Utility.Converter.ToBoolean(this.dataValue);
                case VariableEnum.Float:
                    return Utility.Converter.ToSingle(this.dataValue);
                case VariableEnum.Integer:
                    return Utility.Converter.ToInt32(this.dataValue);
                case VariableEnum.String:
                    return Utility.Converter.ToString(this.dataValue);
                case VariableEnum.Color:
                    return Utility.Converter.ToColor(this.dataValue);
                case VariableEnum.Vector2:
                    return Utility.Converter.ToVector2(this.dataValue);
                case VariableEnum.Vector3:
                    return Utility.Converter.ToVector3(this.dataValue);
                case VariableEnum.Vector4:
                    return Utility.Converter.ToVector4(this.dataValue);
                case VariableEnum.Object:
                    return this.objectValue;
                case VariableEnum.GameObject:
                    return this.objectValue;
                case VariableEnum.Component:
                    return this.objectValue;
                default:
                    throw new System.NotSupportedException();
            }
        }
    }
}
