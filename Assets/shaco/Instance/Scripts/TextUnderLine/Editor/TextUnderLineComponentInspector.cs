namespace shaco.Instance.TextUnderLine
{
    [UnityEditor.CustomEditor(typeof(TextUnderLineComponent))]
    public class TextUnderLineComponentInspector : UnityEditor.Editor
    {
        private TextUnderLineComponent _target;

        void OnEnable()
        {
            _target = target as TextUnderLineComponent;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _target.UpdateUnderLine();
        }
    }
}