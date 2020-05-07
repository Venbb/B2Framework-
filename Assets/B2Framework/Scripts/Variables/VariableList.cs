using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace B2Framework
{
    [System.Serializable]
    public class VariableList
    {
        [SerializeField]
        private List<Variable> variables;

        public ReadOnlyCollection<Variable> Variables
        {
            get { return variables.AsReadOnly(); }
        }

        public Variable this[int index]
        {
            get { return variables[index]; }
        }

        public object Get(string name)
        {
            if (this.variables == null || this.variables.Count <= 0)
                return null;
            var variable = this.variables.Find(v => v.Name.Equals(name));
            if (variable == null)
                return null;
            return variable.GetValue();
        }

        public T Get<T>(string name)
        {
            if (this.variables == null || this.variables.Count <= 0)
                return default(T);
            var variable = this.variables.Find(v => v.Name.Equals(name));
            if (variable == null)
                return default(T);
            return variable.GetValue<T>();
        }

        public static implicit operator List<Variable>(VariableList array)
        {
            return array.variables;
        }

        public static implicit operator VariableList(List<Variable> variables)
        {
            return new VariableList() { variables = variables };
        }
    }
}
